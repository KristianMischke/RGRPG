using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RGRPG.Core;
using UnityEngine.EventSystems;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Controls a Character Action UI button and records actions to the <see cref="GameController"/> (TODO: might change to GameClient, not controller in future)
    /// </summary>
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
            actionText.text = action.GetDisplayText(); //TODO: we may want to split the attack amount from the button
        }

        // Update is called once per frame
        void Update()
        {
            if (action == null)
                return;

            actionButton.interactable = character.IsAlive() && GameController.instance.IsInCombat;
        }

        /// <summary>
        ///     Initialize the action value and the character that owns the action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="character"></param>
        public void Init(ICharacterAction action, Character character)
        {
            this.action = action;
            this.character = character;
        }

        /// <summary>
        ///     Records the action with the <see cref="GameController"/> (TODO: will probably change to GameClient in future)
        /// </summary>
        public void ChooseAction()
        {
            GameController.instance.BeginRecordingAction(action, character);

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}