using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Allows the camera to move freely (via arrow keys) or follow an object
    /// </summary>
    public class CameraController : MonoBehaviour
    {

        public GameObject followObject;


        // controling camera in map editor
        public bool freeControl = false;
        public float speed = 2.0f;
        
        // LateUpdate is called after Update
        void LateUpdate()
        {

            // control camera parent with arrow keys
            if (freeControl)
            {
                Vector3 direction = Vector3.zero;

                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                {
                    direction += Vector3.up;
                }
                else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                {
                    direction += Vector3.down;
                }

                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    direction += Vector3.left;
                }
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    direction += Vector3.right;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - 3f * Time.deltaTime, 1f, 18f);
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + 3f * Time.deltaTime, 1f, 18f);
                }

                direction.Normalize();

                Quaternion rotation = Quaternion.AngleAxis(-45, Vector3.forward);

                direction = rotation * direction;

                transform.position = transform.position + direction * speed * Time.deltaTime;
            }

            if (followObject == null)
                return;

            // flip camera angle based on world or combat mode
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

            // lerp to new position to follow the object smoothly
            Vector3 newPosition = followObject.transform.position;
            newPosition.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 5);
        }
    }
}