using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoSingleton<LocalizationManager>
{
    public SystemLanguage currentLangage {
        get { return _currentLangage; }
    }
    private SystemLanguage _currentLangage = SystemLanguage.English;

    public void OnChangedLangageFr() {
        _currentLangage = SystemLanguage.French;
        TextManager.SetLanguage(_currentLangage);
        Events.Instance.Raise(new OnChangeLanguageUI());
    }

    public void OnChangedLangageEn() {
        _currentLangage = SystemLanguage.English;
        TextManager.SetLanguage(_currentLangage);
        Events.Instance.Raise(new OnChangeLanguageUI());
    }
}
