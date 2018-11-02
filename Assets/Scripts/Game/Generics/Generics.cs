using System;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static void ToList<T>(this T[] originalArray, out List<T> outList)
    {
        outList = new List<T>();
        int l = originalArray.Length;
        for (int i = 0; i < l; i++)
        {
            outList.Add(originalArray[i]);
        }
    }

    public static void Fill<T>(this T[] originalArray, T with)
    {
        int l = originalArray.Length;
        for (int i = 0; i < l; i++)
        {
            originalArray[i] = with;
        }
    }

    public static void Fill<T>(this T[,] originalArray, T with)
    {
        int l = originalArray.Length;
        for (int i = 0; i < l; i++)
        {
            for (int j = 0; j < l; j++)
            {
                originalArray[i, j] = with;
            }
        }
    }
}

public static class PropertyUtils
{
    public static T CastEnum<T>(string value) where T : struct, IConvertible
    {
        T enumCast = new T();
        try
        {
            enumCast = (T)Enum.Parse(typeof(T), value);
            if (Enum.IsDefined(typeof(T), value)) return enumCast;
            else return enumCast;
        }
        catch (ArgumentException)
        {
            Console.WriteLine(string.Format("{0} is not existing in enum {1}", value, typeof(T)));
            return enumCast;
        }
    }

    public static object GetPropertyValue(object src, string propName)
    {
        return src.GetType().GetField(propName).GetValue(src);
    }

    public static bool HasProperty<T>(string propertyName)
    {
        return typeof(T).GetField(propertyName) != null;
    }
}

public class ObjectArray<T> where T : UnityEngine.Object
{
    private Dictionary<string, T> _namedObjs = new Dictionary<string, T>();
    public Dictionary<string, T> NamedObjs { get { return _namedObjs; } }

    protected int _length = 0;
    public int Length { get { return _length; } }

    public void Add(string oName, T obj)
    {
        if (!Contains(oName))
        {
            _namedObjs.Add(oName, obj);
            _length++;
        }
    }

    // Remove by object
    public void Remove(T pObj)
    {
        if (Contains(pObj))
        {
            foreach (KeyValuePair<string, T> obj in _namedObjs)
            {
                if (obj.Key.Equals(pObj))
                {
                    _namedObjs.Remove(obj.Key);
                    _length--;
                    return;
                }
            }
        }
    }

    // Remove by name
    public T Remove(string objName)
    {
        if (Contains(objName))
        {
            T rObj = _namedObjs[objName];
            _namedObjs.Remove(objName);
            _length--;
            return rObj;
        }
        else return null;
    }

    // Contain ? by obj
    public bool Contains(T pObj)
    {
        return _namedObjs.ContainsValue(pObj);
    }

    // Contain ? by name
    public bool Contains(string objName)
    {
        return _namedObjs.ContainsKey(objName);
    }

    // Get obj
    public T Get(string key)
    {
        if (Contains(key)) return _namedObjs[key];
        else return null;
    }

    /// <summary>
    /// Find obj by testing a property equality
    /// </summary>
    /// <param name="propertyName">Property tested</param>
    /// <param name="value">Tested value for equality</param>
    /// <returns></returns>
    public List<T> Find(string propertyName, object value)
    {
        List<T> foundList = new List<T>();
        foreach (KeyValuePair<string, T> item in _namedObjs)
        {
            if (PropertyUtils.HasProperty<T>(propertyName))
            {
                if (value.Equals(PropertyUtils.GetPropertyValue(item.Value, propertyName))) foundList.Add(item.Value);
            }
        }
        return foundList;
    }

    public void CheckIt()
    {
        List<KeyValuePair<string, T>> newArray = new List<KeyValuePair<string, T>>();
        foreach (KeyValuePair<string, T> value in _namedObjs) newArray.Add(value);

        KeyValuePair<string, T> swapObj;
        int iteCheck = Mathf.RoundToInt(_length / 2f);       
        for (int i = 0; i < iteCheck; i++)
        {
            int rIndex1 = UnityEngine.Random.Range(0, _length);
            int rIndex2 = UnityEngine.Random.Range(0, _length);

            if (rIndex1 == rIndex2) i--;
            else
            {
                swapObj = newArray[rIndex1];
                newArray[rIndex1] = newArray[rIndex2];
                newArray[rIndex2] = swapObj;
            }
        }
        Clear();
        _length = newArray.Count;
        for (int i = 0; i < _length; i++) Add(newArray[i].Key, newArray[i].Value);
    }

    public void Clear()
    {
        _namedObjs.Clear();
        _length = 0;
    }

    public static List<T> CheckIt(List<T> list)
    {
        T swapObj;
        int iteCheck = Mathf.RoundToInt(list.Count / 2f);
        int lLength = list.Count;
        for (int i = 0; i < iteCheck; i++)
        {
            int rIndex1 = UnityEngine.Random.Range(0, lLength);
            int rIndex2 = UnityEngine.Random.Range(0, lLength);
            if (rIndex1 == rIndex2) i--;
            else
            {
                swapObj = list[rIndex1];
                list[rIndex1] = list[rIndex2];
                list[rIndex2] = swapObj;
            }
        }
        return list;
    }
}