using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryScreen : MonoSingleton<InventoryScreen>
{
    public Transform dragImageTransform;
    private InventoryElement _targetDraggable;
    private List<RaycastResult> _hitObjects = new List<RaycastResult>();
    private bool _dragging;

    public GameObject elementModel;
    public OneLineScroller scroller;
    public List<InventoryElement> scrollElement;

    public RawImage draggerTransform;

    protected override void Awake() {
        base.Awake();
        scroller.Init();
        Events.Instance.AddListener<OnGive>(Give);
        gameObject.SetActive(false);
    }

    public void MajInventory(OnUpdateInventory e)
    {
        List<Item> items = InventoryPlayer.instance.itemsWornArray;
        int length = items.Count;
        Item item;
        for (int i = length - 1; i >= 0; i--)
        {
            item = items[i];
            InventoryElement ie = scrollElement.Find(el => el.itemName == item.name);
            if (ie != null && InventoryPlayer.instance.nbItems[item.name] <= 0)
            {
                InventoryPlayer.instance.itemsWornArray.Remove(item);
                scrollElement.Remove(ie);
                scroller.Remove(ie);
            }
            else
            {
                if (ie != null) ie.MajText();
                else
                {
                    InventoryElement newE = scroller.Add<InventoryElement>(elementModel);
                    newE.itemName = item.name;
                    newE.Init();
                    scrollElement.Add(newE);
                }
            }
        }
        scroller.Scale();
    }

    protected void HandleDrag(InventoryElement e1, InventoryElement e2)
    {
        Debug.Log(e1.itemName + "/" + e2.itemName);
    }

    protected void OnEnable()
    {
        Dragger<InventoryElement>.Instance._draggableImg = draggerTransform;
        Dragger<InventoryElement>.Instance.AddCallback(HandleDrag);
    }

    protected void Update()
    {
        Dragger<InventoryElement>.Instance.Drag();
    }

    protected void OnDisable()
    {
        Dragger<InventoryElement>.Instance.RemoveCallback(HandleDrag);
    }

    private void Give(OnGive e)
    {

    }

    private void OnDestroy() {
        Events.Instance.RemoveListener<OnGive>(Give);
    }
}
