using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        /* Debug */
        if (Input.touchCount != 0) {
            transform.position = Input.GetTouch(0).position;
        }
        if (Input.touchCount == 0) {
            transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f);
        }
        /* Debug */
    }
}
