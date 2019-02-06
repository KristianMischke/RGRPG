using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RGRPG.Controllers
{
    public class WinScreenController : MonoBehaviour
    {

        public Button mainMenuButton;

        // Use this for initialization
        void Start()
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenuScene");
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}