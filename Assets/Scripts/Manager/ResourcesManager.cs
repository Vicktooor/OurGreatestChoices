using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Game;
using System.Collections;
using Assets.Scripts.Game.Save;

public struct BudgetGlobalImpact
{
    public float forest;
    public float npc;
    public float economy;
    public float cleanliness;
}

namespace Assets.Scripts.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public class ResourcesManager : MonoBehaviour
    {
        [SerializeField]
        private List<BudgetComponent> budgetComponents = new List<BudgetComponent>();
        [SerializeField]
        private List<BudgetWorldValues> bonusValues = new List<BudgetWorldValues>();
        private Dictionary<string, float> boostedBudget = new Dictionary<string, float>();

        private static ResourcesManager _instance;

        /// <summary>
        /// instance unique de la classe     
        /// </summary>
        public static ResourcesManager instance
        {
            get
            {
                return _instance;
            }
        }

        protected void Awake()
        {
            if (_instance != null)
            {
                throw new Exception("Tentative de création d'une autre instance de ResourcesManager alors que c'est un singleton.");
            }
            _instance = this;
            Events.Instance.AddListener<OnBudgetLoaded>(OnBudgetLoaded);
        }

        protected void OnBudgetLoaded(OnBudgetLoaded e)
        {
            foreach (SaveBudgetComponent sc in PlanetSave.BudgetElements)
            {
                AddBudgetComponent(sc.budgetComp);
            }
        }

        public void AddBudgetComponent(BudgetComponent bc)
        {
            if (!budgetComponents.Contains(bc)) budgetComponents.Add(bc);
        }

        public void RemoveBudgetComponent(BudgetComponent bc)
        {
            if (budgetComponents.Contains(bc)) budgetComponents.Remove(bc);
        }

        public void AddBonusImpact(BudgetWorldValues values)
        {
            bonusValues.Add(values);
        }

        public void RemoveImpact(BudgetWorldValues values)
        {
            bonusValues.Remove(values);
        }

        public void UpdateBudget()
        {
            WorldValues.NewYear(budgetComponents, bonusValues);
            foreach (BudgetComponent bc in budgetComponents) bc.PassAYear();
        }

        public BudgetComponent GetBudgetComponent(string testName)
        {
            foreach (BudgetComponent bc in budgetComponents)
            {
                if (bc.name == testName) return bc;
            }
            return null;
        }

        public Dictionary<BudgetComponent, GameObject> GetBudgetBuildingsProp(string bName)
        {
            Dictionary<BudgetComponent, GameObject> linkedBudgets = new Dictionary<BudgetComponent, GameObject>();

            BudgetComponent bc = budgetComponents.Find(e => e.name == bName);
            InteractablePNJ pnj = InteractablePNJ.PNJs.Find(e => e.budgetComponent.name == bName);
            if (bc != null && pnj != null)
            {
                if (pnj.propLinked) linkedBudgets.Add(bc, pnj.propLinked);
                else linkedBudgets.Add(bc, pnj.gameObject);
                for (int i = 0; i < pnj.budgetComponent.budgetLinks.Length; i++)
                {
                    string lName = pnj.budgetComponent.budgetLinks[i];
                    BudgetComponent lBc = budgetComponents.Find(e => e.name == lName);
                    InteractablePNJ lPnj = InteractablePNJ.PNJs.Find(e => e.budgetComponent.name == lName);
                    if (lPnj.propLinked) linkedBudgets.Add(lBc, lPnj.propLinked);
                    else linkedBudgets.Add(lBc, lPnj.gameObject);
                }
            }
            return linkedBudgets;
        }

        protected void OnDestroy()
        {
            Events.Instance.RemoveListener<OnBudgetLoaded>(OnBudgetLoaded);
            _instance = null;
        }

        public void Clear()
        {
            budgetComponents.Clear();
            boostedBudget.Clear();
            bonusValues.Clear();
        }
    }
}