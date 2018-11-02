using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchPanel : MonoBehaviour
{
    public static float TIME_TRANSITION = 1.25f;
    private List<Cloud> _clouds = new List<Cloud>();

    public void Awake()
    {
        Cloud[] clouds = GetComponentsInChildren<Cloud>();
        ArrayExtensions.ToList(clouds, out _clouds);
    }

    public void Move(float t)
    {
        foreach (Cloud c in _clouds)
        {
            c.Move(t);
        }
    }
}
