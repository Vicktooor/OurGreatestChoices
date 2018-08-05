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
		public int index;
		[HideInInspector]
		public Type topicType;

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
				Events.Instance.Raise(new OnActiveSelectTopic(this));
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
	}
}