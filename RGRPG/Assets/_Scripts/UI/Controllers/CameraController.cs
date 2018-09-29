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


            Vector3 newPosition = followObject.transform.position;
            newPosition.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime*5);

        }
    }
}