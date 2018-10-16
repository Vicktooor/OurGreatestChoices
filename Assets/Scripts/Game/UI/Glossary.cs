using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoSingleton<Glossary>
{
    public OneLineScroller scroller;

    [SerializeField]
    private GameObject _itemTransformedNGOImage;
    [SerializeField]
    private GameObject _itemTransformedEcoImage;
    [SerializeField]
    private GameObject _itemTransformedGouvImage;
    [SerializeField]
    private List<Item> _primaryItems;
    private GlossaryInfo[] _infos;
    private int _index = 0;

    protected override void Awake()
    {
        base.Awake();
        scroller.Init();
    }

    private void OnEnable()
    {
        MajScrollIcons();
        scroller.Place(_index);
        Set(_index);
        scroller.SetMoveCallback(OnSrollMove);
        Events.Instance.Raise(new OnClickGlossary());
    }

    private void MajScrollIcons()
    {
        if (_infos == null) _infos = GetComponentsInChildren<GlossaryInfo>();
        for (int i = 0; i < _infos.Length; i++)
        {
            if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[i])) _infos[i].GetComponent<Image>().sprite = _primaryItems[i].icon;
            else  _infos[i].GetComponent<Image>().sprite = _primaryItems[i].hiddenIcon;
        }
    }

    private void Set(int pIndex) {
        if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[pIndex].NGOItem))
        {
            _itemTransformedNGOImage.GetComponent<Image>().sprite = _primaryItems[pIndex].NGOItem.icon;
        }
        else _itemTransformedNGOImage.GetComponent<Image>().sprite = _primaryItems[pIndex].NGOItem.hiddenIcon;

        if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[pIndex].EcoItem))
        {
            _itemTransformedEcoImage.GetComponent<Image>().sprite = _primaryItems[pIndex].EcoItem.icon;
        }
        else _itemTransformedEcoImage.GetComponent<Image>().sprite = _primaryItems[pIndex].EcoItem.hiddenIcon;

        if (InventoryPlayer.instance.knowsItems.Contains(_primaryItems[pIndex].GouvItem))
        {
            _itemTransformedGouvImage.GetComponent<Image>().sprite = _primaryItems[pIndex].GouvItem.icon;
        }
        else _itemTransformedGouvImage.GetComponent<Image>().sprite = _primaryItems[pIndex].GouvItem.hiddenIcon;
    }

    private void OnSrollMove()
    {
        Set(scroller.CurrentIndex);
    }

    private void OnDisable()
    {
        scroller.DelMoveCallback();
    }

    private bool KnowItem(Item pItem) {
        Item knowItem = InventoryPlayer.instance.knowsItems.Find(e => e.Equals(pItem));
        return (knowItem != null) ? true : false; 
    }
}

