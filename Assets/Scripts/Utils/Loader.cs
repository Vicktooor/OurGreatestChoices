using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Assets.Scripts.Game.Load;

public class Loader : MonoBehaviour
{
    public void Start()
    {
        if (GameManager.Instance) return;

        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness0");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness1");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "happyness2");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "Check_Off");
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "Check_On");

        StartCoroutine(LoadMainMenu());   
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
