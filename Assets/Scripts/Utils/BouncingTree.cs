using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingTree : MonoBehaviour {

    #region Private Variables
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
    Player _player;

    public bool canPopItem = false;
    public Item popItem;
    public float popTime;
    public float popPct;
    private bool poping = false;
    private bool popJumping = false;

#endregion

    // Use this for initialization
    private void Start()
    {
        InitOriginTransform();
        InitOriginVecUp();
        InitFunctionValues();
        InitPlayer();
    }

    #region Init

    void InitOriginTransform() {
        _originPosition = transform.position;
        _originRotation = transform.rotation;
    }

    void InitOriginVecUp () {
        _originVecUp = transform.TransformDirection(Vector3.up);
        _vecUp = _originVecUp;
    }

    void InitFunctionValues() {
        _a = -1 / (_maxDistance - _minDistance);
        _b = -_maxDistance * _a;
    }

    void InitPlayer() {
        if (PlayerManager.Instance) _player = PlayerManager.Instance.player;
    }

    #endregion

    public void CustomUpdate () {
        if (_player == null) InitPlayer();
        else {
            SetDistancePivot();
            SetVecUp();

            if (CheckDistancePivot()) {
                if (!popJumping && canPopItem && !poping && transform.parent.gameObject.activeSelf)
                {
                    if (FtueManager.instance.active)
                    {
                        FtueComponent step = FtueManager.instance.currentStep;
                        if (step.popTarget == popItem.itemType && step.CanPop()) StartCoroutine(PopCoroutine(100f));
                        else StartCoroutine(PopCoroutine(-1));
                    }
                    else StartCoroutine(PopCoroutine(popPct));
                }

                SetAngle();
                SetAxis();

                SetNewPosition();
                
                _startTimeLerp = Time.time;
            }
            else LerpToOriginPosition();
        }
	}

    #region Set

    protected IEnumerator PopCoroutine(float popPourcentage)
    {
        poping = true;
        float t = 0;
        float random;
        bool poped = false;
        while (poping && CheckDistancePivot())
        {
            if (popJumping || !PlayerManager.Instance.player.Moving)
            {
                yield return null;
            }
            else
            {
                t += Time.deltaTime * (1f / popTime);
                t = Mathf.Clamp01(t);
                if (t >= 1)
                {
                    random = Random.Range(0f, 100f);
                    if (random <= popPourcentage)
                    {
                        Pop();
                        if (FtueManager.instance.active) FtueManager.instance.currentStep.PopOne();
                        poped = true;
                    }
                    t = 0;
                }
            }
            yield return null;
        }

        if (popPourcentage >= 100f && !poped)
        {
            Pop();
            if (FtueManager.instance.active) FtueManager.instance.currentStep.PopOne();
        }
        poping = false;
    }

    private Vector3 popDirection = Vector3.zero;
    private Vector3 popUpDirection = Vector3.zero;
    private void Pop()
    {
        popUpDirection = transform.position.normalized;
        float randomAngle = Random.Range(0f, 360f);
        Vector3 groundVec = (_player.transform.position - transform.position).normalized;
        popDirection = MathCustom.RotateDirectionAround(Vector3.Cross(popUpDirection, groundVec), randomAngle, popUpDirection);
        Cell cell = GetComponentInParent<Cell>();
        GameObject newProps = EarthManager.Instance.CreateProps(popItem.prefab, transform.position, cell);
        StartCoroutine(PopJumpCoroutine(newProps.transform));
    }

    protected IEnumerator PopJumpCoroutine(Transform target)
    {
        popJumping = true;
        float t = 0f;
        Vector3 basePos = target.position;
        Vector3 baseScale = target.localScale;
        float x = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            t = Mathf.Clamp01(t);
            x = Easing.Arch(t);
            target.position = basePos + (((popDirection * t) + (popUpDirection * x)) * 0.1f);
            target.localScale = new Vector3(t * baseScale.x, t * baseScale.y, t * baseScale.z);
            yield return null;
        }
        popJumping = false;
    }

    // Set Distance between Player and Pivot
    void SetDistancePivot() {
        _distancePivot = Vector3.Distance(transform.position, _player.transform.position);
    }

    void SetVecUp() {
        _vecUp = transform.TransformDirection(Vector3.up);
    }

    void SetAngle() {
        if (_distancePivot <= _minDistance) _angle = _maxAngle;
        else if (_distancePivot >= _maxDistance) _angle = 0;
        else _angle = (_a * _distancePivot + _b) * _maxAngle;
    }

    void SetAxis() {
        _vecDir = Vector3.Normalize(_player.transform.position - transform.position);
        _axis = Vector3.Cross(_vecDir, _vecUp);
    }
    
    void SetNewPosition() {
        transform.position = _originPosition;
        transform.rotation = _originRotation;

        transform.RotateAround(transform.position, _axis, _angle);
    }

    void LerpToOriginPosition() {
        float t =  Mathf.Clamp01((Time.time - _startTimeLerp) / _timeToBack);

        transform.position = Vector3.Lerp(transform.position, _originPosition, t);
        if (t <= 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, _originRotation, 0f);
            if (!popJumping) poping = false;
        }
        else transform.rotation = Quaternion.Lerp(transform.rotation, _originRotation, 0.1f);
    }

    #endregion

    #region Check

    bool CheckDistancePivot() {
        return _distancePivot <= _maxDistance;
    }

    #endregion
}
