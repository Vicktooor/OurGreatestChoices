using System;
using System.Collections.Generic;
using UnityEngine;

public class MainLoader<T> : Singleton<MainLoader<T>> where T : UnityEngine.Object
{
    private Dictionary<Type, ObjectArray<T>> _resources = new Dictionary<Type, ObjectArray<T>>();

    public T Load(string folderPath, string fileName)
    {
        Type lType = typeof(T);
        if (ResourcesContainArrayOf(lType))
        {
            T lAsset = _resources[lType].Get(fileName);
            if (lAsset != null) return lAsset;
            else
            {
                T lResource = Resources.Load<T>(folderPath + fileName);
                _resources[lType].Add(fileName, lResource);
                return lResource;
            }
        }
        else
        {
            _resources.Add(lType, new ObjectArray<T>());
            T lResource = Resources.Load<T>(folderPath + fileName);
            _resources[lType].Add(fileName, lResource);
            return lResource;
        }
    }

    public void LoadAsync(string folderPath, string fileName)
    {
        Type lType = typeof(T);
        AsyncMainLoader.Instance.LoadAsync<T>(folderPath, fileName);
    }

    public void AddAsyncLoadedResource(T loadedAsset, string fileName)
    {
        Type lType = typeof(T);
        if (ResourcesContainArrayOf(lType))
        {
            T lAsset = _resources[lType].Get(fileName);
            if (lAsset != null) return;
            else _resources[lType].Add(fileName, loadedAsset);
        }
        else
        {
            _resources.Add(lType, new ObjectArray<T>());
            _resources[lType].Add(fileName, loadedAsset);
        }
    }

    public T GetResource(string resourceName)
    {
        Type lType = typeof(T);
        if (_resources.ContainsKey(lType))
        {
            return _resources[lType].Get(resourceName);
        }
        else return null;
    }

    public bool ResourcesContainArrayOf(Type lType)
    {
        if (_resources.ContainsKey(lType)) return true;
        else return false;
    }
}