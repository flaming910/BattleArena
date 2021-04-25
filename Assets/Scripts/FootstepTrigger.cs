using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepTrigger : MonoBehaviour
{
    private PlayerControl playerControl;
    private void Start()
    {
        playerControl = GetComponentInParent<PlayerControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            playerControl.FootstepSound();
        }
    }
}
