using System.Collections;
using System.Collections.Generic;

namespace RGRPG.Core.NetworkCore
{
    public class PlayerLobbyEntry
    {
        private string username;
    }

    public class GameRoom
    {
        private string roomName;
        private int maxPlayers;
        private PlayerLobbyEntry host;
        private List<PlayerLobbyEntry> players;
        private List<PlayerLobbyEntry> observers;
        private bool allowObservers = true;
        private bool isPrivate = false;

        public string Name { get { return roomName; } }
        public int MaxPlayers { get { return maxPlayers; } set { maxPlayers = value; } }
        public PlayerLobbyEntry Host { get { return host; } }
        public List<PlayerLobbyEntry> Players { get { return players; } }
        public List<PlayerLobbyEntry> Observers { get { return observers; } }
        public int NumPlayers { get { return players.Count; } }
        public int NumObservers { get { return observers.Count; } }
        public int NumAllPlayers { get { return players.Count + observers.Count; } }
        public bool AllowObservers { get { return allowObservers; }  set { allowObservers = value; } }
        public bool IsPrivate { get { return isPrivate; } }

        public GameRoom(PlayerLobbyEntry host, string roomName, int maxPlayers = 0, bool isPrivate = false)
        {
            this.host = host;
            this.roomName = roomName;
            this.maxPlayers = maxPlayers;
            this.isPrivate = isPrivate;

            players = new List<PlayerLobbyEntry>();
            players.Add(host);
            observers = new List<PlayerLobbyEntry>();
        }
        
        public void AddPlayer(PlayerLobbyEntry p)
        {
            if (players.Count > maxPlayers && maxPlayers > 0)
            {
                observers.Add(p);
            }
            else
            {
                players.Add(p);
            }
        }
    }

    public interface IGameLobbyManager
    {
        // For Lobby
        void CreateRoom(string name, bool isPrivate);
        void JoinRoom(string name);


        // For Rooms
    }
}