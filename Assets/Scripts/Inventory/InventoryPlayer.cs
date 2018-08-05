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

public class InventoryPlayer : MonoBehaviour {

    public List<Item> knowsItems;
    public List<Item> itemsWornArray;

    [SerializeField]
    int MAX_OBJECTS = 50;
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
    }

    #endregion
    public bool Add(Item pItem) {
        //Avoid picking up objects on MapView
        if (pItem == null || GameManager.Instance.LoadedScene == SceneString.MapView) {
            return false;
        }

        if (itemsWornArray.Count < MAX_OBJECTS) {
            if (!knowsItems.Contains(pItem)) {
                knowsItems.Add(pItem);
                Events.Instance.Raise(new OnNewObject());
            }
            itemsWornArray.Add(pItem);
            Events.Instance.Raise(new OnShowPin(EPin.Bag, true));
        }

        else {
            print("NOMBRE D OBJETS MAXIMUM ATTEINT");
            return false;
        }

        Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
        if (QuestManager.Instance) QuestManager.Instance.DisplayQuest();

        return true;
    }

    public void Transformation(OnTransformation e) {
        if (e.item == null) return;

        if (!knowsItems.Contains(e.item)) {
            knowsItems.Add(e.item);
            Events.Instance.Raise(new OnTransformRewardNew());
            Events.Instance.Raise(new OnShowPin(EPin.Glossary, true));
        }
        else {
            print("not new");
            Events.Instance.Raise(new OnTransformReward());
        }
        itemsWornArray[e.index] = e.item;
        Events.Instance.Raise(new OnShowPin(EPin.Bag, true));
        if (QuestManager.Instance) QuestManager.Instance.DisplayQuest();
    }

    void Give(OnGive e) {
        List<Item> itemsArray = InventoryPlayer.instance.itemsWornArray;

        for (int i = e.index; i < itemsArray.Count; i++) {
            if ((i + 1) < itemsArray.Count) {
                itemsArray[i] = itemsArray[i + 1];
            }
            else {
                itemsArray.RemoveAt(i);
                break;
            }
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

        Events.Instance.Raise(new OnClear());
    }

    private void OnDestroy() {
        itemsWornArray.Clear();

        Events.Instance.RemoveListener<OnTransformation>(Transformation);
        Events.Instance.RemoveListener<OnGive>(Give);
    }
}
