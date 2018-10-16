using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : MonoSingleton<InventoryScreen>
{
    public GameObject elementModel;
    public OneLineScroller scroller;
    public List<InventoryElement> scrollElement;

    protected override void Awake() {
        base.Awake();
        scroller.Init();
        Events.Instance.AddListener<OnGive>(Give);
        Events.Instance.AddListener<OnClearInventory>(Clear);
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

    private void Give(OnGive e)
    {

    }

    private void Clear(OnClearInventory e)
    {

    }

    private void OnDestroy() {
        Events.Instance.RemoveListener<OnGive>(Give);
        Events.Instance.RemoveListener<OnClearInventory>(Clear);
    }
}
