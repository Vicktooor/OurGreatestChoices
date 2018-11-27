using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECollision { NONE, GROUND, WALL }

public class Player : MonoBehaviour
{
	protected Cell _associateCell;
    public Cell AssociateCell {
        get { return _associateCell; }
    }

    private Vector3 normalSlideVector;

    private float x = 0.0f;
    private float z = 0.0f;

	private Quaternion _targetRotation = Quaternion.identity;
	private Quaternion _playerRotation;

	private Interactable _targetInteractable;
    private GameObject _targetItemPickUp;
    private Vector3 _tapPosition;

	#region Public Variable
	// GameObject of the Planet
	public GameObject planet;

    // The distance to check the Collision
    public float distanceCollision;

    // Speed of the Player on Axis Vertical ans Horizontal
    public float speedVertical;
    public float speedHorizontal;

    // Number of step to rotate in the move direction
    public int stepRotation = 10;

    // Bool to determine if the player can move
    public bool moveHold = false;

    public GameObject playerAsset;

    public GameObject dustPFB;

    public LayerMask layerCell;

    // Index of the Player
    [HideInInspector]
    public int index;
    #endregion

    #region Private Variable

    // Reference to transform
    Transform _transform;

    // Vector3 for the CheckRaycastHit
    Vector3 _verticalVector = Vector3.zero;
    Vector3 _horizontalVector = Vector3.zero;

    // Start Position of the Tap
    Vector3 _startPosition;

    // Input on Keyboard Arrows
    float _verticalInput;
    float _horizontalInput;

    // Bool to determine Tap and Hold
    bool _isHolding = false;

    // Bool to Cancel Move on UI
    bool _onUI = false;

    // Bool to know if the player is moving with tap
    bool _isTapMoving = false;

    // Radius for hold controller, distance in percent;
    [SerializeField]
    [Range(0, 1)]
    float _holdRadiusPercent = 0.25f;

    // Radius of the Planet
    const float RADIUS = 5;

    // Component FMOD Emitter
    StudioEventEmitter _fmodEmitter;

    // Component FMOD Listener
    StudioListener _fmodListener;

    // Frame for FootStep
    [SerializeField]
    int _frameEmitter = 20;

    // Frame counter for Emitter FootStep;
    int _counterEmitter = 0;

    const string GROUND_EMITTER_STRING = "Sol";

    const int GRASS_EMITTER_VALUE = 5;
    const int SAND_EMITTER_VALUE = 15;
    const int TOWN_EMITTER_VALUE = 25;
    const int MOSS_EMITTER_VALUE = 35;

    // Animator of PlayerAsset
    Animator _animator;

    #endregion

    private const float PLAYER_HEIGHT = 0.5f;
    public static float NPC_HELP_DIST = 0.2f;

    private Vector3 posCallBack = Vector3.zero;
    private Action vCallBack;

    //public Transform targetToMove; //A MODIFIER

    private const string TAG_NONWALKABLE = "NonWalkable";

	public EPlayer playerType;

    private Vector3 _lastPos;
    private Vector3 _currentPos;
    private bool _moving = false;
    public bool Moving { get { return _moving; } }

    public void SetPosCallBack(Vector3 pos, Action cb)
    {
        posCallBack = pos;
        vCallBack = cb;
    }

    void OnEnable() {
		_transform = GetComponent<Transform>();
        Events.Instance.AddListener<OnTap>(LaunchCoroutineTap);
        Events.Instance.AddListener<OnTapNPC>(LaunchCoroutineTapNPC);
        Events.Instance.AddListener<OnTapItemPickUp>(LaunchCoroutineTapItemPickUp);
        Events.Instance.AddListener<OnHold>(CancelCoroutineTap);
        Events.Instance.AddListener<OnRemove>(PermitMove);
        Events.Instance.AddListener<OnInteraction>(OnInteraction);
        Events.Instance.AddListener<OnDesinteraction>(OnDesinteraction);
        Events.Instance.AddListener<SharePlayerPosition>(ReceivePosition);
        Events.Instance.AddListener<OnClickInteractable>(StopMove);
        Events.Instance.AddListener<OnPickUp>(StopMovePickUp);
        Events.Instance.AddListener<OnFusion>(StopMoveFusion);
        Events.Instance.AddListener<OnFtueNextStep>(StopMove);
        Events.Instance.AddListener<OnOpenUI>(CancelMove);
    }

	void OnDisable()
	{
		Events.Instance.RemoveListener<OnTap>(LaunchCoroutineTap);
		Events.Instance.RemoveListener<OnTapNPC>(LaunchCoroutineTapNPC);
        Events.Instance.RemoveListener<OnTapItemPickUp>(LaunchCoroutineTapItemPickUp);
        Events.Instance.RemoveListener<OnHold>(CancelCoroutineTap);
		Events.Instance.RemoveListener<OnRemove>(PermitMove);
		Events.Instance.RemoveListener<OnInteraction>(OnInteraction);
		Events.Instance.RemoveListener<OnDesinteraction>(OnDesinteraction);
		Events.Instance.RemoveListener<SharePlayerPosition>(ReceivePosition);
		Events.Instance.RemoveListener<OnClickInteractable>(StopMove);
		Events.Instance.RemoveListener<OnPickUp>(StopMovePickUp);
		Events.Instance.RemoveListener<OnFusion>(StopMoveFusion);
		Events.Instance.RemoveListener<OnFtueNextStep>(StopMove);
		Events.Instance.RemoveListener<OnOpenUI>(CancelMove);
	}

	#region Move Functions
    protected Vector3 SnapToPlanet(Vector3 point) {
        Ray ray = new Ray(point * 2f, Vector3.zero - point);
        RaycastHit hit;
        if (_associateCell.SelfCollider[0].Raycast(ray, out hit, 25f))
		{
			return hit.point;
		}
		else
		{
			foreach (Cell neighbor in _associateCell.Neighbors)
			{
				if (neighbor.SelfCollider[0].Raycast(ray, out hit, 25f))
				{
					_associateCell = neighbor;
					return hit.point;
				}
			}
		}
        return point;
    }

	protected ECollision CanMoveForward(Vector3 forwardDir)
	{
        normalSlideVector = Vector3.zero;
        Vector3 rayOrigin = transform.position + (forwardDir * 0.15f) + (transform.up * 3f);
		Ray ray = new Ray(rayOrigin, Vector3.zero - rayOrigin);
		RaycastHit hit;

        if (_associateCell.SelfCollider[0].Raycast(ray, out hit, 10f))
        {
            if (!_associateCell.walkable) return ECollision.GROUND;
            float angle = Vector3.Angle(hit.normal, _associateCell.GetCenterPosition());
            if (angle > 35f) return ECollision.GROUND;
        }

        foreach (Cell neighbor in _associateCell.Neighbors)
        {
            if (neighbor.SelfCollider[0].Raycast(ray, out hit, 10f))
            {
                if (!neighbor.walkable) return ECollision.GROUND;
                float angle = Vector3.Angle(hit.normal, neighbor.GetCenterPosition());
                if (angle > 35f) return ECollision.GROUND;
            }
        }

        Vector3 oPos = transform.position + (forwardDir * -0.05f) + transform.position.normalized * 0.05f;
        Ray rayfront = new Ray(oPos, forwardDir);
		RaycastHit buildHit;

        Debug.DrawLine(oPos, oPos + rayfront.direction, Color.yellow);

		foreach (KeyValuePair<Props, Collider[]> item in _associateCell.PropsCollider)
		{
			if (item.Key == null) break;

            for (int i = 0; i < item.Value.Length; i++)
            {
                if (!item.Value[i].enabled) continue;
                if (item.Value[i].Raycast(rayfront, out buildHit, 0.1f) && !(item.Key is ItemPickup))
                {
                    normalSlideVector = buildHit.normal;
                    return ECollision.WALL;
                }
            }
        }

        foreach (Cell c in _associateCell.Neighbors)
		{			
			foreach (KeyValuePair<Props, Collider[]> item in c.PropsCollider)
			{
				if (item.Key == null) continue;

                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (!item.Value[i].enabled) continue;
                    if (item.Value[i].Raycast(rayfront, out buildHit, 0.1f) && !(item.Key is ItemPickup))
                    {
                        normalSlideVector = buildHit.normal;
                        return ECollision.WALL;
                    }
                }
            }
		}

		return ECollision.NONE;
	}
	#endregion

	protected void Awake() {
        var angles = transform.eulerAngles;
        x = angles.y;
        z = angles.x;

        _fmodEmitter = GetComponent<StudioEventEmitter>();
        _fmodListener = GetComponent<StudioListener>();
        _transform = transform;
        if (_transform.GetComponentInChildren<Animator>() != null) _animator = _transform.GetComponentInChildren<Animator>();
    }

	protected void Update()
	{
        if (GameManager.Instance.LoadedScene == SceneString.MapView) return;
		if (!PlayerManager.Instance.player) return;
        if (PlayerManager.Instance.player == gameObject) OnCell();

		if (PlayerManager.Instance.player.GetComponent<Player>().playerType != playerType) return;

        Physics.gravity = PlayerManager.Instance.player.transform.position.normalized * -9.81f;

        if (moveHold)
        {
            if (GameManager.Instance.LoadedScene == SceneString.MapView) return;
            if (moveHold && !_onUI && _isHolding)
            {
                Rotate();
                ECollision lCollision = CanMoveForward(playerAsset.transform.forward);
                Move(lCollision);
                _isTapMoving = false;
                SetAnimation(true);
            }
        }
        else if (!_isTapMoving) SetAnimation(false);
        _currentPos = _transform.position;
    }

	protected void LateUpdate()
	{
		if (_transform.hasChanged)
		{
			if (!GameManager.Instance) return;	
			if (EarthManager.Instance.playerPositions.ContainsKey(playerType))
			{
				EarthManager.Instance.playerPositions[playerType] = new KeyValuePair<int, Vector3>(_associateCell.ID, _transform.position);
			}
        }
        if (_lastPos.Equals(_currentPos)) _moving = false;
        else _moving = true;
        _lastPos = _transform.position;
    }

    #region FootStep

    protected void OnCell() {
        RaycastHit hit;

        //Debug.DrawRay(_transform.position, -_transform.up * 0.5f, Color.blue);

        if (Physics.Raycast(_transform.position, -_transform.up, out hit, 0.5f, layerCell)) {
            CellState lCellState = hit.transform.GetComponent<Cell>().State;
            switch (lCellState) {
                case CellState.GRASS:
                    GetGroundParam().Value = GRASS_EMITTER_VALUE;
                    Events.Instance.Raise(new OnGrass());
                    break;
                case CellState.SAND:
                    GetGroundParam().Value = SAND_EMITTER_VALUE;
                    Events.Instance.Raise(new OnDesert());
                    break;
                case CellState.MOSS:
                    GetGroundParam().Value = MOSS_EMITTER_VALUE;
                    Events.Instance.Raise(new OnMoss());
                    break;
                case CellState.TOWN:
                    GetGroundParam().Value = TOWN_EMITTER_VALUE;
                    Events.Instance.Raise(new OnTown());
                    break;
                case CellState.SNOW:
                    GetGroundParam().Value = SAND_EMITTER_VALUE;
                    Events.Instance.Raise(new OnMountain());
                    break;
                case CellState.ROCK:
                    GetGroundParam().Value = TOWN_EMITTER_VALUE;
                    Events.Instance.Raise(new OnMountain());
                    break;
                case CellState.SEA:
                    GetGroundParam().Value = MOSS_EMITTER_VALUE;
                    Events.Instance.Raise(new OnCoast());
                    break;
            }
        }
    }

	protected ParamRef GetGroundParam() {

        int index;
        int lenght = _fmodEmitter.Params.Length;

        ParamRef param;

        for (index = 0; index < lenght; index++) {
            param = _fmodEmitter.Params[index];
            if (param.Name == GROUND_EMITTER_STRING) return param;
        }

        return null;
    }

	protected void EnabledListener(bool pBool) {
        _fmodListener.enabled = pBool;
    }

    #endregion

    #region Fonctions Alexis

    protected void Move(ECollision colType)
	{
		StopAllCoroutines();
		float dist = ControllerInput.instance.GetMoveInputMagnitude();
		float k = Mathf.Clamp(dist / ControllerInput.instance.circleInputWidth, 0f, 1f);

        if (colType == ECollision.NONE) {
            transform.position += (k * playerAsset.transform.forward) * (Time.deltaTime / 3f);
            transform.position = SnapToPlanet(transform.position);
            if (posCallBack != Vector3.zero)
            {
                if (Vector3.Distance(transform.position, posCallBack) <= 0.1f)
                {
                    vCallBack();
                    posCallBack = Vector3.zero;
                    vCallBack = null;
                }
            }
        }
        else if (colType == ECollision.WALL)
        {
            Vector3 lForward = playerAsset.transform.forward.normalized;
            float angle = Vector3.Angle(lForward, normalSlideVector);
            Vector3 slideDir = Vector3.RotateTowards(lForward, normalSlideVector, angle * Mathf.Deg2Rad, 0f);
            transform.position += ((lForward + slideDir).normalized * k * (Mathf.Clamp(angle, 0, 90f) / 90f)) * (Time.deltaTime / 3f);
            transform.position = SnapToPlanet(transform.position);
        }

        Reoriente();
		DoEmitter();

        UpdateCells(false);
    }

    public void UpdateCells(bool targetNPC)
    {
        Vector3 playerPos = _transform.position;
        _associateCell.UpdateProps();
        _associateCell.ShowBubble(playerPos);
        _associateCell.ShowHelp(playerPos);
        if (!targetNPC) ShowNPCState();
        else UIManager.instance.PNJState.Active(false);

        foreach (Cell c in _associateCell.Neighbors)
        {
            c.UpdateProps();
            c.ShowBubble(playerPos);
            c.ShowHelp(playerPos);
            foreach (Cell c2 in c.Neighbors) c2.ShowHelp(playerPos);
        }
    }

    public void ShowNPCState()
    {
        Vector3 pos = _transform.position;
        List<InteractablePNJ> pnjs = _associateCell.GetProps<InteractablePNJ>();
        InteractablePNJ tPnj = null;
        InteractablePNJ testPnj = null;
        float dist = 50f;
        if (pnjs.Count > 0)
        {
            tPnj = pnjs[0];
            dist = Mathf.Abs(Vector3.Distance(tPnj.transform.position, pos));
            for (int i = 1; i < pnjs.Count; i++)
            {
                testPnj = pnjs[i];
                float nDist = Vector3.Distance(testPnj.transform.position, pos);
                if (nDist < dist && nDist <= NPC_HELP_DIST)
                {
                    tPnj = testPnj;
                    dist = nDist;
                }
            }
        }

        foreach (Cell c in _associateCell.Neighbors)
        {
            List<InteractablePNJ> lpnjs = c.GetProps<InteractablePNJ>();
            if (lpnjs.Count > 0)
            {
                for (int i = 0; i < lpnjs.Count; i++)
                {
                    testPnj = lpnjs[i];
                    float nDist = Vector3.Distance(testPnj.transform.position, pos);
                    if (nDist < dist && nDist <= NPC_HELP_DIST)
                    {
                        tPnj = testPnj;
                        dist = nDist;
                    }
                }
            }
        }

        if (tPnj != null)
        {
            if (tPnj.budgetComponent == null) return;  
            if (tPnj.budgetComponent.type != EBudgetType.None && tPnj.budgetComponent.type != EBudgetType.CERN)
            {
                UIManager.instance.PNJState.pnj = tPnj;
                UIManager.instance.PNJState.SetTarget(PlayerManager.Instance.GetNearestNPCIcon());
                UIManager.instance.PNJState.SetVisibility(dist, NPC_HELP_DIST);
                UIManager.instance.PNJState.Active(true);
            }
            else
            {
                UIManager.instance.PNJState.Clear();
                UIManager.instance.PNJState.Active(false);
            }
        }
        else
        {
            UIManager.instance.PNJState.Clear();
            UIManager.instance.PNJState.Active(false);
        }
    }

    protected void Rotate()
	{
		Vector3 previousPos = transform.position;
		Vector2 mousePos = ControllerInput.instance.touchCenterPosition;
		Vector2 lPlayerPositionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
		Vector2 inputPos = mousePos - new Vector2(0, -lPlayerPositionOnScreen.x);

		float angle = Mathf.Atan2(inputPos.x, inputPos.y) * Mathf.Rad2Deg;
		playerAsset.transform.rotation = transform.rotation * Quaternion.Euler(0, angle, 0);
	}

	protected void StopMove(OnClickInteractable e)
	{
		StopAllCoroutines();
		_isHolding = false;
		moveHold = false;
        SetAnimation(false);
    }

	protected void StopMove(OnPopupBuilding e)
	{
		StopAllCoroutines();
		_isHolding = false;
		moveHold = false;
        SetAnimation(false);
    }

	protected void StopMovePickUp(OnPickUp e)
	{
		StopAllCoroutines();
		moveHold = false;
        SetAnimation(false);
        Reoriente();
    }

	protected void StopMoveFusion(OnFusion e)
	{
		StopAllCoroutines();
		moveHold = false;
	}

	protected void StopMove(OnFtueNextStep e)
	{
		StopAllCoroutines();
		moveHold = false;
        SetAnimation(false);
    }

    protected void StopMove()
    {
        StopAllCoroutines();
        _isHolding = false;
        moveHold = false;
        SetAnimation(false);
    }

    #region Tap

    protected void LaunchCoroutineTap(OnTap pEvent) {
		_targetInteractable = null;
        _tapPosition = pEvent.targetPos;

		if (_onUI || pEvent.playerType != playerType) return;

        if (GameManager.Instance.LoadedScene == SceneString.ZoomView) LaunchTap();
    }

	public void LaunchCoroutineTapNPC(OnTapNPC e)
	{
		if (_onUI || e.playerType != playerType) return;
		_isHolding = false;
		_targetInteractable = e.npc;
        LaunchTap();
    }

    protected void LaunchCoroutineTapItemPickUp(OnTapItemPickUp e) {
        if (_onUI || e.playerType != playerType) return;
        _isHolding = false;
        _targetItemPickUp = e.itemPickUp;
        LaunchTap();
    }

    protected void CancelCoroutineTap(OnHold pEvent) {
        _onUI = false;
        _targetInteractable = null;
        _targetItemPickUp = null;
        _tapPosition = Vector3.zero;
		_isHolding = true;
        moveHold = true;
	}

	protected void PermitMove(OnRemove pEvent) {
		_onUI = false;
        _isHolding = false;
        moveHold = false;
		_counterEmitter = 0;
    }

	protected void CancelMove() {
        StopAllCoroutines();
        _targetInteractable = null;
        _targetItemPickUp = null;
        _tapPosition = Vector3.zero;
        SetAnimation(false);
    }

    protected void CancelMove(OnOpenUI e)
    {
        StopAllCoroutines();
        _targetInteractable = null;
        _targetItemPickUp = null;
        _tapPosition = Vector3.zero;
        SetAnimation(false);
    }

    protected void OnInteraction(OnInteraction e) {
		if (GameManager.Instance.LoadedScene == SceneString.ZoomView) return;

		StopAllCoroutines();
		moveHold = false;
        SetAnimation(false);
    }

	protected void OnDesinteraction(OnDesinteraction e) {
        SetAnimation(false);
    }

	protected void LaunchTap()
	{
        _startPosition = _transform.position;
		InitCoroutineMove();
	}

	protected void InitCoroutineMove()
	{
		StopAllCoroutines();
        if (_targetInteractable != null) StartCoroutine(CoroutineRotation(_targetInteractable.transform.position));
        else if (_targetItemPickUp != null) StartCoroutine(CoroutineRotation(_targetItemPickUp.transform.position));
        else if (_tapPosition != Vector3.zero) StartCoroutine(CoroutineRotation(_tapPosition));
    }

    protected IEnumerator CoroutineRotation(Vector3 targetPos)
	{	
		Vector3 previousPos = transform.position;
		float stopDistance = 0.12f;
        if (_targetInteractable) stopDistance = 0.125f;
        if(_targetItemPickUp) stopDistance = 0.05f;

        SetAnimation(true);
        _isTapMoving = true;

        while (Vector3.Distance(transform.position, targetPos) > stopDistance)
		{
            Vector3 midPos = ((transform.position + targetPos) / 2f);
			Vector3 snapMidPos = midPos.normalized * transform.position.magnitude;
 
            ECollision lCollision = CanMoveForward(playerAsset.transform.forward);

            if (lCollision == ECollision.NONE) {
                previousPos = transform.position;
                transform.position += (snapMidPos - transform.position).normalized * Time.deltaTime / 3f;

                if (previousPos == transform.position) {
                    _isTapMoving = false;
                    break;
                }

				_targetRotation = Quaternion.LookRotation(transform.position - previousPos, transform.position);
				playerAsset.transform.rotation = Quaternion.Lerp(playerAsset.transform.rotation, _targetRotation, Time.deltaTime * 10);
				transform.position = SnapToPlanet(transform.position);
                Reoriente();
                UpdateCells(true);

                if (posCallBack != Vector3.zero)
                {
                    if (Vector3.Distance(transform.position, posCallBack) <= 0.2f)
                    {
                        vCallBack();
                        posCallBack = Vector3.zero;
                        vCallBack = null;
                    }
                }
            }
            else {
                _targetItemPickUp = null;
                _isTapMoving = false;
                StopMove();
                break;
            }

            DoEmitter();

            yield return null;
		}
			
		transform.position = SnapToPlanet(transform.position);
        SetAnimation(false);
        _isTapMoving = false;

        if (!_targetInteractable && !_targetItemPickUp)
        {
            yield break;
        }

        if (_targetItemPickUp) {
            if (PlayerManager.Instance.playerType != EPlayer.ECO) {
                CancelMove();
                yield break;
            }

            _targetItemPickUp.GetComponent<ItemPickup>().PickUp();
            StopMove();
            yield break;
        }

        if (Vector3.Distance(transform.position, targetPos) <= stopDistance)
        {
            InteractablePNJ lPnj = _targetInteractable as InteractablePNJ;
            NPCGender genderComponent = lPnj.GetComponent<NPCGender>();

            if (PlayerManager.Instance.playerType == EPlayer.NGO)
            {
                if (lPnj && lPnj.CanTalkTo(EPlayer.NGO))
                {
                    playerAsset.transform.rotation = Quaternion.LookRotation(lPnj.transform.position - transform.position, transform.up);
                    UIManager.instance.PNJState.Active(false);
                    PointingBubble.instance.Show(true);
                    PointingBubble.instance.SetProperties(lPnj);
                    QuestManager.Instance.NGOTalkTo(lPnj.IDname);
                    lPnj = null;
                    CancelMove();

                    if (genderComponent) Events.Instance.Raise(new OnNPCDialogueSFX(genderComponent.gender));
                } 
                else
                {
                    lPnj = null;
                    CancelMove();
                }
            }
            else if (lPnj && PlayerManager.Instance.playerType == EPlayer.GOV)
            {
                playerAsset.transform.rotation = Quaternion.LookRotation(lPnj.transform.position - transform.position, transform.up);
                if (lPnj.CanTalkTo(EPlayer.GOV)) Events.Instance.Raise(new OnPopupBuilding(lPnj.budgetComponent, lPnj));
                lPnj = null;
                CancelMove();

                if (genderComponent) Events.Instance.Raise(new OnNPCDialogueSFX(genderComponent.gender));
            }
            else if (lPnj && PlayerManager.Instance.playerType == EPlayer.ECO)
            {
                playerAsset.transform.rotation = Quaternion.LookRotation(lPnj.transform.position - transform.position, transform.up);
                if (lPnj.CanTalkTo(EPlayer.ECO))
                {
                    InventoryScreen.Instance.HandleActiveFromNPC(lPnj);
                    QuestManager.Instance.EcoTalkTo(lPnj.budgetComponent.type);
                }
                lPnj = null;
                CancelMove();

                if (genderComponent) Events.Instance.Raise(new OnNPCDialogueSFX(genderComponent.gender));
            }
        }
        else StopMove();

        yield break;
	}

	protected void DoEmitter() {
        _counterEmitter++;

        if (_counterEmitter >= _frameEmitter) {
            _counterEmitter = 0;
            _fmodEmitter.Play();
        }
    }

    #endregion

    #endregion

    void SetAnimation(bool pAnimation) {
        _animator.SetBool("Walk", pAnimation);
        dustPFB.SetActive(pAnimation);
    }

	protected void ReceivePosition(SharePlayerPosition e)
	{
		foreach (KeyValuePair<EPlayer, KeyValuePair<int, Vector3>> item in e.pos)
		{
			if (item.Key == playerType)
			{
				_associateCell = EarthManager.Instance.Cells.Find(cell => cell.ID == item.Value.Key);
				transform.position = item.Value.Value;
				Reoriente();

                Vector3 playerPos = _transform.position;
                _associateCell.ShowHelp(playerPos);
            }
		}
        Events.Instance.AddListener<OnSceneLoaded>(OnLoadScene);
    }

    private void OnLoadScene(OnSceneLoaded e)
    {
        Events.Instance.RemoveListener<OnSceneLoaded>(OnLoadScene);
        Events.Instance.Raise(new OnTransitionEnd());
    }

    protected void Reoriente()
	{
		Vector3 upVector = transform.position.normalized;
		Vector3 rightVector = Vector3.Cross(upVector, Vector3.up).normalized;
		Vector3 forwardVector = MathCustom.GetFaceNormalVector(transform.position, transform.position + rightVector, transform.position + upVector);
		_playerRotation = Quaternion.LookRotation(forwardVector, upVector);
		transform.rotation = _playerRotation;
	}
}