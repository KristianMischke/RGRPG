using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGRPG.Core.NetworkCore
{
    public class PhotonLobbyManager : MonoBehaviour//, IGameLobbyManager
    {

        void Start()
        {
            PhotonNetwork.ConnectUsingSettings(GameGlobals.VERSION_STRING);
        }

        void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");

            //GameLobby class that handles matchmaking
            //TODO: view rooms list and creating private rooms
            /*RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.JoinOrCreateRoom(nameEveryFriendKnows, roomOptions, TypedLobby.Default);*/

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.JoinOrCreateRoom("Test Game", roomOptions, TypedLobby.Default);
        }

        void OnJoinedRoom()
        {
            Debug.Log("Connected to Room");

            //TODO: room options should be able to happen when we get here
        }
    }
}