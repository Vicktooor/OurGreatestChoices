using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class SDGNotification : MonoBehaviour
{
    [SerializeField]
    private float waitingTime = 1f;

    protected Animation anim;
    [HideInInspector]
    public Sprite sprite;

    public void Awake()
    {
        gameObject.SetActive(false);
        anim = GetComponent<Animation>();
    }

    public void Init(Sprite sdgSprite)
    {        
        sprite = sdgSprite;
        GetComponent<Image>().sprite = sprite;
    }

    public void ShowSDG(Action callback)
    {
        gameObject.SetActive(true);
        anim.Play("SGD_Animation");
        StartCoroutine(AnimationCoroutine(callback));
    }

    protected IEnumerator AnimationCoroutine(Action callback)
    {
        while (anim.isPlaying) { yield return null; }
        anim["SGD_Animation"].time = 0f;
        yield return new WaitForSeconds(waitingTime);
        gameObject.SetActive(false);
        callback();
    }
}
