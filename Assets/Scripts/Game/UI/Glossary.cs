using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoBehaviour {

    [Header("GLOSSARY MENU")]
    [SerializeField]
    GameObject _slotsZone;
    [SerializeField]
    GameObject _glossaryPanel;
    [SerializeField]
    GameObject _itemTransformedNGOImage;
    [SerializeField]
    GameObject _itemTransformedEcoImage;
    [SerializeField]
    GameObject _itemTransformedGouvImage;

    private GameObject[] _slotsArray;

    public List<Item> knownItems;

    [SerializeField]
    private Item[] _primaryItems;

    [SerializeField]
    private int _initIndex = 1; //A MODIFIER, display the transformed Screen with init values the first time we open it

    #region Singleton
    private static Glossary _instance;

    public static Glossary instance {
        get {
            return _instance;
        }
    }

    void Awake() {
        if (_instance != null) {
            throw new Exception("Tentative de création d'une autre instance du Glossary alors que c'est un singleton.");
        }
        _instance = this;

        Init();
    }

    #endregion

    void Init() {
        int numberChildren = _slotsZone.transform.childCount;
        _slotsArray = new GameObject[4];

        for (int i = 0; i < numberChildren; i++) {
            _slotsArray[i] = _slotsZone.transform.GetChild(i).gameObject;
        }

        knownItems = new List<Item>();

        Events.Instance.AddListener<OnScrolling>(UpdateTransformedObjects);
    }

    //UI Event
    void OnEnable() {
        SetIcons();
        _initIndex = GetComponentInChildren<LayoutElementDisplayCheck>().currentObjectIndex;
        RefreshTransformedScreen(_initIndex);
        Events.Instance.Raise(new OnClickGlossary());
    }

    void SetIcons() {
        //PB AVEC CONTAINS, EGALITE FAUSSEE
        List<Item> knowsItems = InventoryPlayer.instance.knowsItems;

        for (int i = 0; i < _slotsArray.Length; i++) {

          /*bool isContained = false;

            for (int j = 0; j < knowsItems.Count; j++) {
                for (int k = 0; k < _primaryItems.Length; k++) {
                    if (knowsItems[j].name == _primaryItems[k].name) {
                        isContained = true;
                        break;
                    }
                }               
            }*/

            if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[i])) _slotsArray[i].GetComponent<Image>().sprite = _primaryItems[i].icon;
            //if(isContained) 

            else _slotsArray[i].GetComponent<Image>().sprite = _primaryItems[i].hiddenIcon;
        }
    }

    void RefreshTransformedScreen(int pIndex) {
        if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[pIndex].NGOItem)) _itemTransformedNGOImage.GetComponent<Image>().sprite = _primaryItems[pIndex].NGOItem.icon;
        else _itemTransformedNGOImage.GetComponent<Image>().sprite = _primaryItems[pIndex].NGOItem.hiddenIcon;

        if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[pIndex].EcoItem))
            _itemTransformedEcoImage.GetComponent<Image>().sprite = _primaryItems[pIndex].EcoItem.icon;
        else _itemTransformedEcoImage.GetComponent<Image>().sprite = _primaryItems[pIndex].EcoItem.hiddenIcon;

        if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[pIndex].GouvItem))
            _itemTransformedGouvImage.GetComponent<Image>().sprite = _primaryItems[pIndex].GouvItem.icon;
        else _itemTransformedGouvImage.GetComponent<Image>().sprite = _primaryItems[pIndex].GouvItem.hiddenIcon;
    }


    void UpdateTransformedObjects(OnScrolling e) {
        if (e.targetList == EScrollList.Inventory) return;
        RefreshTransformedScreen(e.index);
    }

    bool CheckIfPrimaryIsContained(Item pItem) {
        List<Item> knowsItems = InventoryPlayer.instance.knowsItems;
        for (int i = 0; i < knowsItems.Count; i++) {
            if (pItem.name == knowsItems[i].name) return true;
        }

        return false;
    }
}

