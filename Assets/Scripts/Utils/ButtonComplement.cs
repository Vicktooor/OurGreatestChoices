using Assets.Scripts.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class ButtonEvent : UnityEvent<Button> {

}

namespace UnityEngine.UI {

    public class ButtonComplement : MonoBehaviour, IPointerDownHandler {

        Transform _transform;
        Button _button;

        public ButtonEvent onPressDown;

        void Awake() {

        }

        // Use this for initialization
        void Start() {
            _transform = transform;
            _button = _transform.GetComponent<Button>();
        }

        // Update is called once per frame
        void Update() {

        }

        /* Function which Invoke an Event for UIManager */
        public void OnPointerDown(PointerEventData eventData) {
            if(_button.interactable) onPressDown.Invoke(_button);
        }
    }
}