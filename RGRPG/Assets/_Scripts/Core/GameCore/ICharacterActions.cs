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
        InfoAction MyInfo { get; }

        /// <summary>
        ///     Method that reports the targets of this action
        /// </summary>
        /// <returns></returns>
        List<Character> GetTargets();

        /// <summary>
        ///     Method that gives this action (a) target(s)
        /// </summary>
        /// <param name="targets"></param>
        void SetTargets(List<Character> targets);

        /// <summary>
        ///     method to execute this action
        /// </summary>
        void DoAction(Character source);

        /// <summary>
        ///     How much mana will be spent when using this action
        /// </summary>
        /// <returns></returns>
        int ManaCost();

        /// <summary>
        ///     The name of the action
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        ///     String displayed on the action button
        /// </summary>
        /// <returns></returns>
        string GetDisplayText();

        /// <summary>
        ///     The string that explains the details of the aaction
        /// </summary>
        /// <returns></returns>
        string GetHelpText();
    }

    /// <summary>
    ///     Basic, no-op action that is spawned if there was a failure detecting the correct action
    /// </summary>
    public class DudAction : ICharacterAction
    {
        public InfoAction MyInfo { get { return null; } }
        public DudAction() { }
        public List<Character> GetTargets() { return null; }
        public void SetTargets(List<Character> targets) { }
        public void DoAction(Character source) { }

        public int ManaCost() { return 0; }

        public string GetName() { return "DUD ACTION"; }
        public string GetDisplayText() { return "<ERROR>"; }
        public string GetHelpText() { return "Basic, no-op action that is spawned if there was a failure detecting the correct action"; }
    }

    /// <summary>
    ///     Basic, no-op action for marking the beginning of a characters turn
    /// </summary>
    public class BeginTurnAction : ICharacterAction
    {
        public InfoAction MyInfo { get { return null; } }
        public BeginTurnAction() { }
        public List<Character> GetTargets() { return null; }
        public void SetTargets(List<Character> targets) { }
        public void DoAction(Character source) { }

        public int ManaCost() { return 0; }

        public string GetName() { return "BEGIN TURN"; }
        public string GetDisplayText() { return ""; }
        public string GetHelpText() { return ""; }
    }

    /// <summary>
    ///     Basic, no-op action for marking a character passing their turn
    /// </summary>
    public class PassTurnAction : ICharacterAction
    {
        public InfoAction MyInfo { get { return null; } }
        public PassTurnAction() { }
        public List<Character> GetTargets() { return null; }
        public void SetTargets(List<Character> targets) { }
        public void DoAction(Character source) { }

        public int ManaCost() { return 0; }

        public string GetName() { return "PASS TURN"; }
        public string GetDisplayText() { return ""; }
        public string GetHelpText() { return ""; }
    }


    /// <summary>
    ///     The basic attack action which can vary in damage and mana cost
    /// </summary>
    public class AttackAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int amount;
        private int manaCost;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public AttackAction() { }

        public void Init(InfoAction myInfo, int damageAmount, int manaCost)
        {
            this.myInfo = myInfo;
            this.amount = damageAmount;
            this.manaCost = manaCost;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {
            foreach (Character c in targets)
            {
                c.Damage(amount);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "ATTACK"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper() + " " + amount; }

        public string GetHelpText() { return "Deals " + amount + " to the targets"; }

    }

    /// <summary>
    ///     The basic defend action which can vary in shield and mana cost
    /// </summary>
    public class DefendAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int amount;
        private int manaCost;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public DefendAction() { }

        public void Init(InfoAction myInfo, int amount, int manaCost)
        {
            this.myInfo = myInfo;
            this.amount = amount;
            this.manaCost = manaCost;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {
            foreach (Character c in targets)
            {
                c.SetShield(amount);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "DEFEND"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper() + " " + amount; }

        public string GetHelpText() { return "Prevents " + amount + " damage from hitting the targets"; }
    }

    /// <summary>
    ///     The basic heal action which can vary in heal amount and mana cost
    /// </summary>
    public class HealAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int amount;
        private int manaCost;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public HealAction() { }

        public void Init(InfoAction myInfo, int healAmount, int manaCost)
        {
            this.myInfo = myInfo;
            this.amount = healAmount;
            this.manaCost = manaCost;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {
            foreach (Character c in targets)
            {
                c.Heal(amount);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "HEAL"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper() + " " + amount; }

        public string GetHelpText() { return "Restores " + amount + " health to the targets"; }

    }

    /////////////////////////////////////Actions implemented for Infos

    public class LifeDrainAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int amount;
        private int manaCost;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public LifeDrainAction() { }

        public void Init(InfoAction myInfo, int drainAmount, int manaCost)
        {
            this.myInfo = myInfo;
            this.amount = drainAmount;
            this.manaCost = manaCost;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {
            source.Heal(amount);
            foreach (Character c in targets)
            {
                c.Damage(amount / targets.Count);
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "HEAL"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper() + " " + amount; }

        public string GetHelpText() { return "Restores " + amount + " health to the targets"; }

    }



    public class NecromancerAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int amount;
        private int manaCost;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public NecromancerAction() { }

        public void Init(InfoAction myInfo, int manaCost)
        {
            this.myInfo = myInfo;
            this.manaCost = manaCost;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {
            //source.Heal(amount);
            foreach (Character c in targets)
            {
                amount = c.MyInfo.Health / 2;
                if (!c.IsAlive())
                {
                    c.Heal(amount);
                }
            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "HEAL"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper() + " " + amount; }

        public string GetHelpText() { return "Restores " + amount + " health to the targets"; }

    }

    public class CounterAttackAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int amount;
        private int manaCost;
        private float counterPercent;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public CounterAttackAction() { }

        public void Init(InfoAction myInfo, float counterPercent, int manaCost)
        {
            this.myInfo = myInfo;
            this.manaCost = manaCost;
            this.counterPercent = counterPercent;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {

            amount = 10
            foreach (Character c in targets)
            {
                if (!c.IsAlive())
                {
                    float damage = amount * 0.25f;

                    c.Damage((int)Mathf.Round(damage));
                }

            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "HEAL"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper() + " " + amount; }

        public string GetHelpText() { return "Restores " + amount + " health to the targets"; }

    }

    public class ShiningBladeAction : ICharacterAction
    {
        private List<Character> targets = new List<Character>();

        private int damageAmount;
        private int manaCost;
        private InfoAction myInfo;

        public InfoAction MyInfo { get { return myInfo; } }

        public ShiningBladeAction() { }

        public void Init(InfoAction myInfo, int manaCost, int damageAmount)
        {
            this.myInfo = myInfo;
            this.manaCost = manaCost;
            this.damageAmount = damageAmount;
        }

        public List<Character> GetTargets()
        {
            return targets;
        }

        public void SetTargets(List<Character> targets)
        {
            this.targets = targets;
        }

        public void DoAction(Character source)
        {
            foreach (Character c in targets)
            {
                int critDamage = damageAmount * 2;
                //This should work? I'm not entirely sure how Random.Range works
                int critChance = (int)Random.Range(1f, 8f);
                if (critChance == 4)
                    c.Damage(critDamage);

            }
        }

        public int ManaCost()
        {
            return manaCost;
        }

        public string GetName() { return "SHINING BLADE"; }

        public string GetDisplayText() { return myInfo.Name.ToUpper(); }

        public string GetHelpText() { return "Adds a 1/8 chance to deal a critical hit to a target"; }

    }
}