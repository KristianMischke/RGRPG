using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RGRPG.Core;
using UnityEngine.SceneManagement;
using RGRPG.Core.Generics;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Connects the UI to the Game data/functions
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In the future, we will probably wrap this class with GameClientController and GameServerController, and thus move some of the functionality there
    ///     </para>
    /// </remarks>
    public class GameController : MonoBehaviour
    {

        public static GameController instance;

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject canvasObject;
        public GameObject playerHUDList;
        public GameObject opponentHUDList;
        public GameObject animationController;

        
        [HideInInspector]
        public List<CharacterController> playerControllers;
        [HideInInspector]
        public List<CharacterHUDController> playerHUDControllers;
        [HideInInspector]
        public List<CharacterController> enemyControllers;
        [HideInInspector]
        public Pool<OpponentHUDController> enemyHUDPool;
        [HideInInspector]
        public List<OpponentHUDController> combatEnemyHUDs;

        // Prefabs
        public GameObject DiscordControllerObject;

        public GameObject worldSceneView;
        public GameObject characterView;
        public GameObject characterHUDView;
        public GameObject opponentHUDView;


        // Data
        bool inGame = false;
        bool firstGameUpdate = true;
        Game game;

        public GameInfos Infos { get { return game.Infos; } }

        private void OnEnable()
        {
            // This class acts kind of like a singleton
            if (instance != null)
            {
                Destroy(this.gameObject); // an instance of GameController already exists, so we should not make a second one
                return;   
            }


            instance = this;
            DontDestroyOnLoad(this.gameObject);

            game = new Game();

            // if the discord controller hasn't been initialized, then initialize it
            if (DiscordController.Instance == null)
            {
                GameObject controllerObject = Instantiate(DiscordControllerObject);
                DontDestroyOnLoad(controllerObject);
            }
        }

        void Start()
        {

            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                SelectCharacters(Infos.GetAll<InfoCharacter>().FindAll(x => !x.IsEnemy).ToArray());
            }

        }

        private void InitOverworld()
        {
            DiscordController.Instance.InOverworld();

            TextAsset terrainXMLText = Resources.Load<TextAsset>(@"Data\TerrainAssets");
            SpriteManager.LoadSpriteAssetsXml(terrainXMLText.text, SpriteManager.AssetType.TERRAIN);

            


            if (worldObjectContainer == null)
            {
                worldObjectContainer = GameObject.Find("WorldObjects");
            }

            if (canvasObject == null)
            {
                canvasObject = FindObjectOfType<Canvas>().gameObject;
            }

            if (playerHUDList == null)
            {
                playerHUDList = GameObject.Find("PlayerList");
            }

            if (opponentHUDList == null)
            {
                opponentHUDList = GameObject.Find("EnemyList");
            }

            if (animationController == null)
            {
                animationController = canvasObject; //TEMP!!!!!!!!!!  //FindObjectOfType<AnimationHUDController>().gameObject;
            }

            playerControllers = new List<CharacterController>();
            enemyControllers = new List<CharacterController>();

            combatEnemyHUDs = new List<OpponentHUDController>();
            enemyHUDPool = new Pool<OpponentHUDController>(
            x =>
            {
                combatEnemyHUDs.Add(x);
                x.gameObject.SetActive(true);
            }, x =>
            {
                combatEnemyHUDs.Remove(x);
                x.gameObject.SetActive(false);
            }, () =>
            {
                OpponentHUDController x = Instantiate(opponentHUDView, opponentHUDList.transform).GetComponent<OpponentHUDController>();
                combatEnemyHUDs.Add(x);
                return x;
            });

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
            Camera.main.transform.parent.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.SelectedCharacter).gameObject;

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
        }

        public void SelectCharacters(InfoCharacter[] playerSelections)
        {
            game.SelectCharacters(playerSelections);
            inGame = true;
        }

        void Update()
        {
            if (game == null || !inGame)
                return;

            if (firstGameUpdate)
            {
                //if (SceneManager.GetActiveScene().name == "GameScene")
                    InitOverworld();
                firstGameUpdate = false;
            }

            game.GameLoop();

            worldObjectContainer.SetActive(!game.IsInCombat);
            opponentHUDList.SetActive(game.IsInCombat);

            if (game.IsInCombat)
            {
                DiscordController.Instance.InBattle();

                if (game.CurrentCombatState == CombatState.BeginCombat)
                {
                    foreach (Character c in game.CombatEnemies)
                    {
                        OpponentHUDController opponent = enemyHUDPool.Get();
                        opponent.Init(c); //TODO selectAction for choosing enemy target
                    }
                }

                if (game.CurrentCombatState == CombatState.EndCombat)
                {

                    foreach (OpponentHUDController x in combatEnemyHUDs)
                    {
                        enemyHUDPool.Deactivate(x);
                    }

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
                    if (!enemyController.character.IsAlive())
                    {
                        enemyControllers.RemoveAt(i);
                        Destroy(enemyController.gameObject);
                    }
                }

                MoveSelectedCharacter();
            }

            if (game.gameMessages.Count > 0)
            {
                EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
                //Marquee.instance.ResetTimer(game.gameMessages.Dequeue());
                //Marquee.instance.StartTimer();
            }

            if (IsInCombat() && Marquee.instance.IsFinished())
            {
                if (game.gameCombatActionQueue.Count > 0)
                {
                    PairStruct<Character, ICharacterAction> characterAction = game.gameCombatActionQueue.Dequeue();

                    if (!(characterAction.second is BeginTurnAction))
                    {
                        Marquee.instance.ResetTimer();
                        Marquee.instance.AddToMultiMessage(characterAction.first.Name + " does: " + characterAction.second.GetName());
                        Marquee.instance.StartTimer();


                        // set up UI character controller
                       

                        //create action animation HUD controller, attach here, need an init function that takes in the Icharacteraction from characteraction.second
                        //inside of this class, compare 
                        //CharacterHUDController playerHUDController = playerHUDView.GetComponent<CharacterHUDController>();
                        //playerHUDController.Init(characterAction.second);

                        AnimationHUDController actionHUDController = animationController.GetComponent<AnimationHUDController>();
                        actionHUDController.executeAction(characterAction.second);
                        //TODO: //characterAction.second.GetType() == typeof(HealAction) <--- put this in the ActionAnimationHUDController to determine wich image to show
                    }
                }
                else
                {
                    game.ProcessNextCombatStep();
                }
            }

            if (!IsInCombat() && Marquee.instance.IsFinished())
            {
                Marquee.instance.Hide();
                Marquee.instance.Clear();
            }
            else
            {
                Marquee.instance.Show();
            }

            if (Input.GetKey(KeyCode.Escape))
                SceneManager.LoadScene("PauseMenuScene");
        }

        /// <summary>
        ///     Allows the user to move the selected character with the arrow keys or WASD
        /// </summary>
        void MoveSelectedCharacter()
        {
            //TODO: things might need to be adjusted due to the camera tilt?!

            float moveMagnitude = 4f;
            float yMovement = 0;
            float xMovement = 0;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                yMovement = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                yMovement = -1;
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                xMovement = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                xMovement = -1;
            }

            Vector2 moveVector = new Vector2(xMovement + yMovement, yMovement- xMovement);
            //moveVector = moveVector * new Vector2(1, 2);
            moveVector.Normalize();
            moveVector *= moveMagnitude*Time.deltaTime;

            game.MoveSelectedCharacter(moveVector.x, moveVector.y);

        }

        /// <summary>
        ///     Tell the game to select a character
        /// </summary>
        /// <param name="c"></param>
        public void SelectCharacter(Character c)
        {
            game.SelectCharacter(c);

            // update camera follow object
            Camera.main.transform.parent.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.SelectedCharacter).gameObject;
            EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        ///     Tell the game to record an action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void RecordAction(ICharacterAction action, Character source, Character target)
        {
            game.RecordAction(action, source, target);
        }

        /// <summary>
        ///     Gets the current enemie(s) in combat
        /// </summary>
        /// <returns></returns>
        public List<Character> GetCombatEnemies()
        {
            return game.CombatEnemies;
        }

        /// <summary>
        ///     Tells the game that the user is done inputting his combat actions
        /// </summary>
        public void FinishPlayerTurnInput()
        {
            game.FinishPlayerTurnInput();
        }

        public bool IsInCombat()
        {
            return game.IsInCombat;
        }

        /// <summary>
        ///     Gets the corresponding dice roll for a given character
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public int GetDiceRoll(Character targetCharacter)
        {

            if (game.TurnOrder == null || game.TurnOrder.Count == 0 || game.CurrentCombatState != CombatState.ExecuteTurns)
            {
                return -1;
            }
            return game.TurnOrder.Count - game.TurnOrder.IndexOf(targetCharacter);
        }
    }

}