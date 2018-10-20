using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public struct TweenerInfo
{
    public bool active;
    public float time;
    public Vector3 startScale;
    public Vector3 targetPos;
    public Vector3 startPos;
    public Action startCB;
    public Action runMethod;
    public Action openCB;
    public Action closeCB;
}

[Serializable]
public class Tweener
{
    private bool _opening = true;
    public bool Opening { get { return _opening; } }

    public bool active = false;
    public float t = 0f;
    public float time = 0f;

    public Vector3 startScale = Vector3.one;
    public Vector3 targetPos = Vector3.zero;

    public Action startCB = null;
    public Action openCB = null;
    public Action closeCB = null;
    public Action runMethod;

    private TweenerInfo _tweenInfo;
    public TweenerInfo TweenInfo { get { return _tweenInfo; } }

    public void SetStartPos(Vector3 position)
    {
        _tweenInfo.startPos = position;
    }

    public void SetProperties(Vector3 iScale, Vector3 tPos, float rTime)
    {
        startScale = iScale;
        targetPos = tPos;
        time = rTime;

        _tweenInfo.startScale = iScale;
        _tweenInfo.targetPos = tPos;
        _tweenInfo.time = rTime;
    }

    public void SetMethods(Action run, Action start, Action open, Action close)
    {
        runMethod = run;
        startCB = start;
        closeCB = close;
        openCB = open;

        _tweenInfo.runMethod = run;
        _tweenInfo.startCB = start;
        _tweenInfo.openCB = open;
        _tweenInfo.closeCB = close;
    }

    public void Open()
    {
        if (t <= 0 && startCB != null) startCB();
        t += Time.deltaTime * (1f / time);
        t = Mathf.Clamp01(t);
        if (runMethod != null) runMethod();
        if (t >= 1f)
        {
            active = false;
            _opening = false;
            if (openCB != null) openCB();
        }
    }

    public void Close()
    {
        t -= Time.deltaTime * (1f / time);
        t = Mathf.Clamp01(t);
        if (runMethod != null) runMethod();
        if (t <= 0f)
        {
            active = false;
            _opening = true;
            if (closeCB != null) closeCB();
        }
    }

    public void Reset()
    {
        active = false;
        time = _tweenInfo.time;
        startScale = _tweenInfo.startScale;
        targetPos = _tweenInfo.targetPos;
        startCB = _tweenInfo.startCB;
        runMethod = _tweenInfo.runMethod;
    }
}

public class UITweener : MonoSingleton<UITweener>
{
    private Dictionary<RectTransform, Tweener> _tweens = new Dictionary<RectTransform, Tweener>();

    public void NewTween(RectTransform element, Tweener properties)
    {
        properties.SetStartPos(element.localPosition);
        if (!_tweens.ContainsKey(element)) _tweens.Add(element, properties);
    }

    public void StartTween(RectTransform element)
    {
        if (_tweens.ContainsKey(element)) _tweens[element].active = true;
    }

    public void StopTween(RectTransform element)
    {
        if (_tweens.ContainsKey(element)) _tweens[element].active = false;
    }

    public void ResetTween(RectTransform element)
    {
        if (_tweens.ContainsKey(element))
        {
            element.localPosition = _tweens[element].TweenInfo.startPos;
            _tweens[element].Reset();
        }
    }

    public void RemoveTween(RectTransform element)
    {
        if (_tweens.ContainsKey(element)) _tweens.Remove(element);
    }

    public void Update()
    {
        int nb = _tweens.Count;
        foreach (KeyValuePair<RectTransform, Tweener> tween in _tweens)
        {
            if (tween.Value.active)
            {
                if (tween.Value.Opening) tween.Value.Open();
                else tween.Value.Close();
            }
        }
    }
}
