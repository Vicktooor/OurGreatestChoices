using Assets.Scripts.Game.Load;
using UnityEngine;

public class TestLoad : MonoBehaviour
{
    public void Start()
    {
        Debug.Log(Application.systemLanguage);
        TextManager.LoadTraduction(Application.systemLanguage);
        Debug.Log(TextManager.GetText("__MajorNam"));
    }
}
