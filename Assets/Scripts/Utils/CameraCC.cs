using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utils
{

	/// <summary>
	/// 
	/// </summary>
	public class CameraCC : MonoBehaviour
	{
		public ECamera type;

		protected virtual void OnEnable()
		{
			Events.Instance.AddListener<OnEditionLeftClick>(OnLeftClick);
			Events.Instance.AddListener<OnEditionRightClick>(OnRightClick);
			Events.Instance.AddListener<OnEdtionInputKeyEvent>(OnInputKey);
			Events.Instance.AddListener<OnEditionMouseScroll>(OnScroll);
		}

		protected virtual void OnDisable()
		{
			Events.Instance.RemoveListener<OnEditionLeftClick>(OnLeftClick);
			Events.Instance.RemoveListener<OnEditionRightClick>(OnRightClick);
			Events.Instance.RemoveListener<OnEdtionInputKeyEvent>(OnInputKey);
			Events.Instance.RemoveListener<OnEditionMouseScroll>(OnScroll);
		}

		protected virtual void OnRightClick(OnEditionRightClick e) { }
		protected virtual void OnLeftClick(OnEditionLeftClick e) { }
		protected virtual void OnInputKey(OnEdtionInputKeyEvent e) { }
		protected virtual void OnScroll(OnEditionMouseScroll e) { }

		public virtual void Reset()
		{

		}
	}
}