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

        public static GameController instance; //TODO: remove

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
        Dictionary<Character, Pair<ICharacterAction, List<Character>>> currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>();

        Character currentSourceAction; //TODO: when implementing multiplayer, this will only be on the client, the server (i.e. host) will handle the dictionary above
        public Character CurrentSourceAction { get { return currentSourceAction; } }


        SceneController worldSceneController;

        public GameClient Client { get { return GameClient.instance; } }
        public GameInfos Infos { get { return Client.Game.Infos; } }
        public List<Character> CombatEnemies { get { return Client.Game.CombatEnemies; } }
        public bool IsInCombat { get { return Client.Game.IsInCombat; } }
        public bool IsIndoors { get { return Client.Game.CurrentScene.IsIndoors; } }
        public string CurrentSceneType { get { return Client.Game.CurrentScene.ZType; } }

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
            doneTurnButton.onClick.AddListener(() => FinishPlayerTurnInput());

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
            foreach (Character playerData in Client.Game.Players)
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
            Camera.main.transform.parent.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == Client.Game.SelectedCharacter).gameObject;

            overworldInitialized = true;
        }

        private void RunSceneTransition()
        {

            UpdateEnemyControllers();
        }

        private void UpdateEnemyControllers()
        {
            worldSceneController.ResetScene(Client.Game.CurrentScene);

            // take down old enemy controllers
            for (int i = enemyControllers.Count-1; i >= 0; i--)
            {
                worldEnemiesPool.Deactivate(enemyControllers[i]);
            }

            // set up the new enemy controllers
            foreach (Character enemyData in Client.Game.Enemies)
            {
                // set up world character controller
                CharacterController enemyController = worldEnemiesPool.Get();
                enemyController.gameObject.name = enemyData.Name;
                enemyController.SetCharacter(enemyData);
            }
        }

        void Update()
        {
            if (Client.Game == null || instance != this)
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

            if (Client.Game.CurrentGameState == GameState.Win)
            {
                winScreen.SetActive(true);
                return;
            }
            if (Client.Game.CurrentGameState == GameState.Loss)
            {
                loseScreen.SetActive(true);
                return;
            }


            // transitioned to a new scene
            if (Client.Game.SceneTransitioned)
            {
                RunSceneTransition();
            }

            worldObjectContainer.SetActive(!Client.Game.IsInCombat);
            opponentHUDList.SetActive(Client.Game.IsInCombat);

            doneTurnButton.gameObject.SetActive(Client.Game.IsInCombat);

            if (Client.Game.IsInCombat && !wasInCombat)
            {
                // just entered combat, so initialize enemy portraits
                DiscordController.Instance.InBattle();

                foreach (Character c in Client.Game.CombatEnemies)
                {
                    OpponentHUDController opponent = enemyHUDPool.Get();
                    opponent.Init(c);
                }
            }

            if (!Client.Game.IsInCombat && wasInCombat)
            {
                // just exited combat, so remove enemy portraits
                for (int i = combatEnemyHUDs.Count - 1; i >= 0; i--)
                {
                    OpponentHUDController x = combatEnemyHUDs[i] as OpponentHUDController;
                    enemyHUDPool.Deactivate(x);
                }
            }

            if (Client.Game.IsInCombat)
            {
                doneTurnButton.GetComponentInChildren<Text>().text = currentSourceAction == null ? "SUBMIT TO BLACKBOARD" : "SUBMIT ACTION TARGETS";

                if (Client.Game.CurrentCombatState == CombatState.NextRound)
                {
                    currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>(); //(SERVER)
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

            if (Client.Game.gameMessages.Count > 0)
            {
                EventQueueManager.instance.AddEventMessage(Client.Game.gameMessages.Dequeue());
                //Marquee.instance.ResetTimer(Client.Game.gameMessages.Dequeue());
                //Marquee.instance.StartTimer();
            }

            if (IsInCombat && Marquee.instance.IsFinished())
            {
                if (Client.Game.gameCombatActionQueue.Count > 0)
                {
                    PairStruct<Character, ICharacterAction> characterAction = Client.Game.gameCombatActionQueue.Dequeue();

                    if (!(characterAction.second is BeginTurnAction))
                    {
                        Marquee.instance.ResetTimer();
                        Marquee.instance.AddToMultiMessage(characterAction.first.Name + " does: " + characterAction.second.GetName());
                        Marquee.instance.StartTimer();

                        AnimationHUDController actionHUDController = animationController.GetComponent<AnimationHUDController>();
                        actionHUDController.executeAction(characterAction.first, characterAction.second);
                    }
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

            wasInCombat = Client.Game.IsInCombat;
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

        /// <summary>
        ///     Tell the game to select a character
        /// </summary>
        /// <param name="c"></param>
        public void SelectCharacter(Character c)
        {
            if (currentSourceAction == null)
            {
                Client.Game.SelectCharacter(c);
            }
            else
            {
                ToggleActionTarget(currentSourceAction, c);
            }

            // update camera follow object
            //Camera.main.transform.parent.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == Client.Game.SelectedCharacter).gameObject;
            //EventSystem.current.SetSelectedGameObject(null);
        }

        public void BeginRecordingAction(ICharacterAction action, Character source)
        {
            if (currentSourceAction != null && currentSourceAction != source) // Cannot choose another player's action while choosing this players action (but choosing a different action on this player will override an unfinished action on this player) TODO: adjust for client and server
            {
                Debug.Log("Finish picking targets before choosing another action");
                return;
            }
            if (currentCharacterActions.ContainsKey(source))
                currentCharacterActions.Remove(source);
            currentSourceAction = source;
            currentCharacterActions.Add(source, new Pair<ICharacterAction, List<Character>>(action, new List<Character>()));

            InfoAction actionInfo = action.MyInfo;
            switch (actionInfo.TargetType)
            {
                case "TARGET_NONE":
                    break;
                case "TARGET_SELF":
                    currentCharacterActions[source].second.Add(source);
                    break;
                case "TARGET_TEAM":
                    if (Client.Game.Players.Contains(source))
                    {
                        currentCharacterActions[source].second.AddRange(Client.Game.Players);
                    }
                    else if (Client.Game.CombatEnemies.Exists(x => x == source))
                    {
                        currentCharacterActions[source].second.AddRange(Client.Game.CombatEnemies);
                    }
                    break;
                case "TARGET_OTHER_TEAM":
                    if (Client.Game.Players.Contains(source))
                    {
                        currentCharacterActions[source].second.AddRange(Client.Game.CombatEnemies);
                    }
                    else if (Client.Game.CombatEnemies.Exists(x => x == source))
                    {
                        currentCharacterActions[source].second.AddRange(Client.Game.Players);
                    }
                    break;
                case "TARGET_FRIEND":

                    break;
                case "TARGET_ENEMY":
                    if(actionInfo.TargetData == Client.Game.CombatEnemies.Count)
                        currentCharacterActions[source].second.AddRange(Client.Game.CombatEnemies);
                    break;
            }

            List<ICharacterHUDController> allCharacterControllers = new List<ICharacterHUDController>();
            allCharacterControllers.AddRange(playerHUDControllers);
            allCharacterControllers.AddRange(combatEnemyHUDs);

            foreach (ICharacterHUDController characterHUD in allCharacterControllers)
            {
                if (currentCharacterActions[source].second.Contains(characterHUD.Character))
                    characterHUD.SetTarget(true);
                else
                    characterHUD.SetTarget(false);
            }
        }

        public void ToggleActionTarget(Character source, Character target)
        {
            if (currentCharacterActions.ContainsKey(source))
            {

                InfoAction actionInfo = currentCharacterActions[source].first.MyInfo;
                switch (actionInfo.TargetType)
                {
                    case "TARGET_NONE":
                    case "TARGET_SELF":
                    case "TARGET_TEAM":
                    case "TARGET_OTHER_TEAM":
                        return;

                    case "TARGET_AMOUNT":
                        if (currentCharacterActions[source].second.Count >= actionInfo.TargetData && !currentCharacterActions[source].second.Contains(target))
                            return;
                        break;
                    case "TARGET_FRIEND":
                        if ((currentCharacterActions[source].second.Count >= actionInfo.TargetData && !currentCharacterActions[source].second.Contains(target)) || !Client.Game.Players.Contains(target) || target == source)
                            return;
                        break;
                    case "TARGET_ENEMY":
                        if ((currentCharacterActions[source].second.Count >= actionInfo.TargetData && !currentCharacterActions[source].second.Contains(target)) || !Client.Game.CombatEnemies.Exists(x => x == target) || target == source)
                            return;
                        break;
                }

                List<ICharacterHUDController> allCharacterControllers = new List<ICharacterHUDController>();
                allCharacterControllers.AddRange(playerHUDControllers);
                allCharacterControllers.AddRange(combatEnemyHUDs);

                if (currentCharacterActions[source].second.Contains(target))
                {
                    currentCharacterActions[source].second.Remove(target);
                    allCharacterControllers.Find(x => x.Character == target).SetTarget(false);
                }
                else
                {
                    currentCharacterActions[source].second.Add(target);
                    allCharacterControllers.Find(x => x.Character == target).SetTarget(true);
                }
            }
            else
            {
                Debug.Log("Character " + source.Name + " did not begin recording an action, and thus cannot toggle the action targets");
            }
        }

        public void FinishRecordingAction(Character source)
        {
            if (!currentCharacterActions.ContainsKey(source))
                return;

            InfoAction actionInfo = currentCharacterActions[source].first.MyInfo;
            switch (actionInfo.TargetType)
            {
                case "TARGET_NONE":
                case "TARGET_SELF":
                case "TARGET_TEAM":
                case "TARGET_OTHER_TEAM":
                    break;

                case "TARGET_AMOUNT":
                case "TARGET_FRIEND":
                case "TARGET_ENEMY":
                    if (currentCharacterActions[source].second.Count > actionInfo.TargetData || currentCharacterActions[source].second.Count == 0)
                        return;
                    break;
            }

            Client.Game.RecordAction(currentCharacterActions[source].first, source, currentCharacterActions[source].second);
            currentCharacterActions.Remove(source);
            currentSourceAction = null;
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
        public void FinishPlayerTurnInput()
        {
            ClearTargets();
            if (currentSourceAction == null)
                Client.FinishPlayerTurnInput();
            else
                FinishRecordingAction(currentSourceAction);
        }

        /// <summary>
        ///     Gets the corresponding dice roll for a given character
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public int GetDiceRoll(Character targetCharacter)
        {

            if (Client.Game.TurnOrder == null || Client.Game.TurnOrder.Count == 0 || Client.Game.CurrentCombatState != CombatState.ExecuteTurns)
            {
                return -1;
            }
            return Client.Game.TurnOrder.Count - Client.Game.TurnOrder.IndexOf(targetCharacter);
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