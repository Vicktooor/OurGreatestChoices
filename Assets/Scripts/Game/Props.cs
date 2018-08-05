using UnityEngine;

namespace Assets.Scripts.Game
{
	public class Props : BaseObject
	{
		public float offsetY = 0;

		[HideInInspector]
		public Cell associateCell;

        private BouncingTree[] _trees;

		protected override void Awake()
		{
            _trees = GetComponentsInChildren<BouncingTree>(true);
			base.Awake();
		}

		public void UpdatePosition()
		{
			RaycastHit hit;
			Vector3 origin = transform.position * 2f;
			float rayMagnitude = (origin.magnitude / 2f) * 5f;

			Ray ray = new Ray(origin, (transform.position - origin));

			if (associateCell.SelfCollider[0].Raycast(ray, out hit, rayMagnitude))
			{
				transform.position = hit.point;
				transform.position += hit.point.normalized * offsetY;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Events.Instance.RemoveListener<OnNewMonth>(OnNewMonthPassed);
			Events.Instance.RemoveListener<OnNewYear>(OnNewYearPassed);
			if (associateCell) associateCell.DestroyProps(this);
		}
		
        public void UpdateInternProps()
        {
            for (int i = 0; i < _trees.Length; i++)
            {
                if (_trees[i].gameObject.activeSelf) _trees[i].CustomUpdate();
            }
        }
	}	
}