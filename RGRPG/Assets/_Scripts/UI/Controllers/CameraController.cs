using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Controllers
{
    public class CameraController : MonoBehaviour
    {

        public GameObject followObject;


        // controling camera in map editor
        public bool freeControl = false;
        public float speed = 2.0f;
        
        void LateUpdate()
        {

            if (freeControl)
            {
                Vector3 direction = Vector3.zero;

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    direction += Vector3.up;
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    direction += Vector3.down;
                }

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    direction += Vector3.left;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    direction += Vector3.right;
                }

                direction.Normalize();

                Quaternion rotation = Quaternion.AngleAxis(-45, Vector3.forward);

                direction = rotation * direction;

                transform.position = transform.position + direction * speed * Time.deltaTime;
            }

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