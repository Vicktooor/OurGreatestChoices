using UnityEngine;
using DG.Tweening;



public class Dotween_FeedbackClickNpc : MonoBehaviour {
public Vector3 strenght = new Vector3 (0,0,0);
public float duration = 1f;
public int vibrato = 1;
public float randomness = 1f;
public bool fadeOut = true;

	// Use this for initialization
	void Start () {
	}	

	void OnEnable (){
		transform.DOShakeScale(duration, strenght, vibrato, randomness, fadeOut);
		}
}
