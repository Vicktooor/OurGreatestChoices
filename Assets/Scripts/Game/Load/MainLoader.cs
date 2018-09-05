using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.Load
{
    public class MainLoader<O> : Singleton<MainLoader<O>> where O : UnityEngine.Object
    {
        private Dictionary<Type, ObjectArray<O>> _resources = new Dictionary<Type, ObjectArray<O>>();

        public O Load(string folderPath, string fileName)
        {
            Type lType = typeof(O);
            if (ResourcesContainArrayOf(lType))
            {
                O lAsset = _resources[lType].FindByName(fileName);
                if (lAsset != null) return lAsset;
                else
                {
                    O lResource = Resources.Load<O>(folderPath + fileName);
                    _resources[lType].Add(lResource, fileName);
                    return lResource;
                }
            }
            else
            {
                _resources.Add(lType, new ObjectArray<O>());
                O lResource = Resources.Load<O>(folderPath + fileName);
                _resources[lType].Add(lResource, fileName);
                return lResource;
            }
        }

        public void LoadAsync(string folderPath, string fileName)
        {
            Type lType = typeof(O);
            AsyncMainLoader.Instance.LoadAsync<O>(folderPath, fileName);
        }

        public void AddAsyncLoadedResource(O loadedAsset, string fileName)
        {
            Type lType = typeof(O);
            if (ResourcesContainArrayOf(lType))
            {
                O lAsset = _resources[lType].FindByName(fileName);
                if (lAsset != null) return;
                else _resources[lType].Add(loadedAsset, fileName);
            }
            else
            {
                _resources.Add(lType, new ObjectArray<O>());
                _resources[lType].Add(loadedAsset, fileName);
            }
        }

        public O GetResource(string resourceName)
        {
            Type lType = typeof(O);
            if (_resources.ContainsKey(lType))
            {
                return _resources[lType].FindByName(resourceName);
            }
            else return null;
        }

        public bool ResourcesContainArrayOf(Type lType)
        {
            if (_resources.ContainsKey(lType)) return true;
            else return false;
        }
    }
}