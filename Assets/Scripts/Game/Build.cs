using UnityEngine;

namespace Assets.Scripts.Game
{
    /// <summary>
    /// 
    /// </summary>
    public class Build : MonoBehaviour
    {
        public Cell associateCell;

		public void UpdatePosition()
		{
			RaycastHit hit;
			Vector3 origin = transform.position * 2f;
			float rayMagnitude = (origin.magnitude / 2f) * 1.25f;

			Ray ray = new Ray(origin, (transform.position - origin));

			if (associateCell.SelfCollider[0].Raycast(ray, out hit, rayMagnitude))
				transform.position = hit.point;
		}
	}
}