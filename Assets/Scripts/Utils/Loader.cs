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
#if UNITY_STANDALONE
        StreamingAssetAccessor.Platform = RuntimePlatform.WindowsPlayer;
#endif
#if UNITY_ANDROID
        StreamingAssetAccessor.Platform = RuntimePlatform.Android;
#endif
#if UNITY_IPHONE
        StreamingAssetAccessor.Platform = RuntimePlatform.IPhonePlayer;
#endif

        TextManager.SetLanguage(SystemLanguage.English);

        ResourcesManager.Instance.Init();

        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness0");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness1");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness2");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "money_icon");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "money_icon_lock");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "Check_On");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "Check_Off");

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
