using System;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool meleeActive;

    [SerializeField] private float maxHealth;
    [SerializeField] private float speed;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject blockCollider;
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

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rigidBody = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        timeSinceShot = reloadTime;
        timeSinceDash = dashCooldown;
        canAttack = true;
        actualSpeed = speed;

        UIManager.Instance.SetPlayerHealth(maxHealth, health);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
            targetVelocity *= actualSpeed;
            rigidBody.velocity = targetVelocity;
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
            blockCollider.SetActive(true);
            blocking = true;
            actualSpeed = speed * 0.3f;
        }
        else
        {
            actualSpeed = speed;
            blocking = false;
            blockCollider.SetActive(false);
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
        var bulletObj = Instantiate(bullet, transform.position, transform.rotation);
        bulletObj.GetComponent<Rigidbody>().velocity = transform.forward * 25;
        bulletObj.GetComponent<Projectile>().lifetime = 1.5f;
        bulletObj.GetComponent<Projectile>().damage = 1f;
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
        dashVelocity = transform.forward * 75;
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

}
