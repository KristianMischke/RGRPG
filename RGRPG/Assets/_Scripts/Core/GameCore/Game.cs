using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{

    public class Game
    {

        private List<Character> players;
        private Character enemy;

        public List<Character> Players { get { return players; } }
        public Character Enemy { get { return enemy; } }

        public Game()
        {

            Init();
        }

        public void Init()
        {
            players = new List<Character>();
            enemy = new Character();

            for (int i = 0; i < 4; i++)
            {
                players.Add(new Character("Player " + (i + 1), 100, 10, 10, new List<ICharacterActions> { new AttackAction(10) }));
            }
        }

    }
}