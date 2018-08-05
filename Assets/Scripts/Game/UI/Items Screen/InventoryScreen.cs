using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : MonoBehaviour {

    private List<GameObject> _slotsList;

    [SerializeField]
    private GameObject _slotsContainer;

    private void Awake() {
        Init();
        Events.Instance.AddListener<OnTransformation>(Transformation);
        Events.Instance.AddListener<OnGive>(Give);
        Events.Instance.AddListener<OnClear>(Clear);
    }

    private void OnEnable() {
        SetIcons();
        Events.Instance.Raise(new OnClickGlossary());
    }

    private void OnDisable() {
        for (int i = 0; i < _slotsList.Count; i++) {
            _slotsList[i].gameObject.SetActive(false);
        }
    }

    void Init() {
        _slotsList = new List<GameObject>();
        InitSlots();
    }

    void InitSlots() {
        int numberChildren = _slotsContainer.transform.childCount;

        for (int i = 0; i < numberChildren; i++) {
            _slotsList.Add(_slotsContainer.transform.GetChild(i).gameObject);
            _slotsList[i].gameObject.SetActive(false);
        }
    }

    void SetIcons() {
        List<Item> itemsArray = InventoryPlayer.instance.itemsWornArray;
        CleanIcons();

        for (int i = 0; i < itemsArray.Count; i++) {

            _slotsList[i].gameObject.SetActive(false);

            if (itemsArray[i].icon) {
                _slotsList[i].gameObject.SetActive(true);
                _slotsList[i].GetComponent<Image>().sprite = itemsArray[i].icon;
            }
            else Debug.Log("THIS ITEM DOESN T HAVE ICON");
        }
    }

    void Transformation(OnTransformation e) {
        if (e.item)
        {
            _slotsList[e.index].GetComponent<Image>().sprite = e.item.icon;
        }
    }

    void Give(OnGive e) {
        for (int i = 0; i < _slotsList.Count; i++) {
            _slotsList[i].gameObject.SetActive(false);
        }

        SetIcons();
    }

    void Clear(OnClear e) {
        _slotsList = new List<GameObject>();
        ResetIcons();
    }

    void ResetIcons() {
        int numberChildren = _slotsContainer.transform.childCount;

        for (int i = 0; i < numberChildren; i++) {
            _slotsList.Add(_slotsContainer.transform.GetChild(i).gameObject);
            _slotsList[i].gameObject.SetActive(false);
            _slotsList[i].GetComponent<Image>().sprite = null;
        }
    }

    void CleanIcons() {
        for (int i = 0; i < _slotsList.Count; i++) {
            _slotsList[i].gameObject.SetActive(false);
        }
    }

    private void OnDestroy() {
        Events.Instance.RemoveListener<OnTransformation>(Transformation);
        Events.Instance.RemoveListener<OnGive>(Give);
        Events.Instance.RemoveListener<OnClear>(Clear);
    }
}
