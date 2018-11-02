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
using System.Collections.Generic;
using Assets.Scripts.Items;

public enum EPartyType { NONE = 0, NEW, SAVE}

/// <summary>
/// 
/// </summary>
public class GameManager : MonoSingleton<GameManager>
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

    private bool _isChangingScene = false;
    public bool IsChangingScene { get { return _isChangingScene; } }

    private bool _gameStarted = false;

    protected override void Awake()
    {
        base.Awake();
        Screen.orientation = ScreenOrientation.Portrait;
        _loadedScene = SceneString.MapView;
    }

    protected void Start()
    {
        LoadGameParty();
    }

    public void OnClickOnStartButton()
	{
        Events.Instance.AddListener<OnEndPlanetCreation>(OnEndCreation);
        FtueManager.instance.Display(false);
        PARTY_TYPE = EPartyType.NEW;
        EarthManager.Instance.playerPositions = PlanetSave.DeserializePlayerPositions(PlanetSave.BasePlayerPos);
        EarthManager.Instance.playingPlanetName = EarthManager.Instance.ftuePlanetName;
        EarthManager.Instance.CreatePlanet();
    }

    public void OnClickOnContinueButton()
    {
        FtueManager.instance.Display(false);
        PARTY_TYPE = EPartyType.SAVE;
        if (PlanetSave.GameStateSave.version != null)
        {
            Events.Instance.AddListener<OnEndPlanetCreation>(OnEndCreation);
            InventoryPlayer.Instance.Load();
            List<SavePlayerPosition> players;
            ArrayExtensions.ToList(PlanetSave.GameStateSave.SavedPlayers, out players);
            EarthManager.Instance.playerPositions = PlanetSave.DeserializePlayerPositions(players);
            EarthManager.Instance.playingPlanetName = EarthManager.Instance.planetName;
            EarthManager.Instance.CreatePlanet();
        }
        else PARTY_TYPE = EPartyType.NONE;
    }

    public void LoadGameParty()
    {
        Events.Instance.Raise(new PartyLoaded(PlanetSave.LoadParty()));
    }

    /// <summary>
	/// Appelé après la créationde la planet
	/// </summary>
	/// <param name="e"></param>
	void OnEndCreation(OnEndPlanetCreation e)
    {
        Events.Instance.RemoveListener<OnEndPlanetCreation>(OnEndCreation);
        StartCoroutine(LoadCoroutine(SceneString.MapView.ToString(), LoadSceneMode.Single));
    }

    protected IEnumerator LoadCoroutine(string sceneName, LoadSceneMode mode)
	{
        _isChangingScene = true;
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneName, mode);
		while (!loadingScene.isDone)
		{
			yield return null;
		}
        _loadedScene = (SceneString)Enum.Parse(typeof(SceneString), sceneName);

        if (!_gameStarted)
        {
            FtueManager.instance.Launch();
            WorldManager.Instance.InitPolution();
        }

        _isChangingScene = false;
        Events.Instance.Raise(new SharePlayerPosition(EarthManager.Instance.playerPositions));
        Events.Instance.Raise(new OnSceneLoaded(_loadedScene));
        if (!_gameStarted) _gameStarted = true;
    }

    /// <summary>
    /// Lance le changement de scene
    /// </summary>
    /// 
    /// 
    /// <param name="e"></param>
    public void ChangeScene()
	{
        Events.Instance.Raise(new OnSwitchScene((_loadedScene == SceneString.MapView) ? ECameraTargetType.ZOOM : ECameraTargetType.MAP));
        if (_loadedScene == SceneString.MapView)
			StartCoroutine(LoadCoroutine(SceneString.ZoomView.ToString(), LoadSceneMode.Single));
		else if (_loadedScene == SceneString.ZoomView)
			StartCoroutine(LoadCoroutine(SceneString.MapView.ToString(), LoadSceneMode.Single));
	}

    public void GoToMenu()
    {
        PlanetSave.SaveParty();

        _gameStarted = false;
        UIManager.instance.Clear();
        TimeManager.Instance.Stop();
        LinkDatabase.Instance.Clear();
        WorldManager.Instance.Clear();
        ControllerInput.instance.ResetDatasTouch();
        InteractablePNJ.PNJs.Clear();
        ResourcesManager.Instance.Clear();
        CameraManager.Instance.Reset();
        EarthManager.Instance.DestroyPlanet();
        InventoryPlayer.Instance.Clear();
        PlayerManager.Instance.Clear();

        Events.Instance.Raise(new OnGoToMenu());

        StartCoroutine(LoadMainMenu());
    }

    protected IEnumerator LoadMainMenu()
    {
        AsyncOperation unloadingScene = SceneManager.UnloadSceneAsync(_loadedScene.ToString());
        while (!unloadingScene.isDone)
        {
            yield return null;
        }
        _loadedScene = SceneString.MapView;
        PARTY_TYPE = EPartyType.NONE;
        LoadGameParty();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    protected void OnDestroy()
    {
        _instance = null;
	}
}