using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RGRPG.Core.Generics;
using RGRPG.Core.NetworkCore;
using Debug = UnityEngine.Debug;

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

        /// <summary>
        ///     sourceCharacter -> Pair(characterActionIndex, List(actionTargets))
        /// </summary>
        private Dictionary<int, Pair<int, List<int>>> currentCharacterActions = new Dictionary<int, Pair<int, List<int>>>();
        private bool actionsDirty = false;
        private int currentActionSource = -1;


        public int ClientID { get { return myClientInfo.ID; } set { myClientInfo.ID = value; } }
        public int MyPlayerNumber { get { return myClientInfo.PlayerNumber; } set { myClientInfo.PlayerNumber = value; } }
        public int NumPlayers { get { return otherClients.Count + 1; } }

        // would be cool if the below reference types could be non-mutated
        public bool IsGameNull { get { return game == null; } }
        public GameInfos Infos { get { return game.Infos; } }
        public Character SelectedCharacter { get { return game.SelectedCharacter; } }
        public List<Character> Players { get { return game.Players; } }
        public List<Enemy> Enemies { get { return game.Enemies; } }
        public List<Character> CombatEnemies { get { return game.CombatEnemies; } }
        public List<Character> TurnOrder { get { return game.TurnOrder; } }
        public bool IsInCombat { get { return game.IsInCombat; } }
        public bool IsIndoors { get { return game.CurrentScene.IsIndoors; } }
        public WorldScene CurrentScene { get { return game.CurrentScene; } }
        public string CurrentSceneType { get { return game.CurrentScene.ZType; } }
        public GameState CurrentGameState { get { return game.CurrentGameState; } }
        public CombatState CurrentCombatState { get { return game.CurrentCombatState; } }
        public bool SceneTransitioned { get { return game.SceneTransitioned; } }
        public Queue<string> GameMessages { get { return game.gameMessages; } }

        public bool MoreCombatActions { get { return game.gameCombatActionQueue.Count > 0; } }
        public PairStruct<Character, ICharacterAction> GetNextCombatAction { get { return game.gameCombatActionQueue.Dequeue(); } }

        public bool AreActionsDirty { get { bool temp = actionsDirty; actionsDirty = false; return temp; } }
        public bool IsMakingAction { get { return currentActionSource > 0; } }

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

        public void UpdateGameState(int gameState)
        {
            overrideGameState = (GameState)gameState;
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

        public void DeleteCharacter(int characterID)
        {
            game.DeleteCharacter(characterID);
        }

        public void MoveCharacter(int xDirection, int yDirection)
        {
            clientManager.MoveCharacter(xDirection, yDirection);
        }

        public Character GetCharacter(int characterID)
        {
            return game.GetCharacter(characterID);
        }

        public ICharacterAction GetCharacterAction(int characterID, int actionIndex)
        {
            return game.GetCharacterAction(characterID, actionIndex);
        }

        //
        // --- COMBAT SYNC ---
        //

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
            if (currentActionSource == -1)
                SendActionsToServer();
            else
                FinishRecordingAction(currentActionSource);
        }

        public void SelectCharacter(int characterID)
        {
            if (currentActionSource == -1)
            {
                game.SelectCharacter(characterID);
            }
            else
            {
                ToggleActionTarget(currentActionSource, characterID);
            }
        }

        public bool IsCharacterCurrentTarget(int characterID)
        {
            if (currentActionSource == -1)
                return false;

            if (!currentCharacterActions.ContainsKey(currentActionSource))
                return false;

            return currentCharacterActions[currentActionSource].second.FindIndex(id => id == characterID) != -1;
        }

        public bool IsActionCurrentSelection(int characterID, int actionIndex)
        {
            if (currentActionSource == -1 || currentActionSource != characterID)
                return false;

            if (currentCharacterActions.ContainsKey(characterID) && currentCharacterActions[characterID].first == actionIndex)
                return true;

            return false;
        }

        public void BeginRecordingAction(int actionIndex, int sourceID)
        {
            if (currentCharacterActions.ContainsKey(sourceID))
                currentCharacterActions.Remove(sourceID);
            currentActionSource = sourceID;
            currentCharacterActions.Add(sourceID, new Pair<int, List<int>>(actionIndex, new List<int>()));

            InfoAction actionInfo = game.GetCharacterAction(sourceID, actionIndex).MyInfo;
            if (actionInfo == null)
                return;

            switch (actionInfo.TargetType)
            {
                case "TARGET_NONE":
                    break;
                case "TARGET_SELF":
                    currentCharacterActions[sourceID].second.Add(sourceID);
                    break;
                case "TARGET_TEAM":
                    if (Character.ListContainsID(game.Players, sourceID))
                    {
                        currentCharacterActions[sourceID].second.AddRange(Character.GetIDFromList(game.Players));
                    }
                    else if (Character.ListContainsID(game.CombatEnemies, sourceID))
                    {
                        currentCharacterActions[sourceID].second.AddRange(Character.GetIDFromList(game.CombatEnemies));
                    }
                    break;
                case "TARGET_OTHER_TEAM":
                    if (Character.ListContainsID(game.Players, sourceID))
                    {
                        currentCharacterActions[sourceID].second.AddRange(Character.GetIDFromList(game.CombatEnemies));
                    }
                    else if (Character.ListContainsID(game.CombatEnemies, sourceID))
                    {
                        currentCharacterActions[sourceID].second.AddRange(Character.GetIDFromList(game.Players));
                    }
                    break;
                case "TARGET_FRIEND":

                    break;
                case "TARGET_ENEMY":
                    if (actionInfo.TargetData == game.CombatEnemies.Count)
                        currentCharacterActions[sourceID].second.AddRange(Character.GetIDFromList(game.CombatEnemies));
                    break;
            }

            actionsDirty = true;
        }

        public void ToggleActionTarget(int sourceID, int targetID)
        {
            if (currentCharacterActions.ContainsKey(sourceID))
            {

                InfoAction actionInfo = game.GetCharacterAction(sourceID, currentCharacterActions[sourceID].first).MyInfo;
                switch (actionInfo.TargetType)
                {
                    case "TARGET_NONE":
                    case "TARGET_SELF":
                    case "TARGET_TEAM":
                    case "TARGET_OTHER_TEAM":
                        return;

                    case "TARGET_AMOUNT":
                        if (currentCharacterActions[sourceID].second.Count >= actionInfo.TargetData && !currentCharacterActions[sourceID].second.Contains(targetID))
                            return;
                        break;
                    case "TARGET_FRIEND":
                        if ((currentCharacterActions[sourceID].second.Count >= actionInfo.TargetData && !currentCharacterActions[sourceID].second.Contains(targetID)) || !Character.ListContainsID(game.Players, targetID) || targetID == sourceID)
                            return;
                        break;
                    case "TARGET_ENEMY":
                        if ((currentCharacterActions[sourceID].second.Count >= actionInfo.TargetData && !currentCharacterActions[sourceID].second.Contains(targetID)) || !Character.ListContainsID(game.CombatEnemies, targetID) || targetID == sourceID)
                            return;
                        break;
                }

                if (currentCharacterActions[sourceID].second.Contains(targetID))
                {
                    currentCharacterActions[sourceID].second.Remove(targetID);
                }
                else
                {
                    currentCharacterActions[sourceID].second.Add(targetID);
                }
            }
            else
            {
                Debug.Log("Character #" + sourceID + " did not begin recording an action, and thus cannot toggle the action targets");
            }

            actionsDirty = true;
        }

        public void FinishRecordingAction(int sourceID)
        {
            if (!currentCharacterActions.ContainsKey(sourceID))
                return;

            InfoAction actionInfo = game.GetCharacterAction(sourceID, currentCharacterActions[sourceID].first).MyInfo;
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
                    if (currentCharacterActions[sourceID].second.Count > actionInfo.TargetData || currentCharacterActions[sourceID].second.Count == 0)
                        return;
                    break;
            }

            currentActionSource = -1;

            actionsDirty = true;
        }

        public void SendActionsToServer()
        {
            currentActionSource = -1;

            int numTargets = 0;
            List<int> sources = new List<int>();
            List<Pair<int, List<int>>> validActions = new List<Pair<int, List<int>>>();
            foreach (KeyValuePair<int, Pair<int, List<int>>> kvp in currentCharacterActions)
            {
                if (kvp.Value.second.Count > 0)
                {
                    sources.Add(kvp.Key);
                    validActions.Add(kvp.Value);
                    numTargets += kvp.Value.second.Count;
                }
            }

            //               actionCount + (source + actionIndex + targetCount) + targets
            object[] data = new object[1 + validActions.Count * 3 + numTargets];

            int index = 0;
            data[index] = validActions.Count;
            index++;

            for (int i = 0; i < validActions.Count; i++)
            {
                data[index] = sources[i];
                index++;
                data[index] = validActions[i].first;
                index++;
                data[index] = validActions[i].second.Count;
                index++;

                for(int j = 0; j < validActions[i].second.Count; j++)
                {
                    data[index] = validActions[i].second[j];
                    index++;
                }
            }

            clientManager.CombatSendActions(data);
        }

        public void ProcessNextCombatStep()
        {
            game.ProcessNextCombatStep();
        }

        //
        // ------
        //

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
            }

            game.GameLoop(deltaTime);

            if (game.IsInCombat)
            {
                if (!waitingForRound && game.CurrentCombatState == CombatState.WaitForNextRound && game.gameCombatActionQueue.Count == 0)
                {
                    currentCharacterActions.Clear();
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
        }
    }
}