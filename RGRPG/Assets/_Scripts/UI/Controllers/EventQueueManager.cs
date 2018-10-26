using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     (depricated) old event queue, can probably remove soon (new system is <see cref="Marquee"/>
    /// </summary>
    public class EventQueueManager : MonoBehaviour
    {
        public static EventQueueManager instance;

        // Prefabs
        public GameObject textTimerObject;

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
            EventTextTimer textTimer = textTimerObject.GetComponent<EventTextTimer>();
            textTimer.InitTimer(text, 4, 0);
            textTimer.ResetTimer();
        }
    }
}