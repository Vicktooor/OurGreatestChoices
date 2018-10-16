using Assets.Scripts.Game.NGO;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Manager;
using Assets.Scripts.PNJ;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EThanksKey { None, Carcass, Battery, GreenBattery, Tracks, FruitSeed, FruitMarker, VegetableGarden, Electricity, GreenElectricity, NeedBudget, WindTurbine }

[Serializable]
public struct ThanksStruct
{
    public EThanksKey key;
    public List<SimpleLocalisationText> text;
}

public class InteractablePNJ : Interactable
{
    public static float helpDistance = 0.9f;

    [HideInInspector]
    public static List<Type> SPEAKABLE_TYPES = new List<Type>()
    {
        typeof(InteractablePNJ),
        typeof(InteractablePNJ_CarsCompany),
        typeof(InteractablePNJ_CERN ),
        typeof(InteractablePNJ_CoalPower),
        typeof(InteractablePNJ_WaterDiversion),
        typeof(InteractablePNJ_WindTurbine),
        typeof(InteractablePNJ_TownHall),
    };

	public static List<InteractablePNJ> PNJs = new List<InteractablePNJ>();

    [Header("Budget Component")]
	public BudgetComponent budgetComponent;
	[Header("Presentation text")]
	public List<SimpleLocalisationText> presentationTexts;
	[Header("Blocnote topics")]
	public List<GovernmentTopic> govTopics;
	public List<ContractorTopic> contTopics;
	[Header("End text")]
	public List<SimpleLocalisationText> leavingTexts;
    [Header("Thanks text")]
    public List<ThanksStruct> thanksTexts = new List<ThanksStruct>();

    [Header("Mood State")]
    protected GameObject _moodState;

	[Header("UI Head sprite")]
	public Sprite pictoHead;

	protected override void Awake()
	{
		base.Awake();
		PNJs.Add(this);
		Events.Instance.AddListener<OnDialoguesLoaded>(LoadDialogues);
		Events.Instance.AddListener<OnBudgetLoaded>(OnBudgetLoaded);
		Events.Instance.AddListener<OnSelectTopic>(SelectTopic);
        Events.Instance.AddListener<OnGiveNPC>(TakeItem);
        Events.Instance.AddListener<OnNewMonth>(OnUpdate);
    }

    protected override void OnEnable() {
        base.OnEnable();
        Events.Instance.AddListener<OnReceiveBudget>(OnReceiveBudget);
        Events.Instance.AddListener<OnGiveBudget>(OnSendBudget);    }

	protected override void OnDisable() {
        base.OnDisable();
        Events.Instance.RemoveListener<OnReceiveBudget>(OnReceiveBudget);
        Events.Instance.RemoveListener<OnGiveBudget>(OnSendBudget);
    }

    public virtual bool HaveHisItem()
    {
        return false;
    }

    /*public override void Update() {
        if (GameManager.Instance.LoadedScene == SceneString.ZoomView)  CheckDistance();
    }*/
    /*protected virtual void CheckDistance() {

        GameObject lPlayer = PlayerManager.instance.player;

        float distance = Vector3.Distance(lPlayer.transform.position, transform.position);
        if (distance <= radius) {

            //A METTRE DANS PNJ/PLAYER
            if (InteractableManager.instance.isTransition) {
				print(gameObject.name);
                if (InteractableManager.instance.canTake(gameObject)) {
                    Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.ACTION_TYPE));
                }
            }
        }
    }*/

    public void LoadDialogues(OnDialoguesLoaded e)
	{
		Events.Instance.RemoveListener<OnDialoguesLoaded>(LoadDialogues);
		foreach (SaveDialogueNPC itemDial in PlanetSave.PNJDialogues)
		{
            Vector3 testPos = transform.position;
            Vector3 pos = new Vector3(itemDial.x, itemDial.y, itemDial.z); 
            if (pos.normalized == testPos.normalized)
			{
				List<SimpleLocalisationText> prezTxts = new List<SimpleLocalisationText>();
				List<SimpleLocalisationText> leaveTxts = new List<SimpleLocalisationText>();
				for (int i = 0; i < itemDial.presentationTxts.Length; i++) prezTxts.Add(itemDial.presentationTxts[i]);
				for (int i = 0; i < itemDial.leavingTxts.Length; i++) leaveTxts.Add(itemDial.leavingTxts[i]);
				presentationTexts = prezTxts;
				leavingTexts = leaveTxts;

				List<GovernmentTopic> gList = new List<GovernmentTopic>();
				foreach (string gtName in itemDial.govTopics) gList.Add(Resources.Load<GovernmentTopic>("Topics/" + gtName));
				govTopics = gList;

				List<ContractorTopic> cList = new List<ContractorTopic>();
				foreach (string ctName in itemDial.contTopics)
				{
					ContractorTopic nTopic = Resources.Load<ContractorTopic>("Topics/" + ctName);
					cList.Add(nTopic);
					Events.Instance.Raise(new OnSetDialogueTarget(nTopic));
				}
				contTopics = cList;
			}
		}		
	}

	public virtual void OnBudgetLoaded(OnBudgetLoaded e)
	{
		Events.Instance.RemoveListener<OnBudgetLoaded>(OnBudgetLoaded);
		foreach (SaveBudgetComponent bc in PlanetSave.BudgetElements)
		{
            Vector3 testPos = transform.position;
            Vector3 pos = new Vector3(bc.x, bc.y, bc.z);
            if (pos.normalized == testPos.normalized)
			{
				budgetComponent = bc.budgetComp;
                budgetComponent.SetWorking();
			}
		}
	}

    public virtual void OnUpdate(OnNewMonth e)
    {
        if (budgetComponent.budget < budgetComponent.targetBudget)
        {
            DisplayMood(true);
        }
        if (budgetComponent.budget >= budgetComponent.targetBudget)
        {
            DisplayMood(false);
        }
    }

    public virtual void OnSendBudget(OnGiveBudget e)
    {
        if (budgetComponent.name != e.comp.name) return;
        if (budgetComponent.budget < budgetComponent.targetBudget)
        {
            DisplayMood(true);
        }
        if (budgetComponent.budget >= budgetComponent.targetBudget)
        {
            DisplayMood(false);
        }
    }

    public virtual void OnReceiveBudget(OnReceiveBudget e) {
        if (budgetComponent.name != e.comp.name) return;
        if (budgetComponent.budget < budgetComponent.targetBudget) {
            DisplayMood(true);
        }
        if (budgetComponent.budget >= budgetComponent.targetBudget) {
            DisplayMood(false);
        }
    }

    protected virtual void DisplayMood(bool pMood) {
        if(_moodState != null) _moodState.SetActive(pMood);
    }

    public override void TransitionMode(OnTransition e) {
        base.TransitionMode(e);
       // _renderer.material.color = _colorDefault;
    }

    public bool CanAccept(Item item) {
        return CheckIfItemIsContained(item);
    }

    protected bool CheckIfItemIsContained(Item pItem) {
        for (int i = 0; i < item.itemsLinked.Count; i++) {
            if (item.itemsLinked[i].name == pItem.name) {
                return true;
            }
        }
        return false;
    }

    public virtual void TakeItem(OnGiveNPC e) {
        if (e.targetNPC != this) return;
    }

    int GetKey(Item pItem) {
        for (int i = 0; i < item.itemsLinked.Count; i++) {
            if (item.itemsLinked[i] == pItem) return i;
        }

        return 0;
    }

	protected void SelectTopic(OnSelectTopic e)
	{
		if (e.selectedNpc != this) return;
		foreach (ContractorTopic ct in contTopics)
		{
			if (e.topicItem.topicType == ct.GetType())
			{
				Events.Instance.Raise(new OnClickSelectTopicContractor(ct));
				return;
			}
		}

		foreach (GovernmentTopic gt in govTopics)
		{
			if (e.topicItem.topicType == gt.GetType())
			{
				Events.Instance.Raise(new OnClickSelectTopicGov());
				return;
			}
		}
	}

    protected void InstantiateFeedback(bool pState, GameObject targetObj)
    {
        StartCoroutine(UpgradeCoroutine(pState, targetObj));
    }

    protected IEnumerator UpgradeCoroutine(bool up, GameObject targetObj)
    {
        GameObject lObj = (up) ? Instantiate(ParticleDatabase.Instance.upFeedback) : Instantiate(ParticleDatabase.Instance.downFeedback);
        lObj.transform.position = targetObj.transform.position;
        lObj.transform.rotation = targetObj.transform.rotation;

        Debug.LogError(lObj.name);

        ParticleSystem emitPs = lObj.GetComponent<ParticleSystem>();

        while (emitPs.IsAlive())
        {
            yield return null;
        }
        DestroyObject(emitPs.gameObject);
    }

    public static string GetThanksLocalizedText(List<ThanksStruct> list, EThanksKey type)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = 0; j < list[i].text.Count; j++)
            {
                if (list[i].text[j].lang == GameManager.LANGUAGE)
                {
                    return list[i].text[j].text;
                }
            } 
        }
        return string.Empty;
    }

    protected override void OnDestroy()
    {
        Events.Instance.RemoveListener<OnNewMonth>(OnUpdate);
        Events.Instance.RemoveListener<OnDialoguesLoaded>(LoadDialogues);
        Events.Instance.RemoveListener<OnBudgetLoaded>(OnBudgetLoaded);
        Events.Instance.RemoveListener<OnSelectTopic>(SelectTopic);
        Events.Instance.RemoveListener<OnGiveNPC>(TakeItem);
        base.OnDestroy();
    }
}
