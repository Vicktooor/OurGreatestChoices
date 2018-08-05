using UnityEngine;

namespace Assets.Scripts.Game.UI
{

	/// <summary>
	/// 
	/// </summary>
	public class UIDesktopObject : MonoBehaviour
	{
		protected void Awake()
		{
#if UNITY_ANDROID
			gameObject.SetActive(false);
#endif
		}
	}
}