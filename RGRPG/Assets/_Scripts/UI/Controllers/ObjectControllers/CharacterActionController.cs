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
        public Text actionText;
        public Button actionButton;

        // Data
        private int characterID = -1;
        private int actionIndex = -1;

        // Use this for initialization
        void Start()
        {
            actionButton.onClick.AddListener(ChooseAction);

            actionText.text = GameClient.instance.GetCharacterAction(characterID, actionIndex).GetDisplayText();
        }

        // Update is called once per frame
        void Update()
        {
            if(actionText != null)
                actionText.color = GameClient.instance.IsActionCurrentSelection(characterID, actionIndex) ? Color.yellow : Color.white;
            if(actionButton != null)
                actionButton.interactable = GameClient.instance.GetCharacter(characterID).IsAlive() && GameClient.instance.IsInCombat;
        }

        /// <summary>
        ///     Initialize the action value and the character that owns the action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="character"></param>
        public void Init(int actionID, int characterID)
        {
            this.actionIndex = actionID;
            this.characterID = characterID;
        }

        /// <summary>
        ///     Records the action with the <see cref="GameController"/> (TODO: will probably change to GameClient in future)
        /// </summary>
        public void ChooseAction()
        {
            GameClient.instance.BeginRecordingAction(actionIndex, characterID);

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}