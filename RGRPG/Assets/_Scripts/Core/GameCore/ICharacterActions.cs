using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public interface ICharacterAction
    {
        void SetTargets(List<Character> targets);
        void DoAction();
        int ManaCost();
        string GetName();
        int GetAmount();
        bool HasAmount();
    }


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

        public string GetName() { return "Attack"; }

        public int GetAmount() { return damage; }

        public bool HasAmount() { return true; }

    }

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

        public string GetName() { return "Defend"; }

        public int GetAmount() { return shield; }

        public bool HasAmount() { return true; }

    }

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

        public string GetName() { return "Heal"; }

        public int GetAmount() { return heal; }

        public bool HasAmount() { return true; }

    }
}