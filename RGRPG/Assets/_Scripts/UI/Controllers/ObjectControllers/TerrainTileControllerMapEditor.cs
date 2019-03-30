using UnityEngine;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    public class TerrainTileControllerMapEditor : TerrainTileController
    {
        public override int GetWallHeight { get { return 3; } }

        public SpriteRenderer borderRenderer; //traversibley/n
        public SpriteRenderer spawnRenderer; //spawn square
        public SpriteRenderer transitionRenderer; //transition check
        public SpriteRenderer entityRenderer;

        protected string editSpawnID;
        protected string editTransitionScene;
        protected string editTransitionSpawnID;

        override public void SetSprite()
        {
            base.SetSprite();

            // map editor only
            borderRenderer.gameObject.SetActive(!prevTileReference.Traversable && MapEditorController.instance != null);
            spawnRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.SpawnID) && MapEditorController.instance != null);
            transitionRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.TransitionScene) && !string.IsNullOrEmpty(prevTileReference.TransitionSpawnID) && MapEditorController.instance != null);
            entityRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_COMBAT, prevTileReference.EntityType);
            entityRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.EntityType) && !prevTileReference.EntityType.Contains("NONE") && MapEditorController.instance != null);
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
                    sceneReference.SetTile(tilePosition, new TerrainTile(t.Type, t.Traversable, t.Position, t.SubType, t.OverlayType, t.OverlaySubType, t.PropType, t.EntityType, t.IsWall, editSpawnID, editTransitionScene, editTransitionSpawnID));
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
    }
}
