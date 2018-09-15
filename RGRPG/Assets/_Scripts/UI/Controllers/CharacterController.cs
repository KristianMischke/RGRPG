using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RGRPG.Core;

namespace RGRPG.Controllers
{

    public class CharacterController : MonoBehaviour
    {
        // Scene Object References
        public GameObject ArtObject;

        public SpriteRenderer spriteRenderer;

        // Data
        public Character character;
        bool firstUpdate = true;

        // Use this for initialization
        void Start()
        {
            spriteRenderer = ArtObject.GetComponentInChildren<SpriteRenderer>();
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

            transform.position = character.Position;
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