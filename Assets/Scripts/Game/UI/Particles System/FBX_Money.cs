using System;
using UnityEngine;

public class FBX_Money : MonoBehaviour {

    #region Singleton
    private static FBX_Money _instance;
    public static FBX_Money instance {
        get {
            return _instance;
        }
    }

    private void InitInstance() {
        if (_instance != null && _instance != this) {
            Destroy(this);
            throw new Exception("An instance of MoneyFBX already exists.");
        }
        else _instance = this;
    }
    #endregion

    // Use this for initialization
    void Start() {
        _instance = this;
    }

    public void Play(Vector3 pPosition) {
        gameObject.transform.position = pPosition;
        GetComponent<UIParticleSystem>().Play();
    }
}

