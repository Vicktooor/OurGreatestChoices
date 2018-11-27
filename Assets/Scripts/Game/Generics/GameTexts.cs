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
    public string value;
}

[Serializable]
public class GameTexts : UnityEngine.Object
{
    public TradWrapper Texts;

    private Dictionary<string, string> _locaTexts;
    public Dictionary<string, string> LocaTexts { get { return _locaTexts; } }

    public void Generate()
    {
        _locaTexts = new Dictionary<string, string>();
        foreach (LocalisedText t in Texts.objects)
        {
            if (!_locaTexts.ContainsKey(t.key)) _locaTexts.Add(t.key, t.value);
        }
    }
}

public static class TextManager
{
    private static string keyPrefix = "#";

    private static SystemLanguage _language = SystemLanguage.English;
    public static SystemLanguage LANG { get { return _language; } } 
    private static Dictionary<SystemLanguage, GameTexts> _texts = new Dictionary<SystemLanguage, GameTexts>();
    private static List<SystemLanguage> _loadedLanguage = new List<SystemLanguage>();

    public static void SetLanguage(SystemLanguage lang)
    {
        if (!_loadedLanguage.Contains(lang))
        {
            if (LoadTraduction(lang))
            {
                _language = lang;
                _loadedLanguage.Add(lang);
            }
        }
    }

    public static bool LoadTraduction(SystemLanguage language)
    {
        if (_texts.ContainsKey(language)) return true; 
        _language = language;
        GameTexts newText = new GameTexts();
        newText.Texts = StreamingAssetAccessor.FromJson<TradWrapper>("Json/" + language + ".json");
        if (newText.Texts != null)
        {
            newText.Generate();
            _texts.Add(language, newText);
            return true;
        }
        else return false;
    }

    public static string GetText(string key)
    {
        string gKey = keyPrefix + key;
        if (!_texts.ContainsKey(_language))
        {
            Debug.LogError(_language + ".json not found");
            return null;
        }
        else if (!_texts[_language].LocaTexts.ContainsKey(gKey))
        {
           return _language + "["+ keyPrefix + key + "] not found";
        }
        return _texts[_language].LocaTexts[gKey];
    }
}
