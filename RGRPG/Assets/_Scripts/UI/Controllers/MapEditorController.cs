using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using RGRPG.Core;
using Crosstales.FB;    

namespace RGRPG.Controllers
{

    public class MapEditorController : MonoBehaviour
    {

        private class TerrainButtonActionData
        {
            public TerrainType type;
            public int subType;
            public bool random;

            public UnityAction action;

            public TerrainButtonActionData(TerrainType t, int i, bool random = false)
            {
                type = t;
                subType = i;
                this.random = random;
    
                action = () => {
                    MapEditorController.SetPaintType(type, subType, random);
                };               
            }
        }

        public static readonly Vector2Int NEGATIVE_ONE = Vector2Int.one * -1;
        public static MapEditorController instance;

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject canvasObject;
        public GameObject terrainTileButtonContainer;

        public InputField widthInput;
        public InputField heightInput;
        public TMP_Text filePathLabel;
        public Button saveButton;
        public Button loadButton;

        public Slider radiusSlider;
        public TMP_Text sliderText;

        public Toggle traversableToggle;
        public Toggle overlayToggle;
        public Toggle spawnToggle;

        // Prefabs
        public GameObject worldSceneView;
        public GameObject terrainTileButton;

        // Data
        SceneController worldSceneController;

        Dictionary<TerrainType, int> terrainToSubCount = new Dictionary<TerrainType, int>();

        //the "cursor" position in map coordinates
        private Vector2Int thisPosition = NEGATIVE_ONE;
        private Vector2Int lastPosition = NEGATIVE_ONE;

        //paint features
        static TerrainType paintType = TerrainType.NONE;
        static int paintSubType = 0;
        static bool isPaintingOverlay = false;
        static bool paintTraversable = true;
        static bool paintSpawn = false;
        static bool randomPaint = false;
        static int radius;

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

            LoadScene(Application.dataPath + @"\Resources\Data\Scenes\worldTest.xml"); //default scene for now

            // set up the scene controller
            GameObject worldSceneObject = Instantiate(worldSceneView);
            worldSceneObject.transform.SetParent(worldObjectContainer.transform);

            worldSceneController = worldSceneObject.GetComponent<SceneController>();
			worldSceneController.scene = currentScene;
            worldSceneController.ResetScene();

            // setup UI listeners
            widthInput.text = currentScene.Width.ToString();
            heightInput.text = currentScene.Height.ToString();
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
                    terrainButton.onClick.AddListener(buttonAction.action);

                    if (!terrainToSubCount.ContainsKey(t))
                    {
                        terrainToSubCount.Add(t, tileSubTypes.Length);
                    }
                }
                if (tileSubTypes.Length > 1)
                {
                    GameObject terrainButtonObj = Instantiate(terrainTileButton, terrainTileButtonContainer.transform);
                    Button terrainButton = terrainButtonObj.GetComponentInChildren<Button>();
                    terrainButton.image.sprite = tileSubTypes[0]; //TODO: maybe figure out how to cycle through the images to show that it will be random

                    Text buttonText = terrainButtonObj.GetComponentInChildren<Text>();
                    buttonText.text = tileTypeName + " RANDOM";

                    TerrainButtonActionData buttonAction = new TerrainButtonActionData(t, 0, true);
                    terrainButton.onClick.AddListener(buttonAction.action);
                }
            }

            saveButton.onClick.AddListener(Save);
            loadButton.onClick.AddListener(Load);
        }

        void Update()
        {
            lastPosition = thisPosition;
            worldObjectContainer.SetActive(true);
            radius = (int)radiusSlider.value;
            sliderText.text = ""+radius;
            paintTraversable = traversableToggle.isOn;
            isPaintingOverlay = overlayToggle.isOn;
            paintSpawn = spawnToggle.isOn;

            // need to look into this: https://www.youtube.com/watch?v=QL6LOX5or84
            // mouse pressed and not clicking on UI element
            thisPosition = NEGATIVE_ONE;
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
                        thisPosition = t.Position;
                        if (thisPosition != NEGATIVE_ONE && lastPosition != NEGATIVE_ONE && Mathf.FloorToInt(Vector2Int.Distance(lastPosition, thisPosition)) > 1){
                            //drag
                            Vector2Int i = lastPosition;
                            while ( i != thisPosition){
                                TerrainTile z = currentScene.GetTileAtIndices(i); //this is the tile that position
                                DoPaint(z);
                                if (i.x < thisPosition.x){
                                    i.x ++;
                                }
                                else if (i.x > thisPosition.x){
                                    i.x --;
                                }
                                if (i.y < thisPosition.y){
                                    i.y ++;
                                }
                                else if (i.y > thisPosition.y){
                                    i.y --;
                                }
                            }
                        }
                        else if (thisPosition !=  NEGATIVE_ONE){
                            //click
                            DoPaint(t);
                        }
                    
                      
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
            //TODO: implement paint size here (maybe paint shape, if we want that
            for (int x = t.Position.x - radius; x <= t.Position.x + radius; x++){
                for (int y = t.Position.y - radius; y <= t.Position.y + radius; y++){
                    Vector2Int tilePosition = new Vector2Int(x, y);
                    //if (Vector2Int.Distance(tilePosition, t.Position) < radius){
                    PaintSingleTile(tilePosition);
                        //currentScene.SetTile(tilePosition, new TerrainTile(paintType, t.Traversable, t.Position, paintSubType, t.Elevation, t.ElevationRamp));
                    //}
                }
            }
            
        }

        private void PaintSingleTile(Vector2Int tilePosition)
        {
            if (randomPaint)
            {
                paintSubType = Random.Range(0, terrainToSubCount[paintType]);
            }
            TerrainTile t = currentScene.GetTileAtIndices(tilePosition);
            if (t != null)
            {
                if (paintType == TerrainType.NONE)
                {
                    // if the paint type is NONE, then just paint features not related to type (i.e. traversable, elevetion [TODO] etc)
                    currentScene.SetTile(tilePosition, new TerrainTile(t.Type, paintTraversable, t.Position, t.SubType,t.OverlayType ,t.Elevation, t.ElevationRamp, paintSpawn));
                }
                else
                {
                    if (isPaintingOverlay)
                    {
                        currentScene.SetTile(tilePosition, new TerrainTile(t.Type, paintTraversable, t.Position, paintSubType, paintType, t.Elevation, t.ElevationRamp, t.IsSpawn));
                    }
                    else
                    {
                        currentScene.SetTile(tilePosition, new TerrainTile(paintType, paintTraversable, t.Position, paintSubType, t.OverlayType, t.Elevation, t.ElevationRamp, t.IsSpawn));
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

        public static void SetPaintType(TerrainType type, int subType = 0, bool random = false)
        {
            paintType = type;
            paintSubType = subType;
            randomPaint = random;
        }

        public void Save()
        {
            EventSystem.current.SetSelectedGameObject(null);
            string extensions = "xml";

            string path = FileBrowser.SaveFile("Save Map File", "", "MyMap", extensions); // TODO: copy file name of map here

            if(!string.IsNullOrEmpty(path))
                filePathLabel.text = path;

            currentScene.Save(path);
        }

        public void Load()
        {
            EventSystem.current.SetSelectedGameObject(null);
            string extensions = "xml";

            string path = FileBrowser.OpenSingleFile("Open Map file", "", extensions);

            if (!string.IsNullOrEmpty(path))
                filePathLabel.text = path;

            LoadScene(path);
        }

        public void LoadScene(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return;

            filePathLabel.text = filepath;

            if(currentScene == null)
                currentScene = new WorldScene(SceneType.NONE, "");
            currentScene.Load(filepath);

            if(worldSceneController != null)
                worldSceneController.ResetScene();
        }
    }

	

}