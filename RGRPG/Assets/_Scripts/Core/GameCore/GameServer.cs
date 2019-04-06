using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Vector2 = UnityEngine.Vector2;
using Time = UnityEngine.Time;

using RGRPG.Core.Generics;
using RGRPG.Core.NetworkCore;

namespace RGRPG.Core
{
    public class ClientInfo
    {
        private int id;
        private int playerNumber;
        private bool isObserver;
        private string[] controllingCharacters = new string[4];
        private bool isReadyToEnterGame = false;
        private bool finishedTurnInput = false;
        private bool readyForNextRound = false;

        public int ID { get { return id; } set { id = value; } }
        public int PlayerNumber { get { return playerNumber; } set { playerNumber = value; } }
        public bool IsObserver { get { return isObserver; } }
        public string[] ControllingCharacters { get { return controllingCharacters; } }
        public bool IsReadyToEnterGame { get { return isReadyToEnterGame; } set { isReadyToEnterGame = value; } }
        public bool FinishedTurnInput { get { return finishedTurnInput; } set { finishedTurnInput = value; } }
        public bool ReadyForNextRound { get { return readyForNextRound; } set { readyForNextRound = value; } }

        public ClientInfo(int ID, int playerNumber, bool isObserver)
        {
            this.ID = ID;
            this.playerNumber = playerNumber;
            this.isObserver = isObserver;
        }

        public void SetCharacterToControl(string zType, int slot)
        {
            controllingCharacters[slot] = zType;
        }

        public void RemoveCharacterFromControl(int slot)
        {
            controllingCharacters[slot] = null;
        }

        public void SyncClientData(object[] data)
        {
            int id = (int)data[0];
            int playerNumber = (int)data[1];
            bool isObserver = (bool)data[2];
            string[] controllingCharacters = new string[] { (string)data[3], (string)data[4], (string)data[5], (string)data[6] };
            bool isReadyToEnterGame = (bool)data[7];
            bool finishedTurnInput = (bool)data[8];
            bool readyForNextRound = (bool)data[9];

            this.id = id;
            this.playerNumber = playerNumber;
            this.isObserver = isObserver;
            this.isReadyToEnterGame = isReadyToEnterGame;
            this.finishedTurnInput = finishedTurnInput;
            this.readyForNextRound = readyForNextRound;

            for (int i = 0; i < controllingCharacters.Length; i++)
            {
                this.controllingCharacters[i] = controllingCharacters[i];
            }
        }

        public object[] GetClientData()
        {
            return new object[] { id, playerNumber, isObserver, controllingCharacters[0], controllingCharacters[1], controllingCharacters[2], controllingCharacters[3], isReadyToEnterGame, finishedTurnInput, readyForNextRound };
        }
    }

    public class GameServer
    {
        public static GameServer instance;

        IGameServerManager serverManager;

        // Data
        private Dictionary<int, ClientInfo> connectedClients = new Dictionary<int, ClientInfo>();

        private bool inGame = false;
        private static bool firstGameUpdate = true;
        private Game game;
        private Dictionary<Character, Pair<ICharacterAction, List<Character>>> currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>();

        public int NumClientsConnected { get { return connectedClients == null ? 0 : connectedClients.Count; } }
        public int NumPlayingClients
        {
            get
            {
                int numPlayingClients = 0;
                foreach (ClientInfo c in connectedClients.Values)
                    if (!c.IsObserver)
                        numPlayingClients++;
                return numPlayingClients;
            }
        }

        public GameServer(IGameServerManager serverManager)
        {
            this.serverManager = serverManager;
            game = new Game();

            instance = this;
        }

        //
        // --- CLIENT SYNC AND CHARACTER SELECT ---
        //

        public void RegisterClient(int id, bool isObserver)
        {
            int numPlayingClients = NumPlayingClients;

            bool overrideObserver = isObserver || numPlayingClients >= 4;

            ClientInfo newClient = new ClientInfo(id, overrideObserver ? -1 : numPlayingClients, overrideObserver);
            connectedClients.Add(id, newClient);
            UnityEngine.Debug.Log("connected client: " + id);

            serverManager.BroadcastClientConnect(id, newClient.PlayerNumber, overrideObserver);
        }

        private void BroadcastSyncClient(int id)
        {
            ClientInfo client = connectedClients[id];
            serverManager.SyncClientInfo(client.GetClientData());
        }

        public void ChooseCharacter(int id, string character, int slot)
        {
            ClientInfo client = connectedClients[id];
            if (GameCharacterSelectHelper.IsValidSlot(client.PlayerNumber, NumPlayingClients, slot))
            {
                client.SetCharacterToControl(character, slot);
                BroadcastSyncClient(id);
            }
        }
        public void RemoveCharacter(int id, int slot)
        {
            connectedClients[id].RemoveCharacterFromControl(slot);
        }

        public void ClientReadyToEnterGame(int id)
        {
            connectedClients[id].IsReadyToEnterGame = true;
            BroadcastSyncClient(id);
        }

        private bool AllPlayingClientsReady()
        {
            if (connectedClients.Count == 0)
                return false;

            foreach (ClientInfo c in connectedClients.Values)
                if (!c.IsReadyToEnterGame && !c.IsObserver)
                    return false;
            return true;
        }

        private string[] ChosenCharacters()
        {
            string[] chosenChars = new string[4];
            foreach (ClientInfo c in connectedClients.Values)
                if (!c.IsObserver)
                    for (int i = 0; i < 4; i++)
                        if (c.ControllingCharacters[i] != null)
                            chosenChars[i] = c.ControllingCharacters[i];

            return chosenChars;
        }
        //
        // ------
        //

        //
        // --- COMBAT STATE MANAGEMENT ---
        //

        public void FinishPlayerTurnInput(int clientID)
        {
            ClientInfo clientInfo;
            connectedClients.TryGetValue(clientID, out clientInfo);

            if (clientInfo != null)
            {
                clientInfo.FinishedTurnInput = true;
                BroadcastSyncClient(clientID);
            }
        }

        public void ClearPlayerTurnInput()
        {
            foreach (ClientInfo c in connectedClients.Values)
            {
                if (c.FinishedTurnInput)
                {
                    c.FinishedTurnInput = false;
                    BroadcastSyncClient(c.ID);
                }
            }
        }

        public void ClientReadyForNextRound(int clientID)
        {
            ClientInfo clientInfo;
            connectedClients.TryGetValue(clientID, out clientInfo);

            if (clientInfo != null)
            {
                clientInfo.ReadyForNextRound = true;
                BroadcastSyncClient(clientID);
            }
        }

        public void ClearPlayerReady()
        {
            foreach (ClientInfo c in connectedClients.Values)
            {
                if (c.FinishedTurnInput)
                {
                    c.ReadyForNextRound = false;
                    BroadcastSyncClient(c.ID);
                }
            }
        }

        //
        // ------
        //

        public void Update(float deltaTime)
        {
            if (game == null)
                return;

            if (!inGame)
            {
                if (AllPlayingClientsReady())
                {
                    inGame = true;
                    string[] chosenChars = ChosenCharacters();
                    game.SelectCharacters(chosenChars);
                    serverManager.BroadcastBeginGame(chosenChars);
                }
                else
                    return;
            }

            if (firstGameUpdate)
            {
                serverManager.BroadcastSceneUpdate(game.CurrentScene.ZType);

                serverManager.BroadcastPlayerUpdate(game.SerializePlayers());
                serverManager.BroadcastEnemyUpdate(game.SerializeEnemies());

                firstGameUpdate = false;
                return;
            }

            game.GameLoop(deltaTime); // GAME UPDATES

            if (game.SceneTransitioned)
                serverManager.BroadcastSceneUpdate(game.CurrentScene.ZType);

            if (game.IsInCombat)
            {
                if (game.CurrentCombatState == CombatState.PlayersChooseActions)
                {
                    int numPlaying = 0;
                    int numFinishedTurn = 0;
                    foreach (ClientInfo c in connectedClients.Values)
                    {
                        if (!c.IsObserver)
                        {
                            numPlaying++;
                            if (c.FinishedTurnInput)
                                numFinishedTurn++;
                        }
                    }

                    if (numPlaying == numFinishedTurn)
                        game.FinishPlayerTurnInput();
                }

                if (game.CurrentCombatState == CombatState.WaitForNextRound)
                {
                    int numPlaying = 0;
                    int numReady = 0;
                    foreach (ClientInfo c in connectedClients.Values)
                    {
                        if (!c.IsObserver)
                        {
                            numPlaying++;
                            if (c.ReadyForNextRound)
                                numReady++;
                        }
                    }

                    if (numPlaying == numReady)
                        game.DoneWaitingForNextRound();
                }


                if (game.PreviousCombatState != game.CurrentCombatState)
                {
                    // The combat state has changed this frame

                    if (game.CurrentCombatState == CombatState.BeginCombat)
                    {
                        // we are beginning combat, so broadcast the new state with enemy info
                        int[] enemyIds = new int[game.CombatEnemies.Count];

                        for (int i = 0; i < game.CombatEnemies.Count; i++)
                        {
                            Character c = game.CombatEnemies[i];
                            enemyIds[i] = c.ID;
                        }

                        serverManager.BroadcastBeginCombat(enemyIds);
                    }
                    else if (game.CurrentCombatState == CombatState.PickTurnOrder)
                    {
                        ClearPlayerTurnInput();
                        ClearPlayerReady();
                        serverManager.BroadcastEnemyUpdate(game.SerializeEnemies());
                        serverManager.BroadcastPlayerUpdate(game.SerializePlayers());
                    }
                    else
                    {
                        // we are transitioning to a new combat state
                        if (game.CurrentCombatState == CombatState.PlayersChooseActions || game.CurrentCombatState == CombatState.ExecuteTurns || game.CurrentCombatState == CombatState.RoundBegin)
                        {
                            serverManager.BroadcastCombatData(game.GetCombatData());
                            serverManager.BroadcastCombatState((int)game.CurrentCombatState);
                        }
                    }
                }

                if (game.CurrentCombatState == CombatState.BeginCombat)
                {

                    foreach (Character c in game.CombatEnemies)
                    {
                        //TODO selectAction for choosing enemy target (SERVER)
                    }
                }

                if (game.CurrentCombatState == CombatState.NextRound)
                {
                    currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>(); //(SERVER)
                }

                if (game.CurrentCombatState == CombatState.EndCombat)
                {
                    
                }
            }
            else
            {
                // Not in Combat

                serverManager.BroadcastEnemyUpdate(game.SerializeEnemies());
            }

            if (game.gameMessages.Count > 0)
            {
                //TODO: send gameMessages?
                //EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
            }

            if (game.IsInCombat)
            {
                //TODO: process next combat step once all clients have finished animtaions
                //game.ProcessNextCombatStep(); // (SERVER)
            }
        }

        /// <summary>
        ///     Recieves move instructions from the clients, applies them to the game and broadcasts the updated locations
        /// </summary>
        public void MoveCharacter(int xMovement, int yMovement)
        {
            //TODO: things might need to be adjusted due to the camera tilt?!

            float moveMagnitude = 4f;
            

            Vector2 moveVector = new Vector2(xMovement + yMovement, yMovement - xMovement);
            //moveVector = moveVector * new Vector2(1, 2);
            moveVector.Normalize();
            moveVector *= moveMagnitude * Time.deltaTime;

            game.MoveSelectedCharacter(moveVector.x, moveVector.y);

            serverManager.BroadcastPlayerUpdate(game.SerializePlayers());
        }
    }
}