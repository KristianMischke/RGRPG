using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Super simple script to make objects point to face the camera
/// </summary>
public class PointAtCamera : MonoBehaviour {
        
	void LateUpdate () {

        transform.rotation = Camera.main.transform.rotation;

	}
}
