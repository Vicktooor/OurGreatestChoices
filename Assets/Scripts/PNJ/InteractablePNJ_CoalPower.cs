using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_CoalPower : InteractablePNJ {
    private bool _updated = false;
    private bool _haveWindTurbine = false;

    [SerializeField]
    public GameObject evolutionProp;

    private Vector3 _itemPos;
    private ItemPickup _powerPlantCollectible;

    protected override void CatchGivedObject()
    {
        if (InventoryPlayer.Instance.givedOject.ContainsKey(IDname))
        {
            if (InventoryPlayer.Instance.givedOject[IDname].Contains(EItemType.WindTurbine))
            {
                _haveWindTurbine = true;
                UpdateZone(false);
            }
        }
    }

    public override void ReceiveItem(EItemType itemType) {
        if (itemType == EItemType.WindTurbine) {
            bool updated = _updated;
            _haveWindTurbine = true;
            UpdateZone();
            if (!updated && _updated) ShowThanks(itemType, HaveBudget(EWorldImpactType.RenewableCoalPowerPlant), _haveWindTurbine);
        }
    }

    public override void ReceiveBudget() {
        UpdateZone();
    }

    public override bool HaveHisItem()
    {
        if (_haveWindTurbine) return true;
        else return false;
    }

    public bool IsUpdated()
    {
        if (_updated) return true;
        else return false;
    }

    public override bool CanAccept(Item item)
    {
        if (item.itemType == EItemType.WindTurbine) return true;
        return base.CanAccept(item);
    }

    public override void OnUpdate(OnNewMonth e)
    {
        base.OnUpdate(e);
        UpdateZone();
    }

    void UpdateZone(bool showFX = true) {
        if (!_updated)
        {
            if (HaveBudget(EWorldImpactType.RenewableCoalPowerPlant) && _haveWindTurbine)
            {
                _updated = true;
                budgetComponent.SetNewImpact(EWorldImpactType.RenewableCoalPowerPlant);
                if (showFX) UIManager.instance.AddSDGNotification(new int[3] { 8, 13, 15 });
                UpdateProps();
            }
        }
    }

    void UpdateProps() {
		Transform _transform = propLinked.transform;
        GameObject lGo = propLinked;
        propLinked = EarthManager.Instance.CreateSavedProps(evolutionProp, _transform.position, associateCell, _transform.rotation);
        Destroy(propLinked.GetComponent<Props>());
        UpdateLinkItem();
        QuestManager.Instance.CheckValidation();
    }

    void UpdateLinkItem() {
        
    }
}
