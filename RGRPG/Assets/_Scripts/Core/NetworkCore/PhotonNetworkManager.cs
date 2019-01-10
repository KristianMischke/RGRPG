using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RGRPG.Core.NetworkCore
{
    public class PhotonNetworkManager : MonoBehaviour
    {

        void Start()
        {
            PhotonNetwork.ConnectUsingSettings("v0.0");
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

            PhotonNetwork.Instantiate("PhotonPlayer", Vector3.zero, Quaternion.identity, 0);
        }
    }
}