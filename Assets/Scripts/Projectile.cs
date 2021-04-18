using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime;
    public float damage;

    // Update is called once per frame
    void Update()
    {
        if (lifetime > 0)
        {
            lifetime -= Time.deltaTime;
        }
        else
        {
            DestroyProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.transform.parent.GetComponent<BossBase>().TakeDamage(damage);
            DestroyProjectile();
        }
        else if (other.CompareTag("Environment"))
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

}
