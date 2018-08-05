using Assets.Scripts.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game
{

	/// <summary>
	/// 
	/// </summary>
	public class LinkObject : Props
	{
		[SerializeField]
		protected BuildingLink buildingLink;
		public BuildingLink BuildingLink { get { return buildingLink; } set { buildingLink = value; } }

		public GameObject propLinked;
		
		protected virtual void OnEnable()
		{
			Events.Instance.AddListener<OnEndPlanetCreation>(ReceiveLink);
		}

		protected virtual void OnDisable()
		{
			Events.Instance.RemoveListener<OnEndPlanetCreation>(ReceiveLink);
		}

		public virtual void Creation()
		{
			Events.Instance.Raise(new OnPropsLinkCreation(buildingLink, this));
		}

		public virtual void ReceiveLink(OnEndPlanetCreation e)
		{
			SearchPropLinked();
		}

		public virtual void SearchPropLinked()
		{
			LinkDatabase lLinkDatabase = LinkDatabase.Instance;
			List<GameObject> foundObjs = lLinkDatabase.GetLinkObjects(buildingLink, typeof(MainItemProp));
			if (foundObjs.Count == 1) propLinked = foundObjs[0];
		}
	}
}