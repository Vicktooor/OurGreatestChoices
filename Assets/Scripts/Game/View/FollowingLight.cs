using UnityEngine;

namespace Assets.Scripts.Game.View
{

	/// <summary>
	/// 
	/// </summary>
	public class FollowingLight : MonoBehaviour
	{
		protected Camera mainCam;

        public Vector3 rotation;

		protected void Start()
		{
			mainCam = Camera.main;
			float[] distances = new float[32];
			for (int i = 0; i < distances.Length; i++) distances[i] = 0;
			mainCam.layerCullDistances = distances;

			Events.Instance.AddListener<OnEndPlanetCreation>(SetPlanet);
		}

		protected void SetPlanet(OnEndPlanetCreation e)
		{
			Events.Instance.RemoveListener<OnEndPlanetCreation>(SetPlanet);
		}

		protected void Update()
		{
			transform.position = Camera.main.transform.position;
			transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(rotation.x, rotation.y, rotation.z);
		}
	}
}