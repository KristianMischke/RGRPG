using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerSpawner : MonoBehaviour
{

    public enum NetworkManagerType
    {
        NONE = 0,

        PHOTON,
        DISCORD
    }

    // Network object prefabs
    public GameObject singleplayerNetworkManager;
    public GameObject punNetworkManager;
    public GameObject discordNetworkManager;

    public NetworkManagerType spawnType = NetworkManagerType.NONE;

	void Start () {
        GameObject networkManager;
        switch (spawnType)
        {
            case NetworkManagerType.PHOTON:
                networkManager = Instantiate(punNetworkManager);
                break;
            case NetworkManagerType.DISCORD:
                networkManager = Instantiate(discordNetworkManager);
                break;
            default:
                networkManager = Instantiate(singleplayerNetworkManager);
                break;
        }

        DontDestroyOnLoad(networkManager);
	}
}
