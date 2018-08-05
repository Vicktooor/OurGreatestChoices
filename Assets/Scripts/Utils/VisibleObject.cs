using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleObject : MonoBehaviour {
    Transform _transform;
    Vector2 _screenPosition;
    Renderer[] rendererArray;

    public bool visible { get { return _visible; } }
    bool _visible = false;

    // Use this for initialization
    protected virtual void Start () {
        _transform = transform;
        rendererArray = GetComponentsInChildren<Renderer>(true);
    }
	
	// Update is called once per frame
	void Update () {
        CheckPosition();
    }

    void CheckPosition() {
        _screenPosition = Camera.main.WorldToScreenPoint(_transform.position);

        if (_screenPosition.x > 0 && _screenPosition.x < Screen.width
            && _screenPosition.y > 0 && _screenPosition.y < Screen.height) {
            CheckChildrenRenderer();
        }
        else _visible = false;
    }

    void CheckChildrenRenderer() {
        Renderer renderer;
        
        int length = rendererArray.Length;
        int index;

        int counter = 0;

        for (index = 0; index < length; index++) {
            renderer = rendererArray[index];
            if (renderer.enabled) counter++;
        }

        if (counter / length >= 0.5f) _visible = true;
        else _visible = false;
    }
}
