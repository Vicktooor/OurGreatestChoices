using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.Game.UI.Ftue;

public class UIFtueImageContainer : UIFtueElement
{
    private float _alpha;
    public float Alpha {
        set {
            _alpha = Mathf.Clamp01(value);
            SetAlpha(_alpha);
        }
        get { return _alpha; }
    }

    private Image[] imgs;
    private RawImage[] rawImgs;

    protected void Awake()
    {
        imgs = GetComponentsInChildren<Image>(true);
        rawImgs = GetComponentsInChildren<RawImage>(true);
    }

    protected virtual void SetAlpha(float alpha)
    {
        if (imgs != null)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                Color lColor = imgs[i].color;
                lColor.a = alpha;
                imgs[i].color = lColor;
            }
        }
        
        if (rawImgs != null)
        {
            for (int i = 0; i < rawImgs.Length; i++)
            {
                Color lColor = rawImgs[i].color;
                lColor.a = alpha;
                rawImgs[i].color = lColor;
            }
        }
        
    }

    public void SetState(bool state)
    {
        gameObject.SetActive(state); if (imgs != null)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                imgs[i].raycastTarget = state;
            }
        }

        if (rawImgs != null)
        {
            for (int i = 0; i < rawImgs.Length; i++)
            {
                rawImgs[i].raycastTarget = state;
            }
        }
    }
}
