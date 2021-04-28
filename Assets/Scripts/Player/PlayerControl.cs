using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public struct AudioSourceParameters
{
    public AudioSource audioSource;
    [HideInInspector] public float initialPitch;
    [HideInInspector] public float initialVolume;
}


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
    [SerializeField] private AudioSourceParameters footstepAudioParams;
    [SerializeField] private AudioSourceParameters gunAudioParams;
    private float initialPitch;
    private float initialVolume;
    [FormerlySerializedAs("audioClips")] [SerializeField] private AudioClip[] footstepAudio;
    [SerializeField]private AudioClip gunshotSound;

    #endregion

    private ChromaticAberration chromaticAberration;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rigidBody = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        footstepAudioParams.initialPitch = footstepAudioParams.audioSource.pitch;
        footstepAudioParams.initialVolume = footstepAudioParams.audioSource.volume;

        gunAudioParams.initialPitch = gunAudioParams.audioSource.pitch;
        gunAudioParams.initialVolume = gunAudioParams.audioSource.volume;

        timeSinceShot = reloadTime;
        timeSinceDash = dashCooldown;
        canAttack = true;
        actualSpeed = speed;

        cam = Camera.main;
        cam.GetComponent<Volume>().profile.TryGet<ChromaticAberration>(out chromaticAberration);

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
            chromaticAberration.active = true;
            cam.fieldOfView = 75;

            if (dislocationFrames == 0)
            {
                beingDislocated = false;
                cam.fieldOfView = 70;
                chromaticAberration.active = false;
            }
        }
        //Dash over multiple frames
        else if (dashing)
        {
            rigidBody.velocity = dashVelocity;
            dashFrames++;

            chromaticAberration.active = true;
            cam.fieldOfView = 75;
            if (dashFrames == 5)
            {
                dashing = false;
                cam.fieldOfView = 70;
                chromaticAberration.active = false;
            }
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
        }

        //Dash
        if (timeSinceDash <= dashCooldown)
        {
            timeSinceDash += Time.deltaTime;
        }
        else if ( (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1)) && !meleeActive && !blocking)
        {
            Dash();
        }


    }

    //Handle bullet instantiating
    private void Shoot()
    {
        var bulletObj = Instantiate(bullet, bulletOrigin.position, transform.rotation);
        bulletObj.GetComponent<Rigidbody>().velocity = transform.forward * 32;
        bulletObj.GetComponent<Projectile>().lifetime = 1.8f;
        bulletObj.GetComponent<Projectile>().damage = 1.8f;
        playerAnim.SetTrigger(ShootParam);

        gunAudioParams.audioSource.pitch = gunAudioParams.initialPitch + (Random.Range(-0.05f, 0.05f));
        gunAudioParams.audioSource.volume = gunAudioParams.initialVolume + (Random.Range(-0.025f, 0.025f));
        gunAudioParams.audioSource.PlayOneShot(gunshotSound);

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
        footstepAudioParams.audioSource.pitch = footstepAudioParams.initialPitch + (Random.Range(-0.05f, 0.05f));
        footstepAudioParams.audioSource.volume = footstepAudioParams.initialVolume + (Random.Range(-0.05f, 0.05f));

        footstepAudioParams.audioSource.PlayOneShot(footstepAudio[Random.Range(0, footstepAudio.Length)]);
    }

}
