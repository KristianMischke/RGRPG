#pragma warning disable 0219 // Variable assigned but not used

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RGRPG.Core;
using UnityEngine.SceneManagement;

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
        public List<CharacterController> combatEnemiesController;

        // Prefabs
        public GameObject DiscordControllerObject;

        public GameObject worldSceneView;
        public GameObject characterView;
        public GameObject characterHUDView;

        // Data
        Game game;

        private void OnEnable()
        {
            if (DiscordController.Instance == null)
            {
                GameObject controllerObject = Instantiate(DiscordControllerObject);
                DontDestroyOnLoad(controllerObject);
            }
        }

        // Use this for initialization
        void Start()
        {
            DiscordController.Instance.InOverworld();

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
            // TODO: make this adjustable for bands of enemies
            GameObject combatEnemyObject = Instantiate(characterView);
            combatEnemiesController.Add(combatEnemyObject.GetComponent<CharacterController>());
            combatEnemiesController[0].transform.SetParent(combatObjectContainer.transform);
        }

        // Update is called once per frame
        void Update()
        {
            if (game == null)
                return;

            game.GameLoop();

            worldObjectContainer.SetActive(!game.IsInCombat);
            combatObjectContainer.SetActive(game.IsInCombat);

            if (game.IsInCombat)
            {
                DiscordController.Instance.InBattle();
                combatEnemiesController[0].SetCharacter(game.CombatEnemies[0]);
                Camera.main.GetComponent<CameraController>().followObject = combatEnemiesController[0].gameObject;

                if (game.CurrentCombatState == CombatState.EndCombat)
                {
                    SelectCharacter(game.SelectedCharacter);
                }
            }
            else
            {
                DiscordController.Instance.InOverworld();
                //TODO: This should not just be for enemies, ultimately when we add pooling, this will be resolved, but for now just remove enemy objects when they die
                for (int i = 0; i < enemyControllers.Count; i++)
                {
                    CharacterController enemyController = enemyControllers[i];
                    if (enemyController.character.Type == CharacterType.Enemy && !enemyController.character.IsAlive())
                    {
                        enemyControllers.RemoveAt(i);
                        Destroy(enemyController.gameObject);
                    }
                }

                MoveSelectedCharacter();
            }

            if(game.gameMessages.Count > 0)
                EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());

            if (Input.GetKey("escape"))
                SceneManager.LoadScene("PauseMenuScene");
        }

        void MoveSelectedCharacter()
        {
            float moveMagnitude = 4f;
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

            Vector2 moveVector = new Vector2(xMovement - yMovement, yMovement);
            moveVector = moveVector * new Vector2(1, 2);
            moveVector.Normalize();
            moveVector *= moveMagnitude*Time.deltaTime;

            game.MoveSelectedCharacter(moveVector.x, moveVector.y);

        }

        public void SelectCharacter(Character c)
        {
            game.SelectCharacter(c);

            // update camera follow object
            Camera.main.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.SelectedCharacter).gameObject;
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void RecordAction(ICharacterAction action, Character source, Character target)
        {
            game.RecordAction(action, source, target);
        }

        public List<Character> GetCombatEnemies()
        {
            return game.CombatEnemies;
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