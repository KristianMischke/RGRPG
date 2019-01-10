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
        bool inGame = false;
        static bool firstGameUpdate = true;
        Game game;
        Dictionary<Character, Pair<ICharacterAction, List<Character>>> currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>();

        Character currentSourceAction; //TODO: when implementing multiplayer, this will only be on the client, the server (i.e. host) will handle the dictionary above
        public Character CurrentSourceAction { get { return currentSourceAction; } }


        SceneController worldSceneController;

        public GameInfos Infos { get { return game.Infos; } }

        public List<Character> CombatEnemies { get { return game.CombatEnemies; } }
        public bool IsInCombat { get { return game.IsInCombat; } }
        public bool IsIndoors { get { return game.CurrentScene.IsIndoors; } }
        public string CurrentSceneType { get { return game.CurrentScene.ZType; } }

        private void OnEnable()
        {
            // This class acts kind of like a singleton
            if (instance != null && instance != this)
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

            if (SceneManager.GetActiveScene().name == "GameScene") // Hacky way of adding all four players when STARTING from the GameScene instead of the Menu
            {
                SelectCharacters(Infos.GetAll<InfoCharacter>().FindAll(x => !x.IsEnemy).ToArray());
            }

        }

        public void SelectCharacters(InfoCharacter[] playerSelections)
        {
            game.SelectCharacters(playerSelections);
            inGame = true;
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

            overworldInitialized = true;
        }

        private void RunSceneTransition()
        {

            UpdateEnemyControllers();
        }

        private void UpdateEnemyControllers()
        {
            worldSceneController.ResetScene(game.CurrentScene);

            // take down old enemy controllers
            for (int i = enemyControllers.Count-1; i >= 0; i--)
            {
                worldEnemiesPool.Deactivate(enemyControllers[i]);
            }

            // set up the new enemy controllers
            foreach (Character enemyData in game.Enemies)
            {
                // set up world character controller
                CharacterController enemyController = worldEnemiesPool.Get();
                enemyController.gameObject.name = enemyData.Name;
                enemyController.SetCharacter(enemyData);
            }
        }

        void Update()
        {
            if (game == null || !inGame || instance != this)
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

            game.GameLoop(Time.deltaTime);

            if (game.CurrentGameState == GameState.Win)
            {
                winScreen.SetActive(true);
                return;
            }
            if (game.CurrentGameState == GameState.Loss)
            {
                loseScreen.SetActive(true);
                return;
            }


            // transitioned to a new scene
            if (game.SceneTransitioned)
            {
                RunSceneTransition();
            }

            worldObjectContainer.SetActive(!game.IsInCombat);
            opponentHUDList.SetActive(game.IsInCombat);

            doneTurnButton.gameObject.SetActive(game.IsInCombat);
            if (game.IsInCombat)
            {
                DiscordController.Instance.InBattle();

                doneTurnButton.GetComponentInChildren<Text>().text = currentSourceAction == null ? "SUBMIT TO BLACKBOARD" : "SUBMIT ACTION TARGETS";

                if (game.CurrentCombatState == CombatState.BeginCombat)
                {
                    foreach (Character c in game.CombatEnemies)
                    {
                        OpponentHUDController opponent = enemyHUDPool.Get();
                        opponent.Init(c); //TODO selectAction for choosing enemy target (SERVER)
                    }
                }

                if (game.CurrentCombatState == CombatState.NextRound)
                {
                    currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>(); //(SERVER)
                }

                if (game.CurrentCombatState == CombatState.EndCombat)
                {
                    for (int i = combatEnemyHUDs.Count-1; i >= 0; i--)
                    {
                        OpponentHUDController x = combatEnemyHUDs[i] as OpponentHUDController;
                        enemyHUDPool.Deactivate(x);
                    }
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

                MoveSelectedCharacter(); // (CLIENT REQUEST)
            }

            if (game.gameMessages.Count > 0)
            {
                EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
                //Marquee.instance.ResetTimer(game.gameMessages.Dequeue());
                //Marquee.instance.StartTimer();
            }

            if (IsInCombat && Marquee.instance.IsFinished())
            {
                if (game.gameCombatActionQueue.Count > 0)
                {
                    PairStruct<Character, ICharacterAction> characterAction = game.gameCombatActionQueue.Dequeue();

                    if (!(characterAction.second is BeginTurnAction))
                    {
                        Marquee.instance.ResetTimer();
                        Marquee.instance.AddToMultiMessage(characterAction.first.Name + " does: " + characterAction.second.GetName());
                        Marquee.instance.StartTimer();

                        AnimationHUDController actionHUDController = animationController.GetComponent<AnimationHUDController>();
                        actionHUDController.executeAction(characterAction.first, characterAction.second);
                        //TODO: //characterAction.second.GetType() == typeof(HealAction) <--- put this in the ActionAnimationHUDController to determine wich image to show
                    }
                }
                else
                {
                    game.ProcessNextCombatStep(); // (SERVER)
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
            if (currentSourceAction == null)
            {
                game.SelectCharacter(c);
            }
            else
            {
                ToggleActionTarget(currentSourceAction, c);
            }

            // update camera follow object
            //Camera.main.transform.parent.GetComponent<CameraController>().followObject = playerControllers.Find(x => x.character == game.SelectedCharacter).gameObject;
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
                    if (game.Players.Contains(source))
                    {
                        currentCharacterActions[source].second.AddRange(game.Players);
                    }
                    else if (game.CombatEnemies.Exists(x => x == source))
                    {
                        currentCharacterActions[source].second.AddRange(game.CombatEnemies);
                    }
                    break;
                case "TARGET_OTHER_TEAM":
                    if (game.Players.Contains(source))
                    {
                        currentCharacterActions[source].second.AddRange(game.CombatEnemies);
                    }
                    else if (game.CombatEnemies.Exists(x => x == source))
                    {
                        currentCharacterActions[source].second.AddRange(game.Players);
                    }
                    break;
                case "TARGET_FRIEND":

                    break;
                case "TARGET_ENEMY":
                    if(actionInfo.TargetData == game.CombatEnemies.Count)
                        currentCharacterActions[source].second.AddRange(game.CombatEnemies);
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
                        if ((currentCharacterActions[source].second.Count >= actionInfo.TargetData && !currentCharacterActions[source].second.Contains(target)) || !game.Players.Contains(target) || target == source)
                            return;
                        break;
                    case "TARGET_ENEMY":
                        if ((currentCharacterActions[source].second.Count >= actionInfo.TargetData && !currentCharacterActions[source].second.Contains(target)) || !game.CombatEnemies.Exists(x => x == target) || target == source)
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

            game.RecordAction(currentCharacterActions[source].first, source, currentCharacterActions[source].second);
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
                game.FinishPlayerTurnInput();
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

            if (game.TurnOrder == null || game.TurnOrder.Count == 0 || game.CurrentCombatState != CombatState.ExecuteTurns)
            {
                return -1;
            }
            return game.TurnOrder.Count - game.TurnOrder.IndexOf(targetCharacter);
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