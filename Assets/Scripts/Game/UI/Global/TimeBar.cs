using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.Game;

public class TimeBar : MonoBehaviour
{

    Image fillImage;

    public void Awake()
    {
        fillImage = GetComponentInChildren<Image>();
    }
    protected void Update()
    {
        if (TimeManager.instance) fillImage.fillAmount = TimeManager.instance.NormalizeYearTime;
    }
}
