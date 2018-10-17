using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using Assets.Scripts.Game;
using Assets.Script;
using Assets.Scripts.Game.UI.Global;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Game.Save;

public enum EPartyType { NONE = 0, NEW, SAVE}

/// <summary>
/// 
/// </summary>
public class GameManager : MonoBehaviour
{
    public static string VERSION = "4.0";
    public static SystemLanguage LANGUAGE = SystemLanguage.English;
    public static EPartyType PARTY_TYPE = EPartyType.NONE;

	[SerializeField]
	private bool _isOnDesk = true;
	public bool IsOnDesk { get { return _isOnDesk; } }

    public float safeLoadFrame = 10;

	private SceneString _loadedScene;
	public SceneString LoadedScene { get { return _loadedScene; } }

	protected static GameManager _instance;
	public static GameManager Instance
	{
		get { return _instance; }
	}

    public static void SetFR() { LANGUAGE = SystemLanguage.French; }
    public static void SetEN() { LANGUAGE = SystemLanguage.English; }

	protected void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this);
			throw new Exception("An instance of GameManager already exists.");
		}
		else _instance = this;

		Screen.orientation = ScreenOrientation.Portrait;
	}

	protected void Start()
	{
        PlanetSave.LoadParty();
		_loadedScene = SceneString.MapView;
		Events.Instance.AddListener<OnEndPlanetCreation>(OnEndCreation);
		Events.Instance.AddListener<ZoomEnd>(ChangeScene);
    }

	public void OnClickOnStartButton()
	{
        FtueManager.instance.Display(false);
        PARTY_TYPE = EPartyType.NEW;
        EarthManager.Instance.CreatePlanet();
	}

    public void OnClickOnContinueButton()
    {
        FtueManager.instance.Display(false);
        PARTY_TYPE = EPartyType.SAVE;
        EarthManager.Instance.CreatePlanet();
    }

	protected IEnumerator LoadCoroutine(string sceneName, LoadSceneMode mode)
	{
		AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneName, mode);
		while (!loadingScene.isDone)
		{
			yield return null;
		}
        Events.Instance.Raise(new SharePlayerPosition(EarthManager.Instance.playerPosition));
        _loadedScene = (SceneString)Enum.Parse(typeof(SceneString), sceneName);
        StartCoroutine(SafeTimeCoroutine(_loadedScene));
	}

    private bool _ftueStarted = false;
    private IEnumerator SafeTimeCoroutine(SceneString nScene)
    {
        if (!_ftueStarted)
        {
            _ftueStarted = true;
            FtueManager.instance.Launch();
        }

        Events.Instance.Raise(new OnSceneLoaded(nScene));

        int cFrame = 0;
        while (cFrame < safeLoadFrame)
        {
            cFrame++;
            yield return null;
        }
    }

    /// <summary>
    /// Lance le changement de scene
    /// </summary>
    /// 
    /// 
    /// <param name="e"></param>
    void ChangeScene(ZoomEnd e)
	{
		if (_loadedScene == SceneString.MapView)
			StartCoroutine(LoadCoroutine(SceneString.ZoomView.ToString(), LoadSceneMode.Single));
		else if (_loadedScene == SceneString.ZoomView)
			StartCoroutine(LoadCoroutine(SceneString.MapView.ToString(), LoadSceneMode.Single));
	}

	/// <summary>
	/// Appelé après la créationde la planet
	/// </summary>
	/// <param name="e"></param>
	void OnEndCreation(OnEndPlanetCreation e)
	{	
		StartCoroutine(LoadCoroutine(_loadedScene.ToString(), LoadSceneMode.Single));
        Events.Instance.Raise(new OnZoomFinish(ECameraTargetType.MAP));
    }

    public void GoToMenu()
    {
        PlanetSave.SaveParty();

        PARTY_TYPE = EPartyType.NONE;
        _ftueStarted = false;

        TimeManager.instance.Stop();
        foreach (Cell c in EarthManager.Instance.Cells)
        {
            c.transform.Clear();
            Destroy(c.gameObject);
        }
        EarthManager.Instance.Clear();
        CameraManager.Instance.Reset();
        Cell.ResetCells();

        LinkDatabase.Instance.Clear();
        WorldManager.instance.Clear();
        ResourcesManager.instance.Clear();

        ControllerInput.instance.ResetDatasTouch();

        Events.Instance.Raise(new OnGoToMenu());
        PlanetSave.LoadParty();
        StartCoroutine(LoadMainMenu());
    }

    protected IEnumerator LoadMainMenu()
    {
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync("InitScene");
        while (!loadingScene.isDone)
        {
            yield return null;
        }
        _loadedScene = SceneString.MapView;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    protected void OnDestroy()
    {
        Events.Instance.RemoveListener<OnEndPlanetCreation>(OnEndCreation);
        _instance = null;
	}
}