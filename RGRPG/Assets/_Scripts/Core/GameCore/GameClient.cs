using System.Collections;
using System.Collections.Generic;
using RGRPG.Core.Generics;
using RGRPG.Core.NetworkCore;

namespace RGRPG.Core
{
    public class GameClient
    {
        private IGameClientManager clientManager;

        // Data
        private int clientID;

        private bool inGame = false;
        private static bool firstGameUpdate = true;
        private Game game;

        private Character currentSourceAction; //TODO: when implementing multiplayer, this will only be on the client

        public int ClientID { get { return clientID; } set { clientID = value; } }
        public Game Game { get { return game; } }

        public GameClient(IGameClientManager clientManager)
        {
            this.clientManager = clientManager;
            game = new Game();
        }

        private bool firstUpdate = true;
        public void Update(float deltaTime)
        {
            if (firstUpdate)
            {
                clientManager.RequestClientID();
                firstUpdate = false;
            }

            if (game == null || !inGame)
                return;

            if (firstGameUpdate)
            {
                firstGameUpdate = false;
                return;
            }

            game.GameLoop(deltaTime);//???????????
            
            
            if (game.IsInCombat)
            {
                DiscordController.Instance.InBattle();
                
                if (game.CurrentCombatState == CombatState.BeginCombat)
                {
                    
                }

                if (game.CurrentCombatState == CombatState.NextRound)
                {
                    
                }

                if (game.CurrentCombatState == CombatState.EndCombat)
                {
                    
                }
            }
            else
            {
                DiscordController.Instance.InOverworld();

                //MoveSelectedCharacter(); // (CLIENT REQUEST)
            }

            if (game.gameMessages.Count > 0)
            {
                //TODO: figure out best way to handle messages
                //EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
            }

            if (game.IsInCombat)
            {
                if (game.gameCombatActionQueue.Count > 0)
                {
                    PairStruct<Character, ICharacterAction> characterAction = game.gameCombatActionQueue.Dequeue();

                }
                else
                {
                }
            }
        }
    }
}