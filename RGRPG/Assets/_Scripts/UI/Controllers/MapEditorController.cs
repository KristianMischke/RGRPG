#pragma warning disable 0219 // Variable assigned but not used

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RGRPG.Core;
using UnityEngine.SceneManagement;

namespace RGRPG.Controllers
{

    public class MapEditorController : MonoBehaviour
    {

        public static MapEditorController instance;

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject canvasObject;
  

        

        // Prefabs

        public GameObject worldSceneView;
        // Data

		int width;
		int height;

		WorldScene currentScene;

        // Use this for initialization
        void Start()
        {
            if (instance == null)
                instance = this;

            if (worldObjectContainer == null)
            {
                worldObjectContainer = GameObject.Find("WorldObjects");
            }

            
            if (canvasObject == null)
            {
                canvasObject = FindObjectOfType<Canvas>().gameObject;
            }
            currentScene = new WorldScene(30,30);

            // set up the scene controller
            GameObject worldSceneObject = Instantiate(worldSceneView);
            worldSceneObject.transform.SetParent(worldObjectContainer.transform);

            SceneController worldSceneController = worldSceneObject.GetComponent<SceneController>();
			worldSceneController.scene = currentScene;
            worldSceneController.ResetScene();
          
        }

        // Update is called once per frame
        void Update()
        {

            worldObjectContainer.SetActive(true);
        }

		public void setWidth(string stringWidth){
			int.TryParse(stringWidth, out width);		}

		public void setHeight(string stringHeight){
			int.TryParse(stringHeight, out height);
			

		}
	}

	

}