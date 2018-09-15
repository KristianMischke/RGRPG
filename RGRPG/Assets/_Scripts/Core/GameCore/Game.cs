using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{

    public class Game
    {
        protected int worldWidth;
        protected int worldHeight;

        protected List<WorldScene> scenes;
        protected WorldScene startScene;
        protected WorldScene currentScene;

        protected List<Character> players;
        protected List<Character> enemies;
        protected Character selectedCharacter;

        public List<WorldScene> Scenes { get { return scenes; } }
        public WorldScene StartScene { get { return startScene; } }
        public WorldScene CurrentScene { get { return currentScene; } }
        public List<Character> Players { get { return players; } }
        public List<Character> Enemies { get { return enemies; } }
        public Character SelectedCharacter { get { return selectedCharacter; } }

        public Game()
        {
            Init();
        }

        public void Init()
        {
            //TODO: load scenes from files, for now just have one scene
            List<WorldScene> scenes = new List<WorldScene>();

            WorldScene newScene = new WorldScene(30, 30);

            scenes.Add(newScene);
            startScene = newScene;
            currentScene = newScene;


            players = new List<Character>();
            enemies = new List<Character>();

            for (int i = 0; i < 4; i++)
            {
                players.Add(new Character(CharacterType.Player, "Player " + (i + 1), 100, 10, 10, new List<ICharacterAction> { new AttackAction(10) }));
                players[i].SetPosition(Random.Range(1, startScene.Width-1), 1);
            }

            // for now just add one enemy. TODO: spawn more
            enemies.Add(new Character(CharacterType.Enemy, "Enemy", 100, 10, 10, new List<ICharacterAction> { new AttackAction(10) }));
            enemies[0].SetPosition(15, 12);

            selectedCharacter = players[0];
        }

        public void Turn()
        {



        }

        public void SelectCharacter(Character target)
        {
            //TODO: put parameters on when/which characters can be selected
            selectedCharacter = target;
        }

        public void MoveSelectedCharacter(int x, int y)
        {
            if (currentScene.IsTerrainTraversable(selectedCharacter.Position.x + x, selectedCharacter.Position.y + y))
            {
                selectedCharacter.Move(x, y);
            }
        }

    }
}