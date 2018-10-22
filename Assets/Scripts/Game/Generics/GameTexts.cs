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

    public void Generate()
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
    private static SystemLanguage _language = SystemLanguage.English;
    public static SystemLanguage LANG { get { return _language; } } 
    private static Dictionary<SystemLanguage, GameTexts> _texts = new Dictionary<SystemLanguage, GameTexts>();

    public static void LoadTraduction(SystemLanguage language)
    {
        if (_texts.ContainsKey(language)) return; 
        _language = language;
        GameTexts newText = new GameTexts();
        newText.Texts = JsonManager.FromJson<TradWrapper>("Json/" + language + ".json", RuntimePlatform.WindowsPlayer);
        if (newText.Texts != null)
        {
            newText.Generate();
            if (_texts.ContainsKey(language)) _texts[language] = newText;
            else _texts.Add(language, newText);
        }
    }

    public static string GetText(string key)
    {
        if (!_texts.ContainsKey(_language))
        {
            Debug.LogError(_language + ".json not found");
            return null;
        }
        else if (!_texts[_language].LocaTexts.ContainsKey(key))
        {
            Debug.LogError(_language + "[" + key + "] not found");
            return null;
        }
        return _texts[_language].LocaTexts[key];
    }
}
