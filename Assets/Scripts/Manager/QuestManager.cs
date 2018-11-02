using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Utils;
using Assets.Scripts.Game.UI.Ftue;

[Serializable]
public class Quest
{
    public int ID = -1;
    public string NGOtarget;
    public EItemType itemType;
    public EBudgetType[] activitiesName;
    public EBudgetType selectedActivity;
    public int unlockQuestID;
    public int step;
    public bool validated;
}

public class QuestManager : MonoBehaviour
{
    private static string ArrowDisplayerName = "Quest";
    public static List<Quest> Quests = new List<Quest>();

    #region Instance
    protected static QuestManager _instance;
    public static QuestManager Instance
    {
        get { return _instance; }
    }
    #endregion

    private UIQuest[] _questList;
    private Quest _runningQuest;

    public UIObjectPointIcon arrowModel;
    public UIFtueVisual pinchSprite;
    public Transform container;
    public Sprite NGOSprite;
    public Sprite EntrepreneurSprite;

    protected ECameraTargetType view;

    protected void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            throw new Exception("An instance of QuestManager already exists.");
        }
        else _instance = this;

        Events.Instance.AddListener<OnSwitchScene>(OnChangeScene);
        view = ECameraTargetType.MAP;

        ArrowDisplayer.Instances(ArrowDisplayerName).SetContainer(container as RectTransform);
        ArrowDisplayer.Instances(ArrowDisplayerName).SetModels(null, arrowModel);

        _questList = GetComponentsInChildren<UIQuest>(true);
        for (int i = 0; i < _questList.Length; i++)
        {
            _questList[i].Init();
            Quests.Add(_questList[i].quest);
        }

        gameObject.SetActive(false);
    }

    protected void OnEnable()
    {
        CheckValidation();
    }

    public void SelectQuest(Quest selectedQuest)
    {
        CheckStep(selectedQuest);

        if (QuestRunning(selectedQuest)) return;
        else
        {
            _runningQuest = selectedQuest;
            Events.Instance.Raise(new OnSelectQuest(_runningQuest));
            DisplayQuest();
        }
    }

    protected void CheckStep(Quest selectedQuest)
    {
        int pnjCount = InteractablePNJ.PNJs.Count;
        for (int i = 0; i < selectedQuest.activitiesName.Length; i++)
        {
            for (int j = 0; j < pnjCount; j++)
            {
                InteractablePNJ pnj = InteractablePNJ.PNJs[j];
                if (pnj.budgetComponent == null) continue;
                if (pnj.budgetComponent.type == selectedQuest.activitiesName[i])
                {
                    InteractablePNJ_TownHall major = pnj as InteractablePNJ_TownHall;
                    InteractablePNJ_CoalPower coalPNJ = pnj as InteractablePNJ_CoalPower;
                    if (major != null)
                    {
                        if (selectedQuest.itemType == EItemType.FruitSeed)
                        {
                            if (major.HasFruitSeed())
                            {
                                ValidQuest(selectedQuest);
                                break;
                            }
                        }
                        else if (selectedQuest.itemType == EItemType.Tracks)
                        {
                            if (major.HasMetro())
                            {
                                ValidQuest(selectedQuest);
                                break;
                            }
                        }
                        else if (selectedQuest.itemType == EItemType.FruitMarket)
                        {
                            if (major.HasFruitBasket())
                            {
                                ValidQuest(selectedQuest);
                                break;
                            }
                        }
                    }
                    else if (coalPNJ != null)
                    {
                        if (selectedQuest.itemType == EItemType.WindTurbine)
                        {
                            if (coalPNJ.IsUpdated())
                            {
                                ValidQuest(selectedQuest);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (selectedQuest.itemType == EItemType.Carcass)
                        {
                            if (pnj.HaveHisItem())
                            {
                                ValidQuest(selectedQuest);
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (selectedQuest.validated)
        {
            UpdateQuestList();
            Events.Instance.Raise(new OnShowPin(EPin.Quest, true));
        }
    }

    protected void UpdateQuestList()
    {
        for (int i = 0; i < _questList.Length; i++) _questList[i].UpdateState();
    }

    public void DisplayQuest()
    {
        ArrowDisplayer.Instances(ArrowDisplayerName).CleanArrows();
        pinchSprite.gameObject.SetActive(false);

        if (_runningQuest == null || _runningQuest.validated) return;

        bool hasItem = InventoryPlayer.Instance.ContainItem(_runningQuest.itemType) != null;

        if (_runningQuest.step < 1 && !hasItem)
        {
            int pnjCount = InteractablePNJ.PNJs.Count;
            for (int i = 0; i < pnjCount; i++)
            {
                InteractablePNJ pnj = InteractablePNJ.PNJs[i];
                if (pnj.IDname == _runningQuest.NGOtarget)
                {
                    UIObjectPointIcon pointer = ArrowDisplayer.Instances(ArrowDisplayerName).UseArrow<UIObjectPointIcon>(350f, 0, true, pnj.transform.position, NGOSprite, ArrowDisplayerName, false);
                    if (view == ECameraTargetType.MAP) ArrowDisplayer.Instances(ArrowDisplayerName).SetActiveArrows(false);
                } 
            }
            return;
        }

        if (hasItem && _runningQuest.step < 1)
        {
            NextQuestStep();
            return;
        }

        if (_runningQuest.step == 1)
        {
            if (!hasItem) return;
            if (_runningQuest.selectedActivity != EBudgetType.None)
            {
                NextQuestStep();
                return;
            }
            else
            {
                if (view == ECameraTargetType.ZOOM)
                {
                    pinchSprite.gameObject.SetActive(true);
                    pinchSprite.GetComponent<Animator>().SetBool("pinch", false);
                }
                else pinchSprite.gameObject.SetActive(false);
            }
        }

        if (_runningQuest.step == 2)
        {
            if (!hasItem) return;
            CheckStep(_runningQuest);
            if (_runningQuest.validated) return;
            if (view == ECameraTargetType.MAP)
            {
                pinchSprite.gameObject.SetActive(true);
                pinchSprite.GetComponent<Animator>().SetBool("pinch", true);
            }
            else
            {
                int pnjCount = InteractablePNJ.PNJs.Count;
                for (int i = 0; i < pnjCount; i++)
                {
                    InteractablePNJ pnj = InteractablePNJ.PNJs[i];
                    if (pnj.budgetComponent.type == _runningQuest.selectedActivity)
                    {
                        UIObjectPointIcon pointer = ArrowDisplayer.Instances(ArrowDisplayerName).UseArrow<UIObjectPointIcon>(350f, 0, true, pnj.transform.position, EntrepreneurSprite, ArrowDisplayerName, false);
                    }
                }
            }
        }
        else CheckStep(_runningQuest);
    }

    private void NextQuestStep()
    {
        _runningQuest.step++;
        DisplayQuest();
    }

    public void NGOTalkTo(string pnjname)
    {
        for (int i = 0; i < Quests.Count; i++)
        {
            if (Quests[i].step < 1)
            {
                if (Quests[i].NGOtarget == pnjname)
                {
                    Quests[i].step++;
                    if (_runningQuest == Quests[i]) DisplayQuest();
                    return;
                }
            }
        }
    }

    public void EcoTalkTo(EBudgetType pnjTargetName)
    {
        for (int i = 0; i < Quests.Count; i++)
        {
            if (Quests[i].step == 2)
            {
                for (int j = 0; j < Quests[i].activitiesName.Length; j++)
                {
                    if (Quests[i].activitiesName[j] == pnjTargetName)
                    {
                        Quests[i].step++;
                        if (_runningQuest == Quests[i]) DisplayQuest();
                        return;
                    }
                }
            }
        }
    }

    public void SelectTarget(InteractablePNJ tPnj)
    {
        _runningQuest.selectedActivity = tPnj.budgetComponent.type;
        NextQuestStep();
    }

    public void ValidQuest(Quest toValidQuest)
    {
        toValidQuest.step = 4;
        toValidQuest.validated = true;
        if (toValidQuest.unlockQuestID != 0) UnlockQuest(toValidQuest.unlockQuestID);
        DisplayQuest();
    }

    public void UnlockQuest(int questID)
    {
        int nbQuest = Quests.Count;
        for (int j = 0; j < nbQuest; j++)
        {
            if (Quests[j].ID == questID)
            {
                for (int i = 0; i < _questList.Length; i++)
                {
                    if (_questList[i].quest.ID == questID) _questList[i].gameObject.SetActive(true);
                }
            }
        }
    }

    public void CheckValidation()
    {
        for (int i = 0; i < Quests.Count; i++)
        {
            if (!Quests[i].validated) CheckStep(Quests[i]);
        }
    }

    protected void OnChangeScene(OnSwitchScene e)
    {
        if (e.mode == ECameraTargetType.MAP)
        {
            view = ECameraTargetType.MAP;
            ArrowDisplayer.Instances(ArrowDisplayerName).SetActiveArrows(false);
        }
        else if (e.mode == ECameraTargetType.ZOOM)
        {
            view = ECameraTargetType.ZOOM;
            ArrowDisplayer.Instances(ArrowDisplayerName).SetActiveArrows(true);
        }
        DisplayQuest();
    }

    private bool QuestRunning(Quest quest)
    {
        if (_runningQuest != quest) return false;
        else return true;
    }

    protected void OnDestroy()
    {
        Events.Instance.RemoveListener<OnSwitchScene>(OnChangeScene);
    }
}
