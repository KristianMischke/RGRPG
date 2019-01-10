using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RGRPG.Core.NetworkCore
{
    public class SinglePlayerManager : MonoBehaviour, IGameClientManager, IGameServerManager
    {
        GameServer server;
        GameClient client;

        
        void Start()
        {
            server = new GameServer(this);
            client = new GameClient(this);
        }

        public void RequestClientID()
        {
            client.ClientID = 0;
            server.RegisterClient(0);
        }

    }
}