﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{

    //TODO: change these
    public enum CharacterType
    {
        Player,
        Enemy,

        COUNT
    }


    /// <summary>
    /// The Character class is data representation that stores all the attributes of a character
    /// </summary>
    /// <remarks>
    /// Both players and enemies classify as "characters"
    /// Data such as health, position and battle actions are stored here
    /// 
    /// Debating on whether or not we should put attack and defense here,
    /// because we are also dealing with similar structures in the ICharacterActions class (<see cref="RGRPG.Core.ICharacterAction"/>)
    /// 
    /// Characters are controlled by Game.cs (<see cref="RGRPG.Core.Game"/>) and is also used as a data representation in UI elements
    /// </remarks>
    public class Character
    {
        protected CharacterType type;
        protected string name;

        protected int health;
        protected int attack; //TODO: we need to consider how we are storing the attack (should it be in the character or the action or both?
        protected int defense;
        protected int mana;

        protected Vector2 position;
        protected float radius = 0.5f;

        protected List<ICharacterAction> actions;

        public CharacterType Type { get { return type; } }
        public string Name { get { return name; } }
        public int Health { get { return health; } }
        public int Attack { get { return attack; } }
        public int Defense { get { return defense; } }
        public int Mana { get { return mana; } }
        public List<ICharacterAction> Actions { get { return actions; } }
        public Vector2 Position { get { return position; } }
        public float Radius { get { return radius; } }

        public Character() { }

        public Character(CharacterType type, string name, int health, int attack, int defense, List<ICharacterAction> actions)
        {
            this.type = type;
            this.name = name;
            this.health = health;
            this.attack = attack;
            this.defense = defense;
            this.actions = actions;
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

        public Enemy(Vector2 pos, string name, int health, int attack, int defense, List<ICharacterAction> actions)
        {
            this.position = pos;
            this.fixedPosition = pos;
            this.type = CharacterType.Enemy;
            this.name = name;
            this.health = health;
            this.attack = attack;
            this.defense = defense;
            this.actions = actions;
        }


        /// <summary>
        /// Basic AI random walk
        /// </summary>
        /// <param name="scene">Reference to the scene the enemy is walking in to make sure that it chooses a traversable tile to walk on <see cref="RGRPG.Core.TerrainTile.traversable"/></param>
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