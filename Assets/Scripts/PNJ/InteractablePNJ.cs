using Assets.Scripts.Game.NGO;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ThanksText
{
    public string itemType;
    public string haveBudgetText;
    public string haveItemText;
    public string fullThanks;
}

[Serializable]
public class DialoguePNJ
{
    public string introText;
    public string outroText;
    public List<int> topicIDs;
    public List<ThanksText> thanks;
}

[Serializable]
public class DialogueWrap
{
    public string key;
    public DialoguePNJ value;
}

[Serializable]
public class DialogueWrapper
{
    public List<DialogueWrap> objects;
}

public class InteractablePNJ : Interactable
{
    public static Dictionary<string, DialoguePNJ> DialoguesDatabase;

    public static List<InteractablePNJ> PNJs = new List<InteractablePNJ>();
    public static float helpDistance = 0.9f;

    public string IDname = string.Empty;
    private string dialogueKey;

    [Header("Budget Component")]
	public BudgetComponent budgetComponent;

    [Header("Mood State")]
    protected GameObject _moodState;

	[Header("UI Head sprite")]
	public Sprite pictoHead;

    private NPCWrap _txtInfo;
    public NPCWrap TxtInfo { get { return _txtInfo; } }

    [HideInInspector]
    public List<EItemType> neededItems = new List<EItemType>();

    public bool HaveBudget(EWorldImpactType target) 
    {
        if (ResourcesManager.Instance.BudgetValues.ContainsKey(target))
        {
            return budgetComponent.budget >= ResourcesManager.Instance.BudgetValues[target].targetBudget;
        }
        else return false;
    }

    public bool HaveRequiredItem(EWorldImpactType target)
    {
        if (ResourcesManager.Instance.BudgetValues.ContainsKey(target))
        {
            return budgetComponent.budget >= ResourcesManager.Instance.BudgetValues[target].targetBudget;
        }
        else return false;
    }

    virtual public bool HaveBudget(EItemType target)
    {
        return false;
    }

    protected override void Awake()
	{
		base.Awake();        
		PNJs.Add(this);
        Events.Instance.AddListener<OnNewMonth>(OnUpdate);
    }

    public override void Init()
    {
        PositionKey found = PlanetSave.PNJs.Find(p => new Vector3(p.x, p.y, p.z) == transform.position);
        IDname = found.key;

        NPCWrap npcText = ResourcesManager.Instance.NPCs.objects.Find(npc => npc.ID == IDname);
        if (npcText != null) _txtInfo = npcText;

        budgetComponent = new BudgetComponent(IDname);
        if (GameManager.PARTY_TYPE == EPartyType.SAVE)
        {
            List<BudgetsSave> saves;
            ArrayExtensions.ToList(PlanetSave.GameStateSave.Budgets, out saves);
            BudgetsSave save = saves.Find(s => s.npcName == IDname);
            if (save != null)
            {
                budgetComponent.budget = save.budget;
                budgetComponent.Investment = save.investment;
            }
        }
        if (InventoryPlayer.Instance) CatchGivedObject();
    }

    protected virtual void CatchGivedObject() { }

    protected override void OnEnable() {
        base.OnEnable();
    }

	protected override void OnDisable() {
        base.OnDisable();
    }

    public virtual bool HaveItem(EItemType itemType)
    {
        return false;
    }

    public virtual void OnUpdate(OnNewMonth e)
    {
        if (budgetComponent != null)
        {
            if (budgetComponent.Working)
            {
                DisplayMood(true);
            }
            else DisplayMood(false);
        }
    }

    public virtual void SendBudget()
    {
        if (budgetComponent.Working)
        {
            DisplayMood(true);
        }
        else
        {
            DisplayMood(false);
        }
    }

    public virtual void ReceiveBudget()
    {
        if (budgetComponent.Working)
        {
            DisplayMood(true);
        }
        else
        {
            DisplayMood(false);
        }
    }

    protected virtual void DisplayMood(bool pMood) {
        if(_moodState != null) _moodState.SetActive(pMood);
    }

    public override void TransitionMode(OnTransition e) {
        base.TransitionMode(e);
    }

    virtual public bool CanAccept(Item item) {
        return false;
    }

    public virtual void ReceiveItem(EItemType itemType) {
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

    public static List<BudgetsSave> GenerateSave()
    {
        List<BudgetsSave> newSave = new List<BudgetsSave>();
        foreach (InteractablePNJ pnj in PNJs)
        {
            if (pnj.budgetComponent != null)
            {
                BudgetsSave save = pnj.budgetComponent.GenerateSave();
                save.npcName = pnj.IDname;
                newSave.Add(save);
            }
        }
        return newSave;
    }

    public bool CanTalkTo(EPlayer playerType)
    {
        if (FtueManager.instance.active)
        {
            if (playerType == EPlayer.ECO)
            {
                if (FtueManager.instance.currentStep.transformTarget != EPlayer.NONE)
                {
                    if (FtueManager.instance.currentStep.transformTarget == item.type) return true;
                    else return false;
                }
                else return false;
            }
            else if (playerType == EPlayer.GOV || playerType == EPlayer.NGO)
            {
                if (FtueManager.instance.currentStep.targetNPCIcon) return true;
                else return false;
            }
        }

        if (playerType == EPlayer.GOV)
        {
            if (budgetComponent.initialBudget > 0) return true;
            return false;
        }
        else if (playerType == EPlayer.NGO)
        {
            if (_txtInfo != null)
            {
                if (DialoguesDatabase.ContainsKey(_txtInfo.NPCText))
                {
                    if (DialoguesDatabase[_txtInfo.NPCText].topicIDs.Count > 0) return true;
                }
                else return false;
            }
            return false;
        }
        return true;
    }

    public virtual void ShowThanks(EItemType targetType, bool haveBudget, bool haveItem)
    {
        NPCWrap dialKey =  ResourcesManager.Instance.NPCs.objects.Find(e => e.ID == IDname);
        if (dialKey != null)
        {
            ThanksText txts = DialoguesDatabase[dialKey.NPCText].thanks.Find(t => t.itemType == targetType.ToString());
            if (txts != null)
            {
                if (haveBudget && haveItem) PointingBubble.instance.PNJThanks(this, txts.fullThanks);
                else if (haveBudget) PointingBubble.instance.PNJThanks(this, txts.haveBudgetText);
                else if (haveItem) PointingBubble.instance.PNJThanks(this, txts.haveItemText);
            }
        }
    }

    virtual public EWorldImpactType GetBudgetEnum(EItemType itemType)
    {
        return EWorldImpactType.None;
    }

    protected override void OnDestroy()
    {
        Events.Instance.RemoveListener<OnNewMonth>(OnUpdate);
        base.OnDestroy();
    }
}
