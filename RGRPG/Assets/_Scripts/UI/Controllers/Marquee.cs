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
        const float TIME_PER_CHAR = 0.1f;
        const float TIME_AFTER_MESSAGE = 1f;

        public TextMeshProUGUI textObject;
        public static Marquee instance;

        private string message;
        int charsDisplayed = 0;
        //public float scrollSpeed = 50;
        float timer = TIME_AFTER_MESSAGE;
        bool isStarted = false;
        bool finishedText = true;


        //Rect messageRect;

        public void StartTimer()
        {
            isStarted = true;
        }

        public void ResetTimer(string text, bool pause = true)
        {
            this.message = text;
            timer = 0;
            isStarted = !pause;
            isStarted = false;
            finishedText = false;
            charsDisplayed = 0;
        }

        public bool IsFinished()
        {
            return finishedText && timer >= TIME_AFTER_MESSAGE;
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
                if (timer >= TIME_PER_CHAR && !finishedText)
                {
                    charsDisplayed++;
                    timer = 0;
                    if (charsDisplayed == message.Length)
                        finishedText = true;
                }
            }
            if (message != null)
                textObject.SetText(message.Substring(0, charsDisplayed));
        }


        //void OnGUI(string text)
        //{
        //    this.message = text;
        //    // Set up message's rect if we haven't already.
        //    if (messageRect.width == 0)
        //    {
        //        var dimensions = GUI.skin.label.CalcSize(new GUIContent(message));

        //        // Start message past the left side of the screen.
        //        messageRect.x = -dimensions.x;
        //        messageRect.width = dimensions.x;
        //        messageRect.height = dimensions.y;
        //    }



        //    //messageRect.x += Time.deltaTime * scrollSpeed;


        //    // If message has moved past the right side, move it back to the left.
        //    if (messageRect.x > Screen.width)
        //    {
        //        messageRect.x = -messageRect.width;
        //    }

        //    //GUI.Label(messageRect, message);
        //}
    }
}
