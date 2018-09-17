using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    public GameObject BackButtonObject;
    public GameObject DiscordButtonObject;

    private Button BackButton;
    private Button DiscordButton;

    void Start()
    {
        BackButton = BackButtonObject.GetComponent<Button>();
        DiscordButton = DiscordButtonObject.GetComponent<Button>();

        BackButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MainMenuScene");
        });

        DiscordButton.onClick.AddListener(() => {
            SceneManager.LoadScene("");
        });
    }

    void Update()
    {

    }
}
