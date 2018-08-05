using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour {

    Transform _transform;
    StudioEventEmitter _emitter;

    // Use this for initialization
    void Start () {
        _transform = transform;
        _emitter = _transform.GetComponent<StudioEventEmitter>();

        Events.Instance.AddListener<OnSceneLoaded>(StopEmitter);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void StopEmitter(OnSceneLoaded pEvent) {
        if (GameManager.Instance.LoadedScene == SceneString.MapView) _emitter.Stop();
        else _emitter.Play();
    }
}