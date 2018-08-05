using Assets.Scripts.Game.UI;
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
    GameObject _transformationButton;

    [SerializeField]
    GameObject givePanel;

    [SerializeField]
    GameObject _giveButton;

    [SerializeField]
    GameObject inventoryButtonClose; //DISABLE THE CLOSE BUTTON OF THE INVENTORY WHEN THIS PANEL IS SHOWN

    public InteractablePNJ clickedNPC;

    public int initIndex;

    private int _currentIndex; //Index of the selected item
    private Item _currentTransformedItem;
    private Item _currentGiveItem;

    [SerializeField]
    Sprite wrongButtonSprite;

    private void Awake() {
        Events.Instance.AddListener<OnScrolling>(UpdateButton);
    }

    private void OnEnable() {
        transformationPanel.SetActive(true);
        inventoryButtonClose.SetActive(false);
        givePanel.SetActive(false);
        InventoryStrip.SetActive(false);

        Events.Instance.Raise(new OnPopUp());
        Events.Instance.Raise(new OnStartSpeakingNPC());
        Init();
    }

    private void OnDisable() {
        Close();
    }

    void Close() {
        inventoryButtonClose.SetActive(true);
        transformationPanel.SetActive(false);
        givePanel.SetActive(false);
        InventoryStrip.SetActive(true);
        NPCPanel.SetActive(false);

        Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.FINISH_TYPE));
        Events.Instance.Raise(new OnEndSpeakingNPC());
        PointingBubble.instance.Show(false);

        clickedNPC = null;
        _currentIndex = 0;
        _currentTransformedItem = null;
        _currentGiveItem = null;
    }

    void Init() {
        _currentIndex = LayoutElementDisplayCheck.currentIndex;

        if (InventoryPlayer.instance.itemsWornArray.Count > 0) UpdateTransformedButton(InventoryPlayer.instance.itemsWornArray[_currentIndex]);
        if (InventoryPlayer.instance.itemsWornArray.Count > 0) UpdateGiveButton(InventoryPlayer.instance.itemsWornArray[_currentIndex]);
    }

	void UpdateButton(OnScrolling e) {
        if (e.targetList == EScrollList.Glossary) return;
        if (e.index < InventoryPlayer.instance.itemsWornArray.Count)
        {
            Item item = InventoryPlayer.instance.itemsWornArray[e.index];
            _currentIndex = e.index;

            if (transformationPanel.activeSelf) UpdateTransformedButton(item);
            else UpdateGiveButton(item);
        }
    }

    void UpdateTransformedButton(Item pItem) {
        if (pItem.NGOItem == null || pItem.EcoItem == null || pItem.GouvItem == null) {
            _transformationButton.GetComponent<Image>().sprite = pItem.icon;
            return;
        }

        if (clickedNPC.item.type == EPlayer.NGO) {
            if (InventoryPlayer.instance.knowsItems.Contains(pItem.NGOItem))
                _transformationButton.GetComponent<Image>().sprite = pItem.NGOItem.hiddenIcon;
            else _transformationButton.GetComponent<Image>().sprite = pItem.NGOItem.hiddenIcon;

            _currentTransformedItem = pItem.NGOItem;
        }

        if (clickedNPC.item.type == EPlayer.ECO) {
            if (InventoryPlayer.instance.knowsItems.Contains(pItem.EcoItem))
                _transformationButton.GetComponent<Image>().sprite = pItem.EcoItem.icon;
            else _transformationButton.GetComponent<Image>().sprite = pItem.EcoItem.hiddenIcon;

            _currentTransformedItem = pItem.EcoItem;
        }

        if (clickedNPC.item.type == EPlayer.GOV) {
            if (InventoryPlayer.instance.knowsItems.Contains(pItem.GouvItem))
                _transformationButton.GetComponent<Image>().sprite = pItem.GouvItem.icon;
            else _transformationButton.GetComponent<Image>().sprite = pItem.GouvItem.hiddenIcon;

            _currentTransformedItem = pItem.GouvItem;
        }
    }

    void UpdateGiveButton(Item pItem) {
        if (pItem.icon != null)
        {
            _giveButton.GetComponent<Image>().sprite = pItem.icon;
            _currentGiveItem = pItem;
        }
    }

    //UI Event
    public void Transformation() {
        //Send an event to InventoryPlayer to update the items Array and to InventoryScreen to update UI
        Events.Instance.Raise(new OnTransformation(_currentIndex, _currentTransformedItem));
        UpdateTransformedButton(_currentTransformedItem);
        FBX_Transform.instance.Play(_transformationButton.transform.position);
        Events.Instance.Raise(new OnClickTransform());
    }

    //UI Event
    public void Give() {
        if (clickedNPC.CanAccept(_currentGiveItem)) {
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
        }
    }

    //UI Event
    public void TransformToGive() {
        _transformationButton.GetComponent<Shake>().Clear();
        transformationPanel.SetActive(false);
        givePanel.SetActive(true);
        if (_currentIndex < InventoryPlayer.instance.itemsWornArray.Count) UpdateGiveButton(InventoryPlayer.instance.itemsWornArray[_currentIndex]);
    }

    //UI Event
    public void GiveToTransform() {
        _giveButton.GetComponent<Shake>().Clear();
        givePanel.SetActive(false);
        transformationPanel.SetActive(true);
        if (_currentIndex < InventoryPlayer.instance.itemsWornArray.Count) UpdateTransformedButton(InventoryPlayer.instance.itemsWornArray[_currentIndex]);
    }
}
