using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PinButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Pin p = GetComponentInChildren<Pin>(true);
        if (p != null) p.gameObject.SetActive(false);
    }
}
