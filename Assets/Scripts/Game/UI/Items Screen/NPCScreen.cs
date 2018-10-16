﻿using Assets.Scripts.Game.UI;
using UnityEngine;
using UnityEngine.UI;

public class NPCScreen : MonoBehaviour {

    [SerializeField]
    GameObject InventoryStrip;
    [SerializeField]
    GameObject NPCPanel;
    [SerializeField]
    GameObject transformationPanel;
    [SerializeField]
    Image _transformationButton;
    [SerializeField]
    GameObject inventoryButtonClose;

    public TransformThumbnail thumbnail;
    public InteractablePNJ clickedNPC;
    public OneLineScroller scroller;

    private Item _sourceItem;
    private Item _resultItem;
    private Sprite _noneSprite;

    private void Awake()
    {
        _noneSprite = _transformationButton.sprite;
    }

    private void OnEnable() {
        transformationPanel.SetActive(true);
        inventoryButtonClose.SetActive(false);
        InventoryStrip.SetActive(false);
        thumbnail.gameObject.SetActive(true);

        if (InventoryPlayer.instance.itemsWornArray.Count > 0)
        {
            scroller.Place(0);
            Set(InventoryPlayer.instance.itemsWornArray[scroller.CurrentIndex]);
        }
        else
        {
            _transformationButton.sprite = _noneSprite;
            thumbnail.gameObject.SetActive(false);
        }

        Events.Instance.Raise(new OnPopUp());
        Events.Instance.Raise(new OnStartSpeakingNPC());
        scroller.SetMoveCallback(SetByIndex);
    }

    private void OnDisable() {
        Close();
    }

    void Close() {
        scroller.DelMoveCallback();
        inventoryButtonClose.SetActive(true);
        transformationPanel.SetActive(false);
        InventoryStrip.SetActive(true);
        NPCPanel.SetActive(false);

        Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
        Events.Instance.Raise(new OnEndSpeakingNPC());
        PointingBubble.instance.Show(false);

        clickedNPC = null;
    }

    public void Set(Item pItem) {
        _sourceItem = pItem;
        thumbnail.gameObject.SetActive(true);
        if (clickedNPC.item.type == EPlayer.NGO) {
            if (InventoryPlayer.instance.knowsItems.Contains(pItem.NGOItem)) _transformationButton.sprite = pItem.NGOItem.icon;
            else
            {
                if (pItem.NGOItem == null)
                {
                    _transformationButton.sprite = _noneSprite;
                    thumbnail.gameObject.SetActive(false);
                }
                else _transformationButton.sprite = pItem.NGOItem.hiddenIcon;
            }
            _resultItem = pItem.NGOItem;
        }
        if (clickedNPC.item.type == EPlayer.ECO) {
            if (InventoryPlayer.instance.knowsItems.Contains(pItem.EcoItem)) _transformationButton.sprite = pItem.EcoItem.icon;
            else
            {
                if (pItem.EcoItem == null)
                {
                    _transformationButton.sprite = _noneSprite;
                    thumbnail.gameObject.SetActive(false);
                }
                else _transformationButton.sprite = pItem.EcoItem.hiddenIcon;
            }         
            _resultItem = pItem.EcoItem;
        }
        if (clickedNPC.item.type == EPlayer.GOV) {
            if (InventoryPlayer.instance.knowsItems.Contains(pItem.GouvItem)) _transformationButton.sprite = pItem.GouvItem.icon;
            else
            {
                if (pItem.GouvItem == null)
                {
                    _transformationButton.sprite = _noneSprite;
                    thumbnail.gameObject.SetActive(false);
                }
                else _transformationButton.sprite = pItem.GouvItem.hiddenIcon;
            }
            _resultItem = pItem.GouvItem;
        }
        thumbnail.Set(_sourceItem, _resultItem);
    }

    private void SetByIndex()
    {
        Set(InventoryPlayer.instance.itemsWornArray[scroller.CurrentIndex]);
    }

    public void OnTransformation() {
        int leftNB = InventoryPlayer.instance.nbItems[_sourceItem.name];
        if (leftNB >= _resultItem.nbForCraft)
        {
            leftNB -= _resultItem.nbForCraft;
            InventoryPlayer.instance.nbItems[_sourceItem.name] = leftNB;
            thumbnail.Set(_sourceItem, _resultItem);
            Events.Instance.AddListener<OnEndTransformation>(OnEndTransformation);
            Events.Instance.Raise(new OnTransformation(scroller.CurrentIndex, _resultItem));
            FBX_Transform.instance.Play(_transformationButton.transform.position);
            Events.Instance.Raise(new OnClickTransform());
        }
    }

    private void OnEndTransformation(OnEndTransformation e)
    {
        Events.Instance.RemoveListener<OnEndTransformation>(OnEndTransformation);
        if (InventoryPlayer.instance.itemsWornArray.Count > 0)
        {
            InventoryScreen.Instance.MajInventory(null);
            if (scroller.CurrentIndex >= InventoryPlayer.instance.itemsWornArray.Count) scroller.Move(1);
            else Set(InventoryPlayer.instance.itemsWornArray[scroller.CurrentIndex]);
        }
        else
        {
            _transformationButton.sprite = _noneSprite;
            thumbnail.gameObject.SetActive(false);
        }
    }

    //UI Event
    public void Give() {
        /*if (clickedNPC.CanAccept(_currentGiveItem)) {
            FBX_Give.instance.Play(new Vector3(_giveButton.transform.position.x, _giveButton.transform.position.y/2, _giveButton.transform.position.z));
            PointingBubble.instance.Show(true);
            PointingBubble.instance.SetPNJ(clickedNPC);
            //if (clickedNPC) PointingBubble.instance.SetProperties(clickedNPC);
            if (LocalizationManager.Instance.currentLangage == EnumClass.Language.en) PointingBubble.instance.ChangeText("thanks!");
            else PointingBubble.instance.ChangeText("Merci!");

            //Events.Instance.Raise(new OnGoodObject());
            Events.Instance.Raise(new OnGive(_currentIndex));
            Events.Instance.Raise(new OnGiveNPC(_currentGiveItem, clickedNPC));
            
            _giveButton.GetComponent<Image>().sprite = wrongButtonSprite;
            _currentGiveItem = null;
        }
        else {
            PointingBubble.instance.Show(true);
            PointingBubble.instance.SetPNJ(clickedNPC);
            //if (clickedNPC) PointingBubble.instance.SetProperties(clickedNPC);
            if (LocalizationManager.Instance.currentLangage == EnumClass.Language.en) PointingBubble.instance.ChangeText("No thanks!");
            else PointingBubble.instance.ChangeText("Non merci!");

            _giveButton.GetComponent<Shake>().DoShake();
            Events.Instance.Raise(new OnWrongObject());
        }*/
    }
}
