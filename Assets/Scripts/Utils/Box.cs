using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Box : MonoBehaviour, IDropHandler {
    
    public UnityEvent onDropEvent;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void OnDrop(PointerEventData eventData) {
        onDropEvent.Invoke();
    }
}
