using System;
using System.Collections;
using UnityEngine;

public class StormCall : MonoBehaviour
{
    private int damage;
    private float windup;

    public void SetValues(int damage, float windup)
    {
        this.windup = windup;
        this.damage = damage;
    }

    // Update is called once per frame
    void Update()
    {
        if (windup <= 0)
        {
            gameObject.GetComponent<SphereCollider>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
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
