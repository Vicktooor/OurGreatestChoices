using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour {

    public EnumClass.Language currentLangage {
        get { return _currentLangage; }
    }
    private EnumClass.Language _currentLangage;

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
        _currentLangage = EnumClass.Language.en;
	}

    public void OnChangedLangageFr() {
        _currentLangage = EnumClass.Language.fr;
        GameManager.SetFR();
        Events.Instance.Raise(new OnChangeLanguageUI());
    }

    public void OnChangedLangageEn() {
        _currentLangage = EnumClass.Language.en;
        GameManager.SetEN();
        Events.Instance.Raise(new OnChangeLanguageUI());
    }
}
