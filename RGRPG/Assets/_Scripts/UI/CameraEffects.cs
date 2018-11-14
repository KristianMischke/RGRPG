using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;
using RGRPG.Controllers;

public class CameraEffects : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


        if ((GameController.instance != null && GameController.instance.IsIndoors) || (MapEditorController.instance != null && MapEditorController.instance.currentScene.IsIndoors))
        {
            Camera.main.backgroundColor = Color.black;
        }
        else
        {
            Camera.main.backgroundColor = new Color(102f/255f, 165/255f, 202/255f);
        }


	}
}
