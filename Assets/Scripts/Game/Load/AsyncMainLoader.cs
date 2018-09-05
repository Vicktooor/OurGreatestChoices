using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.Game.Load;

public class AsyncMainLoader : MonoSingleton<AsyncMainLoader>
{
    public void LoadAsync<T>(string folderPath, string fileName) where T : UnityEngine.Object
    {
        ResourceRequest lReq = Resources.LoadAsync<T>(folderPath + fileName);
        StartCoroutine(LoadAsyncCoroutine<T>(lReq, fileName));
    }

    public IEnumerator LoadAsyncCoroutine<T>(ResourceRequest loadRequest, string fileName) where T : UnityEngine.Object
    {
        while (!loadRequest.isDone) yield return null;
        MainLoader<T>.Instance.AddAsyncLoadedResource(loadRequest.asset as T, fileName);
    }
}
