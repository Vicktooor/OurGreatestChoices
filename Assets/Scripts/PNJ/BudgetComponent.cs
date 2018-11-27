using Assets.Scripts.Game;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class BudgetComponent
    {
        public EBudgetType type = EBudgetType.None;
        public List<EWorldImpactType> impacts = new List<EWorldImpactType>();
        public float initialBudget;
        public float workingLimitBudget;
        public float budget;
        public bool productBenefit = false;

        private float _investment;
        public float Investment
        {
            get { return _investment; }
            set { _investment = value; }
        }

        public bool Working { get { return budget >= workingLimitBudget; } }

        public BudgetComponent() { }
        public BudgetComponent(string pnjKey)
        {
            Init(pnjKey);
        }

        public void Init(string pnjKey)
        {
            NPCWrap info = null;
            int l = ResourcesManager.Instance.NPCs.objects.Count;
            for (int i = 0; i < l; i++)
            {
                NPCWrap wrap = ResourcesManager.Instance.NPCs.objects[i];
                if (wrap.ID == pnjKey)
                {
                    info = wrap;
                    break;
                }
            }
            if (info != null)
            {
                EWorldImpactType baseImpact = PropertyUtils.CastEnum<EWorldImpactType>(info.impactType);
                impacts.Add(baseImpact);
                type = ResourcesManager.GetBuildingType(baseImpact);

                BudgetValues values;
                if (ResourcesManager.Instance.BudgetValues.ContainsKey(baseImpact))
                {
                    values = ResourcesManager.Instance.BudgetValues[baseImpact];
                }
                else
                {
                    values = ResourcesManager.Instance.BudgetValues[EWorldImpactType.None];
                }

                initialBudget = values.initialBudget;
                workingLimitBudget = values.workingLimitBudget;
                budget = values.initialBudget;
                productBenefit = values.productBenefit;
            }
            SetWorking();
            ResourcesManager.Instance.AddBudgetComponent(this);
        }

        public bool GiveBudget()
        {
            if (InventoryPlayer.Instance.GetMoney(WorldValues.TRANSFERT_VALUE) != 0)
            {
                _investment += WorldValues.TRANSFERT_VALUE;
                budget += WorldValues.TRANSFERT_VALUE;
                SetWorking();
                return true;
            }
            return false;
        }

        public bool TakeBudget()
        {
            if (budget - WorldValues.TRANSFERT_VALUE < 0) return false;
            if (InventoryPlayer.Instance.AddMoney(WorldValues.TRANSFERT_VALUE))
            {
                _investment -= WorldValues.TRANSFERT_VALUE;
                budget -= WorldValues.TRANSFERT_VALUE;
                SetWorking();
                return true;
            }
            return false;
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

        public void SetWorking()
        {
            if (type == EBudgetType.CarsCompany || type == EBudgetType.CoalPowerPlant)
            {
                if (Working)
                {
                    if (!impacts.Contains(EWorldImpactType.None)) impacts.Add(EWorldImpactType.None);
                }
                else
                {
                    if (impacts.Contains(EWorldImpactType.None)) impacts.Remove(EWorldImpactType.None);
                }
            }
            else if (type == EBudgetType.TownHall)
            {
                if (Working)
                {
                    if (!impacts.Contains(EWorldImpactType.BankruptedTownHall)) impacts.Add(EWorldImpactType.BankruptedTownHall);
                }
                else
                {
                    if (impacts.Contains(EWorldImpactType.BankruptedTownHall)) impacts.Remove(EWorldImpactType.BankruptedTownHall);
                }
            }
        }

        public void SetNewImpact(EWorldImpactType type)
        {
            if (!impacts.Contains(type)) impacts.Add(type);
        }

        public void RemoveImpact(EWorldImpactType type)
        {
            if (impacts.Contains(type)) impacts.Remove(type);
        }

        public BudgetsSave GenerateSave()
        {
            BudgetsSave newSave = new BudgetsSave();
            newSave.budget = budget;
            newSave.investment = _investment;
            return newSave;
        }
    }
}