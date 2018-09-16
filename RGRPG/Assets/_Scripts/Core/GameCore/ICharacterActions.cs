using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public interface ICharacterAction
    {
        void SetTargets(List<Character> targets);
        void DoAction();
        string GetName();
        int GetAmount();
        bool HasAmount();
    }


    public class AttackAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int damage;

        public AttackAction(int damage)
        {
            this.damage = damage;
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

        public string GetName() { return "Attack"; }

        public int GetAmount() { return damage; }

        public bool HasAmount() { return true; }

    }

    public class DefendAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int shield;

        public DefendAction(int shield)
        {
            this.shield = shield;
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

        public string GetName() { return "Defend"; }

        public int GetAmount() { return shield; }

        public bool HasAmount() { return true; }

    }
}