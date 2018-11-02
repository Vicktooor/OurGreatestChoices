﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoSingleton<Glossary>
{
    public OneLineScroller scroller;

    [SerializeField]
    private GameObject _itemTransformedNGOImage;
    [SerializeField]
    private TextMeshProUGUI _itemTransformedNGODesc;
    [SerializeField]
    private GameObject _itemTransformedEcoImage;
    [SerializeField]
    private TextMeshProUGUI _itemTransformedEcoDesc;
    [SerializeField]
    private GameObject _itemTransformedGouvImage;
    [SerializeField]
    private TextMeshProUGUI _itemTransformedGouvDesc;
    [SerializeField]
    private RawImage _sourceItemImage;
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
        _itemTransformedNGODesc.transform.parent.gameObject.SetActive(false);
        _itemTransformedEcoDesc.transform.parent.gameObject.SetActive(false);
        _itemTransformedGouvDesc.transform.parent.gameObject.SetActive(false);
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
            if (InventoryPlayer.Instance.knowsItems.Contains(_primaryItems[i])) _infos[i].GetComponent<Image>().sprite = _primaryItems[i].icon;
            else _infos[i].GetComponent<Image>().sprite = _primaryItems[i].hiddenIcon;
        }
    }

    private void Set(int pIndex) {
        if (InventoryPlayer.Instance.knowsItems.Contains(_primaryItems[pIndex])) _sourceItemImage.texture = _primaryItems[pIndex].icon.texture;
        else _sourceItemImage.texture = _primaryItems[pIndex].hiddenIcon.texture;

        _itemTransformedNGODesc.text = TextManager.GetText(_primaryItems[pIndex].NGOItem.glossaryDesc);
        _itemTransformedEcoDesc.text = TextManager.GetText(_primaryItems[pIndex].EcoItem.glossaryDesc);
        _itemTransformedGouvDesc.text = TextManager.GetText(_primaryItems[pIndex].GouvItem.glossaryDesc);

        if (InventoryPlayer.Instance.knowsItems.Contains(_primaryItems[pIndex].NGOItem))
        {
            _itemTransformedNGOImage.GetComponent<Image>().sprite = _primaryItems[pIndex].NGOItem.icon;
        }
        else _itemTransformedNGOImage.GetComponent<Image>().sprite = _primaryItems[pIndex].NGOItem.hiddenIcon;

        if (InventoryPlayer.Instance.knowsItems.Contains(_primaryItems[pIndex].EcoItem))
        {
            _itemTransformedEcoImage.GetComponent<Image>().sprite = _primaryItems[pIndex].EcoItem.icon;
        }
        else _itemTransformedEcoImage.GetComponent<Image>().sprite = _primaryItems[pIndex].EcoItem.hiddenIcon;

        if (InventoryPlayer.Instance.knowsItems.Contains(_primaryItems[pIndex].GouvItem))
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
        Item knowItem = InventoryPlayer.Instance.knowsItems.Find(e => e.Equals(pItem));
        return (knowItem != null) ? true : false; 
    }
}

