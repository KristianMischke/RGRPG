using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using RGRPG.Core;

namespace RGRPG.Controllers
{

    public class MapEditorController : MonoBehaviour
    {

        private class TerrainButtonActionData
        {
            public TerrainType type;
            public int subType;
            public UnityAction action;

            public bool random;
            public int minI, maxI;

            public TerrainButtonActionData(TerrainType t, int i, bool random = false, int minI = 0, int maxI = 0)
            {
                type = t;
                subType = i;

                if (random && maxI > minI)
                {
                    this.random = random;
                    this.minI = minI;
                    this.maxI = maxI;
                    action = () => { MapEditorController.SetPaintType(type, subType, random, minI, maxI); };
                }
                else
                {
                    action = () => { MapEditorController.SetPaintType(type, subType); };
                }
            }
        }

        public static MapEditorController instance;

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject canvasObject;
        public GameObject terrainTileButtonContainer;

        public InputField widthInput;
        public InputField heightInput;
        public TMP_InputField fileInput;
        public Button saveButton;

        // Prefabs
        public GameObject worldSceneView;
        public GameObject terrainTileButton;

        // Data
        SceneController worldSceneController;

        static TerrainType paintType = TerrainType.NONE;
        static int paintSubType = 0;

		WorldScene currentScene;


        // see https://answers.unity.com/questions/1079066/how-can-i-prevent-my-raycast-from-passing-through.html
#if UNITY_EDITOR
        private int fingerID = -1; 
# else
        private int fingerID = 0;
#endif

        void Start()
        {
            if (instance == null)
                instance = this;

            if (worldObjectContainer == null)
            {
                worldObjectContainer = GameObject.Find("WorldObjects");
            }

            TextAsset terrainXMLText = Resources.Load<TextAsset>(@"Data\TerrainAssets");
            SpriteManager.LoadSpriteAssetsXml(terrainXMLText.text, SpriteManager.AssetType.TERRAIN);

            if (canvasObject == null)
            {
                canvasObject = FindObjectOfType<Canvas>().gameObject;
            }

            fileInput.text = Application.dataPath + @"\Resources\Data\worldTest.xml";

            currentScene = new WorldScene(30,30);
            currentScene.Load(fileInput.text);

            // set up the scene controller
            GameObject worldSceneObject = Instantiate(worldSceneView);
            worldSceneObject.transform.SetParent(worldObjectContainer.transform);

            worldSceneController = worldSceneObject.GetComponent<SceneController>();
			worldSceneController.scene = currentScene;
            worldSceneController.ResetScene();

            // setup UI listeners
            widthInput.onEndEdit.AddListener(SetWidth);
            heightInput.onEndEdit.AddListener(SetHeight);

            // add buttons for the different tile types and sub types
            foreach (TerrainType t in System.Enum.GetValues(typeof(TerrainType)))
            {
                string tileTypeName = System.Enum.GetName(typeof(TerrainType), t);
                Sprite[] tileSubTypes = SpriteManager.getSpriteSheet(SpriteManager.AssetType.TERRAIN, tileTypeName);
                for (int i = 0; i < tileSubTypes.Length; i++)
                {
                    GameObject terrainButtonObj = Instantiate(terrainTileButton, terrainTileButtonContainer.transform);
                    Button terrainButton = terrainButtonObj.GetComponentInChildren<Button>();
                    terrainButton.image.sprite = tileSubTypes[i];

                    Text buttonText = terrainButtonObj.GetComponentInChildren<Text>();
                    buttonText.text = tileTypeName + " " + i;

                    TerrainButtonActionData buttonAction = new TerrainButtonActionData(t, i);
                    buttonAction.type = t;
                    buttonAction.subType = i;
                    terrainButton.onClick.AddListener(buttonAction.action);
                }
                if (tileSubTypes.Length > 1)
                {
                    GameObject terrainButtonObj = Instantiate(terrainTileButton, terrainTileButtonContainer.transform);
                    Button terrainButton = terrainButtonObj.GetComponentInChildren<Button>();
                    terrainButton.image.sprite = tileSubTypes[0]; //TODO: maybe figure out how to cycle through the images to show that it will be random

                    Text buttonText = terrainButtonObj.GetComponentInChildren<Text>();
                    buttonText.text = tileTypeName + " RANDOM";

                    TerrainButtonActionData buttonAction = new TerrainButtonActionData(t, 0, true, 0, tileSubTypes.Length);
                    buttonAction.type = t;
                    buttonAction.subType = 0;
                    buttonAction.random = true;
                    buttonAction.minI = 0;
                    buttonAction.maxI = tileSubTypes.Length;
                    terrainButton.onClick.AddListener(buttonAction.action);
                }
            }

            saveButton.onClick.AddListener(Save);
        }

        void Update()
        {
            worldObjectContainer.SetActive(true);


            // need to look into this: https://www.youtube.com/watch?v=QL6LOX5or84
            // mouse pressed and not clicking on UI element
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject(fingerID))
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
                        TerrainTile t = currentScene.GetTileAtIndices(tileController.tilePosition); //this is the tile that the user clicked
                        DoPaint(t);
                    }
                }
            }

        }

        /// <summary>
        ///     Applies the correct paint operation to the scene
        /// </summary>
        /// <param name="t">The targeted tile</param>
        private void DoPaint(TerrainTile t)
        {
            //TODO: implement paint size here (maybe paint shape, if we want that)
            currentScene.SetTile(t.Position, new TerrainTile(paintType, t.Traversable, t.Position, paintSubType, t.Elevation, t.ElevationRamp));
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

        public static void SetPaintType(TerrainType type, int subType = 0, bool random = false, int minI = 0, int maxI = 0)
        {
            paintType = type;
            paintSubType = subType;

            if (random)
            {
                paintSubType = Random.Range(minI, maxI);
            }
        }

        public void Save()
        {
            currentScene.Save(fileInput.text);
        }
	}

	

}