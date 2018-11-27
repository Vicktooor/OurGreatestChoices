using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_TownHall : InteractablePNJ {

    [SerializeField]
    private List<GameObject> normalHousesLinked;

    [SerializeField]
    private List<GameObject> citizensList;

    [SerializeField]
    private List<GameObject> metroLinked;

    private bool _haveTracks = false;
    private bool _haveFruitSeed = false;
    private bool _haveElectricity = false;
    private bool _haveGreenElectricity = false;
    private bool _haveVegetableGarden = false;
    private bool _haveFruitBasket = false;

    private bool _metroCreated = false;
    private bool _fruitSeedPlanted = false;
    private bool _electrityHouse = false;
    private bool _greenelectricityHouse = false;
    private bool _gardenAvailable = false;
    private bool _fruitMarketOpen = false;

    private bool _happy = true;

    protected override void Awake()
    {
        base.Awake();
        neededItems = new List<EItemType>()
        {
            EItemType.Tracks,
            EItemType.FruitMarket,
            EItemType.FruitSeed,
            EItemType.Garden,
            EItemType.Electricity,
            EItemType.GreenElectricity
        };
    }

    protected override void CatchGivedObject()
    {
        if (InventoryPlayer.Instance.givedOject.Count <= 0) return;
        if (InventoryPlayer.Instance.givedOject.ContainsKey(IDname))
        {
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.Electricity))
            {
                _haveElectricity = true;
                if (HaveBudget(EWorldImpactType.ElecTownHallV1)) UpdateHousesElectricity();
            }
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.GreenElectricity))
            {
                _haveGreenElectricity = true;
                if (HaveBudget(EWorldImpactType.ElecCarsCompanyV2)) UpdateHousesGreenElectricity();
            }
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.FruitSeed))
            {
                _haveFruitSeed = true;
                UpdateTrees();
            }
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.FruitMarket))
            {
                _haveFruitBasket = true;
                ShowFruitBasket();
            }
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.Garden))
            {
                _haveVegetableGarden = true;
                UpdateVegetableGardens();
            }
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.Tracks))
            {
                SetMetro(false);
            }
        }
        OnUpdate(null);
    }

    public override bool CanAccept(Item item)
    {
        if (item.itemType == EItemType.GreenElectricity) return true;
        if (item.itemType == EItemType.Electricity) return true;
        if (item.itemType == EItemType.FruitMarket) return true;
        if (item.itemType == EItemType.FruitSeed) return true;
        if (item.itemType == EItemType.Tracks) return true;
        if (item.itemType == EItemType.Garden) return true;
        return base.CanAccept(item);
    }

    public override void SearchPropLinked() {
        base.SearchPropLinked();
        normalHousesLinked = new List<GameObject>();
        metroLinked = new List<GameObject>();
        citizensList = new List<GameObject>();

        LinkDatabase lLinkDatabase = LinkDatabase.Instance;
        normalHousesLinked = lLinkDatabase.GetLinkObjects(buildingLink, typeof(HouseProp));
        citizensList = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CitizenProp));
        metroLinked = lLinkDatabase.GetLinkObjects(buildingLink, typeof(MetroProp));
    }

    public override void ReceiveItem(EItemType itemType) {
        if (itemType == EItemType.Electricity) {
            _haveElectricity = true;
            bool haveActiveBudget = HaveBudget(EWorldImpactType.ElecTownHallV1);
            if (haveActiveBudget)
            {
                bool wasActive = _electrityHouse;
                UpdateHousesElectricity();
                if (!wasActive && _electrityHouse) ShowThanks(itemType, haveActiveBudget, _haveElectricity);
            }
            else if (!haveActiveBudget)
            {
                ShowThanks(itemType, haveActiveBudget, _haveElectricity);
            }
        }
        else if (itemType == EItemType.GreenElectricity) {
            _haveGreenElectricity = true;
            bool haveActiveBudget = HaveBudget(EWorldImpactType.ElecTownHallV2);
            if (haveActiveBudget)
            {
                bool wasActive = _greenelectricityHouse;
                UpdateHousesGreenElectricity();
                if (!wasActive && _greenelectricityHouse) ShowThanks(itemType, haveActiveBudget, _haveGreenElectricity);
            }
            else if (!haveActiveBudget)
            {
                ShowThanks(itemType, haveActiveBudget, _haveGreenElectricity);
            }
        }
        else if (itemType == EItemType.FruitSeed) {
            _haveFruitSeed = true;
            bool wasActive = _fruitSeedPlanted;
            UpdateTrees();
            if (!wasActive && _fruitSeedPlanted) ShowThanks(itemType, true, _haveFruitSeed);
        }
        else if (itemType == EItemType.FruitMarket)
        {
            _haveFruitBasket = true;
            bool wasActive = _fruitMarketOpen;
            ShowFruitBasket();
            if (!wasActive && _fruitMarketOpen) ShowThanks(itemType, true, _haveFruitBasket);
        }
        else if (itemType == EItemType.Garden) {
            _haveVegetableGarden = true;
            bool wasActive = _gardenAvailable;
            UpdateVegetableGardens();
            if (!wasActive && _gardenAvailable) ShowThanks(itemType, true, _haveVegetableGarden);
        }
        else if (itemType == EItemType.Tracks)
        {
            bool wasActive = _metroCreated;
            SetMetro();
            if (!wasActive && _metroCreated) ShowThanks(itemType, true, _haveTracks);
        }
    }

    public override void OnUpdate(OnNewMonth e)
    {
        if (!HaveBudget(EWorldImpactType.BasicTownHall)) budgetComponent.SetNewImpact(EWorldImpactType.BankruptedTownHall);
        else budgetComponent.RemoveImpact(EWorldImpactType.BankruptedTownHall);

        if (!budgetComponent.Working)
        {
            _happy = false;
            UpdateNPCMoods(false);
        }
        else
        {
            if (!_happy) UpdateNPCMoods(true);
            _happy = true;
        }
    }

    public override void SendBudget()
    {
        if (!budgetComponent.Working)
        {
            UpdateCleanBuildings(false);
            _happy = false;
            UpdateNPCMoods(false);
        }
        else
        {
            UpdateCleanBuildings(true);
            if (!_happy) UpdateNPCMoods(true);
            _happy = true;
        }
    }

    public override void ReceiveBudget() {
        SendBudget();
        if (InventoryPlayer.Instance.givedOject.ContainsKey(IDname))
        {
            foreach (KeyValuePair<EItemType, int> it in InventoryPlayer.Instance.givedOject[IDname])
            {
                ReceiveItem(it.Key);
            }
        }
    }

    void UpdateTrees(bool showFX = true)
    {
        if (_fruitSeedPlanted) return;
        _fruitSeedPlanted = true;
        budgetComponent.SetNewImpact(EWorldImpactType.WoodyTownHall);
        if (showFX) UIManager.instance.AddSDGNotification(new int[2] { 3, 15 });
        for (int i = 0; i < normalHousesLinked.Count; i++)
        {
            normalHousesLinked[i].GetComponent<HouseProp>().SetTrees();
            if (showFX) normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
        QuestManager.Instance.CheckValidation();
    }

    void UpdateCleanBuildings(bool isClean) {
        for (int i = 0; i < normalHousesLinked.Count; i++) {
            HouseProp house = normalHousesLinked[i].GetComponent<HouseProp>();
            if (house.isClean != isClean)
            {
                house.SetTrash(isClean);
                normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(isClean);
            }
        }
    }

    void UpdateVegetableGardens(bool showFX = true)
    {
        if (_gardenAvailable) return;
        _gardenAvailable = true;
        budgetComponent.SetNewImpact(EWorldImpactType.VegetableTownHall);
        if (showFX) UIManager.instance.AddSDGNotification(new int[2] { 1, 2 });
        for (int i = 0; i < normalHousesLinked.Count; i++)
        {
            normalHousesLinked[i].GetComponent<HouseProp>().SetRoofGardenState();
            if (showFX) normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
    }

    void UpdateHousesElectricity(bool showFX = true)
    {
        if (_electrityHouse) return;
        _electrityHouse = true;
        budgetComponent.SetNewImpact(EWorldImpactType.ElecTownHallV1);
        if (showFX) UIManager.instance.AddSDGNotification(new int[2] { 7, 13 });
        for (int i = 0; i < normalHousesLinked.Count; i++)
        {
            if (showFX) normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
        ActiveLight();
    }

    void UpdateHousesGreenElectricity(bool showFX = true)
    {
        if (_greenelectricityHouse) return;
        _greenelectricityHouse = true;
        budgetComponent.SetNewImpact(EWorldImpactType.ElecTownHallV2);
        if (showFX) UIManager.instance.AddSDGNotification(new int[2] { 7, 13 });
        for (int i = 0; i < normalHousesLinked.Count; i++)
        {
            if (showFX) normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
        ActiveLight();
    }

    void SetMetro(bool showFX = true)
    {
        if (!_haveTracks && !_metroCreated) {
            _metroCreated = true;
            _haveTracks = true;
            if (showFX) UIManager.instance.AddSDGNotification(new int[3] { 7, 13, 15 });
            ResourcesManager.Instance.AddBonusImpact(EWorldImpactType.Metro);
            for (int i = 0; i < metroLinked.Count; i++)
            {
                metroLinked[i].GetComponent<MetroProp>().SetMetroState();
                if (showFX) metroLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
            }
            QuestManager.Instance.CheckValidation();
        }
    }

    void UpdateNPCMoods(bool pIsHappy) {
        if (pIsHappy) UIManager.instance.AddSDGNotification(new int[2] { 1, 2 });
        for (int i = 0; i < citizensList.Count; i++) {
            citizensList[i].GetComponent<CitizenProp>().SetMood(pIsHappy);
        }
    }

    protected void ShowFruitBasket()
    {
        if (_fruitMarketOpen) return;
        _fruitMarketOpen = true;
        budgetComponent.SetNewImpact(EWorldImpactType.FruitMarketTownHall);
        for (int i = 0; i < citizensList.Count; i++)
        {
            citizensList[i].GetComponent<CitizenProp>().DisplayFruitsMarketState();
        }
        QuestManager.Instance.CheckValidation();
    }

    public override bool HaveItem(EItemType itemType)
    {
        if (itemType == EItemType.Electricity) if (_haveElectricity) return true;
        if (itemType == EItemType.GreenElectricity) if (_haveGreenElectricity) return true;
        if (itemType == EItemType.FruitMarket) if (_haveFruitBasket) return true;
        if (itemType == EItemType.FruitSeed) if (_haveFruitSeed) return true;
        if (itemType == EItemType.Tracks) if (_metroCreated) return true;
        if (itemType == EItemType.Garden) if (_haveVegetableGarden) return true;
        return false;
    }

    public override bool HaveBudget(EItemType target)
    {
        if (target == EItemType.Electricity)
        {
            if (_electrityHouse) return true;
            return HaveBudget(EWorldImpactType.ElecTownHallV1);
        }
        else if (target == EItemType.GreenElectricity)
        {
            if (_greenelectricityHouse) return true;
            return HaveBudget(EWorldImpactType.ElecTownHallV2);
        }
        else if (target == EItemType.FruitMarket) return true;
        else if (target == EItemType.FruitSeed) return true;
        else if (target == EItemType.Tracks) return true;
        else if (target == EItemType.Garden) return true;
        return base.HaveBudget(target);
    }

    protected void ActiveLight()
    {
        List<TownHallLight> lights = associateCell.GetProps<TownHallLight>();
        if (lights.Count > 0)
        {
            if (_haveGreenElectricity) lights[0].SetGreen();
            else if (_haveElectricity) lights[0].SetElectricity();
        }
        else
        {
            foreach (Cell c in associateCell.Neighbors)
            {
                lights = c.GetProps<TownHallLight>();
                if (lights.Count > 0)
                {
                    if (_haveGreenElectricity) lights[0].SetGreen();
                    else if (_haveElectricity) lights[0].SetElectricity();
                    return;
                }
            }
        }
    }
}
