using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class Tweener
{
    private bool _opened = false;
    public bool Opened { get { return _opened; } }

    public bool active = false;
    public float t = 0f;
    public float time = 0f;
    public bool relative = false;

    public Vector3 startScale = Vector3.one;
    public Vector3 targetScale = Vector3.one;
    public Vector3 targetPos = Vector3.zero;

    private Vector3 _startPos;
    public Vector3 StartPos { get { return _startPos; } }

    public Action startCB = null;
    public Action openCB = null;
    public Action closeCB = null;
    public Action runMethod = null;

    private Transform _targetTransform;
    public Transform TargetTransform { get { return _targetTransform; } }

    public void Init(Transform trf)
    {
        _targetTransform = trf;
        _startPos = (relative) ? trf.localPosition : trf.position;
        SetScale(0f);
        SetPos(0f);
    }

    public void SetPos(float x)
    {
        if (relative) _targetTransform.localPosition = MathCustom.LerpUnClampVector(_startPos, targetPos, x);
        else _targetTransform.position = MathCustom.LerpUnClampVector(_startPos, targetPos, x);
    }

    public void SetScale(float x)
    {
        _targetTransform.localScale = MathCustom.LerpUnClampVector(startScale, targetScale, x);
    }

    public void SetMethods(Action run, Action start, Action open, Action close)
    {
        runMethod = run;
        startCB = start;
        openCB = open;
        closeCB = close;
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
            _opened = true;
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
            _opened = false;
            if (closeCB != null) closeCB();
        }
    }

    public void Reset()
    {
        if (relative) _targetTransform.localPosition = _startPos;
        else _targetTransform.position = _startPos;
        active = false;
        _opened = false;
        t = 0f;
    }
}

public class TweenerLead : MonoSingleton<TweenerLead>
{
    private List<Tweener> _tweens = new List<Tweener>();

    public void NewTween(Transform element, Tweener properties)
    {
        properties.Init(element);
        if (!_tweens.Contains(properties)) _tweens.Add(properties);
    }

    public void StartTween(Tweener element)
    {
        if (_tweens.Contains(element)) element.active = true;
    }

    public void StopTween(Tweener element)
    {
        if (_tweens.Contains(element)) element.active = false;
    }

    public void ResetTween(Tweener element)
    {
        if (_tweens.Contains(element)) element.Reset();
    }

    public void RemoveTween(Tweener element)
    {
        if (_tweens.Contains(element)) _tweens.Remove(element);
    }

    public void Update()
    {
        int nb = _tweens.Count;
        foreach (Tweener tween in _tweens)
        {
            if (tween.active)
            {
                if (tween.Opened) tween.Close();
                else tween.Open();
            }
        }
    }
}
