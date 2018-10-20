using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_CoalPower : InteractablePNJ {
    [Header("Thanks text")]
    [SerializeField]
    public List<ThanksStruct> whithoutMoneyThanks = new List<ThanksStruct>();

    private bool _updated = false;

    private bool _haveBudget = false;
    private bool _haveWindTurbine = false;

    [SerializeField]
    ItemPickUp newItemPickUp;

    [SerializeField]
    public GameObject evolutionProp;

    private Vector3 _itemPos;
    private ItemPickup _powerPlantCollectible;

    public override void Start()
    {
        base.Start();
        CatchPickUpItemPosition();
    }

    public override void ReceiveItem(OnGiveNPC e) {
        base.ReceiveItem(e);
        if (e.targetNPC != this) return;

        if (e.item.name == item.itemsLinked[0].name) {
            if (_haveBudget) UpdateZone();
            else PointingBubble.instance.ChangeText(GetThanksLocalizedText(whithoutMoneyThanks, EThanksKey.NeedBudget));
            _haveWindTurbine = true;
        }
    }

    public override void OnReceiveBudget(OnReceiveBudget e) {
        if (budgetComponent.name != e.comp.name) return;
        if (!_updated) UpdateZone();
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

    void UpdateZone() {
        if (budgetComponent.budget >= budgetComponent.targetBudget) {
            _haveBudget = true;
            if (_haveWindTurbine)
            {
                budgetComponent.SetBudgetStep(0);
                if (PointingBubble.instance.active) PointingBubble.instance.ChangeText(GetThanksLocalizedText(thanksTexts, EThanksKey.WindTurbine));
                UIManager.instance.AddSDGNotification(new int[3] { 8, 13, 15 });
                UpdateProps();
            }
            return;
        }
        else if (budgetComponent.budget <= budgetComponent.targetBudget) {
            _haveBudget = false;
            if (PointingBubble.instance.active) PointingBubble.instance.ChangeText(GetThanksLocalizedText(whithoutMoneyThanks, EThanksKey.WindTurbine));
        }
        else return;
    }

    void UpdateProps() {
        _updated = true;
		Transform _transform = propLinked.transform;
        GameObject lGo = propLinked;
        propLinked = EarthManager.Instance.CreateSavedProps(evolutionProp, _transform.position, associateCell, _transform.rotation);
        Destroy(propLinked.GetComponent<Props>());      

        Events.Instance.Raise(new OnMusicBeta());

        CatchPickUpItemPosition();
        if (newItemPickUp) UpdatePickUp(newItemPickUp);

        QuestManager.Instance.CheckValidation();
    }

    void UpdatePickUp(ItemPickUp item) {
        RaycastHit lHit;
        if (Physics.Raycast(new Ray(_itemPos, -_itemPos), out lHit, 1f, LayerMask.GetMask(new string[1] { "Cell" })))
        {
            EarthManager.Instance.CreateProps(item.prefab, lHit.point, associateCell);
        }
        CatchPickUpItemPosition();
    }

    private void CatchPickUpItemPosition()
    {
        foreach (KeyValuePair<Props, string> item in associateCell.Props)
        {
            if (item.Key.GetType() == typeof(ItemPickup))
            {
                _powerPlantCollectible = item.Key as ItemPickup;
                _itemPos = item.Key.transform.position;
            }
        }
    }
}
