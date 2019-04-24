using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    public class GameController : NetworkBehaviour
    {

        /*
         * 
         * 
         * GOAL: remove all instances of Client.Game
         * 
         * 
         */

        public static GameController instance;

        // Scene Object References
        public GameObject worldObjectContainer;
        public GameObject canvasObject;
        public GameObject playerHUDList;
        public GameObject opponentHUDList;
        public GameObject animationController;
        public GameObject winScreen;
        public GameObject loseScreen;
        public Button doneTurnButton;

        
        [HideInInspector]
        public List<CharacterController> playerControllers;
        [HideInInspector]
        public List<ICharacterHUDController> playerHUDControllers;
        [HideInInspector]
        public List<CharacterController> enemyControllers;
        [HideInInspector]
        public Pool<CharacterController> worldEnemiesPool;
        [HideInInspector]
        public Pool<OpponentHUDController> enemyHUDPool;
        [HideInInspector]
        public List<ICharacterHUDController> combatEnemyHUDs;

        // Prefabs
        public GameObject DiscordControllerObject;

        public GameObject worldSceneView;
        public GameObject characterView;
        public GameObject characterHUDView;
        public GameObject opponentHUDView;


        // Data
        bool overworldInitialized = false;
        static bool firstGameUpdate = true;
        static bool wasInCombat = false;

        SceneController worldSceneController;

        public GameClient Client { get { return GameClient.instance; } }
        public GameInfos Infos { get { return Client.Infos; } }
        public List<Character> CombatEnemies { get { return Client.CombatEnemies; } }
        public bool IsInCombat { get { return Client.IsInCombat; } }
        public bool IsIndoors { get { return Client.CurrentScene.IsIndoors; } }
        public string CurrentSceneType { get { return Client.CurrentScene.ZType; } }

        private void OnEnable()
        {
            // if the discord controller hasn't been initialized, then initialize it
            if (DiscordController.Instance == null)
            {
                GameObject controllerObject = Instantiate(DiscordControllerObject);
                DontDestroyOnLoad(controllerObject);
            }

            instance = this;
        }

        private void InitOverworld()
        {
            DiscordController.Instance.InOverworld();

            if (worldObjectContainer == null)
            {
                worldObjectContainer = GameObject.Find("WorldObjects");
            }

            if (winScreen == null)
            {
                winScreen = GameObject.Find("WinScreen");
            }
            winScreen.SetActive(false);

            if (loseScreen == null)
            {
                loseScreen = GameObject.Find("LossScreen");                
            }
            loseScreen.SetActive(false);

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

            if (doneTurnButton == null)
            {
                doneTurnButton = GameObject.Find("DoneTurnButton").GetComponent<Button>();
            }
            doneTurnButton.onClick.AddListener(() => BlackboardActionButton());

            if (animationController == null)
            {
                animationController = canvasObject; //TEMP!!!!!!!!!!  //FindObjectOfType<AnimationHUDController>().gameObject;
            }

            playerControllers = new List<CharacterController>();

            enemyControllers = new List<CharacterController>();
            worldEnemiesPool = new Pool<CharacterController>(
            x =>
            {
                enemyControllers.Add(x);
                x.gameObject.SetActive(true);
            }, x =>
            {
                enemyControllers.Remove(x);
                x.gameObject.SetActive(false);
            }, () =>
            {
                CharacterController x = Instantiate(characterView, worldObjectContainer.transform).GetComponent<CharacterController>();
                enemyControllers.Add(x);
                return x;
            });

            combatEnemyHUDs = new List<ICharacterHUDController>();
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
            worldObjectContainer.SetActive(true);
            GameObject worldSceneObject = Instantiate(worldSceneView, worldObjectContainer.transform);
            worldSceneController = worldSceneObject.GetComponent<SceneController>();

            RunSceneTransition();

            playerHUDControllers = new List<ICharacterHUDController>();
            // set up the player controllers
            foreach (Character playerData in Client.Players)
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
            Camera.main.transform.parent.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == Client.SelectedCharacter).gameObject;

            overworldInitialized = true;
        }

        private void RunSceneTransition()
        {
            worldSceneController.ResetScene(Client.CurrentScene);

            UpdateEnemyControllers();
        }

        private void UpdateEnemyControllers()
        {
            // take down old enemy controllers
            for (int i = enemyControllers.Count-1; i >= 0; i--)
            {
                worldEnemiesPool.Deactivate(enemyControllers[i]);
            }

            // set up the new enemy controllers
            foreach (Character enemyData in Client.Enemies)
            {
                // set up world character controller
                CharacterController enemyController = worldEnemiesPool.Get();
                enemyController.gameObject.name = enemyData.Name;
                enemyController.SetCharacter(enemyData);
            }
        }

        void Update()
        {
            if (Client.IsGameNull || instance != this)
                return;

            if (firstGameUpdate)
            {
                if (SceneManager.GetActiveScene().name == "GameScene")
                {
                    InitOverworld();
                    firstGameUpdate = false;
                    return;
                }
            }

            if (!overworldInitialized)
                return;

            if (Client.CurrentGameState == GameState.Win)
            {
                winScreen.SetActive(true);
                return;
            }
            if (Client.CurrentGameState == GameState.Loss)
            {
                loseScreen.SetActive(true);
                return;
            }


            // transitioned to a new scene
            if (Client.SceneTransitioned)
            {
                RunSceneTransition();
            }

            worldObjectContainer.SetActive(!IsInCombat);
            opponentHUDList.SetActive(IsInCombat);

            doneTurnButton.gameObject.SetActive(IsInCombat);

            if (IsInCombat && !wasInCombat)
            {
                // just entered combat, so initialize enemy portraits
                DiscordController.Instance.InBattle();

                foreach (Character c in Client.CombatEnemies)
                {
                    OpponentHUDController opponent = enemyHUDPool.Get();
                    opponent.Init(c);
                }
            }

            if (!IsInCombat && wasInCombat)
            {
                // just exited combat, so remove enemy portraits
                for (int i = combatEnemyHUDs.Count - 1; i >= 0; i--)
                {
                    OpponentHUDController x = combatEnemyHUDs[i] as OpponentHUDController;
                    enemyHUDPool.Deactivate(x);
                }

                UpdateEnemyControllers();
            }

            if (Client.IsInCombat && Client.AreActionsDirty)
            {
                List<ICharacterHUDController> allCharacterControllers = new List<ICharacterHUDController>();
                allCharacterControllers.AddRange(playerHUDControllers);
                allCharacterControllers.AddRange(combatEnemyHUDs);

                foreach (ICharacterHUDController controller in allCharacterControllers)
                {
                    controller.SetTarget(Client.IsCharacterCurrentTarget(controller.Character.ID));
                }
            }

            if (IsInCombat)
            {
                doneTurnButton.GetComponentInChildren<Text>().text = Client.IsMakingAction ? "SUBMIT ACTION TARGETS" : "SUBMIT TO BLACKBOARD";
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

            if (Client.GameMessages.Count > 0)
            {
                EventQueueManager.instance.AddEventMessage(Client.GameMessages.Dequeue());
                //Marquee.instance.ResetTimer(Client.GameMessages.Dequeue());
                //Marquee.instance.StartTimer();
            }

            if (IsInCombat && Marquee.instance.IsFinished())
            {
                if (Client.MoreCombatActions)
                {
                    PairStruct<Character, ICharacterAction> characterAction = Client.GetNextCombatAction;

                    if (!(characterAction.second is BeginTurnAction))
                    {
                        List<Character> targets = characterAction.second.GetTargets();

                        Marquee.instance.ResetTimer();
                        if (characterAction.second.MyInfo == null)
                        {
                            if(characterAction.second is PassTurnAction)
                                Marquee.instance.AddToMultiMessage(characterAction.first.Name + " passes");
                        }
                        else
                        {
                            Marquee.instance.AddToMultiMessage(characterAction.second.MyInfo.GetActionQuip(characterAction.first, (targets == null || targets.Count != 1) ? null : targets[0]));
                        }
                        Marquee.instance.StartTimer();

                        AnimationHUDController actionHUDController = animationController.GetComponent<AnimationHUDController>();
                        actionHUDController.executeAction(characterAction.first, characterAction.second);
                    }
                }
                else
                {
                    Client.ProcessNextCombatStep();
                }
            }

            if (!IsInCombat && Marquee.instance.IsFinished())
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

            wasInCombat = IsInCombat;
        }

        /// <summary>
        ///     Asks the client to move the characters based on the user input from Arrow Keys or WASD
        /// </summary>
        void MoveSelectedCharacter()
        {
            int xInput = 0;
            int yInput = 0;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                yInput = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                yInput = -1;
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                xInput = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                xInput = -1;
            }

            if (xInput != 0 || yInput != 0)
                Client.MoveCharacter(xInput, yInput);
        }

        
        public void ClearTargets()
        {
            List<ICharacterHUDController> allCharacterControllers = new List<ICharacterHUDController>();
            allCharacterControllers.AddRange(playerHUDControllers);
            allCharacterControllers.AddRange(combatEnemyHUDs);
            foreach (ICharacterHUDController charHUD in allCharacterControllers)
            {
                charHUD.SetTarget(false);
            }
        }

        /// <summary>
        ///     Tells the game that the user is done inputting his combat actions
        /// </summary>
        public void BlackboardActionButton()
        {
            EventSystem.current.SetSelectedGameObject(null);
            ClearTargets();
            Client.FinishPlayerTurnInput();
        }

        /// <summary>
        ///     Gets the corresponding dice roll for a given character
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public int GetDiceRoll(Character targetCharacter)
        {

            if (Client.TurnOrder == null || Client.TurnOrder.Count == 0 || Client.CurrentCombatState != CombatState.ExecuteTurns)
            {
                return -1;
            }
            return Client.TurnOrder.Count - Client.TurnOrder.IndexOf(targetCharacter);
        }

        public Transform GetCharacterControllerPosition(Character c)
        {
            ICharacterHUDController hud = playerHUDControllers.Find(x => x.Character == c);
            if (hud != null)
            {
                return hud.Transform;
            }

            ICharacterHUDController opponentHud = combatEnemyHUDs.Find(x => x.Character == c);

            if (opponentHud != null)
                return opponentHud.Transform;

            return null;
        }
    }

}