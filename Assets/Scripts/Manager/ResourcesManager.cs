using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NGO;

[Serializable]
public class BudgetsSave
{
    public string npcName;
    public float budget;
    public float investment;
}

[Serializable]
public class FloatPair
{
    public float val;
    public float multiplicator;
}

[Serializable]
public class BudgetWorldValues
{
    public FloatPair forest;
    public FloatPair npc;
    public FloatPair economy;
    public FloatPair cleanliness;
}

public enum EWorldImpactType
{
    None,
    CERN,
    BasicTownHall,
    BankruptedTownHall,
    FruitMarketTownHall,
    WoodyTownHall,
    VegetableTownHall,
    ElecTownHallV1,
    ElecTownHallV2,
    School,
    Metro,
    CarsCompany,
    ElecCarsCompanyV1,
    ElecCarsCompanyV2,
    CoalPowerPlant,
    RenewableCoalPowerPlant,
    MaritimeAssociation,
    OilPlant
}

public enum EBudgetType
{
    None,
    TownHall,
    CarsCompany,
    CERN,
    CoalPowerPlant
}

public struct BudgetGlobalImpact
{
    public float forest;
    public float npc;
    public float economy;
    public float cleanliness;
}

[Serializable]
public class NPCWrap
{
    public string ID;
    public string impactType;
    public string NPCText;
}

[Serializable]
public class NPCWrapper
{
    public List<NPCWrap> objects;
}

[Serializable]
public class BudgetValues
{
    public float initialBudget;
    public float workingLimitBudget;
    public float targetBudget;
    public bool productBenefit = false;
}

[Serializable]
public class BudgetWrap
{
    public string key;
    public BudgetValues value;
}
[Serializable]
public class BudgetWrapper
{
    public List<BudgetWrap> objects;
}

[Serializable]
public class ImpactWrap
{
    public string key;
    public BudgetWorldValues value;
}
[Serializable]
public class ImpactsWrapper
{
    public List<ImpactWrap> objects;
}

namespace Assets.Scripts.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public class ResourcesManager : MonoSingleton<ResourcesManager>
    {
        public string npcPath;
        public string valuesPath;
        public string impactsPath;
        public string dialoguesPath;

        public List<ContractorTopic> contractTopics;
        public List<GovernmentTopic> govTopics;
        public List<Item> ItemModels;

        private NPCWrapper _npcs;
        public NPCWrapper NPCs { get { return _npcs; } }

        private List<BudgetComponent> _budgetComponents = new List<BudgetComponent>();

        private BudgetWrapper _budgets;
        private Dictionary<EWorldImpactType, int> _bonusValues = new Dictionary<EWorldImpactType, int>();
        private Dictionary<EWorldImpactType, BudgetValues> _budgetValues = new Dictionary<EWorldImpactType, BudgetValues>();
        public Dictionary<EWorldImpactType, BudgetValues> BudgetValues { get { return _budgetValues; } }

        private ImpactsWrapper _impacts;
        private Dictionary<EWorldImpactType, BudgetWorldValues> _impactsDatabase = new Dictionary<EWorldImpactType, BudgetWorldValues>();

        public void Init()
        {
            ItemModels = new List<Item>();
            foreach (EItemType it in Enum.GetValues(typeof(EItemType)))
            {
                if (it != EItemType.None) ItemModels.Add(Resources.Load<Item>("Items/" + it.ToString()));
            }

            _impacts = StreamingAssetAccessor.FromJson<ImpactsWrapper>(impactsPath);
            List<KeyValuePair<string, BudgetWorldValues>> pairImpact = FileManager.GenerateList<BudgetWorldValues, ImpactWrap>(_impacts.objects);
            DicWrapper<BudgetWorldValues> wrapperImpact = FileManager.GenerateDicWrapper(pairImpact);
            _impactsDatabase = FileManager.GenerateDicFromJson<EWorldImpactType, BudgetWorldValues>(wrapperImpact);
            foreach (KeyValuePair<EWorldImpactType, BudgetWorldValues> item in _impactsDatabase) _bonusValues.Add(item.Key, 0);

            _budgets = StreamingAssetAccessor.FromJson<BudgetWrapper>(valuesPath);
            List<KeyValuePair<string, BudgetValues>> pairBudget = FileManager.GenerateList<BudgetValues, BudgetWrap>(_budgets.objects);
            DicWrapper<BudgetValues> wrapperBudget = FileManager.GenerateDicWrapper(pairBudget);
            _budgetValues = FileManager.GenerateDicFromJson<EWorldImpactType, BudgetValues>(wrapperBudget);

            _npcs = StreamingAssetAccessor.FromJson<NPCWrapper>(npcPath);

            DialogueWrapper dialogueWrapper = StreamingAssetAccessor.FromJson<DialogueWrapper>(dialoguesPath);
            List<KeyValuePair<string, DialoguePNJ>> dialoguePairs = FileManager.GenerateList<DialoguePNJ, DialogueWrap>(dialogueWrapper.objects);
            DicWrapper<DialoguePNJ> wrapperDialogue = FileManager.GenerateDicWrapper(dialoguePairs);
            InteractablePNJ.DialoguesDatabase = FileManager.GenerateDicFromJson(wrapperDialogue);
        }

        public void AddBudgetComponent(BudgetComponent bc)
        {
            _budgetComponents.Add(bc);
        }

        public void AddBonusImpact(EWorldImpactType bonusType)
        {
            _bonusValues[bonusType]++;
        }

        public void RemoveBonusImpact(EWorldImpactType bonusType)
        {
            _bonusValues[bonusType]--;
        }

        public void UpdateBudget()
        {
            BudgetGlobalImpact fullImpact = GenerateGlobalImpact();
            WorldValues.NewYear(fullImpact);      
        }

        private BudgetGlobalImpact GenerateGlobalImpact()
        {
            BudgetGlobalImpact globalImpact = new BudgetGlobalImpact();
            foreach (BudgetComponent bc in _budgetComponents)
            {
                BudgetWorldValues impact;
                if (!bc.impacts.Contains(EWorldImpactType.None))
                {
                    foreach (EWorldImpactType e in bc.impacts.ToArray())
                    {
                        impact = _impactsDatabase[e];
                        globalImpact.cleanliness += impact.cleanliness.val + (bc.budget * impact.cleanliness.multiplicator);
                        globalImpact.economy += impact.economy.val + (bc.budget * impact.economy.multiplicator);
                        globalImpact.forest += impact.forest.val + (bc.budget * impact.forest.multiplicator);
                        globalImpact.npc += impact.npc.val + (bc.budget * impact.npc.multiplicator);
                        bc.PassAYear();
                    }
                }
            }
            foreach (KeyValuePair<EWorldImpactType, int> bonus in _bonusValues)
            {
                BudgetWorldValues impact = _impactsDatabase[bonus.Key];
                for (int i = 0; i < bonus.Value; i++)
                {
                    globalImpact.cleanliness += impact.cleanliness.val;
                    globalImpact.economy += impact.economy.val;
                    globalImpact.forest += impact.forest.val;
                    globalImpact.npc += impact.npc.val;
                }
            }
            return globalImpact;
        }

        public void Clear()
        {
            _budgetComponents.Clear();
            _bonusValues.Clear();
        }

        public static EBudgetType GetBuildingType(EWorldImpactType impactType)
        {
            switch (impactType)
            {
                case EWorldImpactType.None:
                    return EBudgetType.None;
                case EWorldImpactType.CERN:
                    return EBudgetType.CERN;
                case EWorldImpactType.BasicTownHall:
                    return EBudgetType.TownHall;
                case EWorldImpactType.BankruptedTownHall:
                    return EBudgetType.TownHall;
                case EWorldImpactType.FruitMarketTownHall:
                    return EBudgetType.TownHall;
                case EWorldImpactType.WoodyTownHall:
                    return EBudgetType.TownHall;
                case EWorldImpactType.VegetableTownHall:
                    return EBudgetType.TownHall;
                case EWorldImpactType.ElecTownHallV1:
                    return EBudgetType.TownHall;
                case EWorldImpactType.ElecTownHallV2:
                    return EBudgetType.TownHall;
                case EWorldImpactType.School:
                    return EBudgetType.None;
                case EWorldImpactType.CarsCompany:
                    return EBudgetType.CarsCompany;
                case EWorldImpactType.ElecCarsCompanyV1:
                    return EBudgetType.CarsCompany;
                case EWorldImpactType.ElecCarsCompanyV2:
                    return EBudgetType.CarsCompany;
                case EWorldImpactType.CoalPowerPlant:
                    return EBudgetType.CoalPowerPlant;
                case EWorldImpactType.RenewableCoalPowerPlant:
                    return EBudgetType.CoalPowerPlant;
                case EWorldImpactType.MaritimeAssociation:
                    return EBudgetType.None;
                case EWorldImpactType.OilPlant:
                    return EBudgetType.None;
                default:
                    return EBudgetType.None;
            }
        }
    }
}