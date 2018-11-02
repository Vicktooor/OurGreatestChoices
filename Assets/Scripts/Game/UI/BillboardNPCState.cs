using UnityEngine;
using System.Collections;
using Assets.Scripts.Manager;
using UnityEngine.UI;
using Assets.Scripts.Items;
using Assets.Script;

public class BillboardNPCState : BillboardElement
{
    public InteractablePNJ pnj;
    public RawImage _smileyRenderer;
    public RawImage _itemMark;
    public RawImage _budgetMark;

    protected override void Awake()
    {
        base.Awake();
        Events.Instance.AddListener<OnUpdateNPCInfo>(CatchUpdate);
    }

    private void CatchUpdate(OnUpdateNPCInfo e)
    {
        if (pnj != null)
        {
            SetVisibility(Vector3.Distance(PlayerManager.Instance.player.transform.position, pnj.Position),  Player.NPC_HELP_DIST);
            UpdateInfo();
        }
    }

    public void SetVisibility(float dist, float refDist)
    {
        if (!gameObject.activeSelf) Active(true);
        float k;
        float minDist = refDist / 2f;
        if (dist > refDist)
        {
            transform.localScale = new Vector3(0f, 0f, 0f);
            return;
        }
        if (dist <= minDist)
        {
            transform.localScale = new Vector3(_baseScale.x, _baseScale.y, _baseScale.z);
        }
        else
        {
            float lDist = refDist - minDist;
            k = Mathf.Clamp01(Mathf.Abs(1f - lDist / (dist - minDist)));
            transform.localScale = new Vector3(_baseScale.x * k, _baseScale.y * k, _baseScale.z * k);
        }
    }

    public void UpdateInfo()
    {
        int happinessPoint = 0;
        BudgetComponent comp = pnj.budgetComponent;
        if (pnj.HaveHisItem())
        {
            happinessPoint++;
            _itemMark.texture = MainLoader<Sprite>.Instance.GetResource("Check_On").texture;
        }
        else _itemMark.texture = MainLoader<Sprite>.Instance.GetResource("Check_Off").texture;
        /*if (comp.budget >= comp.targetBudget)
        {
            happinessPoint++;
            _budgetMark.texture = MainLoader<Sprite>.Instance.GetResource("Check_On").texture;
        }
        else _budgetMark.texture = MainLoader<Sprite>.Instance.GetResource("Check_Off").texture;*/
        _smileyRenderer.texture = MainLoader<Sprite>.Instance.GetResource("happyness" + happinessPoint).texture;
    }

    public void Active(bool state)
    {
        if (!gameObject.activeSelf && state && pnj != null) UpdateInfo();
        gameObject.SetActive(state);
    }

    public void SetTarget(Transform target)
    {
        GetComponent<UIOverlay>().targetTransform = target;
    }

    public void Clear()
    {
        pnj = null;
        GetComponent<UIOverlay>().targetTransform = null;
    }

    protected override void Update() { }

    private void OnDestroy()
    {
        Events.Instance.RemoveListener<OnUpdateNPCInfo>(CatchUpdate);
    }
}
