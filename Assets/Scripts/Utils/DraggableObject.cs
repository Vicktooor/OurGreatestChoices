using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DraggableObject : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler {

    public UnityEvent onBeginDragEvent;
    public UnityEvent onDragEvent;
    public UnityEvent onEndDragEvent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnBeginDrag(PointerEventData eventData) {
        onBeginDragEvent.Invoke();
    }

    public void OnDrag(PointerEventData eventData) {
        onDragEvent.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData) {
        onEndDragEvent.Invoke();
    }
}