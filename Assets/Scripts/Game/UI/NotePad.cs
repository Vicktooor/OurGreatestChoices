using Assets.Script;
using Assets.Scripts.Game.NGO;
using Assets.Scripts.PNJ;
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
		protected List<ContractorTopic> contractorTopic = new List<ContractorTopic>();

		protected InteractablePNJ selectedNpc;

        public UIObjectPointer simpleModel;
        public UIObjectPointIcon iconModel;
        public BillboardElement itemBillboard;

        public float displaySpeed;
		protected float DisplaySpeed { get { return Mathf.Clamp(displaySpeed, 0.25f, 10f); } }

		protected void Awake()
		{
            if (_instance != null)
            {
                throw new Exception("Tentative de création d'une autre instance de NotePad alors que c'est un singleton.");
            }
            _instance = this;

            Events.Instance.AddListener<OnPointGovTarget>(OnPointTargetDialogue);
			Events.Instance.AddListener<OnZoomFinish>(OnChangeScene);
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
				Events.Instance.AddListener<OnClickSelectTopicContractor>(ReceiveClickOnTopicCont);
				Events.Instance.AddListener<OnClickSelectTopicGov>(ReceiveClickOnTopicGov);
			}

			inTransition = false;
		}

		protected void SetTopics(OnSetNotepadTopic e)
		{
			int topicIndex = 0;
			selectedNpc = e.npc;

			if (e.npc.govTopics.Count + e.npc.contTopics.Count > 2) return;
			else
			{
				foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(true);
			}

			foreach (GovernmentTopic gt in e.npc.govTopics)
			{
				foreach (SimpleLocalisationText title in gt.topicTitles)
				{
					if (title.lang == GameManager.LANGUAGE) topics[topicIndex].SetText(title.text);
				}
				topics[topicIndex].SetIcon(gt.icon);
				topics[topicIndex].index = e.npc.govTopics.IndexOf(gt);
				topics[topicIndex].topicType = typeof(GovernmentTopic);
				topicIndex++;
			}

			foreach (ContractorTopic ct in e.npc.contTopics)
			{
				foreach (SimpleLocalisationText title in ct.topicTitles)
				{
					if (title.lang == GameManager.LANGUAGE) topics[topicIndex].SetText(title.text);
				}
				topics[topicIndex].SetIcon(ct.icon);
				topics[topicIndex].index = e.npc.contTopics.IndexOf(ct);
				topics[topicIndex].topicType = typeof(ContractorTopic);
				topicIndex++;
			}

			if (topicIndex == 1) topics[1].gameObject.SetActive(false);
			if (topicIndex == 0)
			{
				foreach (NotepadTopic nt in topics) nt.gameObject.SetActive(false);
			}

			StartCoroutine(DisplayCoroutine());
		}

		protected void ReceiveClickOnTopicGov(OnClickSelectTopicGov e)
		{

		}

		protected void ReceiveClickOnTopicCont(OnClickSelectTopicContractor e)
		{
			contractorTopic.Add(e.contractorTarget);
		}

		protected void SelectTopic(OnActiveSelectTopic e)
		{
			Events.Instance.Raise(new OnSelectTopic(e.topicItem, selectedNpc));
			Events.Instance.RemoveListener<OnActiveSelectTopic>(SelectTopic);
			Events.Instance.RemoveListener<OnClickSelectTopicGov>(ReceiveClickOnTopicGov);
			Events.Instance.RemoveListener<OnClickSelectTopicContractor>(ReceiveClickOnTopicCont);
			foreach (NotepadTopic topic in topics) topic.clickEnable = false;
			StartCoroutine(DisplayCoroutine());
		}

		protected void OnPointTargetDialogue(OnPointGovTarget e)
		{
            for (int i = 0; i < contractorTopic.Count; i++)
            {
                Vector3 pos = contractorTopic[i].targetWorldPos;
                if (contractorTopic[i].arrowIcon != null)
                {
                    UIObjectPointIcon nArrow = ArrowDisplayer.Instances("NotePad").UseArrow<UIObjectPointIcon>(250f, 0, true, pos, contractorTopic[i].arrowIcon, "NotePad");
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
            }
            contractorTopic.Clear();
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

		protected void OnChangeScene(OnZoomFinish e)
		{
			if (e.view == ECameraTargetType.MAP)
			{
				view = ECameraTargetType.MAP;
                ArrowDisplayer.Instances("NotePad").SetActiveArrows(false);
                ArrowDisplayer.Instances("Ftue").SetActiveArrows(false);
                SetBillBoardVisibility(false);
            }
			else if (e.view == ECameraTargetType.ZOOM)
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
            Events.Instance.RemoveListener<OnZoomFinish>(OnChangeScene);
		}
	}
}