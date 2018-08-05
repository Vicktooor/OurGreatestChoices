using UnityEngine;

public class BillboardElement : MonoBehaviour
{
    protected Vector3 _baseScale;

    protected void Awake()
    {
        _baseScale = transform.localScale;
    }

    protected void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}