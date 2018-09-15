using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.UIControllers
{

    public class CharacterController : MonoBehaviour
    {

        // Data
        Character me;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetCharacter(Character character) {
            me = character;
        }
    }

}