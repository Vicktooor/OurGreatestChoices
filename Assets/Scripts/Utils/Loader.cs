using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public void Start()
    {
        if (GameManager.Instance) return;
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
