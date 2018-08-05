using UnityEngine;
using TMPro;
using Assets.Scripts.Items;

public class BillboardBubble : BillboardElement
{
    public TextMeshPro text;
    [HideInInspector]
    public CitizenProp citizen;

    public void SetVisibility(float dist)
    {
        float k;
        float minDist = CitizenProp.talkDistance / 2f;
        if (dist <= minDist)
        {
            transform.localScale = new Vector3(_baseScale.x, _baseScale.y, _baseScale.z);
        }
        else
        {
            float refDist = CitizenProp.talkDistance - minDist;
            k = Mathf.Clamp01(Mathf.Abs(1f - refDist / (dist - minDist)));
            transform.localScale = new Vector3(_baseScale.x * k, _baseScale.y * k, _baseScale.z * k);
        }
    }
}
