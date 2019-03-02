using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using RGRPG.Core.Generics;
using System.Xml;

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

        Win,
        Loss,

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
        protected GameInfos infos;

        protected GameState currentGameState = GameState.Starting;

        // scenes
        protected Dictionary<string, WorldScene> scenes;
        protected WorldScene startScene;
        protected WorldScene currentScene;
        protected bool sceneTransitioned = false;
        public bool SceneTransitioned { get { bool temp = sceneTransitioned; sceneTransitioned = false; return temp; } }

        // characters
        protected int nextCharacterID = 0;
        protected Dictionary<int, Character> allCharacters;
        protected List<Character> players;
        protected Dictionary<string, List<Enemy>> enemies; // scene ID, enemies
        protected Character selectedCharacter;

        // for combat
        protected CombatState previousCombatState = CombatState.NONE;
        protected CombatState currentCombatState = CombatState.NONE;
        protected int turnCounter = 0;
        protected int prevTurnCounter = -1;
        protected int roundCounter = 0;
        protected List<Character> combatEnemies = new List<Character>(); //TODO: do we want to be able to fight multiple enemies at a time?
        protected Dictionary<Character, Queue<ICharacterAction>> characterTurns;
        protected List<Character> turnOrder;
        protected bool doNextCombatStep = false;

        // allows for public access, but not public assignment
        public GameInfos Infos { get { return infos; } }

        public GameState CurrentGameState { get { return currentGameState; } }
        public CombatState PreviousCombatState { get { return previousCombatState; } }
        public CombatState CurrentCombatState { get { return currentCombatState; } }
        public Dictionary<string, WorldScene> Scenes { get { return scenes; } }
        public WorldScene StartScene { get { return startScene; } }
        public WorldScene CurrentScene { get { return currentScene; } }
        public List<Character> Players { get { return players; } }
        public List<Enemy> Enemies { get { return enemies[currentScene.ZType]; } }
        public Character SelectedCharacter { get { return selectedCharacter; } }

        public bool IsInCombat { get { return currentGameState == GameState.Combat; } }
        public List<Character> CombatEnemies { get { return combatEnemies; } }
        public List<Character> TurnOrder { get { return turnOrder; } } 

        public Queue<string> gameMessages = new Queue<string>();
        public Queue<PairStruct<Character, ICharacterAction>> gameCombatActionQueue = new Queue<PairStruct<Character, ICharacterAction>>();

        public Game()
        {
            Init();
        }

        /// <summary>
        ///     Initializes the game, this should only be called when constructing a new game
        /// </summary>
        private void Init()
        {
            allCharacters = new Dictionary<int, Character>();
            players = new List<Character>();
            enemies = new Dictionary<string, List<Enemy>>();

            infos = new GameInfos();

            LoadScenes();

            currentGameState = GameState.WorldMovement;
            currentCombatState = CombatState.NONE;
        }

        /// <summary>
        ///     Loads the list of scenes from the GameScenes.xml file
        /// </summary>
        private void LoadScenes()
        {
            scenes = new Dictionary<string, WorldScene>();

            foreach (InfoScene infoScene in infos.GetAll<InfoScene>())
            {
                WorldScene newScene = new WorldScene(infos.Get<InfoScene>(infoScene.ZType));
                TextAsset sceneXML = Resources.Load<TextAsset>(infoScene.FilePath);
                newScene.LoadXml(sceneXML.text);
                enemies.Add(infoScene.ZType, new List<Enemy>());
                newScene.LoadDefaultEntities(this, enemies[infoScene.ZType]);

                scenes.Add(infoScene.ZType, newScene);

                if (infoScene.IsFirst || scenes.Count == 1)
                {
                    startScene = newScene;
                    currentScene = newScene;
                }
            }
        }

        public object[] SerializePlayers()
        {
            object[] allPlayerData = new object[(Character.NUM_SERIALIZED_FIELDS + 1) * players.Count];

            int i = 0;
            foreach (KeyValuePair<int, Character> kvp in allCharacters)
            {
                if (!(kvp.Value is Enemy))
                {
                    Character player = kvp.Value;

                    object[] eData = player.Serialize();

                    allPlayerData[i] = kvp.Key;
                    i++;
                    for (int j = 0; j < eData.Length; j++)
                    {
                        allPlayerData[i] = eData[j];
                        i++;
                    }
                }
            }

            return allPlayerData;
        }

        public object[] SerializeEnemies()
        {
            object[] allEnemyData = new object[(Enemy.NUM_SERIALIZED_FIELDS + 1) * Enemies.Count];

            int i = 0;
            foreach (KeyValuePair<int, Character> kvp in allCharacters)
            {
                if (kvp.Value is Enemy)
                {
                    Enemy enemy = kvp.Value as Enemy;
                    if (Enemies.Contains(enemy))
                    {

                        object[] eData = enemy.Serialize();

                        allEnemyData[i] = kvp.Key;
                        i++;
                        for (int j = 0; j < eData.Length; j++)
                        {
                            allEnemyData[i] = eData[j];
                            i++;
                        }
                    }
                }
            }

            return allEnemyData;
        }

        public void DeserializeCharacters(object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                int id = (int)data[i];
                i++;

                Character character = allCharacters[id];

                int cDataLength = Character.NUM_SERIALIZED_FIELDS;
                if (character is Enemy)
                {
                    cDataLength = Enemy.NUM_SERIALIZED_FIELDS;
                }

                object[] cData = new object[cDataLength];
                for (int j = 0; j < cDataLength; j++)
                {
                    cData[j] = data[i];
                    if(j < cDataLength-1)
                        i++;
                }

                character.Deserialize(cData);
            }
        }

        /// <summary>
        ///     Initializes the players' characters
        /// </summary>
        /// <param name="playerSelections">The indexed infos for teh selected characters. Assumes length is 4</param>
        public void SelectCharacters(InfoCharacter[] playerSelections)
        {
            for (int i = 0; i < 4; i++)
            {
                Character c = GenerateCharacter(playerSelections[i]);
                players.Add(c);
                //players[i].SetPosition(Random.Range(1, startScene.Width - 1), 1);
                players[i].SetPosition(startScene.getSpawnPos(CurrentScene.MyInfo.FirstSpawnID));
            }

            selectedCharacter = players[0];
        }

        /// <summary>
        ///     Initializes the players' characters
        /// </summary>
        /// <param name="playerSelections">The indexed infos for teh selected characters. Assumes length is 4</param>
        public void SelectCharacters(string[] playerSelections)
        {
            for (int i = 0; i < 4; i++)
            {
                Character c = GenerateCharacter(playerSelections[i]);
                players.Add(c);
                //players[i].SetPosition(Random.Range(1, startScene.Width - 1), 1);
                players[i].SetPosition(startScene.getSpawnPos(CurrentScene.MyInfo.FirstSpawnID));
            }

            selectedCharacter = players[0];
        }

        public Character GenerateCharacter(string zType)
        {
            return GenerateCharacter(Infos.Get<InfoCharacter>(zType));
        }
        public Character GenerateCharacter(InfoCharacter infoCharacter)
        {
            Character c = infoCharacter.GenerateCharacter(this, infos, nextCharacterID);
            allCharacters[nextCharacterID] = c;
            nextCharacterID++;
            return c;
        }
        public Enemy GenerateEnemy(string zType, Vector2 initialPosition)
        {
            return GenerateEnemy(Infos.Get<InfoCharacter>(zType), initialPosition);
        }
        public Enemy GenerateEnemy(InfoCharacter infoCharacter, Vector2 initialPosition)
        {
            Character c = infoCharacter.GenerateCharacter(this, infos, nextCharacterID);
            Enemy e = new Enemy(c, initialPosition);
            allCharacters[nextCharacterID] = e;
            nextCharacterID++;
            return e;
        }

        /// <summary>
        ///     Controls the next iteration of the game loop, currently just switches from combat mode to world movement mode
        /// </summary>
        public void GameLoop(float deltaTime)
        {
            previousCombatState = currentCombatState;

            switch(currentGameState)
            {
                case GameState.WorldMovement:
                    UpdateAI(deltaTime);
                    CheckSceneTransition();
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
        private void UpdateAI(float deltaTime)
        {
            foreach (Enemy e in enemies[currentScene.ZType])
            {
                e.UpdateAI(currentScene, deltaTime);
            }
        }


        /// <summary>
        ///     Executes the combat logic based on the current combat state
        /// </summary>
        private void DoCombat()
        {
            switch (currentCombatState)
            {
                // right now begining combat just resets mana. TODO: determine correct behavior
                case CombatState.BeginCombat:
                    turnCounter = 0;

                    //TODO: change mana behavior
                    foreach (Character p in players)
                        p.SetMana(50);
                    foreach (Enemy e in combatEnemies)
                        e.SetMana(50);

                    currentCombatState = CombatState.PickTurnOrder;
                    break;

                // picks the turn order before actions are chosen (TODO: when implementing GameServer, this information will be hidden until everyone has subbmitted their actions)
                case CombatState.PickTurnOrder:
                    RollDiceForTurnOrder();
                    currentCombatState = CombatState.ChooseEnemyActions;
                    break;

                // chooses who each enemy in combat will attack
                case CombatState.ChooseEnemyActions:
                    foreach (Enemy e in combatEnemies)
                    {
                        Character target = null;
                        int count = Players.Count;
                        while (target == null && count > 0)
                        {
                            int randIndex = Random.Range(0, players.Count);
                            if(players[randIndex].IsAlive())
                                target = players[randIndex];
                            count--;
                        }
                        if(count != 0 && e.Actions.Count > 0)
                            RecordAction(e.Actions[0], e, new List<Character> { target });
                    }
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
                        if (doNextCombatStep)
                        {
                            ProcessNextCombatAction();
                        }
                    }
                    break;

                // go to the next round of combat (currently just recharges some mana)
                case CombatState.NextRound:
                    roundCounter++;
                    turnCounter = 0;
                    prevTurnCounter = -1;
                    LogMessage("ROUND #" + roundCounter + " DONE");
                    foreach (Character c in players)
                        c.Reset();
                    for (int i = 0; i < enemies[currentScene.ZType].Count; i++)
                    {
                        Enemy e = enemies[currentScene.ZType][i];
                        e.Reset();
                        if (!e.IsAlive())
                        {
                            //TODO: spill loot
                            enemies[currentScene.ZType].RemoveAt(i);

                            int id = allCharacters.Single(x => x.Value == e).Key;
                            allCharacters.Remove(id);
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
        private void RollDiceForTurnOrder()
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
        private void CheckExitConditions()
        {
            bool allCombatEnemiesDead = true;
            foreach (Enemy e in combatEnemies)
            {
                allCombatEnemiesDead &= !e.IsAlive();
            }

            bool allPlayersDead = true;
            foreach (Character p in players)
            {
                allPlayersDead &= !p.IsAlive();
            }

            bool allEnemiesDead = true;
            foreach (List<Enemy> sceneEnenies in enemies.Values)
                foreach (Enemy e in sceneEnenies)
                    allEnemiesDead &= !e.IsAlive();

            if (allPlayersDead)
            {
                currentGameState = GameState.Loss;
                LogMessage("The Enemie(s) Won!"); //TODO: actually store a win/lose state instead of just logging
            }

            if (allCombatEnemiesDead)
            {
                LogMessage("YOU Won this battle!"); //TODO: see ^
            }

            if (allEnemiesDead)
            {
                currentGameState = GameState.Win;
                LogMessage("YOU WON THE GAME"); //TODO: beating all the enemies should not be the ultimate win condition (defeating the bosses should be)
            }

            if (allCombatEnemiesDead || allPlayersDead)
                currentCombatState = CombatState.EndCombat;
        }

        /// <summary>
        ///     Executes all the combat actions that the current character has queued up
        /// </summary>
        private void ProcessNextCombatAction()
        {
            // get the player who is currently executing their turn
            Character currentTurn = turnOrder[turnCounter];

            if (currentTurn.IsAlive())
            {
                // we have switched to another player's turn
                if (prevTurnCounter != turnCounter)
                {
                    gameCombatActionQueue.Enqueue(new PairStruct<Character, ICharacterAction>(currentTurn, new BeginTurnAction()));
                }

                // execute the next action
                if (characterTurns.ContainsKey(currentTurn))
                {
                    if (characterTurns[currentTurn].Count > 0)
                    {
                        ICharacterAction currentAction = characterTurns[currentTurn].Dequeue();
                        if (currentAction.ManaCost() > currentTurn.Mana)
                        {
                            LogMessage("NOT ENOUGH MANA");
                        }
                        else
                        {
                            currentTurn.ChangeMana(-currentAction.ManaCost());
                            //LogMessage("Executing Action: " + currentAction.GetName()); //TODO: don't log messages for attacks, instead handle attack messages in ICharacterAction and use the gameCombatActionQueue in GameController.cs to add those messages to the Marque
                            currentAction.DoAction(currentTurn);

                            gameCombatActionQueue.Enqueue(new PairStruct<Character, ICharacterAction>(currentTurn, currentAction));
                        }
                    }
                }
                else
                {
                    gameCombatActionQueue.Enqueue(new PairStruct<Character, ICharacterAction>(currentTurn, new PassTurnAction()));
                }
            }
            else
            {
                if (characterTurns.ContainsKey(currentTurn))
                {
                    characterTurns[currentTurn].Clear();
                }
            }

            // move to the next character if they aren't doing anthing OR have finished their moves
            if (!characterTurns.ContainsKey(currentTurn) || characterTurns[currentTurn].Count == 0)
            {
                prevTurnCounter = turnCounter;
                turnCounter++;
            }

            doNextCombatStep = false;
        }

        /// <summary>
        ///     Sets the combat state to execute turns
        /// </summary>
        public void FinishPlayerTurnInput()
        {
            if (currentGameState == GameState.Combat && currentCombatState == CombatState.PlayersChooseActions)
                currentCombatState = CombatState.ExecuteTurns;
        }

        public void ProcessNextCombatStep()
        {
            doNextCombatStep = true;
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
            for (int i = 1; i < players.Count; i++)
            {
                if (Vector2.Distance(players[i].Position, players[i - 1].Position) >= 1)
                {
                    //float dxi = (players[i - 1].Position.x - players[i].Position.x) / 2;
                    //float dyi = (players[i - 1].Position.y - players[i].Position.y) / 2;
                    float dxi = Mathf.Lerp(players[i].Position.x, players[i - 1].Position.x, .11f) - players[i].Position.x;
                    float dyi = Mathf.Lerp(players[i].Position.y, players[i - 1].Position.y, .11f) - players[i].Position.y;

                    players[i].Move(currentScene, dxi, dyi);
                }
            }
            
        }

        /// <summary>
        ///     Checks to see if any players have moved over a transition tile
        /// </summary>
        void CheckSceneTransition()
        {
            if (currentGameState == GameState.Combat)
                return;

            foreach (Character player in players)
            {
                TerrainTile standingTile = currentScene.GetTileAt(player.Position);
                if (standingTile.IsTransitionTile)
                {
                    TransitionToScene(standingTile.TransitionScene, standingTile.TransitionSpawnID);
                    return;
                }
            }
        }

        /// <summary>
        ///     Transitions the game to a different scene
        /// </summary>
        /// <param name="sceneID">The ID of the scene to transition to</param>
        /// <param name="spawnID">The ID of the tile to spawn at. (if null, only changes scene, doesn't set player's position.)</param>
        public void TransitionToScene(string sceneID, string spawnID = null)
        {
            if (scenes.ContainsKey(sceneID))
            {
                // set new current scene
                currentScene = scenes[sceneID];

                if (!string.IsNullOrEmpty(spawnID))
                {
                    Vector2Int spawnPos = currentScene.getSpawnPos(spawnID);

                    // put players at spawn tile
                    foreach (Character player in players)
                    {
                        player.SetPosition(spawnPos);
                    }
                }

                sceneTransitioned = true;
            }
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
                foreach (Character enemy in enemies[currentScene.ZType])
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
        /// <param name="target">The characters who are the targets of the action</param>
        public void RecordAction(ICharacterAction action, Character source, List<Character> targets)
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

            action.SetTargets(targets);
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