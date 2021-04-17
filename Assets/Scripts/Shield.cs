using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyProjectile"))
        {
            /*
            if (other.GetComponent<Projectile>.blockable)
            {
                Destroy(other.gameObject);
            }
            */
        }
    }
}
