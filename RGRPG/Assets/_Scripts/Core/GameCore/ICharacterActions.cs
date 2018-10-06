using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Abstract interface that stores the blueprint for character's combat actions
    /// </summary>
    /// <remarks>
    ///     Allows for multiple targets
    /// </remarks>
    public interface ICharacterAction
    {
        void SetTargets(List<Character> targets);
        void DoAction();
        int ManaCost();
        string GetName();
        int GetAmount();
        bool HasAmount();
    }


    /// <summary>
    ///     The basic attack action which can vary in damage and mana cost
    /// </summary>
    public class AttackAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int damage;
        private int manaCost;

        public AttackAction(int damage, int manaCost)
        {
            this.damage = damage;
            this.manaCost = manaCost;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction()
        {
            foreach (Character c in targets)
            {
                c.Damage(damage);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "ATTACK"; }

        public int GetAmount() { return damage; }

        public bool HasAmount() { return true; }

    }

    /// <summary>
    ///     The basic defend action which can vary in shield and mana cost
    /// </summary>
    public class DefendAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int shield;
        private int manaCost;

        public DefendAction(int shield, int manaCost)
        {
            this.shield = shield;
            this.manaCost = manaCost;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction()
        {
            foreach (Character c in targets)
            {
                c.SetShield(shield);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "DEFEND"; }

        public int GetAmount() { return shield; }

        public bool HasAmount() { return true; }

    }

    /// <summary>
    ///     The basic heal action which can vary in heal amount and mana cost
    /// </summary>
    public class HealAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int heal;
        private int manaCost;

        public HealAction(int heal, int manaCost)
        {
            this.heal = heal;
            this.manaCost = manaCost;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction()
        {
            foreach (Character c in targets)
            {
                c.Heal(heal);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "HEAL"; }

        public int GetAmount() { return heal; }

        public bool HasAmount() { return true; }

    }
}