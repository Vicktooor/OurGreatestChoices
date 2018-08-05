using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingTree : MonoBehaviour {

    #region Private Variables

    Transform _transform;

    /* -- Distance -- */

    [Header("Distance")]
    [SerializeField]
    float _minDistance = 0.1f;
    [SerializeField]
    float _maxDistance = 0.3f;

    float _distancePivot;

    /* -- Function -- */

    float _a;
    float _b;

    /* -- Angle -- */

    [Header("Angle")]
    [SerializeField]
    float _maxAngle = 30;

    float _angle;

    /* -- Replace Tree -- */
    [SerializeField]
    float _timeToBack = 2;
    float _startTimeLerp = 0;

    /* -- Vector Rotation -- */
    [Header("Pivot")]
    [SerializeField]
    Transform _pivot;

    Vector3 _originVecUp;
    Vector3 _vecUp;
    Vector3 _vecDir;
    Vector3 _axis;
    
    /* -- Origin Position / Rotation -- */

    Vector3 _originPosition;
    Quaternion _originRotation;
    
    /* -- Player -- */

    [Header("Player")]
    [SerializeField]
    GameObject _player;

#endregion

    // Use this for initialization
    void Start () {
        _transform = transform;

        InitOriginTransform();
        InitOriginVecUp();
        InitFunctionValues();

        InitPivot();
        InitPlayer();
    }

    #region Init

    void InitOriginTransform() {
        _originPosition = _transform.position;
        _originRotation = _transform.rotation;
    }

    void InitOriginVecUp () {
        _originVecUp = _transform.TransformDirection(Vector3.up);
        _vecUp = _originVecUp;
    }

    void InitFunctionValues() {
        _a = -1 / (_maxDistance - _minDistance);
        _b = -_maxDistance * _a;
    }

    void InitPivot() {
        if(_pivot == null) {
            _pivot = new GameObject().GetComponent<Transform>();
            _pivot.parent = _transform;

            _pivot.localPosition = new Vector3(0, 0, 0);
            _pivot.localRotation = new Quaternion(0, 0, 0, 0);
            _pivot.localScale = new Vector3(1, 1, 1);

            _pivot.name = "Pivot";
        }
    }

    void InitPlayer() {
        if (PlayerManager.instance) _player = PlayerManager.instance.player;
    }

    #endregion

    public void CustomUpdate () {
        if (_player == null) InitPlayer();
        else {

            SetDistancePivot();
            SetVecUp();

            if (CheckDistancePivot()) {

                SetAngle();
                SetAxis();

                SetNewPosition();
                
                _startTimeLerp = Time.time;
            }
            else LerpToOriginPosition();
        }
	}

    #region Set

    // Set Distance between Player and Pivot
    void SetDistancePivot() {
        _distancePivot = Vector3.Distance(_pivot.position, _player.transform.position);
    }

    void SetVecUp() {
        _vecUp = _transform.TransformDirection(Vector3.up);
    }

    void SetAngle() {
        if (_distancePivot <= _minDistance) _angle = _maxAngle;
        else if (_distancePivot >= _maxDistance) _angle = 0;
        else _angle = (_a * _distancePivot + _b) * _maxAngle;
    }

    void SetAxis() {
        _vecDir = Vector3.Normalize(_player.transform.position - _pivot.position);
        _axis = Vector3.Cross(_vecDir, _vecUp);
    }
    
    void SetNewPosition() {
        _transform.position = _originPosition;
        _transform.rotation = _originRotation;

        _transform.RotateAround(_pivot.position, _axis, _angle);
    }

    void LerpToOriginPosition() {
        float t = (Time.time - _startTimeLerp) / _timeToBack;

        _transform.position = Vector3.Lerp(_transform.position, _originPosition, t);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, _originRotation, t);
    }

    #endregion

    #region Check

    bool CheckDistancePivot() {
        return _distancePivot <= _maxDistance;
    }

    #endregion

    public Transform playerTransform;

    public void Rotate()
    {
        
    }
}
