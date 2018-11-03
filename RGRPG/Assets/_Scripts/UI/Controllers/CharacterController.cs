using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RGRPG.Core;
using TMPro;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Controls the in-world and in-combat characters
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        // Scene Object References
        public GameObject ArtObject;
        public GameObject Widget;
        public GameObject healthTextObject;
        public GameObject healthBarFillParentObject;
        public GameObject healthBarFillObject;

        RectTransform healthBarFillParent;
        RectTransform healthBarFill;
        SpriteRenderer spriteRenderer;

        // Data
        public Character character;

        private bool firstUpdate = true;

        // Use this for initialization
        void Start()
        {
            spriteRenderer = ArtObject.GetComponentInChildren<SpriteRenderer>();

            healthBarFillParent = healthBarFillParentObject.GetComponent<RectTransform>();
            healthBarFill = healthBarFillObject.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (character == null)
            {
                return;
            }

            if (firstUpdate)
            {
                SetSprite();
                firstUpdate = true;
            }

            transform.localPosition = character.Position;

            Widget.SetActive(GameController.instance.IsInCombat());

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            float healthPercentage = Mathf.Min(character.Health / 100f, 1);
            healthBarFill.sizeDelta = new Vector2(healthPercentage * healthBarFillParent.sizeDelta.x, healthBarFill.sizeDelta.y);
        }

        /// <summary>
        ///     Initialize the character for this controller
        /// </summary>
        /// <param name="character"></param>
        public void SetCharacter(Character character)
        {
            this.character = character;
        }

        /// <summary>
        ///     Load the art for the character
        /// </summary>
        void SetSprite()
        {
            if (character == null)
                return;

            Sprite image;
            if (GameController.instance.IsInCombat())
            {
                image = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_COMBAT, System.Enum.GetName(typeof(CharacterType), character.Type));
            }
            else
            {
                image = SpriteManager.getSprite(SpriteManager.AssetType.CHARACTER_WORLD, System.Enum.GetName(typeof(CharacterType), character.Type));
            }
            

            spriteRenderer.sprite = image;
            spriteRenderer.transform.localScale = new Vector2(1 / image.bounds.size.x, 1 / image.bounds.size.y);

            if (GameController.instance.IsInCombat())
                spriteRenderer.transform.localScale *= 4;
        }

    }

}