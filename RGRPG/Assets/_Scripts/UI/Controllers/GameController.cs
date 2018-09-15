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
        public GameObject canvasObject;
        public GameObject playerHUDList;

        // Prefabs
        public GameObject characterView;
        public GameObject characterHUDView;


        // Data
        Game game;

        // Use this for initialization
        void Start()
        {
            if (worldObjectContainer == null)
            {
                worldObjectContainer = GameObject.Find("WorldObjects");
            }

            if (canvasObject == null)
            {
                canvasObject = FindObjectOfType<Canvas>().gameObject;
            }

            game = new Game();

            // set up the player controllers
            foreach (Character playerData in game.Players)
            {
                // set up world character controller
                GameObject playerView = Instantiate(characterView);
                playerView.transform.SetParent(worldObjectContainer.transform);
                playerView.transform.position = new Vector3(Random.Range(-5.0f, 5.0f), 0); //TODO: change. position should probably be stored in the core character class
                
                CharacterController playerController = playerView.GetComponent<CharacterController>();
                playerController.SetCharacter(playerData);

                // set up UI character controller
                GameObject playerHUDView = Instantiate(characterHUDView);
                playerHUDView.transform.SetParent(playerHUDList.transform);

                CharacterHUDController playerHUDController = playerHUDView.GetComponent<CharacterHUDController>();
                playerHUDController.SetCharacter(playerData);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}