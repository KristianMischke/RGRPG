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

        protected List<ICharacterActions> actions;

        public string Name { get { return name; } }
        public int Health { get { return health; } }
        public int Attack { get { return attack; } }
        public int Defense { get { return defense; } }
        public List<ICharacterActions> Actions { get { return Actions; } }

        public Character() { }

        public Character(string name, int health, int attack, int defense, List<ICharacterActions> actions)
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

    }
}