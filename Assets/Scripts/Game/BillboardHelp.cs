using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.Game.NGO;
using Assets.Script;

[Serializable]
public struct PNJHelp
{
    public EPlayer targetPlayer;
    public Sprite sprite;
}

public class BillboardHelp : BillboardElement
{
    public HelpSprites helpSprites;

    protected SpriteRenderer sprite;
    [HideInInspector]
    public InteractablePNJ pnj;

    public void Init(EPlayer playerType)
    {
        sprite = GetComponent<SpriteRenderer>();
        int length = helpSprites.sprites.Count;
        for (int i = 0; i < length; i++)
        {
            if (playerType == helpSprites.sprites[i].targetPlayer)
            {
                sprite.sprite = helpSprites.sprites[i].sprite;
                break;
            }
        }
        SetColor(playerType);
    }

    private void SetColor(EPlayer playerType)
    {
        if (playerType == EPlayer.ECO)
        {
            if (pnj.item.type == EPlayer.ECO) sprite.color = helpSprites.ecoColor;
            else if (pnj.item.type == EPlayer.GOV) sprite.color = helpSprites.gouvColor;
            else if (pnj.item.type == EPlayer.NGO) sprite.color = helpSprites.ngoColor;
        }
        else sprite.color = Color.white;
    }

    public void SetVisibility(float dist)
    {
        float k;
        float minDist = InteractablePNJ.helpDistance / 2f;
        if (dist <= minDist)
        {
            transform.localScale = new Vector3(_baseScale.x, _baseScale.y, _baseScale.z);
        }
        else
        {
            float refDist = InteractablePNJ.helpDistance - minDist;
            k = Mathf.Clamp01(Mathf.Abs(1f - refDist / (dist - minDist)));
            transform.localScale = new Vector3(_baseScale.x * k, _baseScale.y * k, _baseScale.z * k);
        }
    }
}
