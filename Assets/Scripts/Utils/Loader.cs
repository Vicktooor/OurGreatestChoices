using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using Assets.Scripts.Game.Save;

public class Loader : MonoBehaviour
{
    public void Start()
    {
        StreamingAssetAccessor.Platform = RuntimePlatform.WindowsPlayer;
        TextManager.SetLanguage(SystemLanguage.English);

        ResourcesManager.Instance.Init();

        PlanetSave.LoadCitizens(EarthManager.Instance.planetName);
        PlanetSave.LoadPlayer(EarthManager.Instance.planetName);
        PlanetSave.LoadPNJs(EarthManager.Instance.planetName);

        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness0");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness1");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness2");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "Check_Off");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "Check_On");

        if (PlanetMaker.instance)
        {
            if (PlanetMaker.instance.edit == EditState.INACTIVE) StartCoroutine(LoadMainMenu());
        }       
    }

    protected IEnumerator LoadMainMenu()
    {
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync("DiscreetScene", LoadSceneMode.Single);
        while (!loadingScene.isDone)
        {
            yield return null;
        }
    }
}
