using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace RGRPG.Controllers
{
    /// <summary>
    ///     Controls the ticking text at the top of the screen TODO: only show during combat?!
    /// </summary>
    public class Marquee : MonoBehaviour
    {
        public static Marquee instance;

        const float TIME_PER_CHAR = 0.03f;
        const float TIME_AFTER_MESSAGE = 1f;

        public TextMeshProUGUI textObject;

        private Queue<string> multiMessage = new Queue<string>();
        int charsDisplayed = 0;
        float timer = TIME_AFTER_MESSAGE*2;
        bool isStarted = false;
        bool finishedSingle = true;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void StartTimer()
        {
            isStarted = true;
        }

        public void ResetTimer(string message = null, bool pause = true)
        {
            Queue<string> multiMessage = new Queue<string>();
            if (message != null)
            {
                multiMessage.Enqueue(message);
            }
            ResetTimer(multiMessage, pause);
        }

        public void ResetTimer(Queue<string> multiMessage, bool pause = true)
        {
            this.multiMessage.Clear();
            this.multiMessage = multiMessage;
            timer = 0;
            isStarted = !pause;
            isStarted = false;
            finishedSingle = false;
            charsDisplayed = 0;
        }

        public void AddToMultiMessage(string message)
        {
            multiMessage.Enqueue(message);
        }

        public bool IsFinished()
        {
            return multiMessage.Count == 0 && timer >= TIME_AFTER_MESSAGE;
        }

        public void Clear()
        {
            textObject.text = "";
        }

        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (isStarted)
            {
                timer += Time.deltaTime;
                if (timer >= TIME_PER_CHAR && !finishedSingle && multiMessage.Count > 0)
                {
                    charsDisplayed++;
                    timer = 0;
                    if (multiMessage.Count > 0 && charsDisplayed == multiMessage.Peek().Length)
                    {
                        finishedSingle = true;
                    }
                }
            }
            if (multiMessage != null && multiMessage.Count > 0 && multiMessage.Peek() != null)
            {
                textObject.SetText(multiMessage.Peek().Substring(0, Mathf.Min(charsDisplayed, multiMessage.Peek().Length)));
            }

            if (finishedSingle && multiMessage.Count > 0 && timer > TIME_AFTER_MESSAGE)
            {
                multiMessage.Dequeue();
                finishedSingle = false;
                charsDisplayed = 0;
            }
        }
    }
}
