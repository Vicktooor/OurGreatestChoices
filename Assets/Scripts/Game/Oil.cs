using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Game
{

	/// <summary>
	/// 
	/// </summary>
	public class Oil : LinkObject
	{
		protected MeshRenderer cRenderer;

		protected override void Awake()
		{
			base.Awake();
			Events.Instance.AddListener<OnCleanVehicles>(ReceiveCleanEvent);
			cRenderer = GetComponent<MeshRenderer>();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}

		protected void ReceiveCleanEvent(OnCleanVehicles e)
		{
			Events.Instance.RemoveListener<OnCleanVehicles>(ReceiveCleanEvent);
			StartCoroutine(CleanCoroutine());
		}

		protected IEnumerator CleanCoroutine()
		{
			float t = 30f;
			while (t > 0)
			{
				t -= Time.deltaTime;
				Color newColor = cRenderer.material.color;
				if (t <= 0)
				{
					t = 0;
					newColor.a = 0;
				}
				else newColor.a = t / 30;
				cRenderer.material.color = newColor;
				yield return null;
			}
		}
	}
}