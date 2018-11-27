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
        Events.Instance.AddListener<UpdateTime>(UpdateTimer);
        text.text = TextManager.GetText("1month") + " " + TimeManager.INITIAL_YEAR;
    }

    protected void UpdateTimer(UpdateTime e)
    {
        string month = TextManager.GetText(((TimeManager.Instance.ActualMonth % 12) + 1) + "month");
        text.text = month + " " + (TimeManager.INITIAL_YEAR + TimeManager.Instance.ActualYear);
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
        Events.Instance.RemoveListener<UpdateTime>(UpdateTimer);
        Events.Instance.RemoveListener<OnNewMonth>(NewMonth);
    }
}
