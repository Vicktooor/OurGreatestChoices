using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class DraggableComponent : MonoBehaviour { }

public class Dragger<T> : Singleton<Dragger<T>> where T : DraggableComponent
{
    public RawImage _draggableImg;
    private T _currentDraggable;
    private List<RaycastResult> _hitObjects = new List<RaycastResult>();
    private bool _dragging = false;

    public void Drag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _currentDraggable = GetDraggableTransformUnderMouse();
            if (_currentDraggable)
            {
                _dragging = true;
                _draggableImg.gameObject.SetActive(true);
                _draggableImg.transform.SetAsLastSibling();
                _currentDraggable.GetComponent<Image>().raycastTarget = false;
            }
        }

        if (_dragging)
        {
            _draggableImg.transform.position = Input.mousePosition;
            _draggableImg.texture = _currentDraggable.GetComponent<Image>().mainTexture;

            if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
                T targetDraggable = GetDraggableTransformUnderMouse();
                if (targetDraggable)
                {
                    _draggableImg.gameObject.SetActive(false);
                    _currentDraggable.GetComponent<Image>().raycastTarget = true;
                    ActiveCallbacks(_currentDraggable, targetDraggable);
                }
                else
                {
                    _draggableImg.gameObject.SetActive(false);
                    _currentDraggable.GetComponent<Image>().raycastTarget = true;
                }
            }
        }
        else if (_draggableImg.gameObject.activeSelf) _draggableImg.gameObject.SetActive(false);
    }

    protected GameObject GetObjectUnderMouse()
    {
        Touch touch;
        if (Input.touchCount > 0) touch = Input.GetTouch(0);
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointer, _hitObjects);
        if (_hitObjects.Count <= 0) return null;
        else return _hitObjects[0].gameObject;
    }

    public T GetDraggableTransformUnderMouse()
    {
        GameObject go = GetObjectUnderMouse();
        if (!go) return null;
        T draggable = GetObjectUnderMouse().GetComponent<T>();
        if (draggable) return draggable;
        return null;
    }

    #region  Callbacks

    private List<Action<T, T>> _callbacks = new List<Action<T, T>>();
    private void ActiveCallbacks(T dragObj, T targetObj) { foreach (Action<T, T> callbacks in _callbacks) callbacks(dragObj, targetObj); }
    public void AddCallback(Action<T, T> cb)
    {
        if (!_callbacks.Contains(cb)) _callbacks.Add(cb);
    }
    public void RemoveCallback(Action<T, T> cb)
    {
        if (_callbacks.Contains(cb)) _callbacks.Remove(cb);
    }

    #endregion
}