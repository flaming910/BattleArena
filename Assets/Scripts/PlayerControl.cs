using System;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool meleeActive;

    [SerializeField] private float speed;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject blockCollider;
    [SerializeField] private float reloadTime;
    [SerializeField] private float dashCooldown;

    private float actualSpeed;
    private float timeSinceShot;
    private float timeSinceDash;
    private bool dashing;
    private bool blocking;
    private int dashFrames;
    private Vector3 dashVelocity;
    private Rigidbody rigidBody;
    private Animator playerAnim;

    #region AnimatorParameters
    private static readonly int MeleeParam = Animator.StringToHash("Melee");

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        timeSinceShot = reloadTime;
        timeSinceDash = dashCooldown;
        canAttack = true;
        actualSpeed = speed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (meleeActive) return;
        if (dashing)
        {
            rigidBody.velocity = dashVelocity;
            dashFrames++;
            if (dashFrames == 5)
            {
                dashing = false;
            }
        }
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
        //Shoot
        if (timeSinceShot <= reloadTime)
        {
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

        if (Input.GetKey(KeyCode.Space))
        {
            blockCollider.SetActive(true);
            blocking = true;
            actualSpeed = speed * 0.45f;
        }
        else
        {
            actualSpeed = speed;
            blocking = false;
            blockCollider.SetActive(false);
        }
    }

    private void Shoot()
    {
        var bulletObj = Instantiate(bullet, transform.position, transform.rotation);
        bulletObj.GetComponent<Rigidbody>().velocity = transform.forward * 20;
        bulletObj.GetComponent<Projectile>().lifetime = 1.5f;
        bulletObj.GetComponent<Projectile>().damage = 1f;
        canAttack = false;
        timeSinceShot = 0;

    }

    public void MeleeStart()
    {
        meleeActive = !meleeActive;
    }

    private void Melee()
    {
        playerAnim.SetTrigger(MeleeParam);
    }

    private void Dash()
    {
        timeSinceDash = 0;
        dashing = true;
        dashFrames = 0;
        dashVelocity = transform.forward * 75;
//        rigidBody.AddForce(transform.forward * 25, ForceMode.Impulse);
    }
}
