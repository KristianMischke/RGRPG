using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RGRPG.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RGRPG.Controllers
{


    public class CharacterSelectController : MonoBehaviour
    {
        public GameObject CharPrefab;

        public List<InfoCharacter> Players = new List<InfoCharacter>();
        public List<CharacterPortraitController> CharPortraits = new List<CharacterPortraitController>();

        private InfoCharacter[] PlayerSelected = new InfoCharacter[4];
        private int CurrentUser = 0;

        public Button[] ButtonArray = new Button[4];
        public Button SubmitButton;



        // Use this for initialization
        void Start()
        {

            SelectButton(0);
            LoadCharPortraits();

            for (int i = 0; i < ButtonArray.Length; i++)
            {
                int j = i;

                ButtonArray[j].onClick.AddListener(() =>
                {
                    SelectButton(j);

                    //Debug.Log(j);

                    CurrentUser = j;

                });
            }

            SubmitButton.onClick.AddListener(() =>
            {
                bool isValid = true;
                for (int i = 0; i < PlayerSelected.Length; i++)
                {
                    if (PlayerSelected[i] == null)
                    {
                        isValid = false;

                    }
                }
                if (isValid)
                {
                    GameController.instance.SelectCharacters(PlayerSelected);
                    SceneManager.LoadScene("GameScene");
                }
            });

        }

        // Update is called once per frame
        void Update()
        {


        }

        public void LoadCharPortraits()
        {

            // load non-enemy characters from the game infos
            GameController.instance.Infos.GetAll<InfoCharacter>().FindAll(x => !x.IsEnemy).ToList()
            .ForEach(x => {
                Players.Add(x);
            });

            for (int i = 0; i < Players.Count; i++)
            {
                InfoCharacter c = Players[i];
                GameObject playerHUDView = Instantiate(CharPrefab);
                playerHUDView.transform.SetParent(this.transform);
               
                CharacterPortraitController portrait = playerHUDView.GetComponent<CharacterPortraitController>();
                CharPortraits.Add(portrait);
                int j = i;

                portrait.Init(c, () => {
                    SelectCharacter(j);
                });

            }

        }

        public void SelectCharacter(int i)
        {
            //Debug.Log(i);
            PlayerSelected[CurrentUser] = Players[i];

            Text buttonText = ButtonArray[CurrentUser].GetComponentInChildren<Text>();
            //Debug.Log(ButtonArray[i] == null);
            buttonText.text = "Player " + (CurrentUser + 1) + ": " + Players[i].Name;


        }

        public void SelectButton(int i)
        {
            for (int j = 0; j < ButtonArray.Length; j++)
            {
                Button b = ButtonArray[j];

                if (j == i)
                    b.GetComponent<Image>().color = Color.yellow;
                else
                    b.GetComponent<Image>().color = Color.white;
            }
        }

    }
}