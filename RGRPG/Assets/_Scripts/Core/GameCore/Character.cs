using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{

    public class Character
    {
        protected string name;

        protected int health;
        protected int attack; //TODO: we need to consider how we are storing the attack (should it be in the character or the action or both?
        protected int defense;

        protected Vector3 position;

        protected List<ICharacterAction> actions;

        public string Name { get { return name; } }
        public int Health { get { return health; } }
        public int Attack { get { return attack; } }
        public int Defense { get { return defense; } }
        public List<ICharacterAction> Actions { get { return actions; } }
        public Vector3 Position { get { return position; } }

        public Character() { }

        public Character(string name, int health, int attack, int defense, List<ICharacterAction> actions)
        {
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

        public void Move(int x, int y, int z) {

            position += new Vector3(x, y, z);

        }

    }
}