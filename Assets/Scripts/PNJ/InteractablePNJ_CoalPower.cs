using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
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

    public int requiredWindTurbineNb = 1;

    protected override void Awake()
    {
        base.Awake();
        neededItems = new List<EItemType>() { EItemType.WindTurbine };
    }

    protected override void CatchGivedObject()
    {
        if (InventoryPlayer.Instance.givedOject.Count <= 0) return;
        if (InventoryPlayer.Instance.givedOject.ContainsKey(IDname))
        {
            if (InventoryPlayer.Instance.givedOject[IDname].ContainsKey(EItemType.WindTurbine))
            {
                if (InventoryPlayer.Instance.givedOject[IDname][EItemType.WindTurbine] >= requiredWindTurbineNb)
                {
                    _haveWindTurbine = true;
                    UpdateZone(false);
                }
            }
        }
    }

    public override void ReceiveItem(EItemType itemType) {
        if (FtueManager.instance.active) requiredWindTurbineNb = 1;
        if (itemType == EItemType.WindTurbine) {
            if (InventoryPlayer.Instance.givedOject[IDname][EItemType.WindTurbine] >= requiredWindTurbineNb)
            {
                bool updated = _updated;
                _haveWindTurbine = true;
                UpdateZone();
                if (!updated && _updated) ShowThanks(itemType, HaveBudget(EWorldImpactType.RenewableCoalPowerPlant), _haveWindTurbine);
            }
        }
    }

    public override void ReceiveBudget() {
        UpdateZone();
    }

    public override bool HaveItem(EItemType itemType)
    {
        if (itemType == EItemType.WindTurbine) if (_haveWindTurbine) return true;
        return false;
    }

    public override bool HaveBudget(EItemType target)
    {
        if (_updated) return true;
        if (target == EItemType.WindTurbine) return HaveBudget(EWorldImpactType.RenewableCoalPowerPlant);
        return base.HaveBudget(target);
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
                budgetComponent.RemoveImpact(EWorldImpactType.CoalPowerPlant);
                if (showFX) UIManager.instance.AddSDGNotification(new int[3] { 8, 13, 15 });
                UpdateProps();
            }
        }
    }

    void UpdateProps() {
        associateCell.RemoveProps(propLinked.GetComponent<Props>());
        GameObject newPropLink = EarthManager.Instance.CreateSavedProps(evolutionProp, propLinked.transform.position, associateCell, propLinked.transform.rotation);
        Destroy(propLinked);

        List<GeneratorProps> boucings = associateCell.GetProps<GeneratorProps>();
        List<GeneratorProps> nBouncings;
        foreach (Cell c in associateCell.Neighbors)
        {
            nBouncings = c.GetProps<GeneratorProps>();
            for (int i = 0; i < nBouncings.Count; i++)
            {
                boucings.Add(nBouncings[i]);
            }
        }
        foreach (GeneratorProps gp in boucings)
        {
            if (gp.electricModel != null)
            {
                EarthManager.Instance.CreateSavedProps(gp.electricModel, gp.transform.position, gp.associateCell, gp.transform.rotation);
                gp.associateCell.RemoveProps(gp);
                Destroy(gp.gameObject);
            }
        }
        QuestManager.Instance.CheckValidation();
    }
}
