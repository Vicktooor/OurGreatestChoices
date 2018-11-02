using Assets.Script;
using Assets.Scripts.Game.NGO;
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
        protected ContractorTopic contSelected;
        protected GovernmentTopic govSelected;

		protected InteractablePNJ selectedNpc;
        protected DialoguePNJ selectedDialogue;

        public UIObjectPointer simpleModel;
        public UIObjectPointIcon iconModel;
        public BillboardElement itemBillboard;

        public float displaySpeed;
		protected float DisplaySpeed { get { return Mathf.Clamp(displaySpeed, 0.25f, 10f); } }

        private int dialogueIndex = 0;
        private bool giveInfo;

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
        }

		protected void OnEnable()
		{
			Events.Instance.AddListener<OnTalkToNPC>(TalkToNPC);
			Events.Instance.AddListener<OnSetNotepadTopic>(SetTopics);
            Events.Instance.Raise(new OnPopUp());
        }

		protected void OnDisable()
		{
            dialogueIndex = 0;
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
				foreach (NotepadTopic topic in topics) topic.clickEnable = true;
				Events.Instance.AddListener<OnActiveSelectTopic>(SelectTopic);
			}

			inTransition = false;
		}

		protected void SetTopics(OnSetNotepadTopic e)
		{
			selectedNpc = e.npc;
            selectedDialogue = InteractablePNJ.DialoguesDatabase[selectedNpc.IDname];
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
                }
                topicIndex++;
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
                }
                topicIndex++;
            }

			if (topicIndex == 1) topics[1].gameObject.SetActive(false);
			if (topicIndex == 0) foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(false);

			StartCoroutine(DisplayCoroutine());
        }

        protected void SetNextDialogue(int targetIndex)
        {
            int topicIndex = 0;
            if (govSelected != null)
            {
                NPCDialogue dial = GetNPCTextDromDialogueID(govSelected.texts, targetIndex, dialogueIndex);
                if (dial != null)
                {
                    topics[topicIndex].SetText(TextManager.GetText(dial.playerText));
                    topics[topicIndex].SetIcon(govSelected.icon);
                    topics[topicIndex].Clear();
                    topics[topicIndex].npcAnswer = TextManager.GetText(dial.NPCText);
                    topics[topicIndex].govTopic = govSelected;
                    topics[topicIndex].topicType = typeof(GovernmentTopic);
                    giveInfo = dial.getInfo;
                    topicIndex++;
                }
                else topics[targetIndex].gameObject.SetActive(false);
            }
            else if (contSelected != null)
            {
                NPCDialogue dial = GetNPCTextDromDialogueID(contSelected.text, targetIndex, dialogueIndex);
                if (dial != null)
                {
                    topics[topicIndex].SetText(TextManager.GetText(dial.playerText));
                    topics[topicIndex].SetIcon(contSelected.icon);
                    topics[topicIndex].Clear();
                    topics[topicIndex].npcAnswer = TextManager.GetText(dial.NPCText);
                    topics[topicIndex].contractorTopic = contSelected;
                    topics[topicIndex].topicType = typeof(ContractorTopic);
                    giveInfo = dial.getInfo;
                    topicIndex++;
                }
                else topics[targetIndex].gameObject.SetActive(false);
            }

            dialogueIndex++;
            if (topicIndex == 0)
            {
                if (giveInfo)
                {
                    if (govSelected != null)
                    {
                        if (!InventoryPlayer.Instance.knowsItems.Contains(govSelected.targetItem))
                        {
                            InventoryPlayer.Instance.knowsItems.Add(govSelected.targetItem);
                        }
                    }
                    else if (contSelected != null) OnPointTargetDialogue(contSelected);
                }
                PointingBubble.instance.NextDialogueStep(true);
            }
            else PointingBubble.instance.NextDialogueStep(false);

            Events.Instance.AddListener<OnActiveSelectTopic>(SelectTopic);
        }

        public static NPCDialogue GetNPCTextDromDialogueID(NPCDialogue dial, int topicIndex, int dialogueID)
        {
            if (dial.nextTexts.Count <= 0) return null;
            if (dialogueID == 0)
            {
                if (dial.nextTexts.Count <= topicIndex + 1) return dial.nextTexts[topicIndex];
                else return null;
            }
            else
            {
                if (dialogueID == 0)
                {
                    if (dial.nextTexts.Count <= topicIndex + 1) return dial.nextTexts[topicIndex];
                    else return null;
                }
                if (dial.nextTexts.Count <= topicIndex + 1)
                {
                    return GetNPCTextDromDialogueID(dial.nextTexts[topicIndex], topicIndex, dialogueID--);
                }
                else return null;
            }
        }

        protected void SelectTopic(OnActiveSelectTopic e)
		{
            if (e.topicItem.contractorTopic != null) contSelected = e.topicItem.contractorTopic;
            if (e.topicItem.govTopic != null) govSelected = e.topicItem.govTopic;
            PointingBubble.instance.LinkTopic(topics[e.notepadIndex]);
            SetNextDialogue(e.notepadIndex);
            Events.Instance.RemoveListener<OnActiveSelectTopic>(SelectTopic);
		}

        public void CloseNotePad()
        {
            foreach (NotepadTopic topic in topics)
            {
                topic.contractorTopic = null;
                topic.govTopic = null;
                topic.clickEnable = false;
            }
            govSelected = null;
            contSelected = null;
            selectedDialogue = null;
            selectedNpc = null;
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
            }
            else
            {
                UIObjectPointer nArrow = ArrowDisplayer.Instances("NotePad").UseArrow<UIObjectPointer>(250f, 0, true, pos, "NotePad");
                BillboardElement be = Instantiate(itemBillboard, pos + (pos.normalized * 0.15f), Quaternion.identity) as BillboardElement;
                approachedItemsPos.Add(be);
                DontDestroyOnLoad(be);
            }
            dialogueIndex = 0;
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
                ArrowDisplayer.Instances("Ftue").SetActiveArrows(false);
                SetBillBoardVisibility(false);
            }
			else if (e.mode == ECameraTargetType.ZOOM)
			{
				view = ECameraTargetType.ZOOM;
                ArrowDisplayer.Instances("NotePad").SetActiveArrows(true);
                ArrowDisplayer.Instances("Ftue").SetActiveArrows(true);
                SetBillBoardVisibility(true);
            }
		}
             
        private void GoToMenu(OnGoToMenu e)
        {
            ArrowDisplayer.Instances("NotePad").CleanArrows();
            ArrowDisplayer.Instances("Ftue").CleanArrows();
        }

		protected void OnDestroy()
		{
            Events.Instance.RemoveListener<OnGoToMenu>(GoToMenu);
            Events.Instance.RemoveListener<OnSwitchScene>(OnChangeScene);
		}
	}
}