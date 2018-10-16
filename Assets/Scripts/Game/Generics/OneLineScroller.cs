using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EScrollDirection { VERTICAL, HORIZONTAL }

[RequireComponent(typeof(RectTransform))]
public class OneLineScroller : MonoBehaviour
{
    private Action _callback;

    private RectTransform _rect;
    private List<RectTransform> _elementList;
    private int _nbElement;
    private Vector3 _basePos;
    private float _length;
    private int _currentIndex = 0;
    private bool _moving = false;

    public int CurrentIndex { get { return -_currentIndex; } }

    public float defaultLength;
    public bool smoothScale;
    public float minScale;
    public float maxScale;
    public float space;
    public EScrollDirection scrollDirection;
    public float speed = 1f;

    public void Init()
    {
        _rect = GetComponent<RectTransform>();
        _basePos = _rect.localPosition;
        RectTransform parentRect = _rect.parent.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            if (scrollDirection == EScrollDirection.HORIZONTAL) _length = parentRect.rect.width;
            else _length = parentRect.rect.height;
        }
        else _length = defaultLength;
        GetElements();
    }

    private void GetElements()
    {
        _elementList = new List<RectTransform>();
        RectTransform[] objs = GetComponentsInChildren<RectTransform>();
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            RectTransform obj = objs[i];
            if (!_rect.Equals(obj)) Add(obj);
        }
        Scale();
    }

    private void Add(RectTransform element)
    {
        _elementList.Add(element);
        _nbElement = _elementList.Count;
        int index = _nbElement - 1;
        PlaceItem(index, _elementList[index]);
    }

    public T Add<T>(GameObject elementModel) where T : Component
    {
        GameObject newElement = Instantiate(elementModel, _rect);
        RectTransform rect = newElement.GetComponent<RectTransform>();
        if (rect != null)
        {
            Add(rect);
            return newElement.GetComponent<T>();
        }
        else return null;
    }

    public void Remove<T>(T element) where T : Component
    {
        RectTransform toRemove = _elementList.Find(e => e.GetComponent<T>().Equals(element));
        if (toRemove != null)
        {
            _elementList.Remove(toRemove);
            _nbElement = _elementList.Count;
            Destroy(toRemove.gameObject);
            ReplaceAll();
        }
    }

    public T Get<T>(int index) where T : Component
    {
        if (index < _elementList.Count) return _elementList[index] as T;
        else return null;
    }

    private void ReplaceAll()
    {
        int nbE = _elementList.Count;
        for (int i = 0; i < nbE; i++) PlaceItem(i, _elementList[i]);
    }

    private void PlaceItem(int index, Transform eTransform)
    {
        eTransform.localPosition = (Direction(scrollDirection) * index * space);
    }

    public void Place(int index)
    {
        _currentIndex = index;
        _rect.localPosition = _basePos + (Direction(scrollDirection) * index * space);
        Scale();
    }

    private void Place(float value)
    {
        _rect.localPosition = _basePos + (Direction(scrollDirection) * value * space) + (Direction(scrollDirection) * _currentIndex * space);
        Scale();
    }

    private Vector3 GetIndexLocalPos(int index)
    {
        return _elementList[index].localPosition + _rect.localPosition;
    }

    private Vector3 Direction(EScrollDirection scrollDir)
    {
        if (scrollDir == EScrollDirection.HORIZONTAL) return Vector3.right;
        else return Vector3.down;
    }

    public void Scroll(float value)
    {
        if (Stop()) return;
        _rect.localPosition += Direction(scrollDirection) * value;
        Scale();
        GetIndexFromPos();
    }

    public void Scale()
    {
        Vector3 localPos;
        float nDist;
        float size;
        foreach (Transform t in _elementList)
        {
            localPos = t.localPosition + _rect.localPosition;
            if (smoothScale)
            {
                nDist = Easing.SetInRange(Mathf.Clamp(Vector3.Distance(localPos, _basePos), 0f, _length / 2f), 0f, _length / 2f);
                size = Mathf.Clamp(Easing.FlipScale(Easing.Mix(Easing.SmoothStart, 2, Easing.SmoothStop, 2, 0.25f, nDist), maxScale), minScale, maxScale);
                t.localScale = new Vector3(size, size, size);
            }
        }
    }

    private bool Stop()
    {
        if (scrollDirection == EScrollDirection.HORIZONTAL)
        {
            if (_rect.localPosition.x > _basePos.x)
            {
                Place(0);
                return true;
            }
            else 
            {
                Vector3 lastPos = GetIndexLocalPos(_nbElement - 1);
                if (lastPos.x < _basePos.x)
                {
                    Place(-(_nbElement - 1));
                    return true;
                }
            }
            return false;
        }
        else
        {
            if (_rect.localPosition.y < _basePos.y)
            {
                Place(0);
                return true;
            }
            else
            {
                Vector3 lastPos = GetIndexLocalPos(_nbElement - 1);
                if (lastPos.y > _basePos.y)
                {
                    Place(-(_nbElement - 1));
                    return true;
                }
            }
            return false;
        }
    }

    public int GetIndexFromPos()
    {
        int index = 0;
        Vector3 localPos;
        int length = _elementList.Count;
        float dist = 0;
        float lDist = 0;
        for (int i = 0; i < length; i++)
        {
            Transform e = _elementList[i];
            localPos = e.localPosition + _rect.localPosition;
            lDist = Vector3.Distance(localPos, _basePos);
            if (i == 0)
            {
                dist = lDist;
                index = i;
            }
            else if (lDist < dist)
            {
                dist = lDist;
                index = i;
            }
        }
        return index;
    }

    public void Move(int value)
    {
        if (!_moving) StartCoroutine(MoveCoroutine(value));
    }

    public void SetMoveCallback(Action cb) { _callback = cb; }
    public void DelMoveCallback() { _callback = null; }

    private IEnumerator MoveCoroutine(int val)
    {
        _moving = true;
        float t = 0;
        float x = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            t = (t > 1f) ? 1f : t;
            x = Easing.SmoothStop(t, 2) * (float)val;
            Place(x);
            yield return null;
        }
        _currentIndex += (val > 0f) ? 1 : -1;
        if (_currentIndex > 0f || _currentIndex < -(_nbElement - 1)) StartCoroutine(MoveCoroutine(-val));
        else
        {
            _moving = false;
            if (_callback != null) _callback();
        }
    }
}
