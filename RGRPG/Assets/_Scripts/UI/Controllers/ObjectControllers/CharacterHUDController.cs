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
    public interface ICharacterHUDController
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        Character Character { get; }
        void SelectAction();
        void Init(Character character, Action selectAction);
        void SetTarget(bool isTarget);
        void SetTarget(bool isTarget, Color targetColor);
    }

    /// <summary>
    ///     Controls the HUD (Head Up Display) for a character
    /// </summary>
    public class CharacterHUDController : MonoBehaviour, ICharacterHUDController
    {
        // Prefabs
        public GameObject actionView;
   
        // Scene Object References
        public GameObject nameTextObject;
        public GameObject healthTextObject;
        public GameObject manaTextObject;
        public GameObject actionList;
        public Button selectButton;
        public RectTransform healthBarFillParent;
        public RectTransform healthBarFill;
        public RectTransform manaBarFillParent;
        public RectTransform manaBarFill;
        public DiceController myDie;
        public Image targetImage;

        // Data
        Character character;

        public Character Character { get { return character; } }
        public Transform Transform { get { return transform; } }
        public GameObject GameObject { get { return GameObject; } }

        Action overrideAction = null;

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

            TextMeshProUGUI nameText = nameTextObject.GetComponent<TextMeshProUGUI>();
            nameText.text = character.Name;
            nameText.color = character.IsAlive() ? new Color(1, 1, 1) : new Color(1, 0, 0);

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            TextMeshProUGUI manaText = manaTextObject.GetComponent<TextMeshProUGUI>();
            manaText.text = "MN " + character.Mana.ToString();

            float healthPercentage = Mathf.Min(character.Health / (float)character.MyInfo.Health, 1);
            healthBarFill.sizeDelta = new Vector2(healthPercentage * healthBarFillParent.sizeDelta.x, healthBarFill.sizeDelta.y);

            float manaPercentage = Mathf.Min(character.Mana / (float)character.MyInfo.Magic, 1);
            manaBarFill.sizeDelta = new Vector2(manaPercentage * manaBarFillParent.sizeDelta.x, manaBarFill.sizeDelta.y);

            if (GameController.instance != null)
            {
                myDie.SetNumber(GameController.instance.GetDiceRoll(character));
                myDie.gameObject.SetActive(GameController.instance.GetDiceRoll(character) != -1);
            }
        }

        /// <summary>
        ///     Allows the user to select a character to controll TODO: this will change with the new movement system
        /// </summary>
        public void SelectAction() {
            if(overrideAction != null)
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

            foreach (ICharacterAction action in character.Actions)
            {
                GameObject actionObject = Instantiate(actionView);
                actionObject.transform.SetParent(actionList.transform);

                CharacterActionController actionController = actionObject.GetComponent<CharacterActionController>();
                actionController.Init(action, character);
            }
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
