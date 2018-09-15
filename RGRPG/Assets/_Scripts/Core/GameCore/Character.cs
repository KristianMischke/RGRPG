using System.Collections;
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

    public class Character
    {
        protected CharacterType type;
        protected string name;

        protected int health;
        protected int attack; //TODO: we need to consider how we are storing the attack (should it be in the character or the action or both?
        protected int defense;

        protected Vector2Int position;

        protected List<ICharacterAction> actions;

        public CharacterType Type { get { return type; } }
        public string Name { get { return name; } }
        public int Health { get { return health; } }
        public int Attack { get { return attack; } }
        public int Defense { get { return defense; } }
        public List<ICharacterAction> Actions { get { return actions; } }
        public Vector2Int Position { get { return position; } }

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
            health -= amount;
        }

        public void Heal(int amount)
        {
            health += amount;
        }

        public void Move(int x, int y)
        {
            position += new Vector2Int(x, y);
        }

        public void SetPosition(int x, int y)
        {
            position = new Vector2Int(x, y);
        }

    }
}