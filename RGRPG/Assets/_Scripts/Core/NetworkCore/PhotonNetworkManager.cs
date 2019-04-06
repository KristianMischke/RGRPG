using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGRPG.Core.NetworkCore
{
    public class PhotonNetworkManager : MonoBehaviour, IGameClientManager, IGameServerManager
    {
        public enum NetworkEvent : byte
        {
            RECIEVE_CLIENT_CONNECT = 0,
            SYNC_CLIENT_DATA,
            CHOOSE_CHARACTER_TO_PLAY,
            SUBMIT_CHARACTER_SELECTION,
            BEGIN_GAME,
            MOVE_CHARACTER,
            CHARACTER_UPDATE,
            ENEMY_UPDATE,
            SCENE_UPDATE,
            BEGIN_COMBAT,
            UPDATE_COMBAT_STATE,
            UPDATE_COMBAT_DATA,
            COMBAT_FINISH_PLAYER_INPUT,
            COMBAT_PLAYER_READY_FOR_NEXT_ROUND
        }

        GameServer server;
        GameClient client;

        public void OnEnable()
        {
            PhotonNetwork.OnEventCall += OnEvent;
        }

        public void OnDisable()
        {
            PhotonNetwork.OnEventCall -= OnEvent;
        }

        void OnJoinedRoom() // TODO: change to be when the room is ready to enter the game
        {
            Debug.Log("Connected to Room");

            //PhotonNetwork.Instantiate("PhotonPlayer", Vector3.zero, Quaternion.identity, 0);

            client = new GameClient(this, false); //TODO: eventually we will have "observer" players which means this code should take in a value for observer

            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("MASTER");
                server = new GameServer(this);
            }
        }

        void Update()
        {
            if(client != null)
                client.Update(Time.deltaTime);

            if (PhotonNetwork.isMasterClient && server != null)
            {
                server.Update(Time.deltaTime);
            }
        }

        public void OnEvent(byte eventCode, object content, int senderId)
        {
            //bool senderIsServer = senderId == PhotonNetwork.masterClient.ID;

            Debug.Log("[" + System.Enum.GetName(typeof(NetworkEvent), eventCode) + "]");

            switch (eventCode)
            {
                case (byte)NetworkEvent.RECIEVE_CLIENT_CONNECT:
                    {
                        object[] data = (object[])content;
                        int clientID = (int)data[0];
                        int playerNumber = (int)data[1];
                        bool isObserver = (bool)data[2];
                        bool isFromServer = (bool)data[3];

                        if (client != null && isFromServer)
                        {
                            if (clientID == client.ClientID)
                                client.MyPlayerNumber = playerNumber;
                            else
                                client.RegisterOtherClient(clientID, playerNumber, isObserver);
                        }

                        if (PhotonNetwork.isMasterClient && !isFromServer)
                            GameServer.instance.RegisterClient(clientID, isObserver);
                    }
                    break;
                case (byte)NetworkEvent.SYNC_CLIENT_DATA:
                    {
                        object[] data = (object[])content;

                        if (client != null)
                        {
                            client.SyncClientData(data);
                        }
                    }
                    break;
                case (byte)NetworkEvent.CHOOSE_CHARACTER_TO_PLAY:
                    {
                        object[] data = (object[])content;
                        string zType = (string)data[1];
                        int slot = (int)data[2];

                        if (PhotonNetwork.isMasterClient)
                            server.ChooseCharacter(senderId, zType, slot);
                    }
                    break;
                case (byte)NetworkEvent.SUBMIT_CHARACTER_SELECTION:
                    {
                        if (PhotonNetwork.isMasterClient)
                            server.ClientReadyToEnterGame(senderId);
                    }
                    break;
                case (byte)NetworkEvent.BEGIN_GAME:
                    {
                        string[] chosenCharacters = (string[])content;
                        SceneManager.LoadScene("GameScene");
                        client.BeginGame(chosenCharacters);
                    }
                    break;
                case (byte)NetworkEvent.MOVE_CHARACTER:
                    {
                        object[] data = (object[])content;
                        int xDir = (int)data[0];
                        int yDir = (int)data[1];
                        if (PhotonNetwork.isMasterClient)
                            server.MoveCharacter(xDir, yDir);
                    }
                    break;
                case (byte)NetworkEvent.CHARACTER_UPDATE:
                    {
                        object[] data = (object[])content;
                        client.CharacterUpdate(data);
                    }
                    break;
                case (byte)NetworkEvent.ENEMY_UPDATE:
                    {
                        object[] data = (object[])content;
                        client.EnemyUpdate(data);
                    }
                    break;
                case (byte)NetworkEvent.SCENE_UPDATE:
                    {
                        string sceneZType = (string)content;
                        client.SceneUpdate(sceneZType);
                    }
                    break;
                case (byte)NetworkEvent.BEGIN_COMBAT:
                    {
                        int[] enemyIds = (int[])content;
                        client.BeginCombat(enemyIds);
                    }
                    break;
                case (byte)NetworkEvent.UPDATE_COMBAT_STATE:
                    {
                        int combatState = (int)content;
                        client.UpdateCombatState(combatState);
                    }
                    break;
                case (byte)NetworkEvent.COMBAT_FINISH_PLAYER_INPUT:
                    {
                        if (PhotonNetwork.isMasterClient)
                            server.FinishPlayerTurnInput(senderId);
                    }
                    break;
                case (byte)NetworkEvent.UPDATE_COMBAT_DATA:
                    {
                        object[] data = (object[])content;
                        client.UpdateCombatData(data);
                    }
                    break;
                case (byte)NetworkEvent.COMBAT_PLAYER_READY_FOR_NEXT_ROUND:
                    {
                        server.ClientReadyForNextRound(senderId);
                    }
                    break;
            }
        }



        //---------------------------From Client---------------------------
        public void RequestClientID(bool isObserver)
        {
            client.ClientID = PhotonNetwork.player.ID;

            object[] content = new object[] { client.ClientID, -1, isObserver, false };
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.RECIEVE_CLIENT_CONNECT, content, reliable, raiseEventOptions);
        }

        public void ChooseCharacterToPlay(string zType, int slot)
        {
            object[] content = new object[] { zType, slot };
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.CHOOSE_CHARACTER_TO_PLAY, content, reliable, raiseEventOptions);
        }

        public void SubmitCharacterSelection()
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.SUBMIT_CHARACTER_SELECTION, null, reliable, raiseEventOptions);
        }

        public void MoveCharacter(int xDirection, int yDirection)
        {
            object[] content = new object[] { xDirection, yDirection };
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.MOVE_CHARACTER, content, reliable, raiseEventOptions);
        }

        public void CombatFinishPlayerTurnInput()
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.COMBAT_FINISH_PLAYER_INPUT, null, reliable, raiseEventOptions);
        }

        public void CombatWaitingForNextRound()
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.COMBAT_PLAYER_READY_FOR_NEXT_ROUND, null, reliable, raiseEventOptions);
        }

        //---------------------------From Server---------------------------
        public void BroadcastClientConnect(int id, int playerNumber, bool isObserver)
        {
            object[] content = new object[] { id, playerNumber, isObserver, true };
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.RECIEVE_CLIENT_CONNECT, content, reliable, raiseEventOptions);
        }

        public void SyncClientInfo(object[] data)
        {
            object[] content = data;
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.SYNC_CLIENT_DATA, content, reliable, raiseEventOptions);
        }

        public void BroadcastBeginGame(string[] chosenCharacters)
        {
            object[] content = chosenCharacters;
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.BEGIN_GAME, content, reliable, raiseEventOptions);
        }

        public void BroadcastPlayerUpdate(object[] data)
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.CHARACTER_UPDATE, data, reliable, raiseEventOptions);
        }

        public void BroadcastEnemyUpdate(object[] data)
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.ENEMY_UPDATE, data, reliable, raiseEventOptions);
        }

        public void BroadcastSceneUpdate(string sceneID)
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.SCENE_UPDATE, sceneID, reliable, raiseEventOptions);
        }

        // combat messages
        public void BroadcastBeginCombat(int[] enemyIDs)
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.BEGIN_COMBAT, enemyIDs, reliable, raiseEventOptions);
        }
        public void BroadcastCombatState(int combatState)
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.UPDATE_COMBAT_STATE, combatState, reliable, raiseEventOptions);
        }
        public void BroadcastCombatData(object[] data)
        {
            bool reliable = true;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)NetworkEvent.UPDATE_COMBAT_DATA, data, reliable, raiseEventOptions);
        }

    }
}