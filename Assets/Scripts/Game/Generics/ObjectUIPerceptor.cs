using UnityEngine;
using System.Collections;
using System;

public class ObjectUIPerceptor : MonoBehaviour
{
    private RectTransform _rectTransform;
    [SerializeField]
    private Canvas _canvasContainer;

    public float speed;
    public float heightMultiplicator;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private IEnumerator Collect(Transform targetTransform, Action callback)
    {
        Collider col = targetTransform.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Vector3 worldPosition = _canvasContainer.transform.TransformPoint(_rectTransform.localPosition);
        Vector3 startPos = targetTransform.position;
        Vector3 up = _rectTransform.transform.up;
        Vector3 baseScale = targetTransform.localScale;

        Vector3 pos;
        Vector3 yPos;
        float t = 0f;
        float size = 1f;
        while (t < 1f)
        {
            worldPosition = _canvasContainer.transform.TransformPoint(_rectTransform.localPosition);
            t = Mathf.Clamp01(t + (Time.deltaTime * speed));
            size = Easing.SetOutRange(Easing.Flip(t), 0f, baseScale.x);
            targetTransform.localScale = new Vector3(size, size, size);
            pos = Vector3.Lerp(startPos, worldPosition, t);
            yPos = up * Easing.Arch(t) * 2f;
            targetTransform.position = pos + (yPos * heightMultiplicator);
            yield return null;
        }
        callback();
    }

    public void AttractObject(Transform targetTransform, Action callback)
    {
        StartCoroutine(Collect(targetTransform, callback));
    }
}
