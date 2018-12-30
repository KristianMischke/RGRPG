using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTimer : MonoBehaviour
{
    public float waitTime;
    float timer;

    public bool started = false;

    // Use this for initialization
    void Start()
    {
        if (started)
            InitTimer(waitTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            timer += Time.deltaTime;

            if (timer >= waitTime)
            {
                EndTimer();
            }

        }
    }

    public void InitTimer(float waitTime)
    {
        this.waitTime = Mathf.Max(waitTime, 0);
    }

    public void ResetTimer()
    {
        started = true;
        timer = 0;
    }

    public void ResumeTimer()
    {
        started = true;
    }

    public void PauseTimer()
    {
        started = false;
    }

    public void EndTimer()
    {
        Destroy(gameObject);
    }
}
