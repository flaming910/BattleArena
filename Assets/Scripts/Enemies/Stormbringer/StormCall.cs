using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class StormCall : MonoBehaviour
{
    private int damage;
    private float windup;

    [SerializeField] private VisualEffect vfx;
    private bool vfxStarted;

    public void SetValues(int damage, float windup)
    {
        this.windup = windup;
        this.damage = damage;
        vfx.SetFloat("WindupDuration", windup);
        vfx.SendEvent("TriggerWindup");
    }

    // Update is called once per frame
    void Update()
    {
        if (windup <= 0.0868 && !vfxStarted)
        {
            vfx.SendEvent("TriggerLightning");
            vfxStarted = true;
        }
        if (windup <= 0)
        {
            gameObject.GetComponent<SphereCollider>().enabled = true;
            StartCoroutine(DestroyCoroutine());
        }
        else
        {
            windup -= Time.deltaTime;
        }
    }

    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(0.23f);
        gameObject.GetComponent<SphereCollider>().enabled = false;
        yield return new WaitForSeconds(0.40f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerControl>().TakeDamage(damage, false);
        }
    }
}
