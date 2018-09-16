using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    public GameObject BackButtonObject;

    private Button BackButton;

    void Start()
    {
        BackButton = BackButtonObject.GetComponent<Button>();

        BackButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MainMenuScene");
        });
    }

    void Update()
    {

    }
}
