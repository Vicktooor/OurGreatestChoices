using Assets.Script;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_CarsCompany : InteractablePNJ {

    bool _haveBattery = false;
    bool _haveGreenBattery = false;
    bool _hasCarcass = false;

    bool _isCompanyActivated = false;
    bool _normalVehicle = false;
    bool _greenVehicle = false;

    [SerializeField]
    private List<GameObject> _carsList;

    [SerializeField]
    private GameObject _carsCompany;

    protected override void Awake()
    {
        base.Awake();
        neededItems = new List<EItemType>()
        {
            EItemType.Carcass,
            EItemType.Battery,
            EItemType.GreenBattery
        };
    }

    protected override void CatchGivedObject()
    {
        if (InventoryPlayer.Instance.givedOject.Count <= 0) return;
        if (InventoryPlayer.Instance.givedOject.ContainsKey(IDname))
        {
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.Carcass)) _hasCarcass = true;
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.Battery)) _haveBattery = true;
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.GreenBattery)) _haveGreenBattery = true;

            if (_hasCarcass && HaveBudget(EWorldImpactType.CarsCompany))
            {
                if (!_isCompanyActivated) ActivateCarsCompany(false);
            }

            if (_isCompanyActivated && HaveBudget(EWorldImpactType.ElecCarsCompanyV1) && _haveBattery)
                UpdateElectricityCars(false);

            if (_isCompanyActivated && HaveBudget(EWorldImpactType.ElecCarsCompanyV2) && _haveGreenBattery)
                UpdateGreenElectricityCars(false);
        }
    }

    public override bool CanAccept(Item item)
    {
        if (item.itemType == EItemType.Carcass) return true;
        if (item.itemType == EItemType.GreenBattery) return true;
        if (item.itemType == EItemType.Battery) return true;
        return base.CanAccept(item);
    }

    public override void SearchPropLinked() {
        _carsList = new List<GameObject>();

        LinkDatabase lLinkDatabase = LinkDatabase.Instance;
        _carsList = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CarItemProp));
        _carsCompany = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CarsCompanyProp))[0];
        propLinked = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CarsCompanyProp))[0];
    }

    public override void ReceiveItem(EItemType itemType) {
        if (itemType == EItemType.Battery)
        {
            _haveBattery = true;
            bool haveActiveBudget = HaveBudget(EWorldImpactType.ElecCarsCompanyV1);
            if (haveActiveBudget && _isCompanyActivated && !_normalVehicle)
            {
                UpdateGreenElectricityCars();
                ShowThanks(itemType, haveActiveBudget, _haveBattery);
            }
            else if (!haveActiveBudget && _isCompanyActivated && !_normalVehicle)
            {
                ShowThanks(itemType, haveActiveBudget, _haveBattery);
            }
        }
        else if (itemType == EItemType.GreenBattery)
        {
            _haveGreenBattery = true;
            bool haveActiveBudget = HaveBudget(EWorldImpactType.ElecCarsCompanyV2);
            if (haveActiveBudget && _isCompanyActivated && !_greenVehicle && _normalVehicle)
            {
                UpdateGreenElectricityCars();
                ShowThanks(itemType, haveActiveBudget, _haveGreenBattery);
            }
            else if (!haveActiveBudget && _isCompanyActivated && !_greenVehicle)
            {
                ShowThanks(itemType, haveActiveBudget, _haveGreenBattery);
            }
        }
        else if (itemType == EItemType.Carcass)
        {
            _hasCarcass = true;
            bool haveActiveBudget = HaveBudget(EWorldImpactType.CarsCompany);
            if (haveActiveBudget && !_isCompanyActivated)
            {
                ActivateCarsCompany();
                ShowThanks(itemType, haveActiveBudget, _hasCarcass);
            }
            else if (!haveActiveBudget && !_isCompanyActivated)
            {
                ShowThanks(itemType, haveActiveBudget, _hasCarcass);
            }
        }
    }

    void ActivateCarsCompany(bool showFX = true) {
        if (!_isCompanyActivated)
        {
            _isCompanyActivated = true;
            _carsCompany.GetComponent<CarsCompanyProp>().SetActivated();
            if (showFX)
            {
                _carsCompany.GetComponent<PropParticlesDisplay>().DisplayFX(true);
                UIManager.instance.AddSDGNotification(new int[1] { 8 });
            }
            QuestManager.Instance.CheckValidation();
        }
    }

    public override void OnUpdate(OnNewMonth e)
    {
        base.OnUpdate(e);
    }

    public override bool HaveItem(EItemType itemType)
    {
        if (itemType == EItemType.Carcass) if (_hasCarcass) return true;
        if (itemType == EItemType.Battery) if (_haveBattery) return true;
        if (itemType == EItemType.GreenBattery) if (_haveGreenBattery) return true; 
        return false;
    }

    public override bool HaveBudget(EItemType target)
    {
        if (target == EItemType.Battery)
        {
            if (_normalVehicle) return true;
            return HaveBudget(EWorldImpactType.ElecCarsCompanyV1);
        }
        else if (target == EItemType.GreenBattery)
        {
            if (_greenVehicle) return true;
            return HaveBudget(EWorldImpactType.ElecCarsCompanyV2);
        }
        else if (target == EItemType.Carcass)
        {
            if (_isCompanyActivated) return true;
            return HaveBudget(EWorldImpactType.CarsCompany);
        }
        return base.HaveBudget(target);
    }

    public override void ReceiveBudget() {
        if (budgetComponent.Working) {
            DisplayMood(false);
        }
        else DisplayMood(true);
        if (InventoryPlayer.Instance.givedOject.ContainsKey(IDname))
        {
            foreach (KeyValuePair<EItemType, int> it in InventoryPlayer.Instance.givedOject[IDname])
            {
                ReceiveItem(it.Key);
            }
        }
    }

    void UpdateElectricityCars(bool showFX = true) {
        if (_normalVehicle || _greenVehicle) return;
        _normalVehicle = true;
        budgetComponent.RemoveImpact(EWorldImpactType.CarsCompany);
        budgetComponent.SetNewImpact(EWorldImpactType.ElecCarsCompanyV1);
        if (showFX) UIManager.instance.AddSDGNotification(new int[3] { 6, 13, 14 });
        for (int i = 0; i < _carsList.Count; i++) {
            _carsList[i].GetComponent<CarItemProp>().SetElectricityMode();
            Events.Instance.Raise(new OnCleanVehicles(IDname));
            if (showFX) _carsList[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
    }

    void UpdateGreenElectricityCars(bool showFX = true)
    {
        if (_greenVehicle) return;
        _greenVehicle = true;
        budgetComponent.RemoveImpact(EWorldImpactType.ElecCarsCompanyV1);
        budgetComponent.SetNewImpact(EWorldImpactType.ElecCarsCompanyV2);
        if (showFX) UIManager.instance.AddSDGNotification(new int[3] { 6, 13, 14 });
        for (int i = 0; i < _carsList.Count; i++)
        {
            _carsList[i].GetComponent<CarItemProp>().SetGreenElectricityMode();
            Events.Instance.Raise(new OnCleanVehicles(IDname));
            if (showFX) _carsList[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
	}
}
