using UnityEngine;

public class Cloud : MonoBehaviour {

    Transform _transform;

    Vector3 _pivot;

    Vector3 _axis;

    float _speed = 5;

    float _pivotFactor = 5;

	// Use this for initialization
	void Start () {
        _transform = transform;

        _pivot = new Vector3(Random.value * _pivotFactor + _transform.position.x, Random.value * _pivotFactor + _transform.position.y, 0);
        _axis = Random.value < 0.5f ? Vector3.forward : Vector3.back;
	}
	
	// Update is called once per frame
	void Update () {
        _transform.RotateAround(_pivot, _axis, _speed);
        _transform.eulerAngles = Vector3.zero;
	}
}
