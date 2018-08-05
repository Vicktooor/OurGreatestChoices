using Assets.Scripts.Game.Objects;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.Elements
{

	/// <summary>
	/// 
	/// </summary>
	public class OilPlant : Building
	{
		protected void Start()
		{
			//StartCoroutine(StartCoroutine());
		}

		IEnumerator StartCoroutine()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			WorldManager.instance.StartPolutionWay(associateCell);
		}
	}
}