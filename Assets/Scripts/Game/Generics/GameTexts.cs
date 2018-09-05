using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TradWrapper
{
    public List<LocalisedText> objects;
}

[Serializable]
public class LocalisedText
{
    public string key;
    public string txt;
}

[Serializable]
public class GameTexts : UnityEngine.Object
{
    public TradWrapper Texts;

    private Dictionary<string, string> _locaTexts;
    public Dictionary<string, string> LocaTexts { get { return _locaTexts; } }

    public void FinishLoad()
    {
        _locaTexts = new Dictionary<string, string>();
        foreach (LocalisedText t in Texts.objects)
        {
            if (!_locaTexts.ContainsKey(t.key)) _locaTexts.Add(t.key, t.txt);
        }
    }
}

public static class TextManager
{
    public static GameTexts FR = new GameTexts();
    public static GameTexts EN = new GameTexts();

    public static void LoadTexts()
    {
        FR.Texts = JsonManager.FromJson<TradWrapper>("Json/EN.json", RuntimePlatform.WindowsPlayer);
        FR.FinishLoad();
        Debug.Log(FR.LocaTexts["__MajorName"]);
    }
}
