using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game;

#region

public enum EStateLinkObject { MAIN, OTHER }

[Serializable]
public struct BuildingLink
{
    public EnumClass.TypeBuilding type;
    public int id;

    public BuildingLink(EnumClass.TypeBuilding cType, int cId) {
        type = cType;
        id = cId;
    }

	public bool Equal(BuildingLink buildTest)
	{
		return (buildTest.id == id && buildTest.type == type) ? true : false;
	}
}

#endregion

/// <summary>
/// Class which contains all links
/// </summary>
public class LinkDatabase
{
	public Dictionary<BuildingLink, List<LinkObject>> StructBuildings = new Dictionary<BuildingLink, List<LinkObject>>();

	public void Init()
	{
		Events.Instance.AddListener<OnPropsLinkCreation>(ReceiveLink);
	}

	public void Destroy()
	{
		Events.Instance.RemoveListener<OnPropsLinkCreation>(ReceiveLink);
	}

	public void ReceiveLink(OnPropsLinkCreation e)
	{
		if (!StructBuildings.ContainsKey(e.buildingStruct)) {
			StructBuildings.Add(e.buildingStruct, new List<LinkObject>());
			StructBuildings[e.buildingStruct].Add(e.obj);
		}
		else StructBuildings[e.buildingStruct].Add(e.obj);
	}

	public List<GameObject> GetLinkObjects(BuildingLink buildLink, Type objType)
	{
		if (StructBuildings.ContainsKey(buildLink))
		{			
			List<LinkObject> foundList = StructBuildings[buildLink].FindAll(obj => obj.GetType() == objType);
			List<GameObject> goList = new List<GameObject>();
			foreach (LinkObject lObj in foundList) goList.Add(lObj.gameObject);
			return goList;
		}
		else return new List<GameObject>();
	}

    public void Clear()
    {
        StructBuildings.Clear();
    }

	#region Singleton
	static LinkDatabase instanceInternal = null;
	public static LinkDatabase Instance
	{
		get
		{
			if (instanceInternal == null)
			{
				instanceInternal = new LinkDatabase();
			}

			return instanceInternal;
		}
	}
	#endregion
}