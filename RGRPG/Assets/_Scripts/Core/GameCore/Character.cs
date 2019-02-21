using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Data representation that stores all the attributes of a character
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Both players and enemies classify as "characters"
    ///         Data such as health, position and battle actions are stored here
    ///     </para>
    ///     <para>
    ///         Debating on whether or not we should put attack and defense here,
    ///         because we are also dealing with similar structures in the ICharacterActions class (<see cref="ICharacterAction"/>)
    ///     </para>
    ///     <para>
    ///         Characters are controlled by Game.cs (<see cref="Game"/>) and is also used as a data representation in UI elements
    ///     </para>
    /// </remarks>
    public class Character
    {
        protected Game myGame;
        protected InfoCharacter myInfo;
        protected string zType;

        protected int health;
        protected int defense;
        protected int mana;

        protected Vector2 position;
        protected float radius = 0.5f;

        protected List<ICharacterAction> actions;

        public Game MyGame { get { return myGame; } }
        public InfoCharacter MyInfo { get {return myInfo;}}
        public string Type { get { return zType; } }
        public string ClassType { get { return myInfo.Class; } }        
        public string Name { get { return myInfo.Name; } }
        public int Health { get { return health; } }
        public int Defense { get { return defense; } }
        public int Mana { get { return mana; } }
        public List<ICharacterAction> Actions { get { return actions; } }
        public Vector2 Position { get { return position; } }
        public float Radius { get { return radius; } }

        public Character() { }

        public Character(Game myGame, InfoCharacter myInfo, List<ICharacterAction> actions)
        {
            this.myGame = myGame;
            this.myInfo = myInfo;
            this.zType = myInfo.ZType;
            this.health = myInfo.Health;
            //TODO deal with MP
            this.defense = myInfo.Defense;
            this.actions = actions;
        }

        public const int NUM_SERIALIZED_FIELDS = 4;
        public object[] Serialize()
        {
            return new object[] { health, defense, mana, position };
        }
        public void Deserialize(object[] data)
        {
            health = (int)data[0];
            defense = (int)data[1];
            mana = (int)data[2];
            position = (Vector2)data[3];
        }

        public void Damage(int amount)
        {
            /*if (defense >= amount)
                Debug.Log("Deflected damage!");
            else*/
            health -= amount;// - defense;

            if (health <= 0)
                health = 0;
        }

        public void SetShield(int amt)
        {
            defense = amt;
        }

        public void Reset()
        {
            defense = 0;
        }

        public void Heal(int amount)
        {
            health += amount;

            if (health > myInfo.Health)
                health = myInfo.Health;
        }

        public void SetMana(int amount)
        {
            mana = amount;

            if (mana > myInfo.Magic)
                mana = myInfo.Magic;
        }

        public void ChangeMana(int amount)
        {
            mana += amount;

            if (mana > myInfo.Magic)
                mana = myInfo.Magic;
        }

        public void Move(WorldScene scene, float dx, float dy)
        {
            scene.AdjustPlayerMovementToCollisions(this, ref dx, ref dy);
            position += new Vector2(dx, dy);
        }

        public void SetPosition(float x, float y)
        {
            position = new Vector2(x, y);
        }

        public void SetPosition(Vector2Int pos)
        {
            position = pos;
        }

        public bool TouchingCharacter(Character other)
        {
            return Mathf.Pow(other.position.x - position.x, 2) + Mathf.Pow(other.position.y - position.y, 2) <= Mathf.Pow(radius + other.radius, 2);
        }
        public bool IsAlive()
        {
            return health > 0;
        }

    }

    //TODO: Think about whether or not Enemies should be stored basically the same way Characters are
    public class Enemy : Character
    {
        protected Vector2 fixedPosition;
        protected Vector2 targetPosition;
        protected float moveTime;
        protected float waitTime;
        protected bool isAtTarget = true;
        protected bool isDoneWaiting = true;
        protected float range = 5;
        protected float maxR = 7;

        public Enemy(Character baseCharacter, Vector2 pos)
        {
            this.myGame = baseCharacter.MyGame;
            this.myInfo = baseCharacter.MyInfo;
            this.zType = baseCharacter.MyInfo.ZType;
            this.health = baseCharacter.MyInfo.Health;
            //TODO deal with MP
            this.defense = baseCharacter.MyInfo.Defense;
            this.actions = baseCharacter.Actions;
            
            this.position = pos;
            this.fixedPosition = pos;
        }

        new public const int NUM_SERIALIZED_FIELDS = 10;
        new public object[] Serialize()
        {
            return new object[] { health, defense, mana, position, fixedPosition, targetPosition, moveTime, waitTime, isAtTarget, isDoneWaiting };
        }
        new public void Deserialize(object[] data)
        {
            health = (int)data[0];
            defense = (int)data[1];
            mana = (int)data[2];
            position = (Vector2)data[3];

            fixedPosition = (Vector2)data[4];
            targetPosition = (Vector2)data[5];
            moveTime = (float)data[6];
            waitTime = (float)data[7];
            isAtTarget = (bool)data[8];
            isDoneWaiting = (bool)data[9];
        }


        /// <summary>
        ///     Basic AI random walk
        /// </summary>
        /// <param name="scene">Reference to the scene the enemy is walking in to make sure that it chooses a traversable tile to walk on <see cref="TerrainTile.traversable"/></param>
        /// <param name="deltaTime">The amount of time that has passed in the last game loop</param>
        public void UpdateAI(WorldScene scene, float deltaTime)
        {
            if (isAtTarget && isDoneWaiting) {
                float newX; float newY;
                TerrainTile targetTile;
                do
                {
                    newX = Random.Range(position.x - range, position.x + range);
                    newY = Random.Range(position.y - range, position.y + range);
                    targetTile = scene.GetTileAt(newX, newY);
                }
                while (Vector2.Distance(fixedPosition, new Vector2(newX, newY)) > maxR || targetTile == null || !targetTile.Traversable);
                targetPosition = new Vector2(newX, newY);
                isAtTarget = false;

            } else if (isAtTarget && !isDoneWaiting) {
                waitTime -= deltaTime;
                if (waitTime <= 0)
                    isDoneWaiting = true;

            } else {
                moveTime = deltaTime / 3; //2 can be changed

                float dx = targetPosition.x - position.x; float dy = targetPosition.y - position.y;

                Move(scene, dx * moveTime, dy * moveTime);
                if (Vector2.Distance(position, targetPosition) <= 1.0f) {
                    isAtTarget = true;
                    isDoneWaiting = false;
                    waitTime = Random.Range(0.0f, 1f);
                }
                    

            }
            
        }
    }
}