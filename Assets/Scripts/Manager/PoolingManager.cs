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

public class PoolingManager
{
	#region Singleton
	static PoolingManager instanceInternal = null;
	public static PoolingManager Instance
	{
		get
		{
			if (instanceInternal == null)
			{
				instanceInternal = new PoolingManager ();
			}

			return instanceInternal;
		}
	}
	#endregion

	public const string TAG_CONTAINER = "PoolContainer";

	protected List<Type> POOLTYPE = new List<Type>() { };

	private Dictionary<Type, List<BaseObject>> _arrays = new Dictionary<Type, List<BaseObject>>();
	public Dictionary<Type, List<BaseObject>> PoolArrays { get { return _arrays; } }

	public void Active()
	{
		Events.Instance.AddListener<OnNewYear>(PrintObjectsNb);
	}

	public void Desactive()
	{
		Events.Instance.AddListener<OnNewYear>(PrintObjectsNb);
	}

	protected void PrintObjectsNb(OnNewYear e)
	{
		int counter = 0;
		foreach (KeyValuePair<Type, List<BaseObject>> item in _arrays)
		{
			counter += item.Value.Count;
		}
		Debug.Log("Tot : " + counter);
	}

	protected void CreatePoolingArray(Type typeModel, List<BaseObject> poolList)
	{
		if (PoolListExist(typeModel)) return;
		else _arrays.Add(typeModel, poolList);
	}

	protected bool PoolListExist(Type type)
	{
		List<BaseObject> foundList;
		if (_arrays.TryGetValue(type, out foundList)) return true;
		else return false;
	}

	public bool CreatePoolObject(BaseObject obj)
	{
		Type oType = obj.GetType();
		CreatePoolingArray(oType, new List<BaseObject>());
		_arrays[oType].Add(obj);
		if (POOLTYPE.Contains(oType)) obj.gameObject.SetActive(false);
		return true;
	}

	public float GetTypeActivePrct<T>() where T : BaseObject
	{
		List<BaseObject> foundList = new List<BaseObject>();
		List<BaseObject> activeList = new List<BaseObject>();
		if (_arrays.TryGetValue(typeof(T), out foundList))
		{
			activeList = _arrays[typeof(T)].FindAll(obj => (obj.gameObject.activeSelf == true));
		}
		else return -1;

		if (foundList.Count == 0 && activeList.Count == 0) return -1;
		else return (float)activeList.Count / (float)foundList.Count;
	}

    public void Clear()
    {
        _arrays.Clear();
        PoolArrays.Clear();
    }
}

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