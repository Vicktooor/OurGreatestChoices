using UnityEngine;
using System.Collections;
using System;

public class ParticleDatabase : MonoBehaviour
{
    public GameObject upFeedback;
    public GameObject downFeedback;

    protected static ParticleDatabase _instance;
    public static ParticleDatabase Instance
    {
        get { return _instance; }
    }

    protected void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            throw new Exception("An instance of EarthManager already exists.");
        }
        else _instance = this;
    }

    protected void OnDestroy()
    {
        _instance = null;
    }
}
