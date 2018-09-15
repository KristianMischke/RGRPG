using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RGRPG.Core;
using TMPro;

namespace RGRPG.Controllers
{

    public class CharacterHUDController : MonoBehaviour
    {
        // Prefabs
        public GameObject actionView;

        // Scene Object References
        public GameObject nameTextObject;
        public GameObject healthTextObject;
        public GameObject actionList;
        public GameObject selectButtonObject;

        // Data
        public Character character;

        public delegate void SelectActionDelegate();
        SelectActionDelegate mySelectAction;

        // Use this for initialization
        void Start()
        {
            foreach (ICharacterAction action in character.Actions) {
                GameObject actionObject = Instantiate(actionView);
                actionObject.transform.SetParent(actionList.transform);

                CharacterActionController actionController = actionObject.GetComponent<CharacterActionController>();
                actionController.action = action;
            }

            selectButtonObject.GetComponent<Button>().onClick.AddListener(SelectAction);
        }

        // Update is called once per frame
        void Update()
        {
            if (character == null)
                return;

            TextMeshProUGUI nameText = nameTextObject.GetComponent<TextMeshProUGUI>();
            nameText.text = character.Name;

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();
        }

        public void SetSelectAction(SelectActionDelegate actionEvent)
        {
            mySelectAction = actionEvent;
        }

        public void SelectAction() {
            mySelectAction();
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

}