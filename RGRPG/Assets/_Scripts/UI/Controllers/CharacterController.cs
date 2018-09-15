using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.UIControllers
{

    public class CharacterController : MonoBehaviour
    {

        // Data
        public Character character;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (character == null)
                return;

            transform.position = character.Position;

        }

        public void SetCharacter(Character character) {
            this.character = character;
        }
    }

}