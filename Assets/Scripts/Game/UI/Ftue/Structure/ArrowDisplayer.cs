using UnityEngine;
using Assets.Scripts.Game.UI;
using System;
using System.Collections.Generic;

[Serializable]
public class ArrowDisplayer
{
    private static Dictionary<string, ArrowDisplayer> _instances = new Dictionary<string, ArrowDisplayer>();

    private ObjectArray<UIObjectPointer> _arrows = new ObjectArray<UIObjectPointer>();
    private Transform _container;
    private UIObjectPointer _modelSimple;
    private UIObjectPointIcon _modelIcon;

    /// <summary>
    /// instance unique de la classe     
    /// </summary>
    public static ArrowDisplayer Instances(string instanceName)
    {
        if (instanceName == string.Empty)
        {
            Debug.Log("Can't create instance of ArrowDisplayer with empty name");
            return null;
        }

        if (_instances.Count > 0)
        {
            if (_instances.ContainsKey(instanceName)) return _instances[instanceName];
            else return NewInstance(instanceName);
        }
        else return NewInstance(instanceName);
    }

    private static ArrowDisplayer NewInstance(string instanceName)
    {
        ArrowDisplayer nInstance = new ArrowDisplayer();
        _instances.Add(instanceName, nInstance);
        return nInstance;
    }

    public static void DeleteInstance(string instanceName)
    {
        if (instanceName != string.Empty)
        {
            if (_instances.ContainsKey(instanceName))
            {
                _instances[instanceName].CleanArrows();
                _instances.Remove(instanceName);
            }
        }
    }

    public void SetModels(UIObjectPointer simpleModel, UIObjectPointIcon iconModel)
    {
        _modelSimple = simpleModel;
        _modelIcon = iconModel;
    }

    public void SetContainer(RectTransform rtransform)
    {
        _container = rtransform;
    }

    public T UseArrow<T>(float pDistance, float pAngle, bool autoRot, Vector3 worldPos, string displayerName, bool autoDestroy = true) where T : UIObjectPointer
    {
        if (_container == null) return null;
        if (_modelSimple == null) return null;
        else
        {
            UIObjectPointer newPointer;
            newPointer = GameObject.Instantiate<UIObjectPointer>(_modelSimple, _container);
            if (newPointer.gameObject.activeSelf) newPointer.gameObject.SetActive(false);
            newPointer.SetProperties(pDistance, pAngle, autoRot, worldPos, displayerName, autoDestroy);
            newPointer.gameObject.SetActive(true);
            _arrows.Add(newPointer);
            return newPointer as T;
        }
    }

    public T UseArrow<T>(float pDistance, float pAngle, bool autoRot, Transform uiTransform, string displayerName, bool autoDestroy = true) where T : UIObjectPointer
    {
        if (_container == null) return null;
        if (_modelSimple == null) return null;
        else
        {
            UIObjectPointer newPointer = GameObject.Instantiate<UIObjectPointer>(_modelSimple, _container);
            if (newPointer.gameObject.activeSelf) newPointer.gameObject.SetActive(false);
            newPointer.SetProperties(pDistance, pAngle, autoRot, uiTransform, displayerName, autoDestroy);
            newPointer.gameObject.SetActive(true);
            _arrows.Add(newPointer);
            return newPointer as T;
        }
    }

    public T UseArrow<T>(float pDistance, float pAngle, bool autoRot, Vector3 worldPos, Sprite pSprite, string displayerName, bool autoDestroy = true) where T : UIObjectPointIcon
    {
        if (_container == null) return null;
        if (_modelIcon == null) return null;
        else
        {
            UIObjectPointIcon newPointer = GameObject.Instantiate<UIObjectPointIcon>(_modelIcon, _container);
            if (newPointer.gameObject.activeSelf) newPointer.gameObject.SetActive(false);
            newPointer.SetProperties(pDistance, pAngle, autoRot, worldPos, pSprite, displayerName, autoDestroy);
            newPointer.gameObject.SetActive(true);
            _arrows.Add(newPointer);
            return newPointer as T;
        }
    }

    public T UseArrow<T>(float pDistance, float pAngle, bool autoRot, Transform uiTransform, Sprite pSprite, string displayerName, bool autoDestroy = true) where T : UIObjectPointIcon
    {
        if (_container == null) return null;
        if (_modelIcon == null) return null;
        else
        {
            UIObjectPointIcon newPointer = GameObject.Instantiate<UIObjectPointIcon>(_modelIcon, _container);
            if (newPointer.gameObject.activeSelf) newPointer.gameObject.SetActive(false);
            newPointer.SetProperties(pDistance, pAngle, autoRot, uiTransform, pSprite, displayerName, autoDestroy);
            newPointer.gameObject.SetActive(true);
            _arrows.Add(newPointer);
            return newPointer as T;
        }
    }

    public void SetActiveArrows(bool tState)
    {
        foreach (NamedObject<UIObjectPointer> ps in _arrows.Objs) ps.obj.gameObject.SetActive(tState);
    }

    public void DestroyArrow(UIObjectPointer arrow)
    {
        GameObject.Destroy(arrow.gameObject);
        _arrows.Remove(arrow);
    }

    public void CleanArrows()
    {
        foreach (NamedObject<UIObjectPointer> ps in _arrows.Objs) GameObject.Destroy(ps.obj.gameObject);
        _arrows.Clear();
    }
}