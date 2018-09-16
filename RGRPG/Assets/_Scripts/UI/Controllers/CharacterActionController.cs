using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    public class CharacterActionController : MonoBehaviour
    {
        // Scene Object References
        public GameObject actionButtonObject;
        public GameObject actionTextObject;

        public Button actionButton;

        // Data
        private Character character;
        private ICharacterAction action;

        // Use this for initialization
        void Start()
        {
            actionButton = actionButtonObject.GetComponent<Button>();
            actionButton.onClick.AddListener(ChooseAction);

            Text actionText = actionTextObject.GetComponent<Text>();
            actionText.text = action.GetName() + (action.HasAmount() ? action.GetAmount().ToString() : ""); //TODO: we may want to split the attack amount from the button
        }

        // Update is called once per frame
        void Update()
        {
            if (action == null)
                return;

            actionButton.interactable = character.IsAlive() && GameController.instance.IsInCombat();
        }

        public void Init(ICharacterAction action, Character character)
        {
            this.action = action;
            this.character = character;
        }

        public void ChooseAction()
        {
            if (action.GetType() == typeof(DefendAction))
                GameController.instance.RecordAction(action, character, character);
            else if (action.GetType() == typeof(HealAction))
                GameController.instance.RecordAction(action, character, character);
            else
                GameController.instance.RecordAction(action, character, GameController.instance.GetCombatEnemy());

        }
    }
}