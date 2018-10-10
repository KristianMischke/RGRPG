using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Different calculations that the game can make in the game loop
    /// </summary>
    public enum GameState
    {
        Starting,
        WorldMovement,
        Combat,

        COUNT
    }

    /// <summary>
    ///     Different combat calculation that need to be made during a combat round
    /// </summary>
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


    /// <summary>
    ///     Stores all the game data and functionality of a game
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is where all the magic happens in the game.
    ///     </para>
    ///     <para>
    ///         Players and Enemies are updated and maintainted here.
    ///     </para>
    ///     <para>
    ///         The scene list is referenced here, as well as the current scene
    ///     </para>
    ///     </para>
    ///         All of this is still raw data and function, no user interface
    ///     </para>
    /// </remarks>
    public class Game
    {
        protected GameState currentGameState = GameState.Starting;

        // scenes
        protected List<WorldScene> scenes;
        protected WorldScene startScene;
        protected WorldScene currentScene;

        // characters
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

        // allows for public access, but not public assignment
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
        public List<Character> TurnOrder { get { return turnOrder; } } 

        public Queue<string> gameMessages = new Queue<string>();

        public Game()
        {
            Init();
        }

        /// <summary>
        ///     Initializes the game, this should only be called when constructing a new game
        /// </summary>
        private void Init()
        {
            //TODO: load scenes from files, for now just have one scene
            scenes = new List<WorldScene>();

            WorldScene newScene = new WorldScene(30, 30);
            TextAsset worldXMLTest = Resources.Load<TextAsset>(@"Data\WorldSceneTest"); // currently just loads a test scene. TODO: change behaviour
            currentScene.LoadXml(worldXMLTest.text);

            scenes.Add(newScene);
            startScene = newScene;
            currentScene = newScene;


            players = new List<Character>();
            enemies = new List<Enemy>();

            // for now just create 4 players with equal stats, TODO: players need to be loaded from XML then selected by the user(s)...
            // maybe the selected players should be passed in as parameters into this function (that might be good for when multiplayer comes around)
            for (int i = 0; i < 4; i++)
            {
                players.Add(new Character(TempCharacterType.Player, "Player " + (i + 1), 100, 0, 0, new List<ICharacterAction> { new AttackAction(10, 25), new DefendAction(6, 10), new HealAction(9, 30) }));
                players[i].SetPosition(Random.Range(1, startScene.Width-1), 1);
            }

            // for now just add one enemy. TODO: spawn more
            enemies.Add(new Enemy(new Vector2(15, 12), "Enemy", 100, 0, 0, new List<ICharacterAction> { new AttackAction(10, 25) }));
            enemies.Add(new Enemy(new Vector2(28, 28), "Enemy 2", 100, 0, 0, new List<ICharacterAction> { new AttackAction(10, 20) }));

            selectedCharacter = players[0];

            currentGameState = GameState.WorldMovement;
            currentCombatState = CombatState.NONE;
        }

        /// <summary>
        ///     Controls the next iteration of the game loop, currently just switches from combat mode to world movement mode
        /// </summary>
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

        /// <summary>
        ///     Tells all the enemies to execute their AI
        /// </summary>
        void UpdateAI()
        {
            foreach (Enemy e in enemies)
            {
                e.UpdateAI(currentScene, Time.deltaTime);
            }
        }


        /// <summary>
        ///     Executes the combat logic based on the current combat state
        /// </summary>
        void DoCombat()
        {
            switch (currentCombatState)
            {
                // right now begining combat just resets mana. TODO: determine correct behavior
                case CombatState.BeginCombat:
                    turnCounter = 0;
                    foreach (Character p in players)
                        p.SetMana(50);
                    foreach (Enemy e in enemies)
                        e.SetMana(50);
                    currentCombatState = CombatState.PickTurnOrder;
                    break;

                // picks the turn order before actions are chosen (when implementing GameServer, this information will be hidden until everyone has subbmitted their actions)
                case CombatState.PickTurnOrder:
                    RollDiceForTurnOrder();
                    currentCombatState = CombatState.ChooseEnemyActions;
                    break;

                // chooses who each enemy in combat will attack
                case CombatState.ChooseEnemyActions:
                    foreach(Enemy e in combatEnemies)
                        RecordAction(e.Actions[0], e, players[Random.Range(0, players.Count)]);
                    currentCombatState = CombatState.PlayersChooseActions;
                    break;

                // lets players input their turn (this state is exited in FinishPlayerTurnInput())
                case CombatState.PlayersChooseActions:
                    break;

                //
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

                // go to the next round of combat (currently just recharges some mana)
                case CombatState.NextRound:
                    roundCounter++;
                    turnCounter = 0;
                    LogMessage("ROUND #" + roundCounter + " DONE");
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

                // exit to the overworld
                case CombatState.EndCombat:
                    currentCombatState = CombatState.NONE;
                    currentGameState = GameState.WorldMovement;
                    break;
            }
        }

        /// <summary>
        ///     Gets the random order for the enemies and players involved in combat
        /// </summary>
        void RollDiceForTurnOrder()
        {
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

        /// <summary>
        ///     Changes the combat state depending on whether or not the player, enemies, or no one has won
        /// </summary>
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
                LogMessage("The Enemie(s) Won!"); //TODO: actually store a win/lose state instead of just logging
            }

            if (allEnemiesDead)
            {
                LogMessage("YOU Won!"); //TODO: see ^
            }

            if (allEnemiesDead || allPlayersDead)
                currentCombatState = CombatState.EndCombat;
        }

        /// <summary>
        ///     Executes all the combat actions that the current character has queued up
        /// </summary>
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

        /// <summary>
        ///     Sets the combat state to execute turns
        /// </summary>
        public void FinishPlayerTurnInput()
        {
            if (currentGameState == GameState.Combat && currentCombatState == CombatState.PlayersChooseActions)
                currentCombatState = CombatState.ExecuteTurns;
        }

        /// <summary>
        ///     Allows the user to select a player to control TODO: this behavior will change with leaders
        /// </summary>
        /// <param name="target">The character to control</param>
        public void SelectCharacter(Character target)
        {
            //TODO: put parameters on when/which characters can be selected
            selectedCharacter = target;
        }

        /// <summary>
        ///     Tells the selected character to move
        /// </summary>
        /// <param name="dx">The x amount the player should move</param>
        /// <param name="dy">The y amount the player should move</param>
        public void MoveSelectedCharacter(float dx, float dy)
        {
            selectedCharacter.Move(currentScene, dx, dy);
        }

        /// <summary>
        ///     Checks to see if a player is touching an enemy to enter combat
        /// </summary>
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
                        combatEnemies.Add(enemy); //TODO: right now just single enemies per world enemy, in future: one sprite may contain a band of enemies
                        StartCombat();
                    }
                }
            }
        }

        /// <summary>
        ///     Sets up variables for beginning combat in the game loop
        /// </summary>
        void StartCombat()
        {
            currentGameState = GameState.Combat;
            currentCombatState = CombatState.BeginCombat;

            characterTurns = new Dictionary<Character, Queue<ICharacterAction>>();
        }

        /// <summary>
        ///     Records an action chosen by the user for a given player and a target
        /// </summary>
        /// <param name="action">The action to be executed</param>
        /// <param name="source">The character who is trying to execute the action</param>
        /// <param name="target">The character who is the target of the action</param>
        public void RecordAction(ICharacterAction action, Character source, Character target)
        {
            if (currentGameState != GameState.Combat)
            {
                LogMessage("Must be in combat mode to choose your action");
                return;
            }

            if (!source.IsAlive()) {
                LogMessage("You are dead, no big surprise");
                return;
            }

            if (!source.Actions.Contains(action))
            {
                LogMessage("You are trying to execute an action that this player does not possess.");
                return;
            }

            if (!characterTurns.ContainsKey(source))
                characterTurns.Add(source, new Queue<ICharacterAction>());

            action.SetTargets(new List<Character> { target });
            characterTurns[source].Enqueue(action);
            //LogMessage(source.Name + " -> " + action.GetName());
        }

        /// <summary>
        ///     Adds messages to a queue to be displayed by the UI
        /// </summary>
        /// <param name="text">The message to be displayed</param>
        public void LogMessage(string text)
        {
            gameMessages.Enqueue(text);
        }

    }
}