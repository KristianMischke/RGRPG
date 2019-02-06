using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RGRPG.Controllers
{
    public class EventTextTimer : MonoBehaviour
    {
        TextMeshProUGUI textObject;

        float waitTime;
        float fadeTime;
        float timer;

        bool started = false;

        // Use this for initialization
        void Start()
        {
            if (textObject == null)
            {
                textObject = GetComponent<TextMeshProUGUI>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (started)
            {
                timer += Time.deltaTime;

                if (timer >= waitTime)
                {
                    textObject.color = Color.Lerp(textObject.color, Color.clear, Time.deltaTime / fadeTime);
                    if (timer >= waitTime + fadeTime)
                    {
                        EndTimer();
                    }
                }

            }
        }

        public void InitTimer(string text, float waitTime, float fadeTime)
        {
            if (textObject == null)
            {
                textObject = GetComponent<TextMeshProUGUI>();
            }

            textObject.text = text;
            this.waitTime = Mathf.Max(waitTime, 0);
            this.fadeTime = Mathf.Max(fadeTime, 0);
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
}