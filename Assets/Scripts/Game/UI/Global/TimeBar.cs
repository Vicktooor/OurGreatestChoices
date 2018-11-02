using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.Game;
using TMPro;

public class TimeBar : MonoBehaviour
{
    private TextMeshProUGUI text;

    public void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        Events.Instance.AddListener<OnNewMonth>(NewMonth);
        text.text = TextManager.GetText("1month") + " " + TimeManager.INITIAL_YEAR;
    }

    protected void NewMonth(OnNewMonth e)
    {
        if (TimeManager.Instance)
        {
            string month = TextManager.GetText(((TimeManager.Instance.ActualMonth % 12) + 1)+ "month");
            text.text = month + " " + (TimeManager.INITIAL_YEAR + TimeManager.Instance.ActualYear);
        }
    }

    public void OnDestroy()
    {
        Events.Instance.RemoveListener<OnNewMonth>(NewMonth);
    }
}
