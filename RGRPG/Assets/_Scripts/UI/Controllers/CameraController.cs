using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Controllers
{
    public class CameraController : MonoBehaviour
    {

        public GameObject followObject;
        
        void LateUpdate()
        {
            if (followObject == null)
                return;

            if (GameController.instance.IsInCombat())
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);
                Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0, 0, -45);
                Camera.main.transform.localRotation = Quaternion.Euler(-60, 0, 0);
                Camera.main.transform.localPosition = new Vector3(0, -18, 0);
            }

            Vector3 newPosition = followObject.transform.position;
            newPosition.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 5);
        }
    }
}