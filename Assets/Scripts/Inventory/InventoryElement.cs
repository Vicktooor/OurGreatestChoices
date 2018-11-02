using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class InventoryElement : DraggableComponent
{
    public EItemType itemType;
    private Image img;
    private TextMeshProUGUI textMesh;

    public void Init()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        img = GetComponent<Image>();
        Item item = InventoryPlayer.Instance.itemsWornArray.Find(i => i.itemType == itemType);
        img.sprite = item.icon;
        textMesh.text = InventoryPlayer.Instance.nbItems[itemType].ToString();
    }

    public void MajText()
    {
        textMesh.text = InventoryPlayer.Instance.nbItems[itemType].ToString();
    }

}
