using Assets.Script;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_CarsCompany : InteractablePNJ {

    bool _haveBattery = false;
    bool _haveGreenBattery = false;
    public bool HaveBudget { get { return budgetComponent.budget >= budgetComponent.targetBudget; } }
    bool _isCompanyActivated = false;
    bool _hasCarcass = false;

    bool _normalVehicle = false;
    bool _greenVehicle = false;

    [Header("Thanks text")]
    [SerializeField]
    public List<ThanksStruct> whithoutMoneyThanks= new List<ThanksStruct>();

    [SerializeField]
    private List<GameObject> _carsList;

    [SerializeField]
    private GameObject _carsCompany;

    public override void SearchPropLinked() {
        _carsList = new List<GameObject>();

        LinkDatabase lLinkDatabase = LinkDatabase.Instance;
        _carsList = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CarItemProp));
        _carsCompany = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CarsCompanyProp))[0];
        propLinked = lLinkDatabase.GetLinkObjects(buildingLink, typeof(CarsCompanyProp))[0];
    }

    public override void OnBudgetLoaded(OnBudgetLoaded e)
    {
        base.OnBudgetLoaded(e);
        if (!_isCompanyActivated)
        {
            if (ResourcesManager.instance) ResourcesManager.instance.RemoveBudgetComponent(budgetComponent);
        }
    }

    public override void ReceiveItem(OnGiveNPC e) {
        base.ReceiveItem(e);
        if (e.targetNPC != this) return;

        if (item.itemsLinked[0] == null) return;
        else {
            if (item.itemsLinked[0].name == null) return;
        }
        //If item given is Battery
        if (e.item.name == item.itemsLinked[0].name) {
            _haveBattery = true;
            if (_isCompanyActivated && HaveBudget) UpdateElectricityCars();
            else if (!_isCompanyActivated)
            {
                UIManager.instance.AddSDGNotification(new int[3] { 6, 13, 14 });
                PointingBubble.instance.ChangeText(GetThanksLocalizedText(thanksTexts, EThanksKey.Battery));
            }
            else if (!HaveBudget) PointingBubble.instance.ChangeText(GetThanksLocalizedText(whithoutMoneyThanks, EThanksKey.NeedBudget));
        }
        //If item given is Green Battery
        else if (e.item.name == item.itemsLinked[1].name) {
            _haveGreenBattery = true;
            if (_isCompanyActivated && HaveBudget)
            {
                UIManager.instance.AddSDGNotification(new int[3] { 6, 13, 14 });
                UpdateGreenElectricityCars();
            }
            else if (!_isCompanyActivated) PointingBubble.instance.ChangeText(GetThanksLocalizedText(thanksTexts, EThanksKey.GreenBattery));
            else if (!HaveBudget) PointingBubble.instance.ChangeText(GetThanksLocalizedText(whithoutMoneyThanks, EThanksKey.NeedBudget));
        }

        //If item given is Vehicle Carcass
        else if (e.item.name == item.itemsLinked[2].name) {
            _hasCarcass = true;
            if (_hasCarcass && HaveBudget) ActivateCarsCompany();
            PointingBubble.instance.ChangeText(GetThanksLocalizedText(thanksTexts, EThanksKey.Carcass));
        }

        else {
            Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
            return;
        }

        Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
    }

    void ActivateCarsCompany() {
        if (!_isCompanyActivated) {
            UIManager.instance.AddSDGNotification(new int[1] { 8 });
            _carsCompany.GetComponent<CarsCompanyProp>().SetActivated();
            _carsCompany.GetComponent<PropParticlesDisplay>().DisplayFX(true);
            _isCompanyActivated = true;
            ResourcesManager.instance.AddBudgetComponent(budgetComponent);
            QuestManager.Instance.CheckValidation();
        }
    }

    public override void OnUpdate(OnNewMonth e)
    {
        OnReceiveBudget(null);
        base.OnUpdate(e);
    }

    public override bool HaveHisItem()
    {
        if (_hasCarcass) return true;
        return false;
    }

    public override void OnReceiveBudget(OnReceiveBudget e) {
        if (HaveBudget) {
			DisplayMood(false);
            if (_isCompanyActivated) {
                if (_haveGreenBattery) {
                    UpdateGreenElectricityCars();
                    return;
                }

                if (_haveBattery) {
                    UpdateElectricityCars();
                    return;
                }
            }
        }
		else {
			DisplayMood(true);
		}
    }

    void UpdateElectricityCars() {
        if (_normalVehicle) return; 
        _normalVehicle = true;
        budgetComponent.SetBudgetStep(0);
        Events.Instance.Raise(new OnCleanVehicles(budgetComponent.name));
        for (int i = 0; i < _carsList.Count; i++) {
            _carsList[i].GetComponent<CarItemProp>().SetElectricityMode();
            _carsList[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
        }
    }

    void UpdateGreenElectricityCars()
    {
        if (_greenVehicle) return;
        _greenVehicle = true;
        budgetComponent.SetBudgetStep(1);
        Events.Instance.Raise(new OnCleanVehicles(budgetComponent.name));
        for (int i = 0; i < _carsList.Count; i++)
        {
            _carsList[i].GetComponent<CarItemProp>().SetGreenElectricityMode();
            _carsList[i].GetComponent<PropParticlesDisplay>().DisplayFX(true);
		}
	}
}
