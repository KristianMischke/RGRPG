using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RGRPG.Core;
using TMPro;

namespace RGRPG.Controllers
{

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
        bool firstUpdate = true;

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
                return;

            if (firstUpdate)
            {
                LoadCharacterImage();

                firstUpdate = false;
            }

            transform.rotation = Quaternion.identity;
            transform.localPosition = character.Position;
            transform.localScale = new Vector3(1 / transform.parent.localScale.x, 1 / transform.parent.localScale.y, 1 / transform.parent.localScale.z);

            Widget.SetActive(GameController.instance.IsInCombat());

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            float healthPercentage = Mathf.Min(character.Health / 100f, 1);
            healthBarFill.sizeDelta = new Vector2(healthPercentage * healthBarFillParent.sizeDelta.x, healthBarFill.sizeDelta.y);
        }

        public void SetCharacter(Character character)
        {
            this.character = character;
        }

        void LoadCharacterImage()
        {
            if (character == null)
                return;

            Sprite image;
            switch (character.Type)
            {
                case CharacterType.Player:
                    image = Resources.Load<Sprite>("Sprites/baby");
                    break;
                case CharacterType.Enemy:
                    image = Resources.Load<Sprite>("Sprites/troll");
                    break;
                default:
                    image = Resources.Load<Sprite>("Sprites/baby");
                    break;
            }

            spriteRenderer.sprite = image;
            spriteRenderer.transform.localScale = new Vector2(1 / image.bounds.size.x, 1 / image.bounds.size.y);
        }

    }

}