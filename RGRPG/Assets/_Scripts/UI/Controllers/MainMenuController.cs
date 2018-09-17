using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    public GameObject StartButtonObject;
    public GameObject OptionsButtonObject;
    public GameObject CreditsButtonObject;
    public GameObject QuitButtonObject;

    private Button StartButton;
    private Button OptionsButton;
    private Button CreditsButton;
    private Button QuitButton;
    private DiscordController controller = new DiscordController();


    void Start()
    {

        StartButton = StartButtonObject.GetComponent<Button>();
        OptionsButton = OptionsButtonObject.GetComponent<Button>();
        CreditsButton = CreditsButtonObject.GetComponent<Button>();
        QuitButton = QuitButtonObject.GetComponent<Button>();
        controller.OnApplicationQuit();
        controller.OnEnable();
        controller.InMainMenu();

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
