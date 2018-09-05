using UnityEngine;
using System.Collections;
using Assets.Scripts.Game.Load;
using System.Collections.Generic;

public class TestLoad : MonoBehaviour
{
    public void Start()
    {
        MainLoader<Sprite>.Instance.LoadAsync("Sprites/", "pinch");
        TextManager.LoadTexts();
    }
}
