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

    [SerializeField]
    private BudgetWorldValues metroWorldValues;

    private bool _haveBudget = false;
    private bool _haveTracks = false;
    private bool _haveFruitSeed = false;
    private bool _haveElectricity = false;
    private bool _haveGreenElectricity = false;
    private bool _haveVegetableGarden = false;
    private bool _haveFruitBasket = false;

    private bool _metroCreated = false;

    private bool _happy = true;

    public override void SearchPropLinked() {
        base.SearchPropLinked();
        normalHousesLinked = new List<GameObject>();
        metroLinked = new List<GameObject>();
        citizensList = new List<GameObject>();
        //vegetableBuildingEmpty = new List<GameObject>();

        LinkDatabase lLinkDatabase = LinkDatabase.Instance;
        normalHousesLinked = lLinkDatabase.GetLinkObjects(buildingLink, typeof(HouseProp));
        //vegetableBuildingEmpty = lLinkDatabase.GetLinkObjects(buildingLink, typeof(VegetableHouseProp));
        citizensList = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CitizenProp));
        metroLinked = lLinkDatabase.GetLinkObjects(buildingLink, typeof(MetroProp));
    }

    public override void ReceiveItem(OnGiveNPC e) {
        base.ReceiveItem(e);
        if (e.targetNPC != this) return;
        //If item given is Electricity
        if (item.itemsLinked[0] == null) return;
        else {
            if (item.itemsLinked[0].name == null) return;
        }

        if (e.item.name == item.itemsLinked[0].name) {
            UpdateHousesElectricity();
        }
        //If item given is Green Electricity
        else if (e.item.name == item.itemsLinked[1].name) {
            UpdateHousesGreenElectricity();
        }
        //If item given is Fruit Seed
        else if (e.item.name == item.itemsLinked[2].name) {
            if (!_haveFruitSeed)
            {
                _haveFruitSeed = true;
                budgetComponent.SetBudgetStep(2);
                UpdateTrees();
                UIManager.instance.AddSDGNotification(new int[2] { 3, 15 });
                QuestManager.Instance.CheckValidation();
            }
        }
        //If item given is Fruits Market
        else if (e.item.name == item.itemsLinked[3].name) {
            if (!_haveFruitBasket)
            {
                _haveFruitBasket = true;
                ShowFruitBasket();
                QuestManager.Instance.CheckValidation();
            }  
        }
        //If item given is Vegetable Garden
        else if (e.item.name == item.itemsLinked[4].name) {
            UpdateVegetableGardens();
        }
        //If item given is Tracks
        else if (e.item.name == item.itemsLinked[5].name) {
            _haveTracks = true;
            if (_haveBudget) SetMetro();
        }
        else {
            Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
            return;
        }

        Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
    }

    public override void OnUpdate(OnNewMonth e)
    {
        if (budgetComponent.budget < budgetComponent.minBudget)
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

    public override void OnSendBudget(OnGiveBudget e)
    {
        if (budgetComponent.name != e.comp.name) return;
        if (budgetComponent.budget < budgetComponent.targetBudget)
        {
            _haveBudget = false;
            UpdateCleanBuildings(false);
        }

        if (budgetComponent.budget > budgetComponent.targetBudget)
        {

            if (!_haveBudget)
            {
                if (_haveTracks) SetMetro();
                UpdateCleanBuildings(true);
            }

            _haveBudget = true;
        }

        if (budgetComponent.budget < budgetComponent.minBudget)
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

    public override void OnReceiveBudget(OnReceiveBudget e) {
        if (budgetComponent.name != e.comp.name) return;
        if (budgetComponent.budget < budgetComponent.targetBudget) {
            _haveBudget = false;
            UpdateCleanBuildings(false);
        }

        if (budgetComponent.budget > budgetComponent.targetBudget) {

            if (!_haveBudget) {
                if (_haveTracks) SetMetro();
                UpdateCleanBuildings(true);
            }            
            _haveBudget = true;
        }

        if (budgetComponent.budget < budgetComponent.minBudget)
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

    void UpdateTrees()
    {
        for (int i = 0; i < normalHousesLinked.Count; i++)
        {
            normalHousesLinked[i].GetComponent<HouseProp>().SetTrees();
            normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
        QuestManager.Instance.CheckValidation();
    }

    void UpdateCleanBuildings(bool isClean) {
        for (int i = 0; i < normalHousesLinked.Count; i++) {
            normalHousesLinked[i].GetComponent<HouseProp>().SetTrash(isClean);
            normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(isClean);
        }
    }

    void UpdateVegetableGardens() {
        if (!_haveVegetableGarden)
        {
            _haveVegetableGarden = true;
            budgetComponent.SetBudgetStep(4);
            UIManager.instance.AddSDGNotification(new int[2] { 1, 2 });
            for (int i = 0; i < normalHousesLinked.Count; i++)
            {
                normalHousesLinked[i].GetComponent<HouseProp>().SetRoofGardenState();
                normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
            }
        }       
    }

    void UpdateHousesElectricity() {
        if (!_haveElectricity)
        {
            _haveElectricity = true;
            budgetComponent.SetBudgetStep(5);
            UIManager.instance.AddSDGNotification(new int[2] { 7, 13 });
            for (int i = 0; i < normalHousesLinked.Count; i++)
            {
                normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
            }
            ActiveLight();
        }  
    }

    void UpdateHousesGreenElectricity() {
        if (!_haveGreenElectricity)
        {
            _haveGreenElectricity = true;
            budgetComponent.SetBudgetStep(6);
            UIManager.instance.AddSDGNotification(new int[2] { 7, 13 });
            for (int i = 0; i < normalHousesLinked.Count; i++)
            {
                normalHousesLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
            }
            ActiveLight();
        }        
    }

    void SetMetro() {
        if (!_metroCreated)
        {
            _metroCreated = true;
            UIManager.instance.AddSDGNotification(new int[3] { 7, 13, 15 });
            ResourcesManager.instance.AddBonusImpact(metroWorldValues);
            for (int i = 0; i < metroLinked.Count; i++)
            {
                metroLinked[i].GetComponent<MetroProp>().SetMetroState();
                metroLinked[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
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

    protected void ShowFruitBasket()
    {
        for (int i = 0; i < citizensList.Count; i++)
        {
            citizensList[i].GetComponent<CitizenProp>().DisplayFruitsMarketState();
        }
    }

    public bool HasMetro()
    {
        if (_metroCreated) return true;
        else return false;
    }

    public bool HasFruitSeed()
    {
        if (_haveFruitSeed) return true;
        else return false;
    }

    public bool HasTracks()
    {
        if (_haveTracks) return true;
        else return false;
    }

    public bool HasFruitBasket()
    {
        if (_haveFruitBasket) return true;
        else return false;
    }
}
