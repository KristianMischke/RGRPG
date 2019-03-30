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

        //---From Client---
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

        //---From Server---
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

        // combat messages
        public void BroadcastBeginCombat(int[] enemyIDs)
        {
            client.BeginCombat(enemyIDs);
        }
        public void BroadcastCombatState(int combatState)
        {
            client.UpdateCombatState(combatState);
        }

    }
}