using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderWave : MonoBehaviour
{

    private int damage;
    private float speed;
    private float lifetime;
    private float timeElapsed;

    private Vector3 initialScale;

    public void SetValues(int damage, float speed, float lifetime)
    {
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;
        initialScale = transform.localScale;
    }


    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        float scale = Mathf.Lerp(1, 10, timeElapsed / lifetime);
        var scaleVector = initialScale;
        scaleVector *= scale;
        transform.localScale = scaleVector;

        if (timeElapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerControl>().TakeDamage(damage, true);
        }
    }
}
