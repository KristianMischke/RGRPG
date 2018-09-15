using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RGRPG.Core;

namespace RGRPG.UIControllers
{
    public class CharacterActionController : MonoBehaviour
    {
        // Scene Object References
        public GameObject actionButtonObject;
        public GameObject actionTextObject;

        // Data
        public ICharacterAction action;

        // Use this for initialization
        void Start()
        {
            Button actionButton = actionButtonObject.GetComponent<Button>();
            Text actionText = actionTextObject.GetComponent<Text>();
            actionText.text = action.GetName() + (action.HasAmount() ? action.GetAmount().ToString() : ""); //TODO: we may want to split the attack amount from the button
        }

        // Update is called once per frame
        void Update()
        {
            if (action == null)
                return;
        }
    }
}