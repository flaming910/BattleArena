using UnityEngine;

public class BulletImpactDestroy : MonoBehaviour
{

    private float lifetime;
    // Start is called before the first frame update
    void Start()
    {
        lifetime = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
