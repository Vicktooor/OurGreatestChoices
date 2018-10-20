using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

public class InteractableManager : MonoBehaviour {

    #region Public Variables
    [HideInInspector]
    public List<Interactable> itemsList;

    /*public GameObject objectSelected {
        get { return _objectSelected; }
    }*/

    //Manager's state verifications
    public bool isInteract { get { return _state == STATE.interaction; } }
    public bool isNormal { get { return _state == STATE.normal; } }
    public bool isTransition { get { return _state == STATE.transition; } }
    public bool isActionMode { get { return _state == STATE.action; } }

    //Type of interactions
    public string PICK_UP_TYPE = "pickUp";          //Click on a pickUp object
    public string WORN_TYPE = "worn";               //Click on the worn object
    public string FINISH_TYPE = "finish";           //Event when there isn't any more selected object
    public string CANCEL_TYPE = "cancel";           //Re-click on the worn object to cancel Interact Mode
    public string TRANSITION_TYPE = "transition";   //Click on a object on Interaction Mode
    public string ACTION_TYPE = "action";           //When the player can start the action with an object
    #endregion

    #region Private Variables
    //Variables of the selected object
    private bool _canTake = false;
    private GameObject _objectSelected = null;

    //Variables to recreate a primary pickUp
    private GameObject _prefab;
    private Vector3 _position;
    private Cell _parentCell;

    //State's manager
    private enum STATE { interaction, normal, transition, action };
    private STATE _state;
    #endregion

    #region Singleton
    public static InteractableManager instance;

    private void Awake() {

        if (instance != null) {
            Debug.LogWarning("more than one instance");
        }

        instance = this;
        itemsList = new List<Interactable>();
    }
    #endregion

    void Start() {
        _state = STATE.normal;

        Events.Instance.AddListener<OnClickInteractable>(InteractionDatas);
    }

    void Update()
    {
        //print(_objectSelected);
    }

    //Verification when player wants to take an object on ground
    public bool canTake(GameObject pObject) {
        if (_canTake && pObject == _objectSelected && isNormal) return true;

        if (pObject == _objectSelected && (isActionMode || isTransition)) return true;

        return false;
    }

    //Depending the interact type, keep variables in memory and send events
    void InteractionDatas(OnClickInteractable e) {
        if (e.type == PICK_UP_TYPE) {
            if (e.interactableObject == null) {
                Debug.Log("NO OBJECT SEND");
                return;
            }
            _canTake = true;
            _objectSelected = e.interactableObject;
        }

        else if (e.type == FINISH_TYPE) {
            _canTake = false;
            _objectSelected = null;

            if (isActionMode) OnDesinteract();
        }

        else if (e.type == TRANSITION_TYPE) {

            if (e.interactableObject == null) {
                Debug.Log("NO OBJECT SEND");
                return;
            }

            _objectSelected = e.interactableObject;
            OnTransition();
        }

        else if (e.type == ACTION_TYPE) {
            OnAction();
        }

        else {
            return;
        }

    }

    public void OnInteract() {
        _state = STATE.interaction;
        Events.Instance.Raise(new OnInteraction());
    }

    public void OnDesinteract() {
        _state = STATE.normal;
        Events.Instance.Raise(new OnDesinteraction());
    }

    public void OnTransition() {
        _state = STATE.transition;
        Events.Instance.Raise(new OnTransition());
    }

    public void OnAction() {
        _state = STATE.action;
        Events.Instance.Raise(new OnAction());
    }

    private void OnDestroy() {
        _canTake = false;
        _objectSelected = null;
        _prefab = null;
        _parentCell = null;
        _state = STATE.normal;
        _position = Vector3.zero;
    }
}
