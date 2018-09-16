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
        protected List<Enemy> enemies;
        protected Character selectedCharacter;

        // for combat
        protected bool isInCombat = false;
        protected bool playerTurnInputDone = false;
        protected Character combatEnemy; //TODO: do we want to be able to fight multiple enemies at a time?
        protected Dictionary<Character, Queue<ICharacterAction>> characterTurns;
        protected Queue<Character> turnOrder;

        public List<WorldScene> Scenes { get { return scenes; } }
        public WorldScene StartScene { get { return startScene; } }
        public WorldScene CurrentScene { get { return currentScene; } }
        public List<Character> Players { get { return players; } }
        public List<Enemy> Enemies { get { return enemies; } }
        public Character SelectedCharacter { get { return selectedCharacter; } }

        public bool IsInCombat { get { return isInCombat; } }
        public bool PlayerTurnInputDone { get { return playerTurnInputDone; } }
        public Character CombatEnemy { get { return combatEnemy; } }

        public Queue<string> gameMessages = new Queue<string>();

        public Game()
        {
            Init();
        }

        public void Init()
        {
            //TODO: load scenes from files, for now just have one scene
            scenes = new List<WorldScene>();

            WorldScene newScene = new WorldScene(30, 30);

            scenes.Add(newScene);
            startScene = newScene;
            currentScene = newScene;


            players = new List<Character>();
            enemies = new List<Enemy>();

            for (int i = 0; i < 4; i++)
            {
                players.Add(new Character(CharacterType.Player, "Player " + (i + 1), 100, 0, 0, new List<ICharacterAction> { new AttackAction(10), new DefendAction(6), new HealAction(9) }));
                players[i].SetPosition(Random.Range(1, startScene.Width-1), 1);
            }

            // for now just add one enemy. TODO: spawn more
            enemies.Add(new Enemy(new Vector2(15, 12), "Enemy", 100, 0, 0, new List<ICharacterAction> { new AttackAction(10) }));
            enemies.Add(new Enemy(new Vector2(28, 28), "Enemy 2", 100, 0, 0, new List<ICharacterAction> { new AttackAction(10) }));

            selectedCharacter = players[0];
        }

        public void GameLoop()
        {
            if (isInCombat)
            {

                if (playerTurnInputDone)
                {
                    RecordAction(combatEnemy.Actions[0], combatEnemy, players[Random.Range(0, players.Count)]);

                    RollDiceForTurnOrder();
                    ProcessCombatTurns();
                    foreach (Character c in players)
                        c.Reset();
                    combatEnemy.Reset();
                    playerTurnInputDone = false;
                }

                return;
            }

            //Start here
            UpdateAI();

            CheckEncounterEnemy();
        }

        void UpdateAI()
        {
            foreach (Enemy e in enemies)
            {
                e.UpdateAI(currentScene, Time.deltaTime);
            }
        }

        void RollDiceForTurnOrder()
        {
            // get the random order
            List<Character> characters = new List<Character>(characterTurns.Keys);
            turnOrder = new Queue<Character>();
            while (characters.Count > 0)
            {
                int nextCharacter = Random.Range(0, characters.Count);
                turnOrder.Enqueue(characters[nextCharacter]);
                characters.RemoveAt(nextCharacter);
            }
        }

        void ProcessCombatTurns()
        {
            //TODO: this all happens in one frame, we would probably want to break it out or do something ont the controller side to make things happen slower and in order to the player
            while (turnOrder.Count > 0)
            {
                Character currentTurn = turnOrder.Dequeue();
                LogMessage("Processing " + currentTurn.Name + "'s Turn");
                while (characterTurns[currentTurn].Count > 0)
                {
                    ICharacterAction currentAction = characterTurns[currentTurn].Dequeue();
                    LogMessage("Executing Action: " + currentAction.GetName());
                    currentAction.DoAction();
                }
            }
        }

        public void FinishPlayerTurnInput()
        {
            playerTurnInputDone = true;
        }

        public void SelectCharacter(Character target)
        {
            //TODO: put parameters on when/which characters can be selected
            selectedCharacter = target;
        }

        public void MoveSelectedCharacter(float dx, float dy)
        {
            selectedCharacter.Move(currentScene, dx, dy);
        }

        void CheckEncounterEnemy()
        {
            if (isInCombat)
                return;

            foreach (Character player in players)
            {
                foreach (Character enemy in enemies)
                {
                    if (player.TouchingCharacter(enemy))
                    {
                        LogMessage("FIGHT");
                        combatEnemy = enemy;
                        StartCombat();
                    }
                }
            }
        }

        void StartCombat()
        {
            isInCombat = true;

            characterTurns = new Dictionary<Character, Queue<ICharacterAction>>();
        }

        public void RecordAction(ICharacterAction action, Character source, Character target)
        {
            if (!source.IsAlive()) {
                LogMessage("You are dead, not big surprise");
                return;
            }
            if (!characterTurns.ContainsKey(source))
                characterTurns.Add(source, new Queue<ICharacterAction>());

            action.SetTargets(new List<Character> { target });
            characterTurns[source].Enqueue(action);
            LogMessage(source.Name + " -> " + action.GetName());
        }

        public void LogMessage(string text)
        {
            gameMessages.Enqueue(text);
        }

    }
}