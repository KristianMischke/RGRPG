using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public enum GameState
    {
        Starting,
        WorldMovement,
        Combat,

        COUNT
    }

    public enum CombatState
    {
        NONE,

        BeginCombat,
        PickTurnOrder,
        ChooseEnemyActions,
        PlayersChooseActions,
        ExecuteTurns,
        NextRound,
        EndCombat,

        COUNT
    }

    public class Game
    {
        protected GameState currentGameState = GameState.Starting;

        protected int worldWidth;
        protected int worldHeight;

        protected List<WorldScene> scenes;
        protected WorldScene startScene;
        protected WorldScene currentScene;

        protected List<Character> players;
        protected List<Enemy> enemies;
        protected Character selectedCharacter;

        // for combat
        protected CombatState currentCombatState = CombatState.NONE;
        protected int turnCounter = 0;
        protected int roundCounter = 0;
        protected List<Character> combatEnemies = new List<Character>(); //TODO: do we want to be able to fight multiple enemies at a time?
        protected Dictionary<Character, Queue<ICharacterAction>> characterTurns;
        protected List<Character> turnOrder;

        public GameState CurrentGameState { get { return currentGameState; } }
        public CombatState CurrentCombatState { get { return currentCombatState; } }
        public List<WorldScene> Scenes { get { return scenes; } }
        public WorldScene StartScene { get { return startScene; } }
        public WorldScene CurrentScene { get { return currentScene; } }
        public List<Character> Players { get { return players; } }
        public List<Enemy> Enemies { get { return enemies; } }
        public Character SelectedCharacter { get { return selectedCharacter; } }

        public bool IsInCombat { get { return currentGameState == GameState.Combat; } }
        public List<Character> CombatEnemies { get { return combatEnemies; } }        

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
                players.Add(new Character(CharacterType.Player, "Player " + (i + 1), 100, 0, 0, new List<ICharacterAction> { new AttackAction(10, 25), new DefendAction(6, 10), new HealAction(9, 30) }));
                players[i].SetPosition(Random.Range(1, startScene.Width-1), 1);
            }

            // for now just add one enemy. TODO: spawn more
            enemies.Add(new Enemy(new Vector2(15, 12), "Enemy", 100, 0, 0, new List<ICharacterAction> { new AttackAction(10, 25) }));
            enemies.Add(new Enemy(new Vector2(28, 28), "Enemy 2", 100, 0, 0, new List<ICharacterAction> { new AttackAction(10, 20) }));

            selectedCharacter = players[0];

            currentGameState = GameState.WorldMovement;
            currentCombatState = CombatState.NONE;
        }

        public void GameLoop()
        {
            switch(currentGameState)
            {
                case GameState.WorldMovement:
                    UpdateAI();
                    CheckEncounterEnemy();
                    break;
                case GameState.Combat:
                    DoCombat();
                    break;
            }
        }

        void UpdateAI()
        {
            foreach (Enemy e in enemies)
            {
                e.UpdateAI(currentScene, Time.deltaTime);
            }
        }

        void DoCombat()
        {
            switch (currentCombatState)
            {
                case CombatState.BeginCombat:
                    turnCounter = 0;
                    foreach (Character p in players)
                        p.SetMana(50);
                    foreach (Enemy e in enemies)
                        e.SetMana(50);
                    currentCombatState = CombatState.PickTurnOrder;
                    break;
                case CombatState.PickTurnOrder:
                    RollDiceForTurnOrder();
                    currentCombatState = CombatState.ChooseEnemyActions;
                    break;
                case CombatState.ChooseEnemyActions:
                    foreach(Enemy e in combatEnemies)
                        RecordAction(e.Actions[0], e, players[Random.Range(0, players.Count)]);
                    currentCombatState = CombatState.PlayersChooseActions;
                    break;
                case CombatState.PlayersChooseActions:
                    break;
                case CombatState.ExecuteTurns:
                    if (turnCounter >= turnOrder.Count)
                    {
                        currentCombatState = CombatState.NextRound;
                    }
                    else
                    {
                        ProcessNextCombatTurn();
                        turnCounter++;
                    }
                    break;
                case CombatState.NextRound:
                    roundCounter++;
                    turnCounter = 0;
                    LogMessage("ROUND #" + turnCounter + "DONE");
                    foreach (Character c in players)
                        c.Reset();
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        Enemy e = enemies[i];
                        e.Reset();
                        if (!e.IsAlive())
                        {
                            //TODO: spill loot
                            enemies.RemoveAt(i);
                        }
                    }

                    for (int i = 0; i < turnOrder.Count; i++)
                    {
                        Character c = turnOrder[i];
                        c.ChangeMana(i * 10 + 10); // MANA RECHARGE TODO: change behaviour
                    }

                    currentCombatState = CombatState.PickTurnOrder;
                    CheckExitConditions();
                    break;
                case CombatState.EndCombat:
                    currentCombatState = CombatState.NONE;
                    currentGameState = GameState.WorldMovement;
                    break;
            }
        }

        void RollDiceForTurnOrder()
        {
            // get the random order
            List<Character> characters = new List<Character>();
            characters.AddRange(combatEnemies);
            characters.AddRange(players);
            turnOrder = new List<Character>();
            while (characters.Count > 0)
            {
                int nextCharacter = Random.Range(0, characters.Count);
                turnOrder.Add(characters[nextCharacter]);
                characters.RemoveAt(nextCharacter);
            }
        }

        void CheckExitConditions()
        {
            bool allEnemiesDead = true;
            foreach (Enemy e in combatEnemies)
            {
                allEnemiesDead &= !e.IsAlive();
            }

            bool allPlayersDead = true;
            foreach (Character p in players)
            {
                allPlayersDead &= !p.IsAlive();
            }

            if (allPlayersDead)
            {
                LogMessage("The Enemie(s) Won!");
            }

            if (allEnemiesDead)
            {
                LogMessage("YOU Won!");
            }

            if (allEnemiesDead || allPlayersDead)
                currentCombatState = CombatState.EndCombat;
        }

        void ProcessNextCombatTurn()
        {
            Character currentTurn = turnOrder[turnCounter];
            LogMessage("Processing " + currentTurn.Name + "'s Turn");
            if (characterTurns.ContainsKey(currentTurn))
            {
                while (characterTurns[currentTurn].Count > 0)
                {
                    ICharacterAction currentAction = characterTurns[currentTurn].Dequeue();
                    if (currentAction.ManaCost() > currentTurn.Mana)
                    {
                        LogMessage("NOT ENOUGH MANA");
                    }
                    else
                    {
                        currentTurn.ChangeMana(-currentAction.ManaCost());
                        LogMessage("Executing Action: " + currentAction.GetName());
                        currentAction.DoAction();
                    }
                }
            }
            else
            {
                LogMessage("PASS");
            }
        }

        public void FinishPlayerTurnInput()
        {
            if (currentGameState == GameState.Combat && currentCombatState == CombatState.PlayersChooseActions)
                currentCombatState = CombatState.ExecuteTurns;
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
            if (currentGameState == GameState.Combat)
                return;

            foreach (Character player in players)
            {
                foreach (Character enemy in enemies)
                {
                    if (player.TouchingCharacter(enemy))
                    {
                        LogMessage("FIGHT");
                        combatEnemies.Clear();
                        combatEnemies.Add(enemy); //TODO: right now just single enemies per world enemy, in future: bands of enemies
                        StartCombat();
                    }
                }
            }
        }

        void StartCombat()
        {
            currentGameState = GameState.Combat;
            currentCombatState = CombatState.BeginCombat;

            characterTurns = new Dictionary<Character, Queue<ICharacterAction>>();
        }

        public void RecordAction(ICharacterAction action, Character source, Character target)
        {
            if (currentGameState != GameState.Combat)
            {
                LogMessage("Must be in combat mode to choose your action");
                return;
            }

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