using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Super simple script to make objects point to face the camera
/// </summary>
public class PointAtCamera : MonoBehaviour {

    private bool shouldPointAtCamera = true;
    public bool ShouldPointAtCamera { get { return shouldPointAtCamera; } set { shouldPointAtCamera = value; } }

	void LateUpdate () {
        if (shouldPointAtCamera)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
	}
}
