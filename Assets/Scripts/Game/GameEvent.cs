using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NGO;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent { }

public class PlayerEvent : GameEvent {
	public EPlayer playerType;
	public PlayerEvent() {
		if (PlayerManager.instance.player != null) playerType = PlayerManager.instance.player.GetComponent<Player>().playerType;
		else playerType = EPlayer.NONE;
	}
}

#region Ftue Events
public class OnEndFtuePinch : GameEvent { }
public class OnInputFtuePinch : GameEvent { }
public class OnFtueNextStep : GameEvent { }
public class OnValidFtueStep : GameEvent { }
#endregion

#region Dialogue Event
public class OnDialoguesLoaded : GameEvent { }
public class OnBudgetLoaded : GameEvent { }
public class OnDialogueInfo : GameEvent { }
public class OnTalkToNPC : GameEvent { }
public class OnPointGovTarget : GameEvent { }
public class OnClickSelectTopicGov : GameEvent { }
public class OnClickSelectTopicContractor : GameEvent {
	public ContractorTopic contractorTarget;
	public OnClickSelectTopicContractor(ContractorTopic pCont)
	{
		contractorTarget = pCont;
	}
}
public class OnSelectTopic : GameEvent {
	public NotepadTopic topicItem;
	public InteractablePNJ selectedNpc;
	public OnSelectTopic(NotepadTopic cItem, InteractablePNJ pNpc)
	{
		topicItem = cItem;
		selectedNpc = pNpc;
	}
}public class OnActiveSelectTopic : GameEvent {
	public NotepadTopic topicItem;
	public OnActiveSelectTopic(NotepadTopic cItem)
	{
		topicItem = cItem;
	}
}
public class OnStartSpeakingNPC : GameEvent { }
public class OnEndSpeakingNPC : GameEvent { }
public class OnSetNotepadTopic : GameEvent {
	public InteractablePNJ npc;
	public OnSetNotepadTopic(InteractablePNJ pNpc)
	{
		npc = pNpc;
	}
}
public class OnSetDialogueTarget : GameEvent
{
	public ContractorTopic topic;
	public OnSetDialogueTarget(ContractorTopic pTopic)
	{
		topic = pTopic;
	}
}
#endregion

public class OnEndTween : GameEvent { }

public class OnFocusPlayer : GameEvent { }

/* Controller Input */
public class OnTapStepFTUE : PlayerEvent {}

public class OnTap : PlayerEvent {
    public Vector3 targetPos;
    public OnTap(Vector3 pPos)
    {
        targetPos = pPos;
    }
}

public class OnTapNPC: PlayerEvent
{
    public InteractablePNJ npc;
    public OnTapNPC(InteractablePNJ pnpc) {
		npc = pnpc;
    }
}
public class OnTapItemPickUp : PlayerEvent {
    public GameObject itemPickUp;
    public OnTapItemPickUp(GameObject pItem) {
        itemPickUp = pItem;
    }
}

public class OnHold : PlayerEvent {
	public Vector2 touchPosition;
    public OnHold(Vector2 touchPos) {
        touchPosition = touchPos;
    }
}
public class OnRemove : GameEvent { }

public class TimeEvent : GameEvent { }
public class OnNewYear : TimeEvent {
	public int Year;
	public OnNewYear(int pYear) {
		Year = pYear;
	}
}

public class OnNewMonth : TimeEvent
{
	public int Month;
	public OnNewMonth(int pMonth)
	{
		Month = pMonth;
	}
}

public class OnNewWeek : TimeEvent
{
	public int Week;
	public OnNewWeek(int pWeek)
	{
		Week = pWeek;
	}
}

public class OnUpdateForest : GameEvent { }
public class OnUpdateGround : GameEvent { }

/* PlayerManager */
public class SelectPlayer : GameEvent
{
	public Player player;
	public SelectPlayer(Player cPlayer)
	{
		player = cPlayer;
	}
}

public class OnChangeGauges : GameEvent {}

/* PlayerCell */
public class OnGrass : GameEvent { }
public class OnTown : GameEvent { }
public class OnCoast : GameEvent { }
public class OnMoss : GameEvent { }
public class OnMountain : GameEvent { }
public class OnDesert : GameEvent { }

/* Camera Componenet */
public class LerpEnd : GameEvent { }
public class ZoomEnd : GameEvent { }
public class ZoomEndUI : GameEvent { }

/* Panel Transition */
public class PanelLerpEnd : GameEvent { }

public class OnPinchEnd : GameEvent { }
public class OnPinch : GameEvent
{
	public float value;
	public OnPinch(float cValue)
	{
		value = cValue;
	}
}

public class OnChangeQuality : GameEvent { }

public class OnPinchBudget : GameEvent
{
	public float value;
	public OnPinchBudget(float cValue)
	{
		value = cValue;
	}
}

/*Interactable Manager */
public class OnInteraction : GameEvent { }
public class OnDesinteraction: GameEvent { }
public class OnTransition : GameEvent { }
public class OnAction : GameEvent { }
public class OnFinishHold : GameEvent { }

/* Interaction Event */
public class OnPickUp : GameEvent { }
public class OnTransform : GameEvent { }
public class OnFusion : GameEvent { }
//public class OnGive : GameEvent { }
public class OnDrop : GameEvent { }

/*UI sounds events */
public class OnClickGlossary : GameEvent { }
public class OnSwipe : GameEvent { }
public class OnClickTransform : GameEvent { }
public class OnTransformReward : GameEvent { }
public class OnTransformRewardNew : GameEvent { }
public class OnNewObject : GameEvent { }
public class OnWrongObject : GameEvent { }
public class OnGoodObject : GameEvent { }
public class OnPopUp : GameEvent { }
public class OnNPCDialogueSFX : GameEvent {
    public bool gender;
    public OnNPCDialogueSFX(bool pGender) {
        gender = pGender;
    }
}

/* Transition SFX */
public class CloudIn : GameEvent { }
public class CloudOut : GameEvent { }

/*Music Event*/
public class OnMusicBeta : GameEvent { }

public class OnClearInventory : GameEvent { }
public class OnUpdateInventory : GameEvent { }

/*Interaction NPC */
public class OnOpenNPCScreen : GameEvent {
    public InteractablePNJ NPC;

    public OnOpenNPCScreen(InteractablePNJ pNPC) {
        NPC = pNPC;
    }
}

public class OnEndTransformation : GameEvent
{
    public int index;
    public Item item;

    public OnEndTransformation(int pIndex, Item pItem)
    {
        index = pIndex;
        item = pItem;
    }
}

public class OnTransformation : GameEvent {
    public int index;
    public Item item;

    public OnTransformation(int pIndex, Item pItem) {
        index = pIndex;
        item = pItem;
    }
}

public class OnGive : GameEvent {
    public int index;

    public OnGive(int pIndex) {
        index = pIndex;
    }
}

public class OnGiveNPC : GameEvent {
    public Item item;
    public InteractablePNJ targetNPC;

    public OnGiveNPC(Item pItem, InteractablePNJ npc) {
        item = pItem;
        targetNPC = npc;
    }
}

/*InventoryPlayer*/
public class OnFinishDrag : GameEvent {
    public GameObject objectToDrag;
    public GameObject objectToReplace;

    public OnFinishDrag(GameObject pToDrag, GameObject pToReplace) {
        objectToDrag = pToDrag;
        objectToReplace = pToReplace;
    }
}

public class OnPopupObjects : GameEvent {
    public GameObject npc;

    public OnPopupObjects(GameObject pNPC) {
        npc = pNPC;
    }
}

public class OnNotifications : GameEvent {
    public EnumClass.NotificationsType notificationType;
    public bool isDisplay;

    public OnNotifications(EnumClass.NotificationsType pType, bool pDisplay) {
        notificationType = pType;
        isDisplay = pDisplay;   
    }
}

public class OnUpdateObject : GameEvent {
    public ItemPickUp itemWorn;

    public OnUpdateObject(ItemPickUp pItemWorn) {
        itemWorn = pItemWorn;
    }
}

//Event when the player is clicking on an interactable object
public class OnClickInteractable : GameEvent {
    public string type;
    public GameObject interactableObject;

    public OnClickInteractable(string pType, GameObject pObject = null) {
        type = pType;
        interactableObject = pObject;
    }
}

//Event when the Vehicles are clean
public class OnCleanVehicles : GameEvent {
    public string companyName;
    public OnCleanVehicles(string cName)
    {
        companyName = cName;
    }
}
public class OnPropsLinkCreation : GameEvent
{
	public BuildingLink buildingStruct;
	public LinkObject obj;

	public OnPropsLinkCreation(BuildingLink pBuilding, LinkObject cObj)
	{
		buildingStruct = pBuilding;
		obj = cObj;
	}
}

/* Dialogue Manager (in ControllerInput) */
public class OnPopupBuilding : GameEvent {
    public BudgetComponent buildingbudget;
    public InteractablePNJ npc;
    public OnPopupBuilding(BudgetComponent pBuildingbudget, InteractablePNJ pnpc) {
		buildingbudget = pBuildingbudget;
		npc = pnpc;
    }
}

public class OnPopupDialogue : GameEvent {
    public TextAsset textFile;
    public string textString;

    public OnPopupDialogue(TextAsset pTextFile) {
        textFile = pTextFile;
    }

    public OnPopupDialogue(string pString) {
        textString = pString;
    }
}
public class OutPopupDialogue : GameEvent { }

#region Budget events
public class BudgetEvent : GameEvent
{
    public BudgetComponent comp;
    public BudgetEvent Init(BudgetComponent icomp)
    {
        comp = icomp;
        return this;
    }
}
public class OnReceiveBudget : BudgetEvent { }
public class OnGiveBudget : BudgetEvent { }
#endregion

public class OnFocusButton : GameEvent { }
public class OnSwitchScene : GameEvent {
	public SceneString previousScene;
	public OnSwitchScene(SceneString cScene)
	{
		previousScene = cScene;
	}
}

public class OnEditionMouseScroll : GameEvent {
	public float value;
	public OnEditionMouseScroll(float cVal) {
		value = cVal;
	}
}
public class OnEditionRightClick : GameEvent {
	public float posX;
	public float posY;
	public OnEditionRightClick(float cX, float cY) {
		posX = cX;
		posY = cY;
	}
}
public class OnEditionLeftClick : GameEvent
{
	public float posX;
	public float posY;
	public OnEditionLeftClick(float cX, float cY)
	{
		posX = cX;
		posY = cY;
	}
}
public class OnEdtionInputKeyEvent : GameEvent {
	public KeyCode key;
	public OnEdtionInputKeyEvent(KeyCode cKey) {
		key = cKey;
	}
}

public class SharePlayerPosition : GameEvent
{
	public Dictionary<EPlayer, KeyValuePair<int, Vector3>> pos;
	public SharePlayerPosition(Dictionary<EPlayer, KeyValuePair<int, Vector3>> cPos) {
		pos = cPos;
	}
}

/*Planet Creation */
public class OnEndPlanetCreation : GameEvent { }
public class OnPlayerInitFinish : GameEvent { }

public class OnApplyCullingDistance : GameEvent { }

/* camera transition */
public class OnZoomFinish : GameEvent {
	public ECameraTargetType view;
	public OnZoomFinish(ECameraTargetType cView)
	{
		view = cView;
	}
}

public class OnPanelBudget : GameEvent
{
	public BudgetComponent budget;
	public OnPanelBudget(BudgetComponent bComp)
	{
		budget = bComp;
	}
}

public class OnWaterPolutionChange : GameEvent { }

public class OnSceneLoaded : GameEvent {
	public SceneString scene;
	public OnSceneLoaded(SceneString cScene)
	{
		scene = cScene;
	}
}

public class OnMidTransition : GameEvent { }

public class OnOpenPanel : GameEvent { }
public class OnBudgetEndTransition : GameEvent { }
public class OnBudgetTransition : GameEvent
{
	public BudgetComponent budget;
	public OnBudgetTransition(BudgetComponent bComp)
	{
		budget = bComp;
	}
}

public class OnBudgetBoosted : GameEvent
{
    public string name;
    public OnBudgetBoosted(string budgetName)
    {
        name = budgetName;
    }
}

public class OnUpdateMainMenu : GameEvent { }
public class OnChangeLanguageUI : GameEvent { }
public class OnGoToMenu : GameEvent { }

public class PartyLoaded : GameEvent {
    public bool loaded;
    public PartyLoaded(bool iLoaded)
    {
        loaded = iLoaded;
    }
}

public class OnSelectQuest : GameEvent
{
    public Quest quest;
    public OnSelectQuest(Quest iQuest)
    {
        quest = iQuest;
    }
}

public class OnShowPin : GameEvent
{
    public EPin targetPin;
    public bool targetState;
    public OnShowPin(EPin iPin, bool iState)
    {
        targetPin = iPin;
        targetState = iState;
    }
}

public class OnSaveRoads : GameEvent { }

public class Events
{
	static Events instanceInternal = null;
	public static Events Instance
	{
		get
		{
			if (instanceInternal == null)
			{
				instanceInternal = new Events();
			}
			
			return instanceInternal;
		}
	}
	
	public delegate void EventDelegate<T> (T e) where T : GameEvent;
	private delegate void EventDelegate (GameEvent e);
	
	private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
	private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();
	
	public void AddListener<T> (EventDelegate<T> del) where T : GameEvent
	{	
		if (delegateLookup.ContainsKey(del))
			return;

		// Create a new non-generic delegate which calls our generic one.
		// This is the delegate we actually invoke.
		// search "Expressions lambda (Guide de programmation C#)" for details
		EventDelegate internalDelegate = (e) => del((T)e);
		delegateLookup[del] = internalDelegate;
		
		EventDelegate tempDel;
		if (delegates.TryGetValue(typeof(T), out tempDel))
		{
			delegates[typeof(T)] = tempDel += internalDelegate; 
		}
		else
		{
			delegates[typeof(T)] = internalDelegate;
		}
	}
	
	public void RemoveListener<T> (EventDelegate<T> del) where T : GameEvent
	{
		EventDelegate internalDelegate;
		if (delegateLookup.TryGetValue(del, out internalDelegate))
		{
			EventDelegate tempDel;
			if (delegates.TryGetValue(typeof(T), out tempDel))
			{
				tempDel -= internalDelegate;
				if (tempDel == null)
				{
					delegates.Remove(typeof(T));
				}
				else
				{
					delegates[typeof(T)] = tempDel;
				}
			}
			
			delegateLookup.Remove(del);
		}
	}
	
	public void Raise (GameEvent e)
	{
		EventDelegate del;
		if (delegates.TryGetValue(e.GetType(), out del))
		{
			del.Invoke(e);
		}
	}
}