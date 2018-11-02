using Assets.Scripts.Game.NGO;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
	public class PointingBubble : MonoBehaviour
	{
		public bool active = false;

		protected Vector3 speakingPos;
		protected RectTransform rect;
		[SerializeField]
		protected RawImage pointer;
		protected Transform pointertransform;
		[SerializeField]
		protected TextMeshProUGUI textMesh;
		protected InteractablePNJ _speakingNPC;

		protected bool topicSelected;
		protected NotepadTopic selectedTopic;
        private int dialogueStep;

		public void Awake()
		{
			InitInstance();
			rect = GetComponent<RectTransform>();
			pointertransform = pointer.transform;
            gameObject.SetActive(false);
        }

		public void Show(bool state)
		{
			gameObject.SetActive(state);
		}

        private void OnEnable()
        {
            ControllerInput.OpenScreens.Add(transform);
            active = true;
        }

        private void OnDisable()
        {
            ControllerInput.OpenScreens.Remove(transform);
            active = false;
        }

        public void FollowTarget()
		{
			Point();
		}

        public void LinkTopic(NotepadTopic topic)
        {
            selectedTopic = topic;
        }

		public void SetProperties(InteractablePNJ npc)
		{
            dialogueStep = 0;
            Events.Instance.Raise(new OnStartSpeakingNPC());
			topicSelected = false;
			speakingPos = npc.transform.position;
			_speakingNPC = npc;
			Point();
            ChangeText(TextManager.GetText(InteractablePNJ.DialoguesDatabase[npc.IDname].introText));
            StartCoroutine(WaitForTouch());
		}

        public void PNJThanks(InteractablePNJ npc, string textKey)
        {
            speakingPos = npc.transform.position;
            _speakingNPC = npc;
            ChangeText(TextManager.GetText(textKey));
            Point();
        }

        public void ActiveTouchForClose()
        {
            StartCoroutine(TouchForClose());
        }

		public void ChangeText(string newText)
		{
			textMesh.text = newText;
		}

		public void Point()
		{
			Vector2 bubblePos = rect.position;
			Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2f);
			Vector2 targetScreenPos = Camera.main.WorldToScreenPoint(speakingPos);
			targetScreenPos -= bubblePos;

			float angle = Mathf.Atan2(-Mathf.Abs(targetScreenPos.y), targetScreenPos.x) * Mathf.Rad2Deg;

			float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
			float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
			Vector2 dir = new Vector2(cos, sin);

			pointertransform.localRotation = Quaternion.Euler(0f, 0f, angle);
		}

		public void NextDialogueStep(bool toClose = false)
		{
            dialogueStep++;
            ChangeText(selectedTopic.npcAnswer);
            StartCoroutine(WaitForTouch(toClose));
        }

        protected IEnumerator WaitForTouch(bool toClose = false)
		{
			yield return null;
			yield return null;

			bool receiveTouch = false;
			while (!receiveTouch)
			{
				if (Input.GetKeyDown(KeyCode.Mouse0)) receiveTouch = true;
				if (Input.touchCount > 1) receiveTouch = true;
				yield return null;
			}

			if (dialogueStep == 0)
			{
				Events.Instance.Raise(new OnSetNotepadTopic(_speakingNPC));
				Events.Instance.Raise(new OnTalkToNPC());
				Events.Instance.AddListener<OnActiveSelectTopic>(SelectTopic);
			}

            if (toClose)
            {
                selectedTopic = null;
                _speakingNPC = null;
                speakingPos = Vector3.zero;
                Events.Instance.Raise(new OnEndSpeakingNPC());
                Show(false);
            }
        }

        protected IEnumerator TouchForClose()
        {
            yield return null;
            yield return null;

            bool receiveTouch = false;
            while (!receiveTouch)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0)) receiveTouch = true;
                if (Input.touchCount > 1) receiveTouch = true;
                yield return null;
            }
            Show(false);
        }

        protected void SelectTopic(OnActiveSelectTopic e)
		{
			Events.Instance.RemoveListener<OnActiveSelectTopic>(SelectTopic);
		}

        private static PointingBubble _instance;
		public static PointingBubble instance
		{
			get
			{
				return _instance;
			}
		}
		private void InitInstance()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(this);
				throw new Exception("An instance of PointingBubble already exists.");
			}
			else _instance = this;
		}
	}
}
