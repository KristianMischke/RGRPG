using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.Controllers
{

    public class GameController : MonoBehaviour
    {

        public static GameController instance;

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject combatObjectContainer;
        public GameObject canvasObject;
        public GameObject playerHUDList;

        [HideInInspector]
        public List<CharacterController> playerControllers;
        [HideInInspector]
        public List<CharacterHUDController> playerHUDControllers;
        [HideInInspector]
        public List<CharacterController> enemyControllers;
        [HideInInspector]
        public List<CharacterHUDController> enemyHUDControllers;

        [HideInInspector]
        public CharacterController combatEnemyController;

        // Prefabs
        public GameObject worldSceneView;
        public GameObject characterView;
        public GameObject characterHUDView;

        // Data
        Game game;

        // Use this for initialization
        void Start()
        {
            if (instance == null)
                instance = this;

            if (worldObjectContainer == null)
            {
                worldObjectContainer = GameObject.Find("WorldObjects");
            }

            if (combatObjectContainer == null)
            {
                combatObjectContainer = GameObject.Find("CombatObjects");
            }

            if (canvasObject == null)
            {
                canvasObject = FindObjectOfType<Canvas>().gameObject;
            }

            playerControllers = new List<CharacterController>();
            enemyControllers = new List<CharacterController>();

            game = new Game();

            // set up the scene controller
            GameObject worldSceneObject = Instantiate(worldSceneView);
            worldSceneObject.transform.SetParent(worldObjectContainer.transform);

            SceneController worldSceneController = worldSceneObject.GetComponent<SceneController>();
            worldSceneController.scene = game.StartScene;
            worldSceneController.ResetScene();

            // set up the player controllers
            foreach (Character playerData in game.Players)
            {
                // set up world character controller
                GameObject playerView = Instantiate(characterView);
                playerView.transform.SetParent(worldObjectContainer.transform);
                playerView.name = playerData.Name;
                
                CharacterController playerController = playerView.GetComponent<CharacterController>();
                playerController.SetCharacter(playerData);
                playerControllers.Add(playerController);

                // set up UI character controller
                GameObject playerHUDView = Instantiate(characterHUDView);
                playerHUDView.transform.SetParent(playerHUDList.transform);

                CharacterHUDController playerHUDController = playerHUDView.GetComponent<CharacterHUDController>();
                playerHUDController.Init(playerData);
                playerHUDControllers.Add(playerHUDController);
            }
            Camera.main.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.SelectedCharacter).gameObject;

            // set up the enemy controllers
            foreach (Character enemyData in game.Enemies)
            {
                // set up world character controller
                GameObject enemyView = Instantiate(characterView);
                enemyView.transform.SetParent(worldObjectContainer.transform);
                enemyView.name = enemyData.Name;

                CharacterController enemyController = enemyView.GetComponent<CharacterController>();
                enemyController.SetCharacter(enemyData);
                enemyControllers.Add(enemyController);
            }

            // add a character controller for combat view (this will show the enemy in battles)
            GameObject combatEnemyObject = Instantiate(characterView);
            combatEnemyController = combatEnemyObject.GetComponent<CharacterController>();
            combatEnemyController.transform.SetParent(combatObjectContainer.transform);

            combatEnemyController = combatEnemyObject.GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {

            game.GameLoop();

            worldObjectContainer.SetActive(!game.IsInCombat);
            combatObjectContainer.SetActive(game.IsInCombat);

            if (game.IsInCombat)
            {
                combatEnemyController.SetCharacter(game.CombatEnemy);
                Camera.main.GetComponent<CameraController>().followObject = combatEnemyController.gameObject;
            }
            else
            {
                MoveSelectedCharacter();
            }

            if(game.gameMessages.Count > 0)
                EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
        }

        void MoveSelectedCharacter()
        {
            float moveMagnitude = 0.2f;
            float yMovement = 0;
            float xMovement = 0;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                yMovement = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                yMovement = -1;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                xMovement = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                xMovement = -1;
            }

            Vector2 moveVector = new Vector2(xMovement, yMovement);
            moveVector.Normalize();
            moveVector *= moveMagnitude;

            game.MoveSelectedCharacter(moveVector.x, moveVector.y);

        }

        public void SelectCharacter(Character c)
        {
            game.SelectCharacter(c);

            // update camera follow object
            Camera.main.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.SelectedCharacter).gameObject;
        }

        public void RecordAction(ICharacterAction action, Character source, Character target)
        {
            game.RecordAction(action, source, target);
        }

        public Character GetCombatEnemy()
        {
            return game.CombatEnemy;
        }

        public void FinishPlayerTurnInput()
        {
            game.FinishPlayerTurnInput();
        }

        public bool IsInCombat()
        {
            return game.IsInCombat;
        }
    }

}