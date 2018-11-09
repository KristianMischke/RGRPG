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
        protected InfoCharacter myInfo;
        protected string zType;

        protected int health;
        protected int attack; //TODO: we need to consider how we are storing the attack (should it be in the character or the action or both?
        protected int defense;
        protected int mana;

        protected Vector2 position;
        protected float radius = 0.5f;

        protected List<ICharacterAction> actions;
        public InfoCharacter MyInfo { get {return myInfo;}}
        public string Type { get { return zType; } }
        public string ClassType { get { return myInfo.Class; } }        
        public string Name { get { return myInfo.Name; } }
        public int Health { get { return health; } }
        public int Attack { get { return attack; } }
        public int Defense { get { return defense; } }
        public int Mana { get { return mana; } }
        public List<ICharacterAction> Actions { get { return actions; } }
        public Vector2 Position { get { return position; } }
        public float Radius { get { return radius; } }

        public Character() { }

        public Character(Game myGame, string zType, List<ICharacterAction> actions)
        {
            this.zType = zType;
            this.myInfo = myGame.Infos.Get<InfoCharacter>(zType);
            this.health = myInfo.Health;
            //TODO deal with MP
            this.defense = myInfo.Defence;
            
            this.actions = actions; //TODO: load actions from infos
        }

        public void Damage(int amount)
        {
            if (defense >= amount)
                Debug.Log("Deflected damage!");
            else if (health <= 0)
                health = 0;
            else
                health -= amount - defense;
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
        }

        public void SetMana(int amount)
        {
            mana = amount;
        }

        public void ChangeMana(int amount)
        {
            mana += amount;
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

        public bool TouchingCharacter(Character other)
        {
            return Mathf.Pow(other.position.x - position.x, 2) + Mathf.Pow(other.position.y - position.y, 2) <= Mathf.Pow(radius + other.radius, 2);
        }
        public bool IsAlive()
        {
            return health > 0;
        }

    }

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

        public Enemy(Game myGame, string zType, Vector2 pos, List<ICharacterAction> actions)
        {
            this.zType = zType;
            this.myInfo = myGame.Infos.Get<InfoCharacter>(zType);
            this.health = myInfo.Health;
            //TODO deal with MP
            this.defense = myInfo.Defence;

            this.actions = actions; //TODO: load actions from infos
            
            this.position = pos;
            this.fixedPosition = pos;
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