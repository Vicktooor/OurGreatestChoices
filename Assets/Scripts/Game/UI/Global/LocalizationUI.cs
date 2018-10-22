using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationUI : MonoBehaviour {

    //Text _text;
    TextMeshProUGUI _text;
    [SerializeField]
    string frText;

    [SerializeField]
    string enText;

    void Start () {
        _text = GetComponent<TextMeshProUGUI>();
        if (frText == null) print(gameObject.name + "doesn't have a french text!");
        if (enText == null) print(gameObject.name + "doesn't have an english text!");

        Events.Instance.AddListener<OnChangeLanguageUI>(UpdateLanguage);
        UpdateLanguage(new OnChangeLanguageUI());
    }
	
	void UpdateLanguage(OnChangeLanguageUI e) {
        switch (LocalizationManager.Instance.currentLangage) {
            case SystemLanguage.English:
                _text.SetText(enText);
                break;
            case SystemLanguage.French:
                _text.SetText(frText);
                break;
        }
    }
}
