using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour {

    public SystemLanguage currentLangage {
        get { return _currentLangage; }
    }
    private SystemLanguage _currentLangage;

    #region Singleton
    protected static LocalizationManager _instance;
    public static LocalizationManager Instance {
        get { return _instance; }
    }

    protected void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
            throw new Exception("An instance of LocalizationManager already exists.");
        }
        else _instance = this;
    }
    #endregion

    // Use this for initialization
    void Start () {
        _currentLangage = SystemLanguage.English;
	}

    public void OnChangedLangageFr() {
        _currentLangage = SystemLanguage.French;
        TextManager.LoadTraduction(_currentLangage);
        GameManager.SetFR();
        Events.Instance.Raise(new OnChangeLanguageUI());
    }

    public void OnChangedLangageEn() {
        _currentLangage = SystemLanguage.English;
        TextManager.LoadTraduction(_currentLangage);
        GameManager.SetEN();
        Events.Instance.Raise(new OnChangeLanguageUI());
    }
}
