using Assets.Scripts.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

#region Debug Mode Class
public partial class SROptions {

    private Item[] _itemsArray = GameObject.FindObjectOfType<InventoryPlayer>().allItemsArray;

    private int _index;
    private Item _cheatNPC = GameObject.FindObjectOfType<InventoryPlayer>()._cheatNPC;
    //private string _itemName;

    [Category("WORLD VALUES")]
    public float deforestationLevel {
        get { return WorldValues.STATE_FOREST; }
        set {
            if (value < -2 || value > 2) return;
            WorldValues.STATE_FOREST = value;
        }
    }

    [Category("INDEX")]
    public int index {
        get { return _index; }
        set { _index = value; }
    }

    //[Category("ITEM ADDED")]
    //public string itemName {
    //    get { return _itemsArray[index].name; }
    //}

    [Category("INVENTORY FUNCTIONS")]
    public void AddObject() {
        GameObject.FindObjectOfType<InventoryPlayer>().Add(_itemsArray[_index]);
    }
}
#endregion

public class InventoryPlayer : MonoBehaviour
{
    public float moneyStock;
    public float maxStock = 10;

    public List<Item> knowsItems;
    public List<Item> itemsWornArray;
    public Dictionary<string, int> nbItems;

    //PUBLIC FOR DEBUG CLASS
    public Item _cheatNPC;
    public Item _cheatItem;

    #region CheatArray
    //Public only for the debug class
    public Item[] allItemsArray;
    #endregion

    #region Singleton
    private static InventoryPlayer _instance;

    public static InventoryPlayer instance {
        get {
            return _instance;
        }
    }

    protected void Awake() {
        if (_instance != null) {
            throw new Exception("Tentative de création d'une autre instance d'InventoryPlayer alors que c'est un singleton.");
        }
        _instance = this;

        Events.Instance.AddListener<OnTransformation>(Transformation);
        Events.Instance.AddListener<OnGive>(Give);

        knowsItems = new List<Item>();
        nbItems = new Dictionary<string, int>();
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

    #endregion
    public void Add(Item pItem, bool fromTransformation = false) {
        //Avoid picking up objects on MapView
        if (pItem == null || GameManager.Instance.LoadedScene == SceneString.MapView) { return; }

        if (!knowsItems.Contains(pItem))
        {
            knowsItems.Add(pItem);
            itemsWornArray.Add(pItem);
            nbItems.Add(pItem.name, 1);
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
                nbItems[pItem.name]++;
            } 
            else
            {
                itemsWornArray.Add(pItem);
                nbItems[pItem.name]++;
            }           
        }

        Events.Instance.Raise(new OnShowPin(EPin.Bag, true));
        Events.Instance.Raise(new OnUpdateInventory());
        Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
        if (QuestManager.Instance) QuestManager.Instance.DisplayQuest();
    }

    public void Transformation(OnTransformation e) {
        Add(e.item, true);
        Events.Instance.Raise(new OnEndTransformation(e.index, e.item));
    }

    void Give(OnGive e) {
        string eName = itemsWornArray[e.index].name;
        int nb = nbItems[eName] - 1;
        nbItems[eName] = nb;
        if (nb == 0)
        {
            itemsWornArray.RemoveAt(e.index);
            nbItems[eName] = 0;
        }
    }

    public bool InventoryContain(string itemName)
    {
        for (int i = 0; i < itemsWornArray.Count; i++)
        {
            if (itemsWornArray[i].name == itemName) return true;
        }
        return false;
    }

    public bool PlayerKnowRecipeFor(string itemName)
    {
        for (int i = 0; i < knowsItems.Count; i++)
        {
            if (knowsItems[i].name == itemName) return true;
        }
        return false;
    }

    public void Clear() {
        knowsItems.Clear();
        itemsWornArray.Clear();
    }

    private void OnDestroy() {
        itemsWornArray.Clear();

        Events.Instance.RemoveListener<OnTransformation>(Transformation);
        Events.Instance.RemoveListener<OnGive>(Give);
    }
}
