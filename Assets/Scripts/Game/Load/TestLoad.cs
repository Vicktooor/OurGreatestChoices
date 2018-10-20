using Assets.Scripts.Game.Load;
using UnityEngine;

public class TestLoad : MonoBehaviour
{
    public void Start()
    {
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "pinch");
        TextManager.LoadTexts();
    }
}
