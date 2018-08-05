using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



public class dotween_test : MonoBehaviour {
public Vector3 punch = new Vector3 (0,0,0);
public float duration = 1f;
public int vibrato = 1;
public float elasticity = 1f;
public float debut  = 1f;
public float fin  = 1f;
Vector3 zero = new Vector3 (0,0,0);

	// Use this for initialization
	void Start () {
		transform.localScale = zero;
	}	

	void OnEnable (){
        ControllerInput.OpenScreens.Add(transform);
        transform.DOScale(1, debut).OnComplete(etape1);	
	}

	void etape1 () {
        transform.DOPunchScale(punch, duration, vibrato, elasticity);
        Events.Instance.Raise(new OnEndTween());
	}

	public void close (){
		transform.DOScale(0, fin).OnComplete(disable).SetEase(Ease.Flash);
	}

	void disable ()	{
        ControllerInput.OpenScreens.Remove(transform);
        gameObject.SetActive(false);
	}

	void OnDisable()	{
        if (ControllerInput.OpenScreens.Contains(transform)) ControllerInput.OpenScreens.Remove(transform);
        transform.localScale = zero;
	}
}
