using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using Assets.Scripts.Game;
using Assets.Script;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Game.Save;
using System.Collections.Generic;
using Assets.Scripts.Items;
using Assets.Scripts.Game.UI;

public enum EPartyType { NONE = 0, NEW, SAVE}

/// <summary>
/// 
/// </summary>
public class GameManager : MonoSingleton<GameManager>
{
    public static string VERSION = "5.2.0";
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
        FtueManager.instance.Restart();
        PARTY_TYPE = EPartyType.NEW;
        EarthManager.Instance.playingPlanetName = EarthManager.Instance.ftuePlanetName;
        PlanetSave.LoadPlayer(EarthManager.Instance.playingPlanetName);
        PlanetSave.LoadPNJs(EarthManager.Instance.playingPlanetName);
        EarthManager.Instance.playerPositions = PlanetSave.DeserializePlayerPositions(PlanetSave.BasePlayerPos);
        EarthManager.Instance.CreatePlanet();
    }

    public void StartMainPlanet()
    {
        Events.Instance.AddListener<OnEndPlanetCreation>(OnEndCreation);
        FtueManager.instance.ForceClear();
        PARTY_TYPE = EPartyType.NEW;
        EarthManager.Instance.playingPlanetName = EarthManager.Instance.planetName;
        PlanetSave.LoadPlayer(EarthManager.Instance.playingPlanetName);
        PlanetSave.LoadCitizens(EarthManager.Instance.playingPlanetName);
        PlanetSave.LoadPNJs(EarthManager.Instance.playingPlanetName);
        EarthManager.Instance.playerPositions = PlanetSave.DeserializePlayerPositions(PlanetSave.BasePlayerPos);
        EarthManager.Instance.CreatePlanet();
    }

    public void OnClickOnContinueButton()
    {
        PARTY_TYPE = EPartyType.SAVE;
        if (!FtueManager.instance.Finish) FtueManager.instance.ForceClear();
        Events.Instance.AddListener<OnEndPlanetCreation>(OnEndCreation);
        InventoryPlayer.Instance.Load();
        List<SavePlayerPosition> players;
        ArrayExtensions.ToList(PlanetSave.GameStateSave.SavedPlayers, out players);
        EarthManager.Instance.playerPositions = PlanetSave.DeserializePlayerPositions(players);
        EarthManager.Instance.playingPlanetName = EarthManager.Instance.planetName;
        PlanetSave.LoadPlayer(EarthManager.Instance.playingPlanetName);
        PlanetSave.LoadCitizens(EarthManager.Instance.playingPlanetName);
        PlanetSave.LoadPNJs(EarthManager.Instance.playingPlanetName);
        EarthManager.Instance.CreatePlanet();
    }

    public void LoadGameParty()
    {
        bool hasParty = PlanetSave.LoadParty();
        if  (hasParty)
        {
            if (PlanetSave.GameStateSave.version == VERSION)
            {
                Events.Instance.Raise(new PartyLoaded(hasParty));
            }
            else
            {
                PlanetSave.DeleteParty();
                PARTY_TYPE = EPartyType.NONE;
                Events.Instance.Raise(new PartyLoaded(false));
            }
        }
        else Events.Instance.Raise(new PartyLoaded(hasParty));
    }

    /// <summary>
	/// Appelé après la création de la planet
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

        if (PARTY_TYPE == EPartyType.NEW) FtueManager.instance.Launch();

        if (EarthManager.Instance.playingPlanetName == EarthManager.Instance.planetName && !TimeManager.Instance.active)
        {
            WorldManager.Instance.InitPolution();
            if (PARTY_TYPE == EPartyType.SAVE) TimeManager.Instance.LoadSave();
            TimeManager.Instance.Active();
            PlanetSave.SaveParty();
        }

        _isChangingScene = false;
        Events.Instance.Raise(new SharePlayerPosition(EarthManager.Instance.playerPositions));
        Events.Instance.Raise(new OnSceneLoaded(_loadedScene));
        UIManager.instance.ActivePanelTransition(false);
        if (!_gameStarted)
        {
            _gameStarted = true;
            Events.Instance.Raise(new OnSwitchScene(ECameraTargetType.MAP));
        }
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
        if (!FtueManager.instance.active) PlanetSave.SaveParty();

        _gameStarted = false;
        UIManager.instance.Clear();
        TimeManager.Instance.Stop();
        LinkDatabase.Instance.Clear();
        WorldManager.Instance.Clear();
        ControllerInput.instance.ResetDatasTouch();
        InteractablePNJ.PNJs.Clear();
        ResourcesManager.Instance.Clear();
        CameraManager.Instance.Reset();
        InventoryPlayer.Instance.Clear();
        PlayerManager.Instance.Clear();
        EarthManager.Instance.DestroyPlanet();

        Events.Instance.Raise(new OnGoToMenu());

        StartCoroutine(LoadMainMenu());
    }

    public void EndOfFTUE()
    {
        UIManager.instance.ActivePanelTransition(true);
        StartCoroutine(FTUESwitch());
    }

    protected IEnumerator LoadMainMenu()
    {
        AsyncOperation lScene = SceneManager.LoadSceneAsync("TransitionScene", LoadSceneMode.Single);
        while (!lScene.isDone)
        {
            yield return null;
        }
        _loadedScene = SceneString.MapView;
        PARTY_TYPE = EPartyType.NONE;
        LoadGameParty();
    }

    protected IEnumerator FTUESwitch()
    {
        AsyncOperation lScene = SceneManager.LoadSceneAsync("TransitionScene", LoadSceneMode.Single);
        while (!lScene.isDone)
        {
            yield return null;
        }
        _loadedScene = SceneString.MapView;

        UIManager.instance.SwitchFTUE();
        InventoryPlayer.Instance.Clear();
        LinkDatabase.Instance.Clear();
        ControllerInput.instance.ResetDatasTouch();
        InteractablePNJ.PNJs.Clear();
        ResourcesManager.Instance.Clear();
        CameraManager.Instance.Reset();
        PlayerManager.Instance.Clear();
        EarthManager.Instance.DestroyPlanet();
        NotePad.Instance.CleanBillboards();
        NotePad.Instance.GoToMenu(null);

        PARTY_TYPE = EPartyType.NEW;
        StartMainPlanet();
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