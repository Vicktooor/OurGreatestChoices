﻿using Assets.Script;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryScreen : MonoSingleton<InventoryScreen>
{
    private RectTransform _rectTransform;

    public InventoryElement giveTarget;
    private InventoryElement _targetDraggable;
    private List<RaycastResult> _hitObjects = new List<RaycastResult>();
    private bool _dragging;

    public GameObject elementModel;
    public OneLineScroller scroller;
    public List<InventoryElement> scrollElement;

    public RawImage draggerTransform;
    public Tweener tweener;

    public GameObject bagButton;
    public NPCScreen NPCpanel;

    protected override void Awake() {
        base.Awake();
        scroller.Init();
        _rectTransform = GetComponent<RectTransform>();
    }

    protected void Start()
    {
        TweenerLead.Instance.NewTween(_rectTransform, tweener);
    }

    public void HandleActiveFromNPC(InteractablePNJ npc)
    {
        ControllerInput.AddScreen(transform);
        giveTarget.gameObject.SetActive(false);
        NPCpanel.clickedNPC = npc;
        tweener.SetMethods(Move, OpenTransform, CheckFtue, CloseUI);
        TweenerLead.Instance.StartTween(tweener);
    }

    protected void CheckFtue()
    {
        if (FtueManager.instance.active)
        {
            if (FtueManager.instance.currentStep.transformTarget != EPlayer.NONE) FtueManager.instance.ValidStep(); 
        }
    }

    public void HandleActiveFromInventory()
    {
        ControllerInput.AddScreen(transform);
        tweener.SetMethods(Move, OpenInventory, Opened, CloseUI);
        TweenerLead.Instance.StartTween(tweener);
    }

    public void Move()
    {
        float x1 = Easing.Scale(Easing.SmoothStop, tweener.t, 2, 2f);
        float x2 = Easing.FlipScale(Easing.SmoothStart, tweener.t, 2, 2f);
        float x = Easing.Mix(x1, x2, 0.5f, tweener.t);
        tweener.SetPos(x);
    }

    public void OpenTransform()
    {
        UIManager.instance.PNJState.Active(false);
        NPCpanel.gameObject.SetActive(true);
        bagButton.SetActive(false);
    }

    public void OpenInventory()
    {
        if (FtueManager.instance.active)
        {
            if (FtueManager.instance.currentStep.scrollerIndex != -1)
            {
                scroller.Place(-FtueManager.instance.currentStep.scrollerIndex);
            }
        }

        NPCpanel.gameObject.SetActive(false);
        bagButton.SetActive(false);
    }

    public void Opened()
    {
        if (FtueManager.instance.active)
        {
            if (FtueManager.instance.currentStep.scrollerIndex != -1)
            {
                int i = FtueManager.instance.currentStep.scrollerIndex;
                if (FtueManager.instance.currentStep.drag.active)
                    FtueManager.instance.DetachTarget(scrollElement[i].transform);
            }
        }
        ActiveDrag();
    }

    public void CloseUI()
    {
        InteractablePNJ pnj = GetNPC();
        if (pnj)
        {
            if (pnj.neededItems.Count > 0)
            {
                UIManager.instance.PNJState.pnj = pnj;
                UIManager.instance.PNJState.SetTarget(PlayerManager.Instance.GetNearestNPCIcon());
                UIManager.instance.PNJState.SetVisibility(0f, 1f);
                UIManager.instance.PNJState.Active(true);
            }
        }       
        StopDrag();
        giveTarget.gameObject.SetActive(false);
        NPCpanel.gameObject.SetActive(false);
        bagButton.SetActive(true);
        ControllerInput.RemoveScreen(transform);
        ControllerInput.instance.ResetDatasTouch();
    }

    public void Close()
    {
        tweener.SetMethods(Move, null, null, CloseUI);
        TweenerLead.Instance.StartTween(tweener);
    }

    public void MajInventory(OnUpdateInventory e)
    {
        List<Item> items = InventoryPlayer.Instance.itemsWornArray;
        int length = items.Count;
        Item item;
        for (int i = length - 1; i >= 0; i--)
        {
            item = items[i];
            InventoryElement ie = scrollElement.Find(el => el.itemType == item.itemType);
            if (InventoryPlayer.Instance.nbItems[item.itemType] <= 0)
            {
                InventoryPlayer.Instance.itemsWornArray.Remove(item);
                scrollElement.Remove(ie);
                scroller.Remove(ie);
            }
            else
            {
                if (ie != null) ie.MajText();
                else
                {
                    InventoryElement newE = scroller.Add<InventoryElement>(elementModel);
                    newE.itemType = item.itemType;
                    newE.Init();
                    scrollElement.Add(newE);
                }
            }
        }
        scroller.Scale();
    }

    protected void HandleGive(InventoryElement e1, InventoryElement e2)
    {
        Give(InventoryPlayer.Instance.ContainItem(e1.itemType), _nearestNPC);
    }

    private bool _dragActive = false;
    public void ActiveDrag()
    {
        _nearestNPC = GetNPC();
        if (_nearestNPC != null)
        {
            _dragActive = true;
            giveTarget.gameObject.SetActive(true);
            giveTarget.transform.position = Camera.main.WorldToScreenPoint(_nearestNPC.Position);
            Dragger<InventoryElement>.Instance._draggableImg = draggerTransform;
            Dragger<InventoryElement>.Instance.AddCallback(HandleGive, giveTarget);
        }
        else
        {
            giveTarget.gameObject.SetActive(false);
            _dragActive = false;
        }
    }

    protected void StopDrag()
    {
        _dragActive = false;
        Dragger<InventoryElement>.Instance.RemoveCallback(HandleGive, giveTarget);
    }

    private InteractablePNJ _nearestNPC;
    protected InteractablePNJ GetNPC()
    {
        InteractablePNJ npc = null;
        Vector3 playerPos = PlayerManager.Instance.player.transform.position;
        float minDist = 0.15f;
        float dist = 0f;
        int nb = InteractablePNJ.PNJs.Count;
        for (int i = 0; i < nb; i++)
        {
            if (InteractablePNJ.PNJs[i].GetComponent<Renderer>().isVisible)
            {
                dist = Vector3.Distance(InteractablePNJ.PNJs[i].Position, playerPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    npc = InteractablePNJ.PNJs[i];
                }
            }
        }
        return npc;
    }

    protected void Update()
    {
        if (_dragActive) Dragger<InventoryElement>.Instance.Drag();
    }

    public void Give(Item item, InteractablePNJ npc)
    {
        if (npc.CanAccept(item))
        {
            FBX_Give.instance.Play(new Vector3(draggerTransform.transform.position.x, draggerTransform.transform.position.y, draggerTransform.transform.position.z));

            if (FtueManager.instance.active)
            {
                if (FtueManager.instance.currentStep.drag.active)
                {
                    FtueManager.instance.AttachTarget(scrollElement[FtueManager.instance.currentStep.scrollerIndex].transform);
                    FtueManager.instance.ValidStep();
                }
            }
            else
            {
                PointingBubble.instance.Show(true);
                PointingBubble.instance.ActiveTouchForClose();
            }

            int itemIndex = InventoryPlayer.Instance.GetItemIndex(item.itemType);
            InventoryPlayer.Instance.Give(itemIndex, npc.IDname);
            Events.Instance.Raise(new OnGive(itemIndex));
            
            npc.ReceiveItem(item.itemType);
            UIManager.instance.PNJState.SetFromItem(item.itemType);
            Events.Instance.Raise(new OnUpdateNPCInfo());
            MajInventory(null);
            if (scroller.CurrentIndex >= scroller.NbElement && scroller.NbElement > 0) scroller.Move(1);

            if (FtueManager.instance.active) HandleActiveFromInventory();
        }
        else
        {
            PointingBubble.instance.Show(true);
            Events.Instance.Raise(new OnWrongObject());
            PointingBubble.instance.ActiveTouchForClose();
        }
    }
}
