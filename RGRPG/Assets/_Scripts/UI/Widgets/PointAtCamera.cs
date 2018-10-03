using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAtCamera : MonoBehaviour {
        
	void LateUpdate () {

        transform.rotation = Camera.main.transform.rotation;

	}
}
