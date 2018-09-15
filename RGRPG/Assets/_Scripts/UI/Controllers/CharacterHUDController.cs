using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;
using TMPro;

namespace RGRPG.UIControllers
{

    public class CharacterHUDController : MonoBehaviour
    {
        // Prefab References
        public GameObject nameText;
        public GameObject healthText;

        // Data
        Character me;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            nameText.GetComponent<TextMeshProUGUI>().text = me.Name;
            healthText.GetComponent<TextMeshProUGUI>().text = "HP " + me.Health.ToString();
        }

        public void SetCharacter(Character character)
        {
            me = character;
        }
    }

}