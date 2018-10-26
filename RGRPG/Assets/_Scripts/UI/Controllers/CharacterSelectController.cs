using System.Collections;
using System.Collections.Generic;
using RGRPG.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RGRPG.Controllers
{


    public class CharacterSelectController : MonoBehaviour
    {
        public GameObject CharPrefab;

        public List<Character> Players = new List<Character>();
        public List<GameObject> CharPrefabs = new List<GameObject>();

        private Character[] PlayerSelected = new Character[4];
        private int CurrentUser = 0;


        public Button[] ButtonArray = new Button[4];
        //public Button ButtonOne;
        //public Button ButtonTwo;
        //public Button ButtonThree;
        //public Button ButtonFour;

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


        }

        // Update is called once per frame
        void Update()
        {


        }

        public void LoadCharPortraits()
        {

            //TODO: get list of all characters from system that Kristian is developing
            // for now, just use dummy list
            Players.Add(new Character(CharacterType.Player, "test1", 100, 10, 10, new List<ICharacterAction>()));
            Players.Add(new Character(CharacterType.Player, "test2", 100, 10, 10, new List<ICharacterAction>()));
            Players.Add(new Character(CharacterType.Player, "test3", 100, 10, 10, new List<ICharacterAction>()));
            Players.Add(new Character(CharacterType.Player, "test4", 100, 10, 10, new List<ICharacterAction>()));
            Players.Add(new Character(CharacterType.Player, "test5", 100, 10, 10, new List<ICharacterAction>()));


            for (int i = 0; i < Players.Count; i++)
            {
                Character c = Players[i];
                GameObject playerHUDView = Instantiate(CharPrefab);
                playerHUDView.transform.SetParent(this.transform);
                CharacterHUDController hud = playerHUDView.GetComponent<CharacterHUDController>();
                int j = i;

                hud.Init(c, () => {
                    SelectCharacter(j);
                });

            }

        }

        public void SelectCharacter(int i)
        {
            //Debug.Log(i);
            PlayerSelected[CurrentUser] = Players[i];


        }

        public void SelectButton(int i)
        {
            for (int j = 0; j < ButtonArray.Length; j++)
            {
                Button b = ButtonArray[j];

                if (j == i) {



                    //ColorBlock selectedBlock = new ColorBlock();
                    //selectedBlock.normalColor = new Color(1f, 0.92f, 0.016f, 1f);
                    //selectedBlock.pressedColor = new Color(1f, 0.92f, 0.016f, 1f);
                    //selectedBlock.disabledColor = new Color(1f, 0.92f, 0.016f, 1f);
                    //selectedBlock.highlightedColor = new Color(1f, 0.92f, 0.016f, 1f);
                    //b.colors = selectedBlock;
                    b.GetComponent<Image>().color = Color.yellow;

                } else {



                    //ColorBlock selectedBlock = new ColorBlock();
                    //selectedBlock.normalColor = new Color(1f, 1f, 1f, 1f);
                    //selectedBlock.pressedColor = new Color(1f, 1f, 1f, 1f);
                    //selectedBlock.disabledColor = new Color(1f, 1f, 1f, 1f);
                    //selectedBlock.highlightedColor = new Color(1f, 1f, 1f, 1f);
                    //b.colors = selectedBlock;
                    b.GetComponent<Image>().color = Color.white;

                }

            }
        }

    }
}