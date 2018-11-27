using UnityEngine;
using System.Collections;
using Assets.Scripts.Manager;
using UnityEngine.UI;
using Assets.Scripts.Items;
using Assets.Script;
using System.Collections.Generic;

public class BillboardNPCState : BillboardElement
{
    public InteractablePNJ pnj;
    public RawImage smileyRenderer;
    public RawImage itemMark;
    public RawImage budgetMark;

    public RectTransform starFeedback;
    public List<RawImage> stars;

    public Sprite starLockSprite;
    public Sprite starUnlockSprite;

    public List<GameObject> arrows;

    private int targetIndex = 0;

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
            for (int i = 0; i < stars.Count; i++)
            {
                if (i >= pnj.neededItems.Count) stars[i].gameObject.SetActive(false);
                else
                {
                    EItemType lEItem = pnj.neededItems[i];
                    Item lItem = ResourcesManager.Instance.ItemModels.Find(it => it.itemType == lEItem);
                    if (pnj.HaveItem(lEItem) && pnj.HaveBudget(lEItem)) stars[i].texture = starUnlockSprite.texture;
                    else stars[i].texture = starLockSprite.texture;
                    stars[i].gameObject.SetActive(true);
                }
            }
            if (pnj.neededItems.Count > 1) for (int i = 0; i < arrows.Count; i++) arrows[i].SetActive(true);
            else for (int i = 0; i < arrows.Count; i++) arrows[i].SetActive(false);
        }
    }

    public void SetVisibility(float dist, float refDist)
    {
        starFeedback.transform.position = stars[targetIndex].transform.position;

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
        starFeedback.transform.position = stars[targetIndex].transform.position;
        stars[targetIndex].transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

        EItemType lEItem = pnj.neededItems[targetIndex];
        Item lItem = ResourcesManager.Instance.ItemModels.Find(i => i.itemType == lEItem);
        int happinessPoint = 0;
        if (pnj.HaveItem(lEItem))
        {
            happinessPoint++;
            itemMark.texture = lItem.icon.texture;
        }
        else itemMark.texture = lItem.hiddenIcon.texture;
        if (pnj.HaveBudget(lEItem))
        {
            happinessPoint++;
            budgetMark.texture = MainLoader<Sprite>.Instance.GetResource("money_icon").texture;
        }
        else budgetMark.texture = MainLoader<Sprite>.Instance.GetResource("money_icon_lock").texture;
        smileyRenderer.texture = MainLoader<Sprite>.Instance.GetResource("happyness" + happinessPoint).texture;
    }

    public void Active(bool state)
    {
        if (pnj != null)
        {
            if (pnj.neededItems.Count > 0)
            {
                UpdateInfo();
                for (int i = 0; i < stars.Count; i++)
                {
                    if (i >= pnj.neededItems.Count) stars[i].gameObject.SetActive(false);
                    else
                    {
                        EItemType lEItem = pnj.neededItems[i];
                        Item lItem = ResourcesManager.Instance.ItemModels.Find(it => it.itemType == lEItem);
                        if (pnj.HaveItem(lEItem) && pnj.HaveBudget(lEItem)) stars[i].texture = starUnlockSprite.texture;
                        else stars[i].texture = starLockSprite.texture;
                        stars[i].gameObject.SetActive(true);
                    }
                }
                if (pnj.neededItems.Count > 1) for (int i = 0; i < arrows.Count; i++) arrows[i].SetActive(true);
                else for (int i = 0; i < arrows.Count; i++) arrows[i].SetActive(false);
                gameObject.SetActive(state);
            }
            else gameObject.SetActive(false);
        } else gameObject.SetActive(false);
    }

    public void SetTarget(Transform target)
    {
        GetComponent<UIOverlay>().targetTransform = target;
    }

    public void Right()
    {
        stars[targetIndex].transform.localScale = Vector3.one;
        targetIndex = (targetIndex + 1) % pnj.neededItems.Count;
        UpdateInfo();
    }

    public void Left()
    {
        stars[targetIndex].transform.localScale = Vector3.one;
        targetIndex = (targetIndex - 1) % pnj.neededItems.Count;
        if (targetIndex < 0) targetIndex = pnj.neededItems.Count - 1;
        UpdateInfo();
    }

    public void SetFromItem(EItemType iType)
    {
        if (pnj.neededItems.Contains(iType))
        {
            stars[targetIndex].transform.localScale = Vector3.one;
            targetIndex = pnj.neededItems.IndexOf(iType);
            UpdateInfo();
        }
    }

    public void Clear()
    {
        stars[targetIndex].transform.localScale = Vector3.one;
        starFeedback.transform.position = stars[0].transform.position;
        targetIndex = 0;
        pnj = null;
        GetComponent<UIOverlay>().targetTransform = null;
    }

    protected override void Update() { }

    private void OnDestroy()
    {
        Events.Instance.RemoveListener<OnUpdateNPCInfo>(CatchUpdate);
    }
}
