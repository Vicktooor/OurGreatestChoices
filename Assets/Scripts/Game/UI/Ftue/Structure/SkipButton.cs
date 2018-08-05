using Assets.Scripts.Game.UI.Ftue;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkipButton : Button
{
    public Image Img;

    private int frameCounter = 25;
    private int releaseCounter = 0;

    public override void OnPointerDown(PointerEventData eventData)
    {
        releaseCounter++;
        if (releaseCounter >= frameCounter) FtueManager.instance.Clear();
        else Img.fillAmount = Mathf.Clamp(releaseCounter, 0, frameCounter) / frameCounter;
    }
}
