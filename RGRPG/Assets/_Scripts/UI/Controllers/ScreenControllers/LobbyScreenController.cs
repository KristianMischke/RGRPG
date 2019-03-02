using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyScreenController : MonoBehaviour
{
    // Scene Objects
    public Button StartButton;
    public Button BackButton;

    // Prefabs
    public GameObject DiscordControllerObject;

    private void OnEnable()
    {
        if (DiscordController.Instance == null)
        {
            GameObject controllerObject = Instantiate(DiscordControllerObject);
            DontDestroyOnLoad(controllerObject);
        }
    }

    void Start()
    {
        //DiscordController.Instance.InMainMenu(); //TODO: In Lobby or In Room depending

        StartButton.onClick.AddListener(() =>
        {
            NetworkManagerSpawner.instance.SpawnNetowrkManager(NetworkManagerSpawner.NetworkManagerType.PHOTON);
            SceneManager.LoadScene("CharacterScene");
        });

        BackButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });
    }
}
