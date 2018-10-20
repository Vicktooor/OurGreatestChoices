using Assets.Script;
using Assets.Scripts.Game.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryScreen : MonoSingleton<InventoryScreen>
{
    private RectTransform _rectTransform;

    public InventoryElement giveTarget;
    public Transform dragImageTransform;
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
        UITweener.Instance.NewTween(_rectTransform, tweener);
    }

    public void HandleActiveFromNPC(InteractablePNJ npc)
    {
        giveTarget.gameObject.SetActive(false);
        NPCpanel.clickedNPC = npc;
        tweener.SetMethods(Move, OpenTransform, null, CloseUI);
        UITweener.Instance.StartTween(_rectTransform);
    }

    public void HandleActiveFromInventory()
    {
        tweener.SetMethods(Move, OpenInventory, Opened, CloseUI);
        UITweener.Instance.StartTween(_rectTransform);
    }

    public void Move()
    {
        float x1 = Easing.Scale(Easing.SmoothStop, tweener.t, 2, 2f);
        float x2 = Easing.FlipScale(Easing.SmoothStart, tweener.t, 2, 2f);
        float x = Easing.Mix(x1, x2, 0.5f, tweener.t);
        _rectTransform.localPosition = MathCustom.LerpUnClampVector(tweener.TweenInfo.startPos, tweener.targetPos, x);
    }

    public void OpenTransform()
    {
        NPCpanel.gameObject.SetActive(true);
        bagButton.SetActive(false);
        ControllerInput.OpenScreens.Add(transform);
    }

    public void OpenInventory()
    {
        NPCpanel.gameObject.SetActive(false);
        bagButton.SetActive(false);
        ControllerInput.OpenScreens.Add(transform);
    }

    public void Opened()
    {
        ActiveDrag();
    }

    public void CloseUI()
    {
        StopDrag();
        giveTarget.gameObject.SetActive(false);
        NPCpanel.gameObject.SetActive(false);
        bagButton.SetActive(true);
        ControllerInput.OpenScreens.Remove(transform);
    }

    public void MajInventory(OnUpdateInventory e)
    {
        List<Item> items = InventoryPlayer.instance.itemsWornArray;
        int length = items.Count;
        Item item;
        for (int i = length - 1; i >= 0; i--)
        {
            item = items[i];
            InventoryElement ie = scrollElement.Find(el => el.itemName == item.name);
            if (ie != null && InventoryPlayer.instance.nbItems[item.name] <= 0)
            {
                InventoryPlayer.instance.itemsWornArray.Remove(item);
                scrollElement.Remove(ie);
                scroller.Remove(ie);
            }
            else
            {
                if (ie != null) ie.MajText();
                else
                {
                    InventoryElement newE = scroller.Add<InventoryElement>(elementModel);
                    newE.itemName = item.name;
                    newE.Init();
                    scrollElement.Add(newE);
                }
            }
        }
        scroller.Scale();
    }

    protected void HandleGive(InventoryElement e1, InventoryElement e2)
    {
        Give(InventoryPlayer.instance.ContainItem(e1.itemName), _nearestNPC);
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
        Vector3 playerPos = PlayerManager.instance.player.transform.position;
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
            PointingBubble.instance.Show(true);
            PointingBubble.instance.SetPNJ(_nearestNPC);
            if (LocalizationManager.Instance.currentLangage == EnumClass.Language.en) PointingBubble.instance.ChangeText("Thanks!");
            else PointingBubble.instance.ChangeText("Merci !");

            int itemIndex = InventoryPlayer.instance.GetItemIndex(item.name);
            InventoryPlayer.instance.Give(itemIndex);
            Events.Instance.Raise(new OnGive(itemIndex));
            Events.Instance.Raise(new OnGiveNPC(item, _nearestNPC));
            PointingBubble.instance.ActiveTouchForClose();
        }
        else
        {
            PointingBubble.instance.Show(true);
            PointingBubble.instance.SetPNJ(_nearestNPC);
            if (LocalizationManager.Instance.currentLangage == EnumClass.Language.en) PointingBubble.instance.ChangeText("No thanks!");
            else PointingBubble.instance.ChangeText("Non merci !");
            Events.Instance.Raise(new OnWrongObject());
            PointingBubble.instance.ActiveTouchForClose();
        }
    }
}
