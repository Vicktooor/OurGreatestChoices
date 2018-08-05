using System.Collections;
using UnityEngine;

public class PanelTransition : MonoBehaviour {

    #region Public Variable

    // Speed of the Translation Panel
    public float timeTransition = 2;

    // Bool to know if it's the left panel or the right one
    public bool isLeft = true;
    
    #endregion

    #region Private Variable

    // Reference of Transform
    Transform _transform;

    // Vector3 of the local start position of the panel
    Vector3 _startLocalPosition;

    // Float Distance for the Lerp 
    float _distance;

    // Bool to know if the animation will cover the screen or not
    bool _isAppear = true;

    #endregion

    // Use this for initialization
	void Awake()
	{
		_transform = transform;
		_startLocalPosition = _transform.localPosition;

		_transform.localPosition = Vector3.zero;
		_distance = Mathf.Abs(_startLocalPosition.x - _transform.localPosition.x);

		_transform.localPosition = _startLocalPosition;
		Events.Instance.AddListener<OnSceneLoaded>(LaunchCoroutineTransitionGM);
	}

	protected void OnEnable()
	{
		Events.Instance.AddListener<OnSwitchScene>(LaunchCoroutineTransitionUIM);
	}

	protected void OnDisable()
	{
		Events.Instance.RemoveListener<OnSwitchScene>(LaunchCoroutineTransitionUIM);
	}

	void LaunchCoroutineTransitionGM(OnSceneLoaded e) {
        if (_isAppear) return;
        StartCoroutine(MovePanelCoroutine());

        Events.Instance.Raise(new CloudOut());
    }

    void LaunchCoroutineTransitionUIM(OnSwitchScene e) {
        StartCoroutine(MovePanelCoroutine());

        Events.Instance.Raise(new CloudIn());
    }

	IEnumerator MovePanelCoroutine() {

		float pct = 0;
		while (pct < 1)
		{
			pct += timeTransition * Time.deltaTime;
			if (pct > 1) pct = 1;
			_transform.localPosition = new Vector3((_isAppear ? _startLocalPosition.x : 0) + _distance * pct * (isLeft ? 1 : -1) * (_isAppear ? 1 : -1), _startLocalPosition.y, _startLocalPosition.z);
			yield return null;
		}

		if (_isAppear) {
            _transform.localPosition = Vector3.zero;
            _isAppear = !_isAppear;
            yield break;
        }
        else { 
            transform.localPosition = _startLocalPosition;
            _isAppear = !_isAppear;

			/* Event for UIManager */
            Events.Instance.Raise(new PanelLerpEnd());

            yield break;
        }
    }

	protected void OnDestroy()
	{
		Events.Instance.RemoveListener<OnSceneLoaded>(LaunchCoroutineTransitionGM);
	}
}