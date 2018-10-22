using UnityEngine;

public class BillboardElement : MonoBehaviour
{
    protected Vector3 _baseScale;

    protected virtual void Awake()
    {
        _baseScale = transform.localScale;
    }

    protected virtual void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}