using UnityEngine;
using System.Collections;
using System;

public class PanelTweener : MonoBehaviour
{
    public GameObject bgMask;
    public Tweener tweener;

    private Action callbackEnd;
    public void SetEndCallBack(Action cb)
    {
        callbackEnd = cb;
    }

    public void Awake()
    {
        tweener.SetMethods(Tween, null, null, Disable);
        gameObject.SetActive(false);
        if (bgMask != null) bgMask.gameObject.SetActive(false);
        TweenerLead.Instance.NewTween(GetComponent<RectTransform>(), tweener);
    }

    public void OnEnable()
    {
        ControllerInput.AddScreen(transform);
        TweenerLead.Instance.StartTween(tweener);
    }

    protected void Tween()
    {
        float x1 = Easing.Scale(Easing.SmoothStop, tweener.t, 2, 2f);
        float x2 = Easing.FlipScale(Easing.SmoothStart, tweener.t, 2, 2f);
        float x = Easing.Mix(x1, x2, 0.5f, tweener.t);
        tweener.SetScale(x);
    }

    public void Close()
    {
        if (tweener.Opened)
        {
            TweenerLead.Instance.StartTween(tweener);
        }
    }

    public void Disable()
    {
        if (bgMask != null) bgMask.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        if (callbackEnd != null) callbackEnd();
        ControllerInput.RemoveScreen(transform);
    }
}
