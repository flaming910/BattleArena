using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    public float lifetime;
    public float damage;

    [SerializeField] private GameObject bulletImpact;

    // Update is called once per frame
    void Update()
    {
        if (lifetime > 0)
        {
            lifetime -= Time.deltaTime;
        }
        else
        {
            //DestroyProjectile();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.transform.parent.GetComponent<BossBase>().TakeDamage(damage);
            DestroyProjectile(other);
        }
        else if (other.CompareTag("Environment"))
        {
            DestroyProjectile(other);
        }
    }

    private void DestroyProjectile(Collider other)
    {

        bulletImpact.transform.parent = null;
//        bulletImpact.transform.position = other.ClosestPointOnBounds(transform.position)- (transform.forward * 0.07f);
        bulletImpact.SetActive(true);
        Destroy(gameObject);
    }

}
