using Assets.Script;
using Assets.Scripts.Game.NGO;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.UI
{
	/// <summary>
	/// 
	/// </summary>
	public class NotePad : MonoBehaviour
	{
        public static List<BillboardElement> approachedItemsPos = new List<BillboardElement>();

        #region Instance
        private static NotePad _instance;

        /// <summary>
        /// instance unique de la classe     
        /// </summary>
        public static NotePad Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        [SerializeField]
		protected float targetNearDistance = 0.25f;
		protected ECameraTargetType view;

		private bool inTransition;
		public Transform arrowContainer;

		protected NotepadTopic[] topics;
        public NotepadTopic[] Topics { get { return topics; } }
        protected ContractorTopic contSelected;
        protected GovernmentTopic govSelected;

		protected InteractablePNJ selectedNpc;
        protected DialoguePNJ selectedDialogue;
        protected NPCDialogue activeDialogue;

        public UIObjectPointer simpleModel;
        public UIObjectPointIcon iconModel;
        public BillboardElement itemBillboard;
        public NotePadInfo endInfo;

        public float displaySpeed;
		protected float DisplaySpeed { get { return Mathf.Clamp(displaySpeed, 0.25f, 10f); } }

        private bool _open = false;
        public bool Open { get { return _open; } }

		protected void Awake()
		{
            if (_instance != null)
            {
                throw new Exception("Tentative de création d'une autre instance de NotePad alors que c'est un singleton.");
            }
            _instance = this;

			Events.Instance.AddListener<OnSwitchScene>(OnChangeScene);
			Events.Instance.AddListener<OnGoToMenu>(GoToMenu);
			inTransition = false;
			topics = GetComponentsInChildren<NotepadTopic>();

            ArrowDisplayer.Instances("NotePad").SetContainer(arrowContainer as RectTransform);
            ArrowDisplayer.Instances("NotePad").SetModels(simpleModel, iconModel);
            endInfo.gameObject.SetActive(false);
        }

		protected void OnEnable()
		{
            Events.Instance.AddListener<OnTalkToNPC>(TalkToNPC);
			Events.Instance.AddListener<OnSetNotepadTopic>(SetTopics);
            Events.Instance.Raise(new OnPopUp());
        }

		protected void OnDisable()
		{
            Events.Instance.RemoveListener<OnTalkToNPC>(TalkToNPC);
			Events.Instance.RemoveListener<OnSetNotepadTopic>(SetTopics);
		}

		protected void TalkToNPC(OnTalkToNPC e)
		{
			if (!inTransition)
			{
				foreach (NotepadTopic topic in topics) topic.clickEnable = false;
				StartCoroutine(DisplayCoroutine());
			}
		}

		protected IEnumerator DisplayCoroutine()
		{
			inTransition = true;
			Vector3 startPos = transform.position;
			Vector3 targetPos = startPos;
			targetPos.y = -startPos.y;
			float k = 0;
			while (k < 1)
			{
				k += Time.deltaTime * DisplaySpeed;
				k = Mathf.Clamp01(k);
				transform.position = Vector3.Lerp(startPos, targetPos, k);
				yield return null;
			}

			if (targetPos.y > 0)
			{
                _open = true;
                foreach (NotepadTopic topic in topics) topic.clickEnable = true;
                Events.Instance.Raise(new OnFTUEOpenDialogue());
				Events.Instance.AddListener<OnActiveSelectTopic>(SelectTopic);
                ControllerInput.AddScreen(transform);
            }
            inTransition = false;
		}

		protected void SetTopics(OnSetNotepadTopic e)
		{
            UIManager.instance.PNJState.Active(false);
            selectedNpc = e.npc;
            selectedDialogue = InteractablePNJ.DialoguesDatabase[selectedNpc.TxtInfo.NPCText];

            int topicIndex = 0;
            foreach (ContractorTopic ct in ResourcesManager.Instance.contractTopics)
            {
                if (selectedDialogue.topicIDs.Contains(ct.id))
                {
                    int index = selectedDialogue.topicIDs.IndexOf(ct.id);
                    topics[index].SetText(TextManager.GetText(ct.text.playerText));
                    topics[index].npcAnswer = TextManager.GetText(ct.text.NPCText);
                    topics[index].SetIcon(ct.icon);
                    topics[index].contractorTopic = ct;
                    topics[index].topicType = typeof(ContractorTopic);
                    topicIndex++;
                }
            }

            foreach (GovernmentTopic gt in ResourcesManager.Instance.govTopics)
            {
                if (selectedDialogue.topicIDs.Contains(gt.id))
                {
                    int index = selectedDialogue.topicIDs.IndexOf(gt.id);
                    topics[index].SetText(TextManager.GetText(gt.texts.playerText));
                    topics[index].npcAnswer = TextManager.GetText(gt.texts.NPCText);
                    topics[index].SetIcon(gt.icon);
                    topics[index].govTopic = gt;
                    topics[index].topicType = typeof(GovernmentTopic);
                    topicIndex++;
                }
            }

			if (topicIndex == 1) topics[1].gameObject.SetActive(false);
			else if (topicIndex == 0) foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(false);
            else foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(true);

            StartCoroutine(DisplayCoroutine());
        }

        protected void SetNextDialogue()
        {
            int topicIndex = 0;
            for (int i = 0; i < activeDialogue.nextTexts.Count; i++)
            {
                topics[topicIndex].SetText(TextManager.GetText(activeDialogue.nextTexts[i].playerText));
                topics[topicIndex].Clear();
                topics[topicIndex].npcAnswer = TextManager.GetText(activeDialogue.nextTexts[i].NPCText);

                if (contSelected != null)
                {
                    topics[topicIndex].SetIcon(contSelected.icon);
                    topics[topicIndex].contractorTopic = contSelected;
                    topics[topicIndex].topicType = typeof(ContractorTopic);
                }
                else if (govSelected != null)
                {
                    topics[topicIndex].SetIcon(govSelected.icon);
                    topics[topicIndex].govTopic = govSelected;
                    topics[topicIndex].topicType = typeof(GovernmentTopic);
                }                
                topicIndex++;
            }

            if (topicIndex == 1) topics[1].gameObject.SetActive(false);
            else if (topicIndex == 0) foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(false);
            else foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(true);
            Events.Instance.AddListener<OnActiveSelectTopic>(SelectTopic);
        }

        protected void SelectTopic(OnActiveSelectTopic e)
		{
            Events.Instance.RemoveListener<OnActiveSelectTopic>(SelectTopic);
            PointingBubble.instance.LinkTopic(topics[e.notepadIndex]);
            if (e.topicItem.contractorTopic != null)
            {
                contSelected = e.topicItem.contractorTopic;
                if (activeDialogue != null) activeDialogue = activeDialogue.nextTexts[e.notepadIndex];
                else activeDialogue = contSelected.text;
            }
            else if (e.topicItem.govTopic != null)
            {
                govSelected = e.topicItem.govTopic;
                if (activeDialogue != null) activeDialogue = activeDialogue.nextTexts[e.notepadIndex];
                else activeDialogue = govSelected.texts;
            }

            if (activeDialogue.nextTexts.Count <= 0)
            {
                PointingBubble.instance.NextDialogueStep(true);
                ShowNextAnswer(true);
                CloseNotePad();
            }
            else
            {
                ShowNextAnswer(false);
                PointingBubble.instance.NextDialogueStep(false);
                SetNextDialogue();
            }
		}

        protected void ShowNextAnswer(bool end)
        {
            if (!FtueManager.instance.active && end && govSelected != null)
            {
                if (activeDialogue.smileySprite != null && activeDialogue.iconSprite != null)
                {
                    endInfo.smiley.texture = activeDialogue.smileySprite.texture;
                    endInfo.icon.texture = activeDialogue.iconSprite.texture;
                    endInfo.gameObject.SetActive(true);
                }
            }
            if (activeDialogue.getInfo)
            {
                if (govSelected != null)
                {
                    if (!InventoryPlayer.Instance.knowsItems.Contains(govSelected.targetItem))
                    {
                        InventoryPlayer.Instance.knowsItems.Add(govSelected.targetItem);
                        Events.Instance.Raise(new OnShowPin(EPin.Glossary, true));
                    }
                }
                else if (contSelected != null) OnPointTargetDialogue(contSelected);
            }
        }

        public void CloseNotePad()
        {
            foreach (NotepadTopic topic in topics)
            {
                topic.contractorTopic = null;
                topic.govTopic = null;
                topic.clickEnable = false;
            }
            _open = false;
            govSelected = null;
            contSelected = null;
            selectedDialogue = null;
            selectedNpc = null;
            activeDialogue = null;
            ControllerInput.RemoveScreen(transform);
            StartCoroutine(DisplayCoroutine());
            Events.Instance.RemoveListener<OnActiveSelectTopic>(SelectTopic);
        }

		protected void OnPointTargetDialogue(ContractorTopic topic)
		{
            Vector3 pos = topic.targetWorldPos;
            if (topic.arrowIcon != null)
            {
                UIObjectPointIcon nArrow = ArrowDisplayer.Instances("NotePad").UseArrow<UIObjectPointIcon>(250f, 0, true, pos, topic.arrowIcon, "NotePad");
                BillboardElement be = Instantiate(itemBillboard, pos + (pos.normalized * 0.15f), Quaternion.identity) as BillboardElement;
                approachedItemsPos.Add(be);
                DontDestroyOnLoad(be);
                if (FtueManager.instance.active) nArrow.AddCallBack(FtueManager.instance.ValidStep);
                else nArrow.AddCallBackPos(CleanBillboard, pos);
            }
            else
            {
                UIObjectPointer nArrow = ArrowDisplayer.Instances("NotePad").UseArrow<UIObjectPointer>(250f, 0, true, pos, "NotePad");
                BillboardElement be = Instantiate(itemBillboard, pos + (pos.normalized * 0.15f), Quaternion.identity) as BillboardElement;
                approachedItemsPos.Add(be);
                DontDestroyOnLoad(be);
                if (FtueManager.instance.active) nArrow.AddCallBack(FtueManager.instance.ValidStep);
                else nArrow.AddCallBackPos(CleanBillboard, pos);
            }
        }

        public void PointTarget(Vector3 worldPos, Sprite icon)
        {
            UIObjectPointIcon nArrow = ArrowDisplayer.Instances("NotePad").UseArrow<UIObjectPointIcon>(250f, 0, true, worldPos, icon, "NotePad");
            if (FtueManager.instance.active) nArrow.AddCallBack(FtueManager.instance.ValidStep);
            else nArrow.AddCallBackPos(CleanBillboard, worldPos);
        }

        public void PointTarget(Vector3 worldPos)
        {
            UIObjectPointer nArrow = ArrowDisplayer.Instances("NotePad").UseArrow<UIObjectPointer>(250f, 0, true, worldPos, "NotePad");
            if (FtueManager.instance.active) nArrow.AddCallBack(FtueManager.instance.ValidStep);
            else nArrow.AddCallBackPos(CleanBillboard, worldPos);
        }

        public void CleanBillboard(Vector3 targetItemPos)
        {
            foreach (BillboardElement be in approachedItemsPos)
            {
                if (Vector3.Distance(be.transform.position, targetItemPos) <= 0.3f)
                {
                    approachedItemsPos.Remove(be);
                    Destroy(be.gameObject);
                    return;
                }
            }
        }

        public void CleanBillboards()
        {
            BillboardElement[] bList = approachedItemsPos.ToArray();
            approachedItemsPos.Clear();
            for (int i = 0; i < bList.Length; i++) Destroy(bList[i].gameObject);
        }

        private void SetBillBoardVisibility(bool targetState)
        {
            foreach (BillboardElement be in approachedItemsPos) be.gameObject.SetActive(targetState);
        }

		protected void OnChangeScene(OnSwitchScene e)
		{
			if (e.mode == ECameraTargetType.MAP)
			{
				view = ECameraTargetType.MAP;
                ArrowDisplayer.Instances("NotePad").SetActiveArrows(false);
                ArrowDisplayer.Instances("FtueArrow").SetActiveArrows(false);
                SetBillBoardVisibility(false);
            }
			else if (e.mode == ECameraTargetType.ZOOM)
			{
				view = ECameraTargetType.ZOOM;
                ArrowDisplayer.Instances("NotePad").SetActiveArrows(true);
                ArrowDisplayer.Instances("FtueArrow").SetActiveArrows(true);
                SetBillBoardVisibility(true);
            }
		}
             
        public void GoToMenu(OnGoToMenu e)
        {
            CleanBillboards();
            ArrowDisplayer.Instances("NotePad").CleanArrows();
            ArrowDisplayer.Instances("FtueArrow").CleanArrows();
            _open = false;
        }

		protected void OnDestroy()
		{
            Events.Instance.RemoveListener<OnGoToMenu>(GoToMenu);
            Events.Instance.RemoveListener<OnSwitchScene>(OnChangeScene);
		}
	}
}