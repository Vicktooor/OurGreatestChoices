using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIElementDragger : MonoBehaviour {

    private bool dragging = false;

    private Vector2 originalPosition;
    private Transform objectToDrag;
    private Image objectToDragImage;

    List<RaycastResult> hitObjects = new List<RaycastResult>();

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            objectToDrag = GetDraggableTransformUnderMouse();

            if (objectToDrag != null) {
                dragging = true;

                objectToDrag.SetAsLastSibling();

                originalPosition = objectToDrag.position;
                objectToDragImage = objectToDrag.GetComponent<Image>();
                objectToDragImage.raycastTarget = false;

                objectToDrag.transform.parent.transform.parent.GetComponent<ScrollRect>().enabled = false;
            }
        }

        if (dragging) {
            if (GameManager.Instance.IsOnDesk) objectToDrag.position = Input.mousePosition;
            else objectToDrag.position = Input.GetTouch(0).position;
        }

        if (Input.GetMouseButtonUp(0)) {
            if (objectToDrag != null) {
                var objectToReplace = GetDraggableTransformUnderMouse();

                if (objectToReplace != null) 
                {
                    objectToDrag.position = originalPosition;
                    Events.Instance.Raise(new OnFinishDrag(objectToDrag.gameObject, objectToReplace.gameObject));

                    objectToDrag.transform.parent.transform.parent.GetComponent<ScrollRect>().enabled = true;
                }
                else {
                    objectToDrag.position = originalPosition;
                }

                objectToDragImage.raycastTarget = true;
                objectToDrag = null;
            }

            dragging = false;
        }
    }

    private GameObject GetObjectUnderMouse() {
        var pointer = new PointerEventData(EventSystem.current);

        pointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(pointer, hitObjects);

        if (hitObjects.Count <= 0) return null;

        return hitObjects.First().gameObject;
    }

    private Transform GetDraggableTransformUnderMouse() {
        var clickedObject = GetObjectUnderMouse();
        return null;
    }
}
