using Assets.Script;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerInput : MonoBehaviour {

    public static List<Transform> OpenScreens = new List<Transform>();

	#region Public Variable

	public float circleInputWidth = 50f;

    public Vector2 touchCenterPosition {
        get { return _touchCenterPosition; }
    }

    public Vector2 touchPosition {
        get { return _touchPosition; }
    }

    public Vector3 tapPosition {
        get { return _tapPosition; }
    }

    // Number of frame to difference Tap and Hold
    public uint frameTap;

    #endregion

    #region Private Variable
    // Position of the Touch Input from the center in Percent (Player ControllerTouch)
    Vector2 _touchCenterPosition = Vector2.zero;

    // Position of the Touch Input on screen.
    Vector2 _touchPosition = Vector2.zero;

    // Position on the Planet of the Tap Input
    Vector3 _tapPosition = Vector3.zero;

    //Bool to check if the player is doing a hold to know if the player finishing a hold or click move
    bool _isHolding = false;

    // piching bool
    bool _isPiching = false;

    // Reference to the Touch
    Touch _touchInput;

    // Reference of the Mouse Position
    Vector3 _mousePosition;

    // Position of the Center of the Screen
    Vector2 _screenCenter;

    // Frame Counter
    float _frameCounter = 0;

    // The radius of the Planet
    float _planetRadius;

    // Bool to know if the player click on UI elements
    bool _isOnUI = false;

    // Result of raycast;
    RaycastHit _hit;
    public Interactable Hit { get
        {
            Interactable lHit = _hit.transform.gameObject.GetComponent<Interactable>();
            if (lHit != null) return lHit;
            else return null;
        }
    }

    [Header("Layers")]
    [SerializeField]
    LayerMask _layerUI;

    [SerializeField]
    LayerMask _layerCell;

    [SerializeField]
    LayerMask _layerInteract;

    [SerializeField]
    LayerMask _layerNature;

    #endregion


    private int _panelNumber = 0;
    private int _previousPanelNumber = 0;
    private bool _safeBlocking = false;
    private bool _controlEnable = true;

    private uint _startFrameTap;

    private static ControllerInput _instance;

    public static ControllerInput instance {
        get {
            return _instance;
        }
    }

    protected void Awake() {
        _instance = this;
        _startFrameTap = frameTap;
        _screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }


    void Update() {
        _panelNumber = OpenScreens.Count;
        if (_safeBlocking) return;

        if (_panelNumber != _previousPanelNumber && _panelNumber == 0)
        {
            StartCoroutine(SafeClosePanelCoroutine());
            _previousPanelNumber = _panelNumber;
            return;
        }

        if (GameManager.Instance.LoadedScene == SceneString.MapView) frameTap = 1;
        else frameTap = _startFrameTap;

        if (FtueManager.instance.active && FtueManager.instance.activeInput == FtueInputs.PINCH) {
            if (Pinch()) return;
        }

        if (!FtueManager.instance.active && _panelNumber == 0)
        {
            _controlEnable = true;
            if (Pinch()) return;
            TouchHit();
        }
        else if (OpenScreens.Count > 0 && _controlEnable) ResetDatasTouch();

        _previousPanelNumber = _panelNumber;
    }

    private int _closeFrameCount = 2;
    protected IEnumerator SafeClosePanelCoroutine()
    {
        _safeBlocking = true;
        int counter = 0;
        while (counter <= _closeFrameCount)
        {
            counter++;
            yield return null;
        }
        _safeBlocking = false;
    }

    protected bool Pinch() {
        if (_isPiching) return true;
		if (Input.touchCount == 2 && !GameManager.Instance.IsOnDesk)
		{
			StartCoroutine(PinchCoroutine());
			return true;
		}

        if (Input.GetKey(KeyCode.Space))
        {
            Events.Instance.Raise(new OnPinch(-3f));
        }
        if (Input.GetKey(KeyCode.D)) { Events.Instance.Raise(new OnPinch(3f)); }
		return false;
	}

    protected IEnumerator PinchCoroutine() {
        _isPiching = true;
		while (Input.touchCount == 2)
		{
			Vector2 touchZeroPrevPos = Input.touches[0].position - Input.touches[0].deltaPosition;
			Vector2 touchOnePrevPos = Input.touches[1].position - Input.touches[1].deltaPosition;

			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (Input.touches[0].position - Input.touches[1].position).magnitude;

			Events.Instance.Raise(new OnPinch(prevTouchDeltaMag - touchDeltaMag));
			yield return null;
		}      
        _isPiching = false;
    }

	public float GetMoveInputMagnitude()
	{
		Player player = PlayerManager.Instance.player;
		if (player)
		{
			Vector2 playerPos = Camera.main.WorldToViewportPoint(player.transform.position);
			playerPos = new Vector2(0, -playerPos.x);
			return Vector2.Distance(_touchCenterPosition, playerPos);
		}
		else return 0f;
	}

    bool _tapInteract = false;
    bool _tapCell = false;
    bool _tapUI = false;
    bool _touching = false;

    Touch _lastTouch;

    void TouchHit() {

        if (GameManager.Instance.IsOnDesk)
        {
            GetTapPosition();
            GetTouchPositionFromCenter();
            GetTouchPosition();

            if (Input.GetMouseButton(0))
            {
                if (CheckTapUILayer()) _tapUI = true;
                if (_tapUI)
                {
                    ResetDatasTouch();
                    return;
                }

                _touching = true;
                _frameCounter++;

                if (CheckTapLayer(_layerInteract)) _tapInteract = true;
                else if (CheckTapLayer(_layerCell)) _tapCell = true;
            }
            else _touching = false;

            if (!_touching)
            {
                if (_frameCounter > 0 && _frameCounter <= frameTap)
                {
                    if (_tapInteract)
                    {
                        if (!TapInteract()) TapCell();
                        ResetDatasTouch();
                        return;
                    }
                    else if (_tapCell)
                    {
                        TapCell();
                        ResetDatasTouch();
                        return;
                    }
                }
                else if (_frameCounter > 0)
                {
                    ResetDatasTouch();
                    return;
                }
            }
            else
            {
                if (_frameCounter > frameTap && !_isOnUI) Events.Instance.Raise(new OnHold(Input.mousePosition));
            }
        }
        else
        {
            if (Input.touchCount == 1)
            {
                _lastTouch = Input.GetTouch(0);

                GetTapPosition(_lastTouch);
                GetTouchPositionFromCenter(_lastTouch);
                GetTouchPosition(_lastTouch);

                _touching = true;
                _frameCounter++;

                if (_frameCounter > frameTap)
                {
                    Events.Instance.Raise(new OnHold(_lastTouch.position));
                    return;
                }

                if (CheckTapLayer(_layerInteract, _lastTouch)) _tapInteract = true;
                else if (CheckTapLayer(_layerCell, _lastTouch)) _tapCell = true;
            }
            else _touching = false;

            if (!_touching)
            {
                GetTapPosition(_lastTouch);
                GetTouchPositionFromCenter(_lastTouch);
                GetTouchPosition(_lastTouch);

                if (_frameCounter > 0 && _frameCounter <= frameTap)
                {
                    if (_tapInteract)
                    {
                        if (!TapInteract()) TapCell();
                        ResetDatasTouch();
                        return;
                    }
                    else if (_tapCell)
                    {
                        TapCell();
                        ResetDatasTouch();
                        return;
                    }
                }
                else if (_frameCounter > 0)
                {
                    ResetDatasTouch();
                    return;
                }
            }
        }
    }

    bool CheckTapUILayer()
    {
        if(Input.touchCount > 0) {
            if (EventSystem.current.IsPointerOverGameObject(_touchInput.fingerId)) {
                return true;
            }
        }
        return EventSystem.current.IsPointerOverGameObject();
    }

    bool CheckTapLayer(LayerMask pLayer)
    {
        Ray ray = GetRay();
        if (Physics.Raycast(ray, out _hit, 2, pLayer)) return true;
        else return false;
    }

    bool CheckTapLayer(LayerMask pLayer, Touch lastTouch)
    {
        Ray ray = GetRay(lastTouch);
        if (Physics.Raycast(ray, out _hit, 2, pLayer)) return true;
        else return false;
    }

    bool TapInteract()
    {
		InteractablePNJ pnj = _hit.transform.GetComponent<InteractablePNJ>();
        
		if (pnj != null) {
            Events.Instance.Raise(new OnTapNPC(pnj));
			return true;
        }
        else if (_hit.transform.GetComponent<ItemPickup>() && PlayerManager.Instance.playerType == EPlayer.ECO) {
            Events.Instance.Raise(new OnTapItemPickUp(_hit.transform.gameObject));		
			return true;
        }
		return false;
    }

    void TapCell() {
        if (_tapPosition != Vector3.zero) Events.Instance.Raise(new OnTap(_tapPosition)); 
    }

    public void ResetDatasTouch() {
        _isOnUI = false;
        _isHolding = false;
        _frameCounter = 0;

        _tapInteract = false;
        _tapCell = false;
        _tapUI = false;

        _controlEnable = false;

        Events.Instance.Raise(new OnRemove());
    }

    void GetTouchPosition() {
        if (Input.touchCount == 0 && !Input.GetMouseButton(0)) {
            _touchPosition = Vector2.zero;
            return;
        }
        else {
            if (!GameManager.Instance.IsOnDesk) {
				if (Input.touchCount > 0)
				{
					_touchInput = Input.GetTouch(0);
					_touchPosition = new Vector2(_touchInput.position.x, _touchInput.position.y);
				}
            }
            else {
                _mousePosition = Input.mousePosition;
                _touchPosition = new Vector2(_mousePosition.x, _mousePosition.y);
            }
        }
    }

    void GetTouchPosition(Touch lastTouch)
    {
        _touchPosition = new Vector2(lastTouch.position.x, lastTouch.position.y);
    }

    //Retourne la position de l'input par rapport au centre de l'écran.
    void GetTouchPositionFromCenter() {
        float lTouchDistanceX;
        float lTouchDistanceY;

        if (Input.touchCount == 0 && !Input.GetMouseButton(0)) {
            _touchCenterPosition = Vector2.zero;
            return;
        }
        else {
            if (!GameManager.Instance.IsOnDesk) {
                _touchInput = Input.GetTouch(0);

                lTouchDistanceX = (_touchInput.position.x - _screenCenter.x) / _screenCenter.x;
                lTouchDistanceY = (_touchInput.position.y - _screenCenter.y) / _screenCenter.y;
            }
            else {
                _mousePosition = Input.mousePosition;

                lTouchDistanceX = (_mousePosition.x - _screenCenter.x) / _screenCenter.x;
                lTouchDistanceY = (_mousePosition.y - _screenCenter.y) / _screenCenter.y;
            }

            _touchCenterPosition = new Vector2(lTouchDistanceX, lTouchDistanceY);
        }
    }

    void GetTouchPositionFromCenter(Touch lastTouch)
    {
        float lTouchDistanceX;
        float lTouchDistanceY;

        lTouchDistanceX = (lastTouch.position.x - _screenCenter.x) / _screenCenter.x;
        lTouchDistanceY = (lastTouch.position.y - _screenCenter.y) / _screenCenter.y;

        _touchCenterPosition = new Vector2(lTouchDistanceX, lTouchDistanceY);
    }

    void GetTapPosition() {
        Ray ray = GetRay();

        if (ray.direction != Vector3.zero) {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50, _layerCell)) _tapPosition = hit.point;
            else _tapPosition = Vector3.zero;
        }
    }

    void GetTapPosition(Touch lastTouch)
    {
        Ray ray = GetRay(lastTouch);
        if (ray.direction != Vector3.zero)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50, _layerCell)) _tapPosition = hit.point;
            else _tapPosition = Vector3.zero;
        }
    }

    //Return a ray depending of the support
    Ray GetRay() {
        Ray ray = new Ray();

        if (Camera.main) {
			if (!GameManager.Instance.IsOnDesk)
			{
				if (Input.touchCount == 1) return Camera.main.ScreenPointToRay(_touchInput.position);
			}
			else return Camera.main.ScreenPointToRay(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
        return ray;
    }

    Ray GetRay(Touch lastTouch)
    {
        Ray ray = new Ray();

        if (Camera.main)
        {
            if (!GameManager.Instance.IsOnDesk)
            {
                return Camera.main.ScreenPointToRay(lastTouch.position);
            }
            else return Camera.main.ScreenPointToRay(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
        return ray;
    }
}

