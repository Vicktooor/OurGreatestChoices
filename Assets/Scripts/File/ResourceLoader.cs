using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FolderTrackingStruct
{
	private int nbAsset;
	private int currentLoadNb;
	public float progress {
		get {
			if (nbAsset == 0 || currentLoadNb == 0) return 0;
			return (currentLoadNb / nbAsset) * 100f;
		}
	}

	public FolderTrackingStruct()
	{
		nbAsset = 0;
		currentLoadNb = 0;
	}
	
	public void IncrementProgress() { currentLoadNb = currentLoadNb + 1; }
	public void AddResources() { nbAsset = nbAsset + 1; }
}

public class OnResourceLoadRequest : GameEvent
{
	public UnityEngine.Object asset;
	public string path;
	public OnResourceLoadRequest(string assetPath)
	{
		path = assetPath;
	}
}

public class ResourceLoader : MonoBehaviour
{
	private static string _resourcePath = "Assets/Resources/";
	private static Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
	private static Dictionary<string, FolderTrackingStruct> _asyncTrakingPct = new Dictionary<string, FolderTrackingStruct>();

	private void Awake()
	{
		Events.Instance.AddListener<OnResourceLoadRequest>(OnLoadRequest);
	}

	private void OnDestroy()
	{
		Events.Instance.RemoveListener<OnResourceLoadRequest>(OnLoadRequest);
	}

	private void OnLoadRequest(OnResourceLoadRequest e)
	{
		StartCoroutine(CoroutineLoadRequest(e));
	}

	/// <summary>
	/// Track loading object
	/// </summary>
	private IEnumerator CoroutineLoadRequest(OnResourceLoadRequest e)
	{
		ResourceRequest loadReq = Resources.LoadAsync(e.path);

		string[] cutPath = e.path.Split('/');
		string folderName = cutPath[cutPath.Length - 2] + "/";

		if (loadReq.asset != null)
		{
			e.asset = loadReq.asset;
			while (!loadReq.isDone)
			{
				yield return null;
			}
			if (!_resources.ContainsKey(loadReq.asset.name))
				_resources.Add(loadReq.asset.name, loadReq.asset);

			if (_asyncTrakingPct.ContainsKey(folderName))
				_asyncTrakingPct[folderName].IncrementProgress();
		}
		else
		{
			if (_asyncTrakingPct.ContainsKey(folderName))
				_asyncTrakingPct[folderName].IncrementProgress();
		}
	}

	#region Static
	/// <summary>
	/// Load folder
	/// </summary>
	public static void LoadFolder(string folderPath)
	{
		if (folderPath[folderPath.Length - 1] != '/') folderPath += '/';
		string[] filesPath = FileProcess.GetFileNames(_resourcePath + folderPath);
		for (int i = 0; i < filesPath.Length; i++) LoadResource(folderPath + filesPath[i]);
	}

	/// <summary>
	/// Async load folder
	/// </summary>
	public static string LoadFolderAsync(string folderPath)
	{
		if (folderPath[folderPath.Length - 1] != '/') folderPath += '/';
		AddTrackingLoad(folderPath, 0);
		string[] filesPath = FileProcess.GetFileNames(_resourcePath + folderPath);	
		for (int i = 0; i < filesPath.Length; i++) LoadResourceAsync(folderPath + filesPath[i], folderPath);
		return folderPath;
	}

	/// <summary>
	/// Get loading progress (only with folder loading)
	/// </summary>
	public static float GetProgress(string folderPath)
	{
		if (folderPath[folderPath.Length - 1] != '/') folderPath += '/';
		if (_asyncTrakingPct.ContainsKey(folderPath))
			return _asyncTrakingPct[folderPath].progress;
		else return -1;
	}

	public static UnityEngine.Object GetResource(string assetName)
	{
		return LoadResource(assetName, true);
	}

	public static T GetResource<T>(string assetName) where T : UnityEngine.Object
	{
		return LoadResource<T>(assetName, true);
	}

	/// <summary>
	/// Load or get resource
	/// </summary>
	public static UnityEngine.Object LoadResource(string path, bool pathAsName = false)
	{
		if (pathAsName)
		{
			if (_resources.ContainsKey(path)) return _resources[path];
			else
			{
				Debug.LogError("Loaded resources doesn't contains [ " + path + " ]");
				return null;
			}
		}
		else return Load<UnityEngine.Object>(path);
	}

	/// <summary>
	/// Load or get resource specifying type
	/// </summary>
	public static T LoadResource<T>(string path, bool pathAsName = false) where T : UnityEngine.Object
	{
		if (pathAsName)
		{
			if (_resources.ContainsKey(path)) return (T)_resources[path];
			else
			{
				Debug.LogError("Loaded resources doesn't contains [ " + path + " ]");
				return null;
			}
		}
		else return Load<T>(path);
	}

	/// <summary>
	/// Main load function
	/// </summary>
	private static T Load<T>(string path) where T : UnityEngine.Object
	{
		string[] pathParts = path.Split(new char[1] { '/' });
		if (pathParts.Length > 0)
		{
			string oName = pathParts[pathParts.Length - 1];
			if (_resources.ContainsKey(oName)) return (T)_resources[oName];
			else
			{
				T obj = Resources.Load<T>(path);
				if (obj != null)
				{
					_resources.Add(obj.name, obj);
					return obj;
				}
				else return null;
			}
		}
		else
		{
			Debug.LogError("Void resource path");
			return null;
		}
	}

	/// <summary>
	/// Ask ResourceLoader to load & track asset load
	/// </summary>
	private static void LoadResourceAsync(string path, string folderName)
	{
		string[] pathParts = path.Split(new char[1] { '/' });
		if (pathParts.Length > 1)
		{
			_asyncTrakingPct[folderName].AddResources();
			string oName = pathParts[pathParts.Length - 1];		
			Events.Instance.Raise(new OnResourceLoadRequest(path));
		}
		else Debug.LogError("Void folder path - cannot track folder laod async with main resources folder");
	}

	/// <summary>
	/// Add folder to tracking list
	/// </summary>
	private static void AddTrackingLoad(string pfolderName, int pNbAsset)
	{
		if (_asyncTrakingPct.ContainsKey(pfolderName)) return;
		else _asyncTrakingPct.Add(pfolderName, new FolderTrackingStruct());
	}
	#endregion
}