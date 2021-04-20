using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    [SerializeField] private GameObject player;

    public GameObject Player
    {
        get
        {
            if (player) return player;
            return null;
        }
    }

    public Vector3 PlayerPosition
    {
        get
        {
            if (player) return player.transform.position;
            return Vector3.zero;
        }
    }
}
