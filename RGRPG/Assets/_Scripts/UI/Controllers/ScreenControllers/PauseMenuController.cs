using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject ResumeButtonObject;
    public GameObject OptionsButtonObject;
    public GameObject CreditsButtonObject;
    public GameObject QuitButtonObject;

    private Button ResumeButton;
    private Button OptionsButton;
    private Button CreditsButton;
    private Button QuitButton;


    void Start()
    {
        ResumeButton = ResumeButtonObject.GetComponent<Button>();
        OptionsButton = OptionsButtonObject.GetComponent<Button>();
        CreditsButton = CreditsButtonObject.GetComponent<Button>();
        QuitButton = QuitButtonObject.GetComponent<Button>();

        ResumeButton.onClick.AddListener(() => {
            SceneManager.LoadScene("GameScene");
        });
        OptionsButton.onClick.AddListener(() => {
            SceneManager.LoadScene("OptionsScene");
        });
        CreditsButton.onClick.AddListener(() => {
            SceneManager.LoadScene("CreditsScene");
        });
        QuitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    void Update()
    {

    }
}
