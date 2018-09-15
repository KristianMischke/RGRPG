using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.UIControllers
{

    public class GameController : MonoBehaviour
    {

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

        // Prefabs
        public GameObject characterView;
        public GameObject characterHUDView;


        // Data
        Game game;
        public bool combatMode = false;

        // Use this for initialization
        void Start()
        {
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

            // set up the player controllers
            foreach (Character playerData in game.Players)
            {
                // set up world character controller
                GameObject playerView = Instantiate(characterView);
                playerView.transform.SetParent(worldObjectContainer.transform);
                
                CharacterController playerController = playerView.GetComponent<CharacterController>();
                playerController.SetCharacter(playerData);
                playerControllers.Add(playerController);

                // set up UI character controller
                GameObject playerHUDView = Instantiate(characterHUDView);
                playerHUDView.transform.SetParent(playerHUDList.transform);

                CharacterHUDController playerHUDController = playerHUDView.GetComponent<CharacterHUDController>();
                playerHUDController.character = playerData;
                playerHUDController.SetSelectAction(() => { game.selectedCharacter = playerHUDController.character; });
                playerHUDControllers.Add(playerHUDController);
            }

            // set up the enemy controllers
            foreach (Character enemyData in game.Enemies)
            {
                // set up world character controller
                GameObject enemyView = Instantiate(characterView);
                enemyView.transform.SetParent(worldObjectContainer.transform);

                CharacterController enemyController = enemyView.GetComponent<CharacterController>();
                enemyController.SetCharacter(enemyData);
                enemyControllers.Add(enemyController);
            }
        }

        // Update is called once per frame
        void Update()
        {

            worldObjectContainer.SetActive(!combatMode);
            combatObjectContainer.SetActive(combatMode);

            if (combatMode)
            {

            }
            else
            {
                Camera.main.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.selectedCharacter).gameObject;
                MoveSelectedCharacter();
            }

        }

        void MoveSelectedCharacter()
        {
            int yMovement = 0;
            int xMovement = 0;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                yMovement = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                yMovement = -1;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                xMovement = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                xMovement = -1;
            }

            game.selectedCharacter.Move(xMovement, yMovement, 0);

        }
    }

}