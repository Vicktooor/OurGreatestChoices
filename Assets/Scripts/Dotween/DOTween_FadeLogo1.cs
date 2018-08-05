using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DOTween_FadeLogo1 : MonoBehaviour {

    public float duration, pauseVisible, pauseInvisible, initPause;
    public SpriteRenderer logo;

    private void OnEnable () {
        logo.DOFade(0, 0.01f);
        StartCoroutine(InitInvisible());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator InitInvisible() {
        yield return new WaitForSeconds(initPause);
        FadeInLogo();
        yield break;
    }

    IEnumerator VisibleCoroutine() {
        yield return new WaitForSeconds(pauseVisible);
        FadeOutLogo();
        yield break;
    }

    IEnumerator InvisibleCoroutine() {
        yield return new WaitForSeconds(pauseInvisible);
        FadeInLogo();
        yield break;
    }

    void FadeInLogo()
    {
        logo.DOFade(1, duration);
        StartCoroutine(VisibleCoroutine());
    }

    void FadeOutLogo() 
    {
        logo.DOFade(0, duration);
        StartCoroutine(InvisibleCoroutine());
    }

}
