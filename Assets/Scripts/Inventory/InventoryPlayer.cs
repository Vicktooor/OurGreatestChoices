using Assets.Scripts.Game;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class InventoryPlayer : MonoSingleton<InventoryPlayer>
{
    public float moneyStock;
    public float maxStock = 10;

    public List<Item> knowsItems = new List<Item>();
    public List<Item> itemsWornArray = new List<Item>();
    public Dictionary<EItemType, int> nbItems = new Dictionary<EItemType, int>();
    public Dictionary<string, Dictionary<EItemType, int>> givedOject = new Dictionary<string, Dictionary<EItemType, int>>();

    protected void Start()
    {
        Events.Instance.AddListener<OnTransformation>(Transformation);
    }

    public void Load()
    {
        PartySave partySave = PlanetSave.GameStateSave;
        moneyStock = partySave.moneyStock;

        List<GivedItemSave> outGived;
        ArrayExtensions.ToList(partySave.GivedItems, out outGived);
        foreach (GivedItemSave item in outGived)
        {
            for (int i = 0; i < item.givedItems.Length; i++)
            {
                GiveItem(item.npcName, item.givedItems[i]);
            }
        }
        List<InventoryItemSave> outInventory;
        ArrayExtensions.ToList(partySave.items, out outInventory);
        foreach (InventoryItemSave item in outInventory)
        {
            for (int i = 0; i < item.nb; i++)
            {
                AddFromType(item.type);
            }
        }
        for (int i = 0; i < PlanetSave.GameStateSave.knowsItem.Length; i++)
        {
            AddKnowFromType(PlanetSave.GameStateSave.knowsItem[i]);
        }
        Events.Instance.Raise(new OnUpdateInventory());
    }

    public bool AddMoney(float value)
    {
        float newStock = moneyStock + value;
        if (newStock > maxStock) return false;
        else
        {
            moneyStock += value;
            return true;
        }
    }

    public float GetMoney(float value)
    {
        float newStock = moneyStock - value;
        if (newStock < 0) return 0f;
        else
        {
            moneyStock -= value;
            return value;
        }
    }

    public void Add(Item pItem, bool fromTransformation = false) {
        //Avoid picking up objects on MapView
        if (pItem == null || GameManager.Instance.LoadedScene == SceneString.MapView) { return; }

        if (!knowsItems.Contains(pItem))
        {
            knowsItems.Add(pItem);
            itemsWornArray.Add(pItem);
            nbItems.Add(pItem.itemType, 1);
            Events.Instance.Raise(new OnNewObject());
            if (fromTransformation)
            {
                Events.Instance.Raise(new OnTransformRewardNew());
                Events.Instance.Raise(new OnShowPin(EPin.Glossary, true));
            }
        }
        else
        {
            if (fromTransformation) Events.Instance.Raise(new OnTransformReward());
            if (itemsWornArray.Contains(pItem))
            {
                nbItems[pItem.itemType] = nbItems[pItem.itemType] + 1;
            } 
            else
            {
                itemsWornArray.Add(pItem);
                if (nbItems.ContainsKey(pItem.itemType)) nbItems[pItem.itemType] = nbItems[pItem.itemType] + 1;
                else nbItems.Add(pItem.itemType, 1);
            }           
        }

        if (FtueManager.instance.active)
        {
            if (FtueManager.instance.currentStep.inventoryTarget == pItem.itemType)
            {
                FtueManager.instance.currentStep.PickUp();
                if (FtueManager.instance.currentStep.HaveMaxItem())
                {
                    if (FtueManager.instance.currentStep.getNbTarget > 1) NotePad.Instance.CleanBillboards();
                    FtueManager.instance.ValidStep();
                }
            }
        }

        Events.Instance.Raise(new OnShowPin(EPin.Bag, true));
        Events.Instance.Raise(new OnUpdateInventory());
        if (QuestManager.Instance) QuestManager.Instance.DisplayQuest();
    }

    private void AddFromType(EItemType pItem)
    {
        Item newItem = ResourcesManager.Instance.ItemModels.Find(i => i.itemType == pItem);
        if (newItem == null) return;
        if (!knowsItems.Contains(newItem))
        {
            knowsItems.Add(newItem);
            itemsWornArray.Add(newItem);
            nbItems.Add(newItem.itemType, 1);
        }
        else
        {
            if (itemsWornArray.Contains(newItem))
            {
                nbItems[newItem.itemType]++;
            }
            else
            {
                itemsWornArray.Add(newItem);
                nbItems[newItem.itemType]++;
            }
        }
    }

    private void AddKnowFromType(EItemType pItem)
    {
        Item newItem = ResourcesManager.Instance.ItemModels.Find(i => i.itemType == pItem);
        if (newItem == null) return;
        if (!knowsItems.Contains(newItem)) knowsItems.Add(newItem);
    }

    public void Transformation(OnTransformation e) {
        Add(e.item, true);
        Events.Instance.Raise(new OnEndTransformation(e.index, e.item));
    }

    private void GiveItem(string npcName, EItemType itemType)
    {
        if (givedOject.ContainsKey(npcName))
        {
            if (givedOject[npcName] != null)
            {
                if (givedOject[npcName].ContainsKey(itemType))
                {
                    givedOject[npcName][itemType]++;
                }
            }
            else
            {
                givedOject[npcName] = new Dictionary<EItemType, int>
                {
                    { itemType, 1 }
                };
            }
        }
        else
        {
            givedOject.Add(npcName, new Dictionary<EItemType, int>());
            givedOject[npcName] = new Dictionary<EItemType, int>
            {
                { itemType, 1 }
            };
        }
    }

    public void Give(int index, string npcName) {
        EItemType eName = itemsWornArray[index].itemType;
        int nb = nbItems[eName] - 1;
        nbItems[eName] = nb;
        GiveItem(npcName, eName);
    }

    public int GetItemIndex(EItemType itemName)
    {
        for (int i = 0; i < itemsWornArray.Count; i++)
        {
            if (itemsWornArray[i].itemType == itemName) return i;
        }
        return -1;
    }

    public Item ContainItem(EItemType itemName)
    {
        for (int i = 0; i < itemsWornArray.Count; i++)
        {
            if (itemsWornArray[i].itemType == itemName) return itemsWornArray[i];
        }
        return null;
    }

    public bool PlayerKnowRecipeFor(EItemType itemName)
    {
        for (int i = 0; i < knowsItems.Count; i++)
        {
            if (knowsItems[i].itemType == itemName) return true;
        }
        return false;
    }

    public void Clear() {
        knowsItems.Clear();
        itemsWornArray.Clear();
        nbItems.Clear();
        givedOject.Clear();
        Events.Instance.Raise(new OnUpdateInventory());
    }

    private void OnDestroy() {
        Events.Instance.RemoveListener<OnTransformation>(Transformation);
    }
}
