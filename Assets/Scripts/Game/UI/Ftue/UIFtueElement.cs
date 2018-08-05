using UnityEngine;

public enum EFtueSpecialUI { None = 0, Jauges, GameButtons }

namespace Assets.Scripts.Game.UI.Ftue
{
	/// <summary>
	/// 
	/// </summary>
	public class UIFtueElement : MonoBehaviour
	{
		protected virtual void Start()
		{
			/*Events.Instance.AddListener<OnEndFtue>(OnEndOfFtue);
			Events.Instance.AddListener<OnFtueNextStep>(ReveiveNextStep);*/
		}

		protected virtual void ReveiveNextStep(OnFtueNextStep e)
		{

		}
	}
}