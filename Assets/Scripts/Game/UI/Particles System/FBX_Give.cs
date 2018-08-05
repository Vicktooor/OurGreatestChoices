using System;
using UnityEngine;

public class FBX_Give : MonoBehaviour {

    #region Singleton
    private static FBX_Give _instance;
    public static FBX_Give instance {
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

    private void Awake()
    {
        InitInstance();
    }

    public void Play(Vector3 pPosition) {
        gameObject.transform.position = pPosition;
        GetComponent<UIParticleSystem>().Play();
    }
}
