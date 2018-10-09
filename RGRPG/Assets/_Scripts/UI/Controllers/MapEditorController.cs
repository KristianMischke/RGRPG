using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        public InputField widthInput;
        public InputField heightInput;

        // Prefabs
        public GameObject worldSceneView;

        // Data
        SceneController worldSceneController;

		WorldScene currentScene;

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

            worldSceneController = worldSceneObject.GetComponent<SceneController>();
			worldSceneController.scene = currentScene;
            worldSceneController.ResetScene();

            // setup UI listeners
            widthInput.onEndEdit.AddListener(SetWidth);
            heightInput.onEndEdit.AddListener(SetHeight);

        }

        void Update()
        {
            worldObjectContainer.SetActive(true);


            // mouse pressed
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // raycast hit
                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;

                    TerrainTileController tileController = objectHit.GetComponent<TerrainTileController>();
                    if (tileController != null) // has a tileController
                    {
                        TerrainTile t = currentScene.GetTileAtIndices(tileController.tilePosition);
                        currentScene.SetTile(t.Position, new TerrainTile(TerrainType.Water, t.Traversable, t.Position, t.Elevation, t.ElevationRamp)); //TODO: align to paint tool
                    }
                }
            }

        }

		public void SetWidth(string stringWidth)
        {
            int width = currentScene.Width;
            int.TryParse(stringWidth, out width);

            currentScene.AdjustDimensions(width, currentScene.Height);
            Debug.Log(currentScene.Width);
            worldSceneController.ResetScene();
        }

		public void SetHeight(string stringHeight)
        {
            int height = currentScene.Height;
			int.TryParse(stringHeight, out height);

            currentScene.AdjustDimensions(currentScene.Width, height);
            Debug.Log(currentScene.Height);
            worldSceneController.ResetScene();
        }
	}

	

}