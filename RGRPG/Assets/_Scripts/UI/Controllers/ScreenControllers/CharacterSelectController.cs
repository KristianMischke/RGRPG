using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RGRPG.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RGRPG.Controllers
{
    public class CharacterSelectController : MonoBehaviour
    {
        private enum ItemType
        {
            NONE = 0,

            PLAYER_SLOT_BUTTON,
            READY_BUTTON,
            SELECT_CHARACTER
        }

        private const int NUM_PLAYER_SLOTS = 4;

        public GameObject characterPrefab;

        private List<InfoCharacter> allInfoPlayers = new List<InfoCharacter>();
        private List<CharacterPortraitController> characterPortraits = new List<CharacterPortraitController>();

        private InfoCharacter[] playerSelected = new InfoCharacter[NUM_PLAYER_SLOTS];
        private bool[] myPlayerSlots = new bool[NUM_PLAYER_SLOTS];
        private int currentSelectedSlot = 0;

        public Button[] playerSlotButtonArray = new Button[NUM_PLAYER_SLOTS];
        public Button readyButton;

        GameClient Client { get { return GameClient.instance; } }

        void Start()
        {
            ChooseNewSlot(0);
            LoadCharPortraits();

            for (int i = 0; i < playerSlotButtonArray.Length; i++)
            {
                int j = i;

                playerSlotButtonArray[j].onClick.AddListener(() => OnButtonPressed(ItemType.PLAYER_SLOT_BUTTON, j));
            }

            readyButton.onClick.AddListener(() => OnButtonPressed(ItemType.READY_BUTTON));
        }

        void Update()
        {
            for (int i = 0; i < myPlayerSlots.Length; i++)
            {
                myPlayerSlots[i] = GameCharacterSelectHelper.IsValidSlot(Client.MyPlayerNumber, Client.NumPlayers, i);
                playerSlotButtonArray[i].interactable = myPlayerSlots[i];
            }

            if (currentSelectedSlot >= 0 && currentSelectedSlot <= 4 && !myPlayerSlots[currentSelectedSlot])
                currentSelectedSlot = -1;

            foreach (ClientInfo client in Client.AllClients)
            {
                if (!client.IsObserver)
                {
                    for (int i = 0; i < NUM_PLAYER_SLOTS; i++)
                    {
                        if(client.ControllingCharacters[i] != null)
                            UpdateSlotSelection(i, allInfoPlayers.FindIndex(x => x.ZType == client.ControllingCharacters[i]), false);
                    }
                }
            }

            if (Client.MyClientInfo.IsReadyToEnterGame)
            {
                readyButton.interactable = false;
                readyButton.GetComponentInChildren<Text>().text = "Waiting for server...";
            }
            else
            {
                readyButton.interactable = true;
                readyButton.GetComponentInChildren<Text>().text = "Ready";
            }
        }

        public void LoadCharPortraits()
        {
            // load non-enemy characters from the game infos
            Client.Infos.GetAll<InfoCharacter>().FindAll(x => !x.IsEnemy).ToList()
            .ForEach(x => {
                allInfoPlayers.Add(x);
            });

            for (int i = 0; i < allInfoPlayers.Count; i++)
            {
                InfoCharacter c = allInfoPlayers[i];
                GameObject playerHUDView = Instantiate(characterPrefab);
                playerHUDView.transform.SetParent(this.transform);
               
                CharacterPortraitController portrait = playerHUDView.GetComponent<CharacterPortraitController>();
                characterPortraits.Add(portrait);
                int j = i;

                portrait.Init(c, () => OnButtonPressed(ItemType.SELECT_CHARACTER, j));

            }

        }

        private void OnButtonPressed(ItemType itemType, int data = -1)
        {
            switch (itemType)
            {
                case ItemType.PLAYER_SLOT_BUTTON:
                    {
                        if (GameCharacterSelectHelper.IsValidSlot(Client.MyPlayerNumber, Client.NumPlayers, data))
                        {
                            ChooseNewSlot(data);
                            currentSelectedSlot = data;
                        }
                    }
                    break;
                case ItemType.SELECT_CHARACTER:
                    {
                        if(GameCharacterSelectHelper.IsValidSlot(Client.MyPlayerNumber, Client.NumPlayers, currentSelectedSlot))
                            UpdateSlotSelection(currentSelectedSlot, data, true);
                    }
                    break;
                case ItemType.READY_BUTTON:
                    {
                        //Make sure this player selected all of their own characters
                        bool isValid = true;
                        for (int i = 0; i < NUM_PLAYER_SLOTS; i++)
                        {
                            if (playerSelected[i] == null && GameCharacterSelectHelper.IsValidSlot(Client.MyPlayerNumber, Client.NumPlayers, i))
                            {
                                isValid = false;
                            }
                        }
                        if (isValid)
                        {
                            Client.CharacterSelectionFinished();
                        }
                    }
                    break;
            }
        }

        public void UpdateSlotSelection(int slot, int iCharacter, bool isSelecting)
        {
            playerSelected[slot] = allInfoPlayers[iCharacter];

            Text buttonText = playerSlotButtonArray[slot].GetComponentInChildren<Text>();
            buttonText.text = "Player " + (slot + 1) + ": " + allInfoPlayers[iCharacter].Name;

            if(isSelecting)
                Client.ChooseCharacter(allInfoPlayers[iCharacter].ZType, slot);
        }

        public void ChooseNewSlot(int i)
        {
            for (int j = 0; j < playerSlotButtonArray.Length; j++)
            {
                Button b = playerSlotButtonArray[j];

                if (j == i && GameCharacterSelectHelper.IsValidSlot(Client.MyPlayerNumber, Client.NumPlayers, i))
                    b.GetComponent<Image>().color = Color.yellow;
                else
                    b.GetComponent<Image>().color = Color.white;
            }
        }

    }
}