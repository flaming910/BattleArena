using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    public bool timerRunning;
    private float runTime;
    // Start is called before the first frame update
    void Start()
    {
        timerRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerRunning)
        {
            runTime += Time.deltaTime;
            var time = TimeSpan.FromSeconds(runTime);
            int minutes = time.Minutes;
            int seconds = time.Seconds;
            int milliseconds = time.Milliseconds;
            UIManager.Instance.UpdateRunTime(string.Format("{0:0}:{1:00}:{2:000}", minutes, seconds, milliseconds));
        }

    }

}
