using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RGRPG.Core;
using TMPro;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Controls the HUD (Head Up Display) for an opponent
    /// </summary>
    public class OpponentHUDController : MonoBehaviour
    {
        // Prefabs

        // Scene Object References
        public GameObject healthTextObject;
        public GameObject selectButtonObject;
        public GameObject healthBarFillParentObject;
        public GameObject healthBarFillObject;
        public GameObject dieView;
        public Image myImage;

        RectTransform healthBarFillParent;
        RectTransform healthBarFill;

        DiceController myDie;

        Action overrideAction;

        // Data
        Character character;

        bool firstUpdate = true;

        // Use this for initialization
        void Start()
        {
            selectButtonObject.GetComponent<Button>().onClick.AddListener(SelectAction);

            healthBarFillParent = healthBarFillParentObject.GetComponent<RectTransform>();
            healthBarFill = healthBarFillObject.GetComponent<RectTransform>();

            myDie = dieView.GetComponent<DiceController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (character == null)
                return;

            if (firstUpdate)
            {
                SetSprite();
                firstUpdate = true;
            }

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            float healthPercentage = Mathf.Min(character.Health / 100f, 1);
            healthBarFill.sizeDelta = new Vector2(healthPercentage * healthBarFillParent.sizeDelta.x, healthBarFill.sizeDelta.y);

            if (GameController.instance != null)
            {
                myDie.SetNumber(GameController.instance.GetDiceRoll(character));
                myDie.gameObject.SetActive(GameController.instance.GetDiceRoll(character) != -1);
            }
        }

        /// <summary>
        ///     Allows the user to select a character to controll TODO: this will change with the new movement system
        /// </summary>
        public void SelectAction()
        {
            if (overrideAction != null)
            {
                overrideAction.Invoke();
            }
            else
            {
                GameController.instance.SelectCharacter(character);
            }

            EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        ///     Sets up the <see cref="CharacterActionController"/>s for the character's HUD
        /// </summary>
        /// <param name="character"></param>
        public void Init(Character character, Action selectAction = null)
        {
            this.character = character;
            this.overrideAction = selectAction;
        }

        /// <summary>
        ///     Load the art for the character
        /// </summary>
        void SetSprite()
        {
            if (character == null)
                return;

            Sprite image;
            if (GameController.instance.IsInCombat)
            {
                image = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_COMBAT, character.Type);
            }
            else
            {
                image = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_WORLD, character.Type);
            }


            myImage.sprite = image;
        }
    }

}
