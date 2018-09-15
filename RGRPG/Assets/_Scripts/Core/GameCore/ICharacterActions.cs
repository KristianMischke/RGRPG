using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public interface ICharacterActions
    {

        void DoAction(List<Character> targets);

        string GetName();

    }


    public class AttackAction : ICharacterActions
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

        public string GetName()
        {
            return "Attack";
        }

    }
}