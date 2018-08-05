using UnityEngine;

public class ControllerStick : MonoBehaviour {

    #region Public Variable
    public Vector2 stickPosition {
        get { return _stickPosition; }
    }
    #endregion

    #region Private Variable
    // Reference of the stick
    Transform _stick;

    // Local Position of the Stick in Percent (Player ControllerStick)
    Vector2 _stickPosition = Vector2.zero;

    // Angle of the Stick from the middle of the Circle
    float _stickAngle = 0f;

    // Radius of the Circle ControllerStick
    float _radius;

    // Distance to detect the Touch on the Stick
    float _maxTouchDistance;

    // Reference to the Touch
    Touch _touchInput;
    #endregion

    private static ControllerStick _instance;
    
    public static ControllerStick instance {
        get {
            return _instance;
        }
    }

    protected void Awake() {
        _instance = this;
    }

    // Use this for initialization
    void Start () {
        _radius = GetComponent<RectTransform>().rect.width / 2;
        _maxTouchDistance = Mathf.Sqrt(Mathf.Pow(_radius, 2) * 2);
        _stick = transform.GetChild(0);
    }
	
	// Update is called once per frame
	void Update () {

        GetTouchPosition();
        SetStickPosition();
        PositioningStick();
    }

    void GetTouchPosition() {

        float lTouchDistance;
        float lTouchDistanceX;
        float lTouchDistanceY;

        if (Input.touchCount == 0) {
            _stick.localPosition = Vector3.zero;
            return;
        }
        else {
            _touchInput = Input.GetTouch(0);

            lTouchDistanceX = _touchInput.position.x - transform.position.x;
            lTouchDistanceY = _touchInput.position.y - transform.position.y;

            lTouchDistance = Mathf.Sqrt(Mathf.Pow(lTouchDistanceX, 2f) + Mathf.Pow(lTouchDistanceY, 2f));

            if (lTouchDistance < _maxTouchDistance) _stick.localPosition = new Vector3(lTouchDistanceX, lTouchDistanceY, 0);
            else _stick.localPosition = Vector3.zero;
        }
    }

    void SetStickPosition() {
        _stickPosition = new Vector2( _stick.localPosition.x / _radius, _stick.localPosition.y / _radius);
    }

    // Function to repositioning the stick in the circle.
    void PositioningStick() {
        
        float lStickPerPosX;
        float lStickPerPosY;

        _stickAngle = Mathf.Atan(Mathf.Abs(_stickPosition.y) / Mathf.Abs(_stickPosition.x));
        
        if (float.IsNaN(_stickAngle)) return;
        
        if (_stickPosition.x < 0 && _stickPosition.y > 0) _stickAngle = Mathf.PI - _stickAngle;
        else if (_stickPosition.x < 0 && _stickPosition.y < 0) _stickAngle = Mathf.PI + _stickAngle;
        else if (_stickPosition.x > 0 && _stickPosition.y < 0) _stickAngle *= -1;

        lStickPerPosX = Mathf.Cos(_stickAngle) * _radius;
        lStickPerPosY = Mathf.Sin(_stickAngle) * _radius;
        
        if (Mathf.Abs(lStickPerPosX) < Mathf.Abs(_stick.localPosition.x) && Mathf.Abs(lStickPerPosY) < Mathf.Abs(_stick.localPosition.y)) _stick.localPosition = new Vector3(lStickPerPosX, lStickPerPosY, 0);
    }
}