using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class UIOverlay : MonoBehaviour
{
    public Vector2 pivot;
    public Transform targetTransform;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Set(Vector2 newPivot, Transform newTransform)
    {
        pivot = newPivot;
        targetTransform = newTransform;
    }

    private void Update()
    {
        if (targetTransform != null)
        {
            Vector3 newPos = Camera.main.WorldToScreenPoint(targetTransform.position);
            newPos += (Vector3)pivot;
            _rectTransform.position = newPos;
        }
    }
}
