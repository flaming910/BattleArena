using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerControl : MonoBehaviour
{
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool meleeActive;

    [SerializeField] private float maxHealth;
    [SerializeField] private float speed;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private float reloadTime;
    [SerializeField] private float dashCooldown;

    public float health;
    private float actualSpeed;

    private float timeSinceShot;
    private float timeSinceDash;
    private float iFrames;

    private bool dashing;
    private bool beingDislocated;
    private bool blocking;
    private int dashFrames;
    private int dislocationFrames;

    private Vector3 dashVelocity;
    private Vector3 dislocationVelocity;

    private Rigidbody rigidBody;
    private Animator playerAnim;

    #region AnimatorParameters
    private static readonly int MeleeParam = Animator.StringToHash("Melee");
    private static readonly int ShootParam = Animator.StringToHash("Shoot");
    private static readonly int BlockingParam = Animator.StringToHash("Blocking");
    private static readonly int DirectionParam = Animator.StringToHash("Direction");
    private static readonly int XDirectionParam = Animator.StringToHash("xDirection");
    private static readonly int ZDirectionParam = Animator.StringToHash("zDirection");
    #endregion

    #region AudioParameters
    private AudioSource audioSource;
    private float initialPitch;
    private float initialVolume;
    [SerializeField] private AudioClip[] audioClips;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rigidBody = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        initialPitch = audioSource.pitch;
        initialVolume = audioSource.volume;
        timeSinceShot = reloadTime;
        timeSinceDash = dashCooldown;
        canAttack = true;
        actualSpeed = speed;

        UIManager.Instance.SetPlayerHealth(maxHealth, health);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerAnim.SetInteger(DirectionParam, 0);
        if (beingDislocated)
        {
            transform.Translate(dislocationVelocity);
//            rigidBody.velocity = dislocationVelocity;
            dislocationFrames--;
            if (dislocationFrames == 0)
            {
                beingDislocated = false;
            }
        }
        //Dash over multiple frames
        else if (dashing)
        {
            rigidBody.velocity = dashVelocity;
            dashFrames++;
            if (dashFrames == 5)
            {
                dashing = false;
            }
        }
        //Prevent movement during melee
        else if (meleeActive)
        {
            rigidBody.velocity = Vector3.zero;
        }
        //Move
        else
        {
            float zVelocity = Input.GetAxis("Vertical");
            float xVelocity = Input.GetAxis("Horizontal");
            var targetVelocity = new Vector3(xVelocity, 0, zVelocity);
            targetVelocity = Vector3.ClampMagnitude(targetVelocity, 1);
            var directionalVelocity = Quaternion.AngleAxis(-transform.rotation.eulerAngles.y, Vector3.up) * targetVelocity;
            targetVelocity *= actualSpeed;
            rigidBody.velocity = targetVelocity;
            bool fullSpeed = Math.Abs(actualSpeed - speed) < 0.01f;

            if (!fullSpeed)
            {
                directionalVelocity *= 0.5f;
            }

            playerAnim.SetFloat(XDirectionParam, directionalVelocity.x);
            playerAnim.SetFloat(ZDirectionParam, directionalVelocity.z);
        }
    }

    private void Update()
    {
        if (iFrames > 0)
        {
            iFrames -= Time.deltaTime;
        }
        //Look at mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 target = ray.GetPoint(distance);
            Vector3 direction=target-transform.position;
            float rotation=Mathf.Atan2(direction.x, direction.z)*Mathf.Rad2Deg;
            transform.rotation=Quaternion.Euler(0, rotation, 0);
        }

        //Block
        if (Input.GetKey(KeyCode.Space))
        {
            playerAnim.SetBool(BlockingParam, true);
            blocking = true;
            actualSpeed = speed * 0.3f;
        }
        else
        {
            actualSpeed = speed;
            blocking = false;
            playerAnim.SetBool(BlockingParam, false);
        }

        //Shoot
        if (timeSinceShot <= reloadTime)
        {
            actualSpeed = speed * 0.38f;
            timeSinceShot += Time.deltaTime;
        }

        if (canAttack && !dashing && !blocking)
        {
            if (Input.GetKey(KeyCode.Mouse0) && timeSinceShot >= reloadTime)
            {
                Shoot();
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                Melee();
            }
        }

        //Dash
        if (timeSinceDash <= dashCooldown)
        {
            timeSinceDash += Time.deltaTime;
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && !meleeActive && !blocking)
        {
            Dash();
        }


    }

    //Handle bullet instantiating
    private void Shoot()
    {
        var bulletObj = Instantiate(bullet, bulletOrigin.position, transform.rotation);
        bulletObj.GetComponent<Rigidbody>().velocity = transform.forward * 32;
        bulletObj.GetComponent<Projectile>().lifetime = 1.4f;
        bulletObj.GetComponent<Projectile>().damage = 1.6f;
        playerAnim.SetTrigger(ShootParam);
        //canAttack = false;
        timeSinceShot = 0;
    }

    //Call melee animation which handles all the necessary things
    private void Melee()
    {
        //3 damage
        playerAnim.SetTrigger(MeleeParam);
    }

    //Initiate dashing
    private void Dash()
    {
        timeSinceDash = 0;
        dashing = true;
        dashFrames = 0;
        if (rigidBody.velocity.magnitude <= 0.05f)
        {
            dashVelocity = transform.forward * 75;
        }
        else
        {
            dashVelocity = rigidBody.velocity.normalized * 75;
        }

    }


    public void TakeDamage(int damage, bool blockable)
    {
        if (blockable && blocking) return;
        health -= damage;
        iFrames = 0.17f;
        UIManager.Instance.SetPlayerHealth(maxHealth, health);
        StartCoroutine(UIManager.Instance.GetHit());
        if (health <= 0)
        {
            //Die
            Destroy(this.gameObject);
            UIManager.Instance.TriggerDeathScreen();
        }
    }

    public void Dislocate(Vector3 velocity, int framesToDislocate)
    {
        dislocationFrames = framesToDislocate;
        dislocationVelocity = velocity;
        beingDislocated = true;
    }

    public void FootstepSound()
    {
        audioSource.pitch = initialPitch + (Random.Range(-0.05f, 0.05f));
        audioSource.volume = initialVolume + (Random.Range(-0.05f, 0.05f));

        audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
    }

}
