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

        private class ButtonActionData
        {
            public string type;
            public int subType;
            public bool random;

            public UnityAction action;

            public ButtonActionData(string t, int i, bool random = false)
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

        readonly string[] PAINT_TYPE_OPTIONS = new string[]
        {
            "TERRAIN",
            "TERRAIN_OVERLAYS",
            "TERRAIN_PROPS",
            "ENTITIES"
        };

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject canvasObject;
        public GameObject terrainTileButtonContainer;
        public GameObject terrainOverlayButtonContainer;
        public GameObject terrainPropButtonContainer;
        public GameObject entityButtonContainer;

        public InputField widthInput;
        public InputField heightInput;
        public TMP_Text filePathLabel;
        public Button saveButton;
        public Button loadButton;

        public Slider radiusSlider;
        public TMP_Text sliderText;

        public Toggle indoorsToggle;
        public Toggle traversableToggle;
        public Toggle overlayToggle;
        public Toggle spawnToggle;
        public Toggle transitionToggle;

        public Toggle bucketToggle;

        public Toggle ignoreSub;

        public Dropdown paintKindDropdown;

        // Prefabs
        public GameObject worldSceneView;
        public GameObject terrainTileButton;
        public GameObject dividerPrefab;

        // Data
        SceneController worldSceneController;

        Dictionary<string, int> terrainToSubCount = new Dictionary<string, int>();

        //the "cursor" position in map coordinates
        private Vector2Int thisPosition = NEGATIVE_ONE;
        private Vector2Int lastPosition = NEGATIVE_ONE;

        //paint features
        static string paintType = "TERRAIN_NONE";
        static int paintSubType = 0;
        static bool isPaintingOverlay = false;
        static bool paintTraversable = true;
        static bool paintSpawn = false;
        static bool paintTransition = false;
        static bool randomPaint = false;
        static int radius;

        static bool paintFill = false;

        static bool paintIgnore = false;

        public WorldScene currentScene;
        GameInfos infos;


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

            infos = new GameInfos();

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

            // setup dropdown options
            paintKindDropdown.options = new List<Dropdown.OptionData>();
            foreach(string option in PAINT_TYPE_OPTIONS)
                paintKindDropdown.options.Add(new Dropdown.OptionData(option));
            
            paintKindDropdown.onValueChanged.AddListener(SetButtonContainer);
            paintKindDropdown.value = 0;
            SetButtonContainer(0);

            CreatePanelButtons(SpriteManager.AssetType.TERRAIN, terrainTileButtonContainer, infos.GetAll<InfoTerrain>(false));
            CreatePanelButtons(SpriteManager.AssetType.TERRAIN, terrainOverlayButtonContainer, infos.GetAll<InfoTerrainOverlay>(false));
            CreatePanelButtons(SpriteManager.AssetType.TERRAIN, terrainPropButtonContainer, infos.GetAll<InfoTerrainProp>(false));
            CreatePanelButtons(SpriteManager.AssetType.CHARACTER_WORLD, entityButtonContainer, infos.GetAll<InfoCharacter>(false).FindAll(x => x.IsEnemy || x.ZType == "CHARACTER_NONE"));

            saveButton.onClick.AddListener(Save);
            loadButton.onClick.AddListener(Load);
        }


        void CreatePanelButtons<T>(SpriteManager.AssetType assetType, GameObject container, List<T> infosList) where T : InfoBase
        {
            // add buttons for the different tile types and sub types
            foreach (T thisInfo in infosList)
            {
                int numButtonsAdded = 0;

                string zType = thisInfo.ZType;
                Sprite[] tileSubTypes = SpriteManager.getSpriteSheet(assetType, zType);
                for (int i = 0; i < tileSubTypes.Length; i++)
                {
                    CreateButton(container, tileSubTypes[i], zType + " " + i, zType, i);
                    numButtonsAdded++;
                    
                    if (!terrainToSubCount.ContainsKey(zType))
                    {
                        terrainToSubCount.Add(zType, tileSubTypes.Length);
                    }
                }

                bool doRandomButton = false;
                if (thisInfo as InfoTerrain != null)
                    doRandomButton = (thisInfo as InfoTerrain).HasMultiple;
                if (thisInfo as InfoTerrainOverlay != null)
                    doRandomButton = (thisInfo as InfoTerrainOverlay).HasMultiple;
                if (thisInfo as InfoTerrainProp != null)
                    doRandomButton = (thisInfo as InfoTerrainProp).HasMultiple;

                if (doRandomButton)
                {
                    CreateButton(container, tileSubTypes[0], zType + " RANDOM", zType, 0, true); //TODO: maybe figure out how to cycle through the images to show that it will be random
                    numButtonsAdded++;
                }

                if (typeof(T) == typeof(InfoTerrain) || typeof(T) == typeof(InfoTerrain))
                {
                    if (numButtonsAdded % 3 != 0)
                    {
                        for (int i = 0; i < 3 - (numButtonsAdded % 3); i++)
                        {
                            Instantiate(dividerPrefab, container.transform);
                        }
                    }
                }
            }
        }

        void CreateButton(GameObject container, Sprite sprite, string buttonString, string zType, int subType, bool random = false)
        {
            GameObject ButtonObj = Instantiate(terrainTileButton, container.transform);
            Button button = ButtonObj.GetComponentInChildren<Button>();
            button.image.sprite = sprite;

            Text buttonText = ButtonObj.GetComponentInChildren<Text>();
            buttonText.text = buttonString;

            ButtonActionData buttonAction = new ButtonActionData(zType, subType, random);
            button.onClick.AddListener(buttonAction.action);
        }

        void Update()
        {
            lastPosition = thisPosition;
            worldObjectContainer.SetActive(true);
            radius = (int)radiusSlider.value;
            sliderText.text = ""+radius;
            paintTraversable = traversableToggle.isOn;
            isPaintingOverlay = overlayToggle.isOn;
            currentScene.SetIndoors(indoorsToggle.isOn);
            paintFill = bucketToggle.isOn;
            paintIgnore = ignoreSub.isOn;
            ignoreSub.enabled = bucketToggle.isOn;


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

                        if (paintFill){
                            FillBucket(thisPosition);
                        }
                        else {

                        
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
        private void FillBucket(Vector2Int tilePosition){
            TerrainTile startingTile = currentScene.GetTileAtIndices(tilePosition);
            List<Vector2Int> tilesVisited = new List<Vector2Int>();
            List<Vector2Int> tilesVisiting = new List<Vector2Int>();
            tilesVisiting.Add(tilePosition);
            while(tilesVisiting.Count != 0){
                Vector2Int currentPosition = tilesVisiting[0];
                TerrainTile t = currentScene.GetTileAtIndices(currentPosition);
                Debug.Log(currentPosition);
                if (t.EqualsMapEditor(startingTile, paintIgnore) && !tilesVisited.Contains(currentPosition)){
                    PaintSingleTile(currentPosition);
                    tilesVisiting.Add(new Vector2Int (currentPosition.x + 1, currentPosition.y));
                    tilesVisiting.Add(new Vector2Int (currentPosition.x - 1, currentPosition.y));
                    tilesVisiting.Add(new Vector2Int (currentPosition.x, currentPosition.y + 1));
                    tilesVisiting.Add(new Vector2Int (currentPosition.x, currentPosition.y - 1));

                }

                tilesVisited.Add(tilesVisiting[0]);
                tilesVisiting.RemoveAt(0);
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

                switch (paintKindDropdown.value)
                {
                    case 0: // TERRAIN
                        if (paintType.Contains("NONE"))
                        {
                            // if the paint type is NONE, then just paint features not related to type (i.e. traversable, elevetion [TODO] etc)
                            currentScene.SetTile(tilePosition, new TerrainTile(t.Type, paintTraversable, t.Position, t.SubType, t.OverlayType, t.OverlaySubType, t.PropType, t.EntityType, t.Elevation, t.ElevationRamp, t.SpawnID));
                        }
                        else
                        {
                            currentScene.SetTile(tilePosition, new TerrainTile(paintType, paintTraversable, t.Position, paintSubType, t.OverlayType, t.OverlaySubType, t.PropType, t.EntityType, t.Elevation, t.ElevationRamp, t.SpawnID));
                        }
                        break;
                    case 1: // OVERLAYs
                        currentScene.SetTile(tilePosition, new TerrainTile(t.Type, paintTraversable, t.Position, t.SubType, paintType, paintSubType, t.PropType, t.EntityType, t.Elevation, t.ElevationRamp, t.SpawnID));
                        break;
                    case 2: // PROPS
                        currentScene.SetTile(tilePosition, new TerrainTile(t.Type, paintTraversable, t.Position, t.SubType, t.OverlayType, t.OverlaySubType, paintType, t.EntityType, t.Elevation, t.ElevationRamp, t.SpawnID));
                        break;
                    case 3: // ENTITIES
                        currentScene.SetTile(tilePosition, new TerrainTile(t.Type, true, t.Position, t.SubType, t.OverlayType, t.OverlaySubType, t.PropType, paintType, t.Elevation, t.ElevationRamp, t.SpawnID));
                        break;
                }
            }
        }

        public void SetWidth(string stringWidth)
        {
            int width = currentScene.Width;
            int.TryParse(stringWidth, out width);

            currentScene.AdjustDimensions(width, currentScene.Height, paintKindDropdown.value == 0 ? paintType : "TERRAIN_NONE");
            Debug.Log(currentScene.Width);
            worldSceneController.ResetScene();
        }

		public void SetHeight(string stringHeight)
        {
            int height = currentScene.Height;
			int.TryParse(stringHeight, out height);

            currentScene.AdjustDimensions(currentScene.Width, height, paintKindDropdown.value == 0 ? paintType : "TERRAIN_NONE");
            Debug.Log(currentScene.Height);
            worldSceneController.ResetScene();
        }

        public static void SetPaintType(string type, int subType = 0, bool random = false)
        {
            paintType = type;
            paintSubType = subType;
            randomPaint = random;
        }

        public void SetButtonContainer(int choice)
        {
            paintType = "NONE";
            paintSubType = 0;

            terrainTileButtonContainer.SetActive(false);
            terrainOverlayButtonContainer.SetActive(false);
            terrainPropButtonContainer.SetActive(false);
            entityButtonContainer.SetActive(false);
            switch (choice)
            {
                case 0:
                    terrainTileButtonContainer.SetActive(true);
                    break;
                case 1:
                    terrainOverlayButtonContainer.SetActive(true);
                    break;
                case 2:
                    terrainPropButtonContainer.SetActive(true);
                    break;
                case 3:
                    entityButtonContainer.SetActive(true);
                    break;
            }
        }


        public void SetPaintSpawn(bool isOn)
        {
            paintSpawn = isOn;
            transitionToggle.isOn = paintTransition = isOn ? false : paintTransition;
        }


        public void SetPaintTransition(bool isOn)
        {
            paintTransition = isOn;
            spawnToggle.isOn = paintSpawn = isOn ? false : paintSpawn;
        }


        public void Save()
        {
            EventSystem.current.SetSelectedGameObject(null);
            string extensions = "xml";

            string path = FileBrowser.SaveFile("Save Map File", "", "___", extensions);

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

            if (currentScene == null)
            {
                currentScene = new WorldScene(infos.Get<InfoScene>("SCENE_NONE"));
            }
            currentScene.Load(filepath);

            if(worldSceneController != null)
                worldSceneController.ResetScene();
        }
    }

	

}