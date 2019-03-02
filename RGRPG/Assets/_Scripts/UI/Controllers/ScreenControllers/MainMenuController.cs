using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    // Scene Objects
    public Button StartButton;
    public Button MultiplayerButton;
    public Button OptionsButton;
    public Button CreditsButton;
    public Button QuitButton;

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
        DiscordController.Instance.InMainMenu();

        StartButton.onClick.AddListener(() =>
        {
            NetworkManagerSpawner.instance.SpawnNetowrkManager(NetworkManagerSpawner.NetworkManagerType.NONE);
            SceneManager.LoadScene("CharacterScene");
        });

        MultiplayerButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("NetworkLobbyScene");
        });

        OptionsButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("OptionsScene");
        });

        CreditsButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("CreditsScene");
        });

        QuitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    void Update()
    {

    }
}
