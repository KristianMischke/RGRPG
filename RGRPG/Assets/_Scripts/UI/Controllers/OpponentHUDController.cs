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
    public class OpponentHUDController : MonoBehaviour, ICharacterHUDController
    {
        // Prefabs

        // Scene Object References
        public GameObject healthTextObject;
        public Button selectButton;
        public RectTransform healthBarFillParent;
        public RectTransform healthBarFill;
        public DiceController myDie;
        public Image myImage;
        public Image targetImage;

        Action overrideAction;

        // Data
        Character character;

        public Character Character { get { return character; } }
        public Transform Transform { get { return transform; } }
        public GameObject GameObject { get { return GameObject; } }

        bool firstUpdate = true;

        // Use this for initialization
        void Start()
        {
            selectButton.onClick.AddListener(SelectAction);
            targetImage.gameObject.SetActive(false);
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

            float healthPercentage = Mathf.Min(character.Health / (float)character.MyInfo.Health, 1);
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

            Sprite image = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_COMBAT, character.Type);

            myImage.sprite = image;
        }

        public void SetTarget(bool isTarget)
        {
            SetTarget(isTarget, Color.white);
        }

        public void SetTarget(bool isTarget, Color targetColor)
        {
            targetImage.gameObject.SetActive(isTarget);
            targetImage.color = targetColor;
        }
    }

}
