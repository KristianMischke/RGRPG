﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RGRPG.Core.Generics;
using RGRPG.Core.NetworkCore;

namespace RGRPG.Core
{
    public class GameClient
    {
        public static GameClient instance;

        private IGameClientManager clientManager;

        // Data
        private Dictionary<int, ClientInfo> otherClients = new Dictionary<int, ClientInfo>();
        private ClientInfo myClientInfo;

        private bool inGame = false;
        private static bool firstGameUpdate = true;
        private static bool waitingForRound = false;
        private Game game;

        private GameState overrideGameState = GameState.Starting;
        private CombatState overrideCombatState = CombatState.NONE;

        private Dictionary<Character, Pair<ICharacterAction, List<Character>>> currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>();
        private Character currentSourceAction; //TODO: when implementing multiplayer, this will only be on the client


        public int ClientID { get { return myClientInfo.ID; } set { myClientInfo.ID = value; } }
        public int MyPlayerNumber { get { return myClientInfo.PlayerNumber; } set { myClientInfo.PlayerNumber = value; } }
        public int NumPlayers { get { return otherClients.Count + 1; } }

        public Game Game { get { return game; } }
        public GameInfos Infos { get { return game.Infos; } }
        public List<Character> CombatEnemies { get { return game.CombatEnemies; } }
        public bool IsInCombat { get { return game.IsInCombat; } }
        public bool IsIndoors { get { return game.CurrentScene.IsIndoors; } }
        public string CurrentSceneType { get { return game.CurrentScene.ZType; } }

        public ClientInfo MyClientInfo { get { return myClientInfo; } }
        public List<ClientInfo> OtherClients { get { return otherClients.Values.ToList(); } }
        public List<ClientInfo> AllClients { get { List<ClientInfo> all = otherClients.Values.ToList(); all.Add(myClientInfo); return all; } }

        public GameClient(IGameClientManager clientManager, bool isObserver)
        {
            myClientInfo = new ClientInfo(-1, -1, isObserver);
            this.clientManager = clientManager;
            game = new Game();

            instance = this;
        }

        public void RegisterOtherClient(int id, int playerNumber, bool observer)
        {
            if (id == ClientID)
                return;
            ClientInfo newClient = new ClientInfo(id, playerNumber, observer);
            otherClients.Add(id, newClient);
        }

        public void SyncClientData(object[] data)
        {
            int id = (int)data[0];

            if (id == ClientID)
                myClientInfo.SyncClientData(data);
            else
                otherClients[id].SyncClientData(data);
        }

        public void ChooseCharacter(string zType, int slot)
        {
            clientManager.ChooseCharacterToPlay(zType, slot);
        }

        public void CharacterSelectionFinished()
        {
            if(!myClientInfo.IsReadyToEnterGame)
                clientManager.SubmitCharacterSelection();
        }

        public void BeginGame(string[] characters)
        {
            overrideGameState = GameState.WorldMovement;
            game.SelectCharacters(characters);
            inGame = true;
        }

        public void CharacterUpdate(object[] data)
        {
            game.DeserializeCharacters(data);
        }

        public void EnemyUpdate(object[] data)
        {
            game.DeserializeCharacters(data);
        }

        public void SceneUpdate(string zType)
        {
            game.TransitionToScene(zType);
        }

        public void MoveCharacter(int xDirection, int yDirection)
        {
            clientManager.MoveCharacter(xDirection, yDirection);
        }

        public void BeginCombat(int[] enemyIds)
        {
            game.BeginCombat(enemyIds);
            overrideGameState = GameState.Combat;
            overrideCombatState = CombatState.BeginCombat;
        }

        public void UpdateCombatState(int combatState)
        {
            overrideCombatState = (CombatState)combatState;
        }

        public void UpdateCombatData(object[] data)
        {
            game.UpdateCombatData(data);
        }

        public void EndCombat()
        {
            overrideGameState = GameState.WorldMovement;
            overrideCombatState = CombatState.NONE;
        }

        public void FinishPlayerTurnInput()
        {
            clientManager.CombatFinishPlayerTurnInput();
        }
    
        private bool firstUpdate = true;
        public void Update(float deltaTime)
        {
            if (firstUpdate)
            {
                clientManager.RequestClientID(myClientInfo.IsObserver);
                firstUpdate = false;
            }

            if (game == null || !inGame)
                return;

            if (firstGameUpdate)
            {
                firstGameUpdate = false;
                return;
            }

            if (overrideCombatState < CombatState.COUNT && overrideGameState < GameState.COUNT)
                game.OverrideCombatState(overrideGameState, overrideCombatState);
            
            if (game.IsInCombat)
            {
                DiscordController.Instance.InBattle();

            }
            else
            {
                DiscordController.Instance.InOverworld();

                //MoveSelectedCharacter(); // (CLIENT REQUEST)
            }

            if (overrideCombatState == CombatState.ExecuteTurns)
            { }

            game.GameLoop(deltaTime);

            if (game.IsInCombat)
            {
                if (!waitingForRound && game.CurrentCombatState == CombatState.WaitForNextRound && game.gameCombatActionQueue.Count == 0)
                {
                    clientManager.CombatWaitingForNextRound();
                    waitingForRound = true;
                }
                else if (game.CurrentCombatState != CombatState.WaitForNextRound)
                {
                    waitingForRound = false;
                }
            }


            if (game.gameMessages.Count > 0)
            {
                //TODO: figure out best way to handle messages
                //EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
            }

            if (game.IsInCombat)
            {
                if (game.gameCombatActionQueue.Count > 0)
                {
                    //PairStruct<Character, ICharacterAction> characterAction = game.gameCombatActionQueue.Dequeue();

                }
                else
                {
                }
            }
        }
    }
}