using Assets.Scripts.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class JsonManager
{
    private static string _computerPath = Application.dataPath + "/StreamingAssets/";
    public static string ComputerPath { get { return _computerPath; } }

    private static string _androidPath = Application.dataPath + "/Raw/";
    public static string AndroidPath { get { return _androidPath; } }

    private static string _iosPath = "jar:file://" + Application.dataPath + "!/assets/";
    public static string IOSPath { get { return _iosPath; } }

    public static T FromJson<T>(string filePath, RuntimePlatform platform) where T : class
    {
        string path = string.Empty;
        switch (platform)
        {
            case RuntimePlatform.Android:
                path = _androidPath + filePath;
                break;
            case RuntimePlatform.IPhonePlayer:
                path = _iosPath + filePath;
                break;
            default:
                path = _computerPath + filePath;
                break;
        }

        if (File.Exists(path))
        {
            StreamReader reader = File.OpenText(path);
            return JsonUtility.FromJson<T>(reader.ReadToEnd());
        }
        else return null;
    }

    public static void SaveRoad(RoadInfo road)
    {
        string newJson = JsonUtility.ToJson(road);
        using (FileStream fs = new FileStream(_computerPath + "Json/Roads/" + road.name + ".json", FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(newJson);
            }
        }
    }
}
