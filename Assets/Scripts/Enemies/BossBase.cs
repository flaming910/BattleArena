using UnityEngine;

public class BossBase : MonoBehaviour
{

    protected int phase;
    protected bool playerInRange;

    protected Rigidbody rigidBody;

    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] protected int contactDamage;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        phase = 1;
        health = maxHealth;
        rigidBody = GetComponent<Rigidbody>();

        UIManager.Instance.SetBossHealth(maxHealth, health);
    }

    //Handles taking damage and change of phases
    public void TakeDamage(float damage)
    {
        health -= damage;
        UIManager.Instance.SetBossHealth(maxHealth, health);
        if (health <= maxHealth * 0.75f && health > maxHealth * 0.5f)
        {
            phase = 2;
        }
        else if (health <= maxHealth * 0.5f && health > maxHealth * 0.25f)
        {
            phase = 3;
        }
        else if (health <= maxHealth * 0.25f && health > 0)
        {
            phase = 4;
        }
        else if(health < 0)
        {
            //Die
            Destroy(this.gameObject);
            UIManager.Instance.TriggerVictoryScreen();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
