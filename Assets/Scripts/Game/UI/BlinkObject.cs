using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkObject : MonoBehaviour
{
    public Color blinkColor = Color.white;

    private bool blink = false;
    private RawImage rawImg;

    protected void Awake()
    {
        rawImg = GetComponent<RawImage>();
    }

    protected void OnEnable()
    {
        StartCoroutine(Blink());
    }

    protected void OnDisable()
    {
        blink = false;
    }

    private IEnumerator Blink ()
    {
        blink = true;
        float t = 0;
        bool b = true;
        while (blink)
        {
            t += (Time.deltaTime * 2f) * ((b) ? 1 : -1);
            if (t <= 0) {
                t = 0;
                b = !b;
            }
            else if (t >= 1) {
                t = 1;
                b = !b;
            }
            rawImg.color = Color.Lerp(Color.white, blinkColor, t);
            yield return null;
        }
    }
}
