using Assets.Scripts.Game;
using UnityEngine;

public class TownHallLight : Props
{
    public GameObject normalLight;
    public GameObject greenLight;

    public void SetElectricity()
    {
        normalLight.SetActive(true);
        greenLight.SetActive(false);
    }

    public void SetGreen()
    {
        normalLight.SetActive(false);
        greenLight.SetActive(true);
    }
}