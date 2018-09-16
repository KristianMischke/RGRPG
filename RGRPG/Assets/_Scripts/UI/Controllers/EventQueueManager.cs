using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Controllers
{
    public class EventQueueManager : MonoBehaviour
    {
        public static EventQueueManager instance;

        // Prefabs
        public GameObject eventTextTimer;

        // Use this for initialization
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddEventMessage(string text)
        {
            GameObject textTimerObject = Instantiate(eventTextTimer);
            textTimerObject.transform.SetParent(transform);

            EventTextTimer textTimer = textTimerObject.GetComponent<EventTextTimer>();
            textTimer.InitTimer(text, 4, 0);
            textTimer.ResetTimer();
        }
    }
}