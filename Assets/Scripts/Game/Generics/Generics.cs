using System;
using UnityEngine;

public struct DataStruct<V> where V : UnityEngine.Object
{
    public string name;
    public V obj;

    public DataStruct(V pObj, string pName = "")
    {
        name = pName;
        obj = pObj;
    }
}

public class ObjectArray<T> where T : UnityEngine.Object
{
    public Type ObjectType { get { return typeof(T); } }

    private DataStruct<T>[] _array = new DataStruct<T>[0];
    public DataStruct<T>[] Objs { get { return _array; } }

    protected int _length;
    public int Length { get { return _length; } }

    public void Add(T pObj, string pName = "")
    {
        if (pObj == null) return;

        DataStruct<T>[] newArray;
        if (_array.Length == 0) newArray = new DataStruct<T>[1] { new DataStruct<T>(pObj, pName) };
        else
        {
            newArray = new DataStruct<T>[_array.Length + 1];
            for (int i = 0; i < _array.Length; i++)
            {
                newArray[i] = _array[i];
            }
            newArray[newArray.Length - 1] = new DataStruct<T>(pObj, pName);
        }
        _array = newArray;
        _length = newArray.Length;
    }

    // Remove by object
    public T Remove(T pObj)
    {
        T foundObj = null;
        if (pObj == null) return foundObj;
        if (_array.Length == 0) return foundObj;
        DataStruct<T>[] newArray = new DataStruct<T>[_array.Length - 1];
        int index = -1;
        for (int i = 0; i < _array.Length; i++)
        {
            if (pObj.Equals(_array[i].obj))
            {
                foundObj = pObj;
                index = i;
                break;
            }
        }

        if (index < 0) return foundObj;

        bool insert = false;
        for (int i = 0; i < newArray.Length; i++)
        {
            if (i == index) insert = true;

            if (!insert) newArray[i] = _array[i];
            else newArray[i] = _array[i + 1];
        }

        _array = newArray;
        _length = newArray.Length;
        return foundObj;
    }

    // Remove by name
    public T Remove(string objName)
    {
        T foundObj = null;
        if (_array.Length == 0) return foundObj;
        DataStruct<T>[] newArray = new DataStruct<T>[_array.Length - 1];
        int index = -1;
        for (int i = 0; i < _array.Length; i++)
        {
            DataStruct<T> fStruct = _array[i];
            if (objName.Equals(fStruct.name))
            {
                foundObj = fStruct.obj;
                index = i;
                break;
            }
        }

        if (index < 0) return foundObj;

        bool insert = false;
        for (int i = 0; i < newArray.Length; i++)
        {
            if (i == index) insert = true;

            if (!insert) newArray[i] = _array[i];
            else newArray[i] = _array[i + 1];
        }

        _array = newArray;
        _length = newArray.Length;
        return foundObj;
    }

    public void CheckIt()
    {
        int iteCheck = Mathf.RoundToInt(_array.Length / 2f);
        int[] newArray = new int[_array.Length];
        for (int i = 0; i < iteCheck; i++)
        {
            DataStruct<T> SwapObj;
            int rIndex1 = UnityEngine.Random.Range(0, _array.Length);
            int rIndex2 = UnityEngine.Random.Range(0, _array.Length);

            if (rIndex1 == rIndex2) i--;
            else
            {
                SwapObj = _array[rIndex1];
                _array[rIndex1] = _array[rIndex2];
                _array[rIndex2] = SwapObj;
            }
        }
    }

    public void CleanDoublon()
    {
        ObjectArray<T> tArray = new ObjectArray<T>();

        for (int i = 0; i < _array.Length; i++)
        {
            DataStruct<T> lObj = _array[i];
            if (!tArray.Contains(lObj.obj)) tArray.Add(lObj.obj, lObj.name);
        }

        _array = tArray.Objs;
        _length = tArray.Length;
    }

    // Contain ? by obj
    public bool Contains(T pObj)
    {
        if (pObj == null)
        {
            Debug.LogError("ObjectArray.Contains(T pObj) -> pObj is null, can't find it");
            return false;
        }
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i].obj.Equals(pObj)) return true;
        }
        return false;
    }

    // Contain ? by name
    public bool Contains(string objName)
    {
        if (objName == string.Empty)
        {
            Debug.LogError("ObjectArray.Contains(string objName) -> objName is empty, can't find it");
            return false;
        }
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i].name.Equals(objName)) return true;
        }
        return false;
    }

    // Find object by name
    public T FindByName(string objName)
    {
        if (objName == string.Empty)
        {
            Debug.LogError("ObjectArray.FindByName(stringoObjName) -> objName is empty, can't find it");
            return null;
        }
        for (int i = 0; i < _array.Length; i++)
        {
            DataStruct<T> lData = _array[i];
            if (lData.name.Equals(objName)) return lData.obj;
        }
        return null;
    }

    public void Clear()
    {
        _array = new DataStruct<T>[0];
        _length = 0;
    }
}