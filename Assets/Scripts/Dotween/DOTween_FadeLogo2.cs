using UnityEngine;
using DG.Tweening;



public class DOTween_FadeLogo2 : MonoBehaviour {

    public float duration, pause;
    public SpriteRenderer logo1;

	void Start () {
        logo1.DOFade(1, duration).SetDelay(pause).OnComplete(FadeInLogo1);
	}

    void FadeInLogo1()
    {
        logo1.DOFade(0, duration).SetDelay(pause).OnComplete(Start);
    } 

}
