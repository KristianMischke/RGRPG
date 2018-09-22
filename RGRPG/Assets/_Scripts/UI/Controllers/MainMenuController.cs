using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    // Scene Objects
    public GameObject StartButtonObject;
    public GameObject OptionsButtonObject;
    public GameObject CreditsButtonObject;
    public GameObject QuitButtonObject;

    private Button StartButton;
    private Button OptionsButton;
    private Button CreditsButton;
    private Button QuitButton;

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

        StartButton = StartButtonObject.GetComponent<Button>();
        OptionsButton = OptionsButtonObject.GetComponent<Button>();
        CreditsButton = CreditsButtonObject.GetComponent<Button>();
        QuitButton = QuitButtonObject.GetComponent<Button>();
        DiscordController.Instance.InMainMenu();

        StartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("GameScene");
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
