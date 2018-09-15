using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{

    public class Game
    {

        private List<Character> players;
        private List<Character> enemies;

        public List<Character> Players { get { return players; } }
        public List<Character> Enemy { get { return enemies; } }

        public Character selectedCharacter;

        public Game()
        {
            Init();
        }

        public void Init()
        {
            players = new List<Character>();
            enemies = new List<Character>();

            for (int i = 0; i < 4; i++)
            {
                players.Add(new Character("Player " + (i + 1), 100, 10, 10, new List<ICharacterAction> { new AttackAction(10) }));
                players[i].Move(Random.Range(-5, 5), Random.Range(-5, 5), 0);
            }

            // for now just add one enemy. TODO: spawn more
            enemies.Add(new Character("Enemy", 100, 10, 10, new List<ICharacterAction> { new AttackAction(10) }));

            selectedCharacter = players[0];
        }

        public void Turn() {



        }

    }
}