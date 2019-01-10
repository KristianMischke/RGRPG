using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core.NetworkCore
{
    public class PhotonPlayer : Photon.MonoBehaviour, IGameClientManager, IGameServerManager
    {

        GameServer server;
        GameClient client;

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (!photonView.isMine)
                return;

            Debug.Log("SELF");
            client = new GameClient(this);

            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("MASTER");
                server = new GameServer(this);
            }
        }

        void Update()
        {
            if (!photonView.isMine)
                return;

            client.Update(Time.deltaTime);

            if (PhotonNetwork.isMasterClient && server != null)
            {
                server.Update(Time.deltaTime);
            }
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            /*if (stream.isWriting)
                stream.SendNext(rigidbody.position);
            else
                rigidbody.position = (Vector3)stream.ReceiveNext();*/
        }

        [PunRPC]
        void ClientConnect(int clientID)
        {
            if (!photonView.isMine)
                return;

            //if(photonView.isMine)
                //client.RegisterClient(clientID) ???? need way to serialize character selection screen between client and server (as this doesn't require the Game to have been created yet)

             if (PhotonNetwork.isMasterClient && server != null)
                server.RegisterClient(clientID);
        }



        //---From Client---
        public void RequestClientID()
        {
            if (!photonView.isMine)
                return;

            client.ClientID = photonView.ownerId;
            photonView.RPC("ClientConnect", PhotonTargets.AllBuffered, client.ClientID);
        }



        //---From Server---
    }
}