using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class StreamingAssetAccessor
{
    private static string _computerPath = Application.dataPath + "/StreamingAssets/";
    public static string ComputerPath { get { return _computerPath; } }

    private static string _androidPath = "jar:file://" + Application.dataPath + "!/assets/";
    public static string AndroidPath { get { return _androidPath; } }

    private static string _iosPath = Application.dataPath + "/Raw/";
    public static string IOSPath { get { return _iosPath; } }

    private static RuntimePlatform _platform = RuntimePlatform.WindowsPlayer;
    public static RuntimePlatform Platform { set { _platform = value; } }

    public static string GetStreamingAssetPath()
    {
        return GetStreamingAssetPath(_platform);
    }

    public static string GetStreamingAssetPath(RuntimePlatform platform)
    {
        string path = string.Empty;
        switch (platform)
        {
            case RuntimePlatform.Android:
                path = _androidPath;
                break;
            case RuntimePlatform.IPhonePlayer:
                path = _iosPath;
                break;
            default:
                path = _computerPath;
                break;
        }
        return path;
    }

    public static T FromJson<T>(string filePath) where T : class
    {
        return FromJson<T>(filePath, _platform);
    }

    public static T FromJson<T>(string filePath, RuntimePlatform platform) where T : class
    {
        string path = GetStreamingAssetPath(platform) + filePath;
        Debug.Log(string.Format("From json strPath[{0}]", path));
        if (File.Exists(path))
        {
            if (platform != RuntimePlatform.Android)
            {
                StreamReader reader = File.OpenText(path);
                T json = JsonUtility.FromJson<T>(reader.ReadToEnd());
                reader.Close();
                return json;
            }
            else return null;
        }
        else if (platform == RuntimePlatform.Android)
        {
            string jsonTxt = ReadAndroidText(path);
            return JsonUtility.FromJson<T>(jsonTxt);
        }
        else return null;
    }

    public static object Deserialize(string pathFile)
    {
        return Deserialize(pathFile, _platform);
    }

    public static object Deserialize(string pathFile, RuntimePlatform platform)
    {
        string path = GetStreamingAssetPath(platform) + pathFile;
        string persistentPath = PersistenDataManager.GetPersistentPath(pathFile);
        Debug.Log(string.Format("Deserialize strPath[{0}] psrtPath[{1}]", path, persistentPath));
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(path))
        {
            if (platform != RuntimePlatform.Android)
            {
                FileStream reader = File.OpenRead(path);
                object obj = bf.Deserialize(reader);
                reader.Close();
                return obj;
            }
            else return null;
        }
        else if (platform == RuntimePlatform.Android)
        {
            byte[] file = ReadAndroidBytes(path);
            File.WriteAllBytes(persistentPath, file);
            StreamReader wrp = new StreamReader(persistentPath);
            object obj = bf.Deserialize(wrp.BaseStream);
            wrp.Close();
            return obj;
        }
        else return null;
    }

    public static byte[] ReadAndroidBytes(string filePath)
    {
        WWW wwwfile = new WWW(filePath);
        while (!wwwfile.isDone)
        {
            if (!string.IsNullOrEmpty(wwwfile.error))
            {
                Debug.LogError(wwwfile.error);
                return null;
            }
        }
        return wwwfile.bytes;
    }

    public static string ReadAndroidText(string filePath)
    {
        WWW wwwfile = new WWW(filePath);
        while (!wwwfile.isDone)
        {
            if (!string.IsNullOrEmpty(wwwfile.error))
            {
                Debug.LogError(wwwfile.error);
                return null;
            }
        }
        return wwwfile.text;
    }
}

public static class PersistenDataManager
{
    public static void SaveAsJson<T>(T targetClass, string filePath)
    {
        string persistentPath = GetPersistentPath(filePath);
        string newJson = JsonUtility.ToJson(targetClass);
        if (File.Exists(persistentPath)) FileManager.WriteText(persistentPath, newJson);
        else FileManager.CreateText(persistentPath, newJson);
    }

    public static T FromJson<T>(string filePath) where T : class
    {
        string persistentPath = GetPersistentPath(filePath);
        Debug.Log(string.Format("From json prstPath[{0}]", persistentPath));
        if (File.Exists(persistentPath))
        {
            StreamReader reader = File.OpenText(persistentPath);
            T json = JsonUtility.FromJson<T>(reader.ReadToEnd());
            reader.Close();
            return json;
        }
        else return null;
    }

    public static void Serialize(object obj, string filePath)
    {
        string persistentPath = GetPersistentPath(filePath);
        if (File.Exists(persistentPath)) FileManager.WriteFile(persistentPath, obj);
        else FileManager.CreateFile(persistentPath, obj);
    }

    public static object Deserialize(string filePath)
    {
        string persistentPath = GetPersistentPath(filePath);
        Debug.Log(string.Format("Deserialize prstPath[{0}]", persistentPath));
        if (File.Exists(persistentPath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            StreamReader wrp = new StreamReader(persistentPath);
            object obj = bf.Deserialize(wrp.BaseStream);
            wrp.Close();
            return obj;
        }
        else return null;
    }

    public static string GetPersistentPath(string filePath)
    {
        string[] cutPath = filePath.Split(new char[1] { '/' });
        return string.Format("{0}/{1}", Application.persistentDataPath, cutPath[cutPath.Length - 1]);
    }
}

[Serializable]
public class PairWrapper<T> where T : class
{
    public string key;
    public T value;
}

[Serializable]
public class PairWrapperID<T> where T : class
{
    public int key;
    public T value;
}

[Serializable]
public class DicWrapper<T> where T : class
{
    public List<PairWrapper<T>> objects = new List<PairWrapper<T>>();
}

[Serializable]
public class DicWrapperID<T> where T : class
{
    public List<PairWrapperID<T>> objects = new List<PairWrapperID<T>>();
}

public static class FileManager
{
    /// <summary>
    /// Json should be like : 
    /// {
    ///     "objects" : [
    ///      {
    ///         "keyName" : keyString,
    ///         "valueName" : 
    ///         {
    ///             *properties*
    ///         }
    ///      }
    ///     ]
    /// }
    /// Json wrapper should be like :
    /// class JsonWrapper {
    ///     List<JsonObject> objects;
    /// }
    /// class JsonObject {
    ///     string keyName;
    ///     OtherJsonObject valueName;
    /// }
    /// class OtherJsonObject {
    ///     *properties*
    /// }
    /// </summary>
    /// <typeparam name="T">key type</typeparam>
    /// <typeparam name="T2">value type</typeparam>
    /// <returns></returns>
    public static Dictionary<string, T> GenerateDicFromJson<T>(DicWrapper<T> wrapper) where T : class
    {
        Dictionary<string, T> dic = new Dictionary<string, T>();
        foreach (PairWrapper<T> obj in wrapper.objects)
        {
            if (!dic.ContainsKey(obj.key)) dic.Add(obj.key, obj.value);
        }
        return dic;
    }

    public static Dictionary<int, T> GenerateDicFromJson<T>(DicWrapperID<T> wrapper) where T : class
    {
        Dictionary<int, T> dic = new Dictionary<int, T>();
        foreach (PairWrapperID<T> obj in wrapper.objects)
        {
            if (!dic.ContainsKey(obj.key)) dic.Add(obj.key, obj.value);
        }
        return dic;
    }

    public static Dictionary<T, T2> GenerateDicFromJson<T, T2>(DicWrapper<T2> wrapper) where T : struct, IConvertible where T2 : class 
    {
        Dictionary<T, T2> dic = new Dictionary<T, T2>();
        foreach (PairWrapper<T2> obj in wrapper.objects)
        {
            T type = PropertyUtils.CastEnum<T>(obj.key);
            if (!dic.ContainsKey(type)) dic.Add(type, obj.value);
        }
        return dic;
    }

    public static List<KeyValuePair<string, T>> GenerateList<T, T2>(List<T2> baseList) where T : class where T2 : class
    {
        int l = baseList.Count;
        if (l > 0)
        {
            if (PropertyUtils.HasProperty<T2>("key") && PropertyUtils.HasProperty<T2>("value"))
            {
                List<KeyValuePair<string, T>> list = new List<KeyValuePair<string, T>>();
                for (int i = 0; i < l; i++)
                {
                    T2 obj = baseList[i];
                    string key = (string)PropertyUtils.GetPropertyValue(obj, "key");
                    T value = (T)PropertyUtils.GetPropertyValue(obj, "value");
                    KeyValuePair<string, T> pair = new KeyValuePair<string, T>(key, value);
                    list.Add(pair);
                }
                return list;
            }
            else
            {
                Debug.LogError("property \"key\" or \"value\" doesn't exist in " + typeof(T2));
                return null;
            }
        }
        else return null;
    }

    public static List<KeyValuePair<int, T>> GenerateIDList<T, T2>(List<T2> baseList) where T : class where T2 : class
    {
        int l = baseList.Count;
        if (l > 0)
        {
            if (PropertyUtils.HasProperty<T2>("key") && PropertyUtils.HasProperty<T2>("value"))
            {
                List<KeyValuePair<int, T>> list = new List<KeyValuePair<int, T>>();
                for (int i = 0; i < l; i++)
                {
                    T2 obj = baseList[i];
                    int key = (int)PropertyUtils.GetPropertyValue(obj, "key");
                    T value = (T)PropertyUtils.GetPropertyValue(obj, "value");
                    KeyValuePair<int, T> pair = new KeyValuePair<int, T>(key, value);
                    list.Add(pair);
                }
                return list;
            }
            else
            {
                Debug.LogError("property \"key\" or \"value\" doesn't exist in " + typeof(T2));
                return null;
            }
        }
        else return null;
    }

    public static DicWrapper<T> GenerateDicWrapper<T>(List<KeyValuePair<string, T>> values) where T : class
    {
        DicWrapper<T> newDic = new DicWrapper<T>();
        foreach (KeyValuePair<string, T> e in values)
        {
            PairWrapper<T> wrap = new PairWrapper<T>();
            wrap.key = e.Key;
            wrap.value = e.Value;
            newDic.objects.Add(wrap);
        }
        return newDic;
    }

    public static DicWrapperID<T> GenerateDicWrapperID<T>(List<KeyValuePair<int, T>> values) where T : class
    {
        DicWrapperID<T> newDic = new DicWrapperID<T>();
        foreach (KeyValuePair<int, T> e in values)
        {
            PairWrapperID<T> wrap = new PairWrapperID<T>();
            wrap.key = e.Key;
            wrap.value = e.Value;
            newDic.objects.Add(wrap);
        }
        return newDic;
    }

    public static void CreateText(string path, string text)
    {
        Debug.Log(string.Format("Create text Path[{0}]", path));
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs)) writer.Write(text);
        }
    }

    public static void WriteText(string path, string text)
    {
        File.Delete(path);
        CreateText(path, text);
    }

    public static void CreateFile(string path, object obj)
    {
        Debug.Log(string.Format("Create file Path[{0}]", path));
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(path);
        bf.Serialize(file, obj);
        file.Close();
    }

    public static void WriteFile(string path, object obj)
    {
        File.Delete(path);
        CreateFile(path, obj);
    }
}
