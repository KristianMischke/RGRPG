using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Controls the terrain tile game objects and updates their display
    /// </summary>
    public class TerrainTileController : MonoBehaviour
    {

        // Scene Object References

        public SpriteRenderer spriteRenderer; //actual tile
        public SpriteRenderer borderRenderer; //traversibley/n
        public SpriteRenderer spawnRenderer; //spawn square
        public SpriteRenderer transitionRenderer; //transition check
        public SpriteRenderer overlayRenderer;
        public SpriteRenderer propRenderer;
        public SpriteRenderer entityRenderer;
        // Prefabs

        // Data
        public Vector2Int tilePosition;
        public WorldScene sceneReference;

        private string editSpawnID;
        private string editTransitionScene;
        private string editTransitionSpawnID;


        private TerrainTile prevTileReference; // used for checking if the tile changed and needs to undergo a visual update

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {            
            if (sceneReference != null)
            {
                TerrainTile sceneTile = sceneReference.GetTileAtIndices(tilePosition);
                if (sceneTile != null)
                {
                    if (prevTileReference == null || !prevTileReference.Equals(sceneTile))
                    {
                        // the last referece we got is not equal to the current one
                        prevTileReference = sceneTile;
                        SetSprite();
                    }
                }
            }

            transform.localPosition = new Vector3(tilePosition.x, tilePosition.y);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void OnGUI()
        {
            if (MapEditorController.instance != null && MapEditorController.EditTileOverlay == tilePosition)
            {
                Vector3 viewportPos = Camera.main.WorldToScreenPoint(transform.position);
                viewportPos.y = Screen.height - viewportPos.y;

                TerrainTile t = sceneReference.GetTileAtIndices(tilePosition);
                if (editSpawnID == null)
                {
                    editSpawnID = t.SpawnID;
                }
                if (editTransitionScene == null)
                {
                    editTransitionScene = t.TransitionScene;
                }
                if (editTransitionSpawnID == null)
                {
                    editTransitionSpawnID = t.TransitionSpawnID;
                }

                GUIStyle bigFontLabel = new GUIStyle(GUI.skin.GetStyle("Label"));
                bigFontLabel.fontSize = 24;

                GUIStyle bigFontField = new GUIStyle(GUI.skin.GetStyle("TextField"));
                bigFontField.fontSize = 24;

                GUIStyle bigFontButton = new GUIStyle(GUI.skin.GetStyle("Button"));
                bigFontField.fontSize = 24;

                Vector3 size = new Vector3(300, 300);


                GUILayout.BeginArea(new Rect(viewportPos - size * 0.5f, size));


                if (MapEditorController.PaintSpawn)
                {
                    GUI.color = Color.red;
                    GUILayout.Label("my SpawnID: ", bigFontLabel);
                    GUI.color = Color.white;
                    editSpawnID = GUILayout.TextField(editSpawnID, bigFontField);
                }
                else if (MapEditorController.PaintTransition)
                {
                    GUI.color = Color.red;
                    GUILayout.Label("Transition to Scene: ", bigFontLabel);
                    GUI.color = Color.white;
                    editTransitionScene = GUILayout.TextField(editTransitionScene, bigFontField);
                    GUI.color = Color.red;
                    GUILayout.Label("Go to tile with SpawnID: ", bigFontLabel);
                    GUI.color = Color.white;
                    editTransitionSpawnID = GUILayout.TextField(editTransitionSpawnID, bigFontField);
                }

                if (GUILayout.Button("SAVE", bigFontButton))
                {
                    sceneReference.SetTile(tilePosition, new TerrainTile(t.Type, t.Traversable, t.Position, t.SubType, t.OverlayType, t.OverlaySubType, t.PropType, t.EntityType, t.Elevation, t.ElevationRamp, editSpawnID, editTransitionScene, editTransitionSpawnID));
                    MapEditorController.DoneTileOverlay();
                }
                if (GUILayout.Button("CANCEL", bigFontButton))
                {
                    MapEditorController.DoneTileOverlay();
                }
                GUILayout.EndArea();
            }
            else
            {
                editSpawnID = null;
                editTransitionScene = null;
                editTransitionSpawnID = null;
            }
        }

        /// <summary>
        ///     Initializes the sprite associated for this tile. TODO: also initialize walls when we support buildings
        /// </summary>
        public void SetSprite()
        {
            spriteRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.Type, prevTileReference.SubType);

            overlayRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.OverlayType, prevTileReference.OverlaySubType);
            overlayRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.OverlayType) && !prevTileReference.OverlayType.Contains("NONE"));

            propRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.PropType);

            entityRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_WORLD, prevTileReference.EntityType);
            propRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.PropType) && !prevTileReference.PropType.Contains("NONE"));

            // map editor only
            borderRenderer.gameObject.SetActive(!prevTileReference.Traversable && MapEditorController.instance != null);
            spawnRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.SpawnID) && MapEditorController.instance != null);
            transitionRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.TransitionScene) && !string.IsNullOrEmpty(prevTileReference.TransitionSpawnID) && MapEditorController.instance != null);
            entityRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.EntityType) && !prevTileReference.EntityType.Contains("NONE") && MapEditorController.instance != null);
        }
    }
}