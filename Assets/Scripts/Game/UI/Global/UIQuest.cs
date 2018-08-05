using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIQuest : Selectable
{
    private UIQuestChecker[] coches;
    private Image img;

    public Quest quest;

    protected override void Awake()
    {
        base.Awake();
        img = GetComponent<Image>();
        Events.Instance.AddListener<OnSelectQuest>(HandleQuestSelection);
    }

    public void Init()
    {
        coches = GetComponentsInChildren<UIQuestChecker>(true);
    }

    protected void HandleQuestSelection(OnSelectQuest e)
    {
        if (e.quest == quest) img.color = colors.pressedColor;
        else img.color = Color.white;
    }

    public void UpdateState()
    {
        if (quest.validated)
        {
            interactable = false;
            for (int i = 0; i < coches.Length; i++)
            {
                if (coches[i].questStateTarget == EQuestState.valid) coches[i].gameObject.SetActive(true);
                else coches[i].gameObject.SetActive(false);
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        QuestManager.Instance.SelectQuest(quest);
        base.OnPointerDown(eventData);
    }

    protected override void OnDestroy()
    {
        Events.Instance.RemoveListener<OnSelectQuest>(HandleQuestSelection);
        base.OnDestroy();
    }
}
