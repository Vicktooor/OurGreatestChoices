using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class InventoryElement : MonoBehaviour
{
    public string itemName;
    private Image img;
    private TextMeshProUGUI textMesh;

    public void Init()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        img = GetComponent<Image>();
        Item item = InventoryPlayer.instance.itemsWornArray.Find(i => i.name == itemName);
        img.sprite = item.icon;
        textMesh.text = InventoryPlayer.instance.nbItems[itemName].ToString();
    }

    public void MajText()
    {
        textMesh.text = InventoryPlayer.instance.nbItems[itemName].ToString();
    }
}
