using System.Collections;
using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;
using Time = UnityEngine.Time;

using RGRPG.Core.Generics;
using RGRPG.Core.NetworkCore;

namespace RGRPG.Core
{
    public class ClientInfo
    {
        public int ID;
        public bool isObserver;

        public ClientInfo(int ID, bool isObserver)
        {
            this.ID = ID;
            this.isObserver = isObserver;
        }
    }

    public class GameServer
    {
        IGameServerManager serverManager;

        // Data
        private List<ClientInfo> connectedClients = new List<ClientInfo>();

        private bool inGame = false;
        private static bool firstGameUpdate = true;
        private Game game;
        private Dictionary<Character, Pair<ICharacterAction, List<Character>>> currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>();

        public int NumPlayersConnected { get { return connectedClients == null ? 0 : connectedClients.Count; } }

        public GameServer(IGameServerManager serverManager)
        {
            this.serverManager = serverManager;
            game = new Game();
        }

        public void RegisterClient(int id)
        {
            int numPlayingClients = 0;
            foreach (ClientInfo c in connectedClients)
                if (!c.isObserver)
                    numPlayingClients++;

            ClientInfo newClient = new ClientInfo(id, numPlayingClients >= 4);
            connectedClients.Add(newClient);
            UnityEngine.Debug.Log("connected client: " + id);
        }

        public void Update(float deltaTime)
        {
            if (game == null || !inGame)
                return;

            if (firstGameUpdate)
            {
                firstGameUpdate = false;
                return;
            }

            
            game.GameLoop(deltaTime);

            if (game.IsInCombat)
            {
                if (game.CurrentCombatState == CombatState.BeginCombat)
                {
                    foreach (Character c in game.CombatEnemies)
                    {
                        //TODO selectAction for choosing enemy target (SERVER)
                    }
                }

                if (game.CurrentCombatState == CombatState.NextRound)
                {
                    currentCharacterActions = new Dictionary<Character, Pair<ICharacterAction, List<Character>>>(); //(SERVER)
                }

                if (game.CurrentCombatState == CombatState.EndCombat)
                {
                    
                }
            }
            else
            {
                
            }

            if (game.gameMessages.Count > 0)
            {
                //TODO: send gameMessages?
                //EventQueueManager.instance.AddEventMessage(game.gameMessages.Dequeue());
            }

            if (game.IsInCombat)
            {
                //TODO: process next combat step once all clients have finished animtaions
                //game.ProcessNextCombatStep(); // (SERVER)
            }
        }

        /// <summary>
        ///     Allows the user to move the character with the arrow keys or WASD
        /// </summary>
        public void MoveCharacter(int i, int yMovement, int xMovement)
        {
            //TODO: things might need to be adjusted due to the camera tilt?!

            float moveMagnitude = 4f;
            

            Vector2 moveVector = new Vector2(xMovement + yMovement, yMovement - xMovement);
            //moveVector = moveVector * new Vector2(1, 2);
            moveVector.Normalize();
            moveVector *= moveMagnitude * Time.deltaTime;

            game.MoveSelectedCharacter(moveVector.x, moveVector.y);

        }
    }
}