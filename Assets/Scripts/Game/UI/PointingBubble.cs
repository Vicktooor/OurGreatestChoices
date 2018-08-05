using Assets.Scripts.Game.NGO;
using Assets.Scripts.PNJ;
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
		protected int dialogueStep;

		protected bool topicSelected;
		protected NotepadTopic selectedTopic;

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

		public void SetProperties(InteractablePNJ npc)
		{       
            Events.Instance.Raise(new OnStartSpeakingNPC());
			dialogueStep = 0;
			topicSelected = false;
			speakingPos = npc.transform.position;
			_speakingNPC = npc;
			Point();
            StartCoroutine(WaitForTouch());
		}

        public void SetPNJ(InteractablePNJ npc)
        {
            speakingPos = npc.transform.position;
            _speakingNPC = npc;
            Point();
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

		protected void NextDialogueStep()
		{
			dialogueStep++;

			string textToUse = GetNextNpcText();
			if (textToUse == string.Empty)
			{
				if (selectedTopic.topicType == typeof(GovernmentTopic))
				{
					if (!InventoryPlayer.instance.knowsItems.Contains(_speakingNPC.govTopics[selectedTopic.index].targetItem))
                    {
                        InventoryPlayer.instance.knowsItems.Add(_speakingNPC.govTopics[selectedTopic.index].targetItem);
                        Events.Instance.Raise(new OnShowPin(EPin.Glossary, true));
                    }
				}

				selectedTopic = null;
				_speakingNPC = null;
				dialogueStep = 0;
				speakingPos = Vector3.zero;
				Events.Instance.Raise(new OnEndSpeakingNPC());
				Show(false);
			}
			else
			{
				ChangeText(textToUse);
				StartCoroutine(WaitForTouch());
			}		
		}

		protected string GetNextNpcText()
		{
			if (dialogueStep > 0)
			{
				if (selectedTopic.topicType == typeof(GovernmentTopic)) return GetGovText();
				else return GetContText();
			}
			return string.Empty;
		}

		protected string GetGovText()
		{
			foreach (LocaNPCDialogue item in _speakingNPC.govTopics[selectedTopic.index].texts)
			{
				if (item.lang == GameManager.LANGUAGE)
				{
					if (dialogueStep - 1 == item.texts.Count) return GetLastText();
					else if (dialogueStep - 1 > item.texts.Count) return string.Empty;
					return item.texts[dialogueStep - 1];
				}
			}

			return string.Empty;
		}

		protected string GetContText()
		{
			foreach (LocaNPCDialogue item in _speakingNPC.contTopics[selectedTopic.index].texts)
			{
				if (item.lang == GameManager.LANGUAGE)
				{
					if (dialogueStep - 1 == item.texts.Count) return GetLastText();
					else if (dialogueStep - 1 > item.texts.Count) return string.Empty;
					return item.texts[dialogueStep - 1];
				}
			}
			return string.Empty;
		}

		protected string GetLastText()
		{
			foreach (SimpleLocalisationText preText in _speakingNPC.leavingTexts)
			{
				if (preText.lang == GameManager.LANGUAGE)
				{
					Events.Instance.Raise(new OnPointGovTarget());
					return preText.text;
				}
			}
            return string.Empty;
        }

		protected IEnumerator WaitForTouch()
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
			else
			{
				NextDialogueStep();
			}		
		}

		protected void SelectTopic(OnActiveSelectTopic e)
		{
			Events.Instance.RemoveListener<OnActiveSelectTopic>(SelectTopic);
			selectedTopic = e.topicItem;
			NextDialogueStep();
		}

		protected IEnumerator WaitForSelectTopic()
		{
			yield return null;
			yield return null;
	
			while (!topicSelected)
			{
				yield return null;
			}

			NextDialogueStep();
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
