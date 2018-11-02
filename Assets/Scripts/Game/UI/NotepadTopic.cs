using Assets.Scripts.Game.NGO;
using FMODUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
	/// <summary>
	/// 
	/// </summary>
	public class NotepadTopic : MonoBehaviour, IPointerClickHandler
	{
		[HideInInspector]
		public Type topicType;
        public int index;

        public ContractorTopic contractorTopic;
        public GovernmentTopic govTopic;

        public string npcAnswer;

		protected TextMeshProUGUI textMesh;
		protected RawImage icon;

		public bool clickEnable;

        [SerializeField]
        StudioEventEmitter _emitterClick;

		public void Awake()
		{
			textMesh = GetComponentInChildren<TextMeshProUGUI>();
			icon = GetComponentInChildren<RawImage>();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (clickEnable)
			{
				Events.Instance.Raise(new OnActiveSelectTopic(this, index));
                _emitterClick.Play();
            }
		}

		public void SetIcon(Sprite img)
		{
			icon.texture = img.texture;
		}

		public void SetText(string txt)
		{
			textMesh.text = txt;
		}

        public void Clear()
        {
            npcAnswer = string.Empty;
            contractorTopic = null;
            govTopic = null;
        }
	}
}