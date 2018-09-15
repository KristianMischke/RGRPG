using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public interface ICharacterAction
    {
        void DoAction(List<Character> targets);
        string GetName();
        int GetAmount();
        bool HasAmount();
    }


    public class AttackAction : ICharacterAction
    {

        private int damage;

        public AttackAction(int damage)
        {
            this.damage = damage;
        }

        public void DoAction(List<Character> targets)
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
}