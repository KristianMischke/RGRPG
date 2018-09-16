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
        public GameObject healthBarFillParentObject;
        public GameObject healthBarFillObject;

        RectTransform healthBarFillParent;
        RectTransform healthBarFill;

        // Data
        public Character character;

        // Use this for initialization
        void Start()
        {
            selectButtonObject.GetComponent<Button>().onClick.AddListener(SelectAction);

            healthBarFillParent = healthBarFillParentObject.GetComponent<RectTransform>();
            healthBarFill = healthBarFillObject.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (character == null)
                return;

            TextMeshProUGUI nameText = nameTextObject.GetComponent<TextMeshProUGUI>();
            nameText.text = character.Name;
            nameText.color = character.IsAlive() ? new Color(1, 1, 1) : new Color(1, 0, 0);

            TextMeshProUGUI healthText = healthTextObject.GetComponent<TextMeshProUGUI>();
            healthText.text = "HP " + character.Health.ToString();

            float healthPercentage = Mathf.Min(character.Health / 100f, 1);
            //Debug.Log(healthBarFill == null);
            //Debug.Log(healthBarFillParent == null);

            healthBarFill.sizeDelta = new Vector2(healthPercentage * healthBarFillParent.sizeDelta.x, healthBarFill.sizeDelta.y);


        }

        public void SelectAction() {
            GameController.instance.SelectCharacter(character);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void Init(Character character)
        {
            this.character = character;

            foreach (ICharacterAction action in character.Actions)
            {
                GameObject actionObject = Instantiate(actionView);
                actionObject.transform.SetParent(actionList.transform);

                CharacterActionController actionController = actionObject.GetComponent<CharacterActionController>();
                actionController.Init(action, character);
            }
        }
    }

}