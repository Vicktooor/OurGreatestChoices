using UnityEngine;
using System.Collections;

public class Cloud : MonoBehaviour
{
    private Vector3 startPosition;
    public Transform targetPos;

    public void Awake()
    {
        startPosition = transform.localPosition;
    }

    public void Move(float t)
    {
        transform.localPosition = Vector3.Lerp(startPosition, targetPos.localPosition, t);
    }
}
