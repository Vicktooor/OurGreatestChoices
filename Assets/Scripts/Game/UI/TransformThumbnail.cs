using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.Game.UI.Ftue;

public class TransformThumbnail : MonoBehaviour
{
    public Color badColor;
    public Color goodColor;

    private TextMeshProUGUI _textMesh;
    private RawImage _img;

    public void Set(Item sourceItem, Item resultItem)
    {
        if (sourceItem == null || resultItem == null) return;

        if (_textMesh == null || _img == null)
        {
            _textMesh = GetComponentInChildren<TextMeshProUGUI>(true);
            _img = GetComponent<RawImage>();
        }

        int nbSource = InventoryPlayer.Instance.nbItems[sourceItem.itemType];
        if (nbSource >= resultItem.nbForCraft) _textMesh.color = goodColor;
        else _textMesh.color = badColor;
        _textMesh.text = InventoryPlayer.Instance.nbItems[sourceItem.itemType] + "/" + ((FtueManager.instance.active) ? 3 : resultItem.nbForCraft);
        _img.texture = sourceItem.icon.texture;
    }
}
