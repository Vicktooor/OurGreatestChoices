using Assets.Scripts.Game;
using System;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    [Serializable]
    public struct FloatPair
    {
        public float val;
        public float multiplicator;
    }

    [Serializable]
    public struct BudgetStepInfo
    {
        public float targetBudget;
        public BudgetWorldValues worldValues;
    }

    [Serializable]
    public struct BudgetWorldValues
    {
        public FloatPair forest;
        public FloatPair npc;
        public FloatPair economy;
        public FloatPair cleanliness;
    }

    [Serializable]
    public class BudgetComponent
    {
        [Header("World values impact")]
        public BudgetWorldValues worldValues;

        [Header("Economic properties => minBudget != 0 = ON/OFF building")]
        public string name;
        public float minBudget;
        public float targetBudget;
        public bool productBenefit = false;

        public float budget;

        private float _investment;
        public float Investment
        {
            get { return _investment; }
            set { _investment = value; }
        }

        public BudgetStepInfo[] bubgetSteps;
        public string[] budgetLinks;

        private int budgetState = 0;
        private bool working = true;
        public bool Working { get { return working; } }

        public EnumClass.TypeBuilding buildingType;

        public void SetWorking()
        {
            if (minBudget > 0)
            {
                if (budget < minBudget) working = false;
                else working = true;
            }
        }

        public bool TargetBudgetAchieved { get { return budget >= targetBudget; } }

        public void GiveBudget()
        {
            if (InventoryPlayer.instance.GetMoney(WorldValues.TRANSFERT_VALUE) != 0)
            {
                _investment += WorldValues.TRANSFERT_VALUE;
                budget += WorldValues.TRANSFERT_VALUE;
                SetWorking();
                Events.Instance.Raise(new OnReceiveBudget().Init(this));
            }
        }

        public void TakeBudget()
        {
            if (budget - WorldValues.TRANSFERT_VALUE < 0) return;
            if (InventoryPlayer.instance.AddMoney(WorldValues.TRANSFERT_VALUE))
            {
                _investment -= WorldValues.TRANSFERT_VALUE;
                budget -= WorldValues.TRANSFERT_VALUE;
                SetWorking();
                Events.Instance.Raise(new OnGiveBudget().Init(this));
            }
        }

        public void PassAYear()
        {
            float newBudget = 0f;
            newBudget += (WorldValues.BOOST_TARGET_VALUE * _investment);
            if (productBenefit)
            {
                newBudget += budget * (1f + (0.1f * WorldValues.STATE_ECONOMY));
            }
            else newBudget += budget;
            budget = newBudget;
            if (budget < 0) budget = 0;
            _investment = 0;

            SetWorking();
        }

        public BudgetGlobalImpact GetWorldImpact()
        {
            BudgetGlobalImpact impact = new BudgetGlobalImpact();

            float newBudget = Investment;
            if (productBenefit) newBudget += budget * (1f + (0.1f * WorldValues.STATE_ECONOMY));
            else newBudget += budget;

            if (working)
            {
                impact.forest = worldValues.forest.val + (budget * worldValues.forest.multiplicator);
                impact.npc = worldValues.npc.val + (budget * worldValues.npc.multiplicator);
                impact.economy = worldValues.economy.val + (budget * worldValues.economy.multiplicator);
                impact.cleanliness = worldValues.cleanliness.val + (budget * worldValues.cleanliness.multiplicator);
            }
            else if (name == "Dryport City" || name == "Hillside" || name == "Neo New Ville")
            {
                impact.npc = -1.5f + (newBudget * 0.1f);
                impact.cleanliness = -0.5f + (newBudget * 0.03f);
            }

            return impact;
        }

        public static BudgetGlobalImpact GetBonusImpact(BudgetWorldValues values)
        {
            BudgetGlobalImpact impact = new BudgetGlobalImpact();
            impact.forest = values.forest.val;
            impact.npc = values.npc.val;
            impact.economy = values.economy.val;
            impact.cleanliness = values.cleanliness.val;
            return impact;
        }

        public void SetBudgetStep(int level = -1)
        {
            if (level < 0) return;
            else if (budgetState < bubgetSteps.Length && level < bubgetSteps.Length)
            {
                targetBudget = bubgetSteps[level].targetBudget;
                worldValues = bubgetSteps[level].worldValues;
                budgetState = level;
            }
        }
    }
}