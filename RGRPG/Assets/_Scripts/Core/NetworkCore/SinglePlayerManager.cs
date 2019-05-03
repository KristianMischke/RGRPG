using RGRPG.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RGRPG.Core.NetworkCore
{
    public class SinglePlayerManager : MonoBehaviour, IGameClientManager, IGameServerManager
    {
        const int SINGLE_PLAYER_CLIENT_ID = 0;

        GameServer server;
        GameClient client;

        
        void Start()
        {
            server = new GameServer(this);
            client = new GameClient(this, false);
        }

        void Update()
        {
            server.Update(Time.deltaTime);
            client.Update(Time.deltaTime);
        }

        //
        // --- FROM CLIENT ---
        //
        public void RequestClientID(bool isObserver)
        {
            client.ClientID = SINGLE_PLAYER_CLIENT_ID;
            server.RegisterClient(client.ClientID, false);
        }

        public void ChooseCharacterToPlay(string zType, int slot)
        {
            server.ChooseCharacter(client.ClientID, zType, slot);
        }
        public void SubmitCharacterSelection()
        {
            server.ClientReadyToEnterGame(client.ClientID);
        }

        public void MoveCharacter(int xDirection, int yDirection)
        {
            server.MoveCharacter(xDirection, yDirection);
        }

        // combat messages
        public void CombatWaitingForNextRound()
        {
            server.ClientReadyForNextRound(client.ClientID);
        }

        public void CombatSendActions(object[] data)
        {
            server.ReceivePlayerActions(client.ClientID, data);
        }

        //
        // ------
        //

        //
        // --- FROM SERVER ---
        //
        public void BroadcastClientConnect(int id, int playerNumber, bool isObserver)
        {
            client.MyPlayerNumber = playerNumber;
        }
        public void SyncClientInfo(object[] data)
        {
            client.SyncClientData(data);
        }

        public void BroadcastBeginGame(string[] chosenCharacters)
        {
            SceneManager.LoadScene("GameScene");
            client.BeginGame(chosenCharacters);
        }
        public void BroadcastPlayerUpdate(object[] data)
        {
            client.CharacterUpdate(data);
        }
        public void BroadcastEnemyUpdate(object[] data)
        {
            client.EnemyUpdate(data);
        }
        public void BroadcastSceneUpdate(string sceneID)
        {
            client.SceneUpdate(sceneID);
        }
        public void BroadcastDeleteCharacter(int characterID)
        {
            client.DeleteCharacter(characterID);
        }
        public void BroadcastGameState(int gameState)
        {
            client.UpdateGameState(gameState);
        }

        // combat messages
        public void BroadcastBeginCombat(int[] enemyIDs)
        {
            client.BeginCombat(enemyIDs);
        }
        public void BroadcastCombatState(int combatState)
        {
            client.UpdateCombatState(combatState);
        }
        public void BroadcastCombatData(object[] data)
        {
            client.UpdateCombatData(data);
        }
        public void BroadcastEndCombat()
        {
            client.EndCombat();
        }
        //
        // ------
        //
    }
}