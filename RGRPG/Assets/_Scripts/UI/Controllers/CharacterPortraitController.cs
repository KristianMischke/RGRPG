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
    ///     Controls the Selection Portrait for a Character
    /// </summary>
    public class CharacterPortraitController : MonoBehaviour
    {
        // Prefabs
        public GameObject actionView;

        // Scene Object References
        public GameObject nameTextObject;
        public GameObject healthTextObject;
        public GameObject manaTextObject;
        public GameObject actionList;
        public GameObject selectButtonObject;
        public Image portraitImageObject;

        // Data
        public Character character;

        public Action selectAction = null;

        bool firstUpdate = true;

        // Use this for initialization
        void Start()
        {
            selectButtonObject.GetComponent<Button>().onClick.AddListener(SelectAction);
        }

        // Update is called once per frame
        void Update()
        {
            if (character == null)
                return;

            if (firstUpdate)
            {
                SetSprite();
                firstUpdate = false;
            }

            TextMeshProUGUI nameText = nameTextObject.GetComponent<TextMeshProUGUI>();
            nameText.text = character.Name;
            nameText.color = character.IsAlive() ? new Color(1, 1, 1) : new Color(1, 0, 0);

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            TextMeshProUGUI manaText = manaTextObject.GetComponent<TextMeshProUGUI>();
            manaText.text = "MN " + character.Mana.ToString();
        }

        /// <summary>
        ///     Allows the user to select a character to controll TODO: this will change with the new movement system
        /// </summary>
        public void SelectAction()
        {
            selectAction.Invoke();

            EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        ///     Sets up the <see cref="CharacterActionController"/>s for the character's HUD
        /// </summary>
        /// <param name="character"></param>
        public void Init(Character character, Action selectAction)
        {
            this.character = character;
            this.selectAction = selectAction;

            foreach (ICharacterAction action in character.Actions)
            {
                GameObject actionObject = Instantiate(actionView);
                actionObject.transform.SetParent(actionList.transform);

                CharacterActionController actionController = actionObject.GetComponent<CharacterActionController>();
                actionController.Init(action, character);
            }
        }

        /// <summary>
        ///     Load the art for the character
        /// </summary>
        void SetSprite()
        {
            if (character == null)
                return;

            Sprite image = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_PORTRAIT, System.Enum.GetName(typeof(CharacterType), character.Type));


            //TODO: apply an offset to the portrait image based on which character it is (when implementing GameInfos)


            portraitImageObject.sprite = image;
            //spriteRenderer.transform.localScale = new Vector2(1 / image.bounds.size.x, 1 / image.bounds.size.y);
        }
    }

}
