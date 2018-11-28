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
        private Vector2 prevPosition;
        private float pDx;
        private float pDy;
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

            if (firstUpdate || prevPosition != character.Position)
            {
                SetSprite();
                firstUpdate = false;
            }

            transform.localPosition = character.Position;

            Widget.SetActive(GameController.instance.IsInCombat);

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            float healthPercentage = Mathf.Min(character.Health / 100f, 1);
            healthBarFill.sizeDelta = new Vector2(healthPercentage * healthBarFillParent.sizeDelta.x, healthBarFill.sizeDelta.y);

            prevPosition = character.Position;
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

            SpriteManager.AssetType direction;

            Vector2 worldMoveVector = character.Position - prevPosition;
            //Vector2 moveVector = new Vector2(xMovement + yMovement, yMovement - xMovement);
            //change in screen coordinates
            float dx = worldMoveVector.x - worldMoveVector.y;
            float dy = worldMoveVector.y + worldMoveVector.x;
            if (dx == 0)
                dx = pDx;
            if (dy == 0)
                dy = pDy;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                // moving left/right
                if(dx < 0)
                    direction = SpriteManager.AssetType.CHARACTER_WORLD_LEFT;
                else
                    direction = SpriteManager.AssetType.CHARACTER_WORLD_RIGHT;
            }
            else
            {
                // moving up/down
                if (dy > 0)
                    direction = SpriteManager.AssetType.CHARACTER_WORLD_UP;
                else
                    direction = SpriteManager.AssetType.CHARACTER_WORLD_DOWN;
            }
            pDx = dx;
            pDy = dy;

            Sprite image = SpriteManager.getSprite(direction, character.Type);
       
            

            spriteRenderer.sprite = image;
        }

    }

}