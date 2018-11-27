using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

public enum EItemType
{
    None,
    Iron,
    Fruit,
    Electricity,
    GreenElectricity,
    Battery,
    GreenBattery,
    FruitSeed,
    FruitMarket,
    Carcass,
    Tracks,
    WindTurbine,
    Garden,
    ElectricEnergy,
    GreenElectricEnergy,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {

    public EItemType itemType;
    public GameObject prefab = null; //A METTRE DANS PICK UP

    public EPlayer type;
    public bool isPrimary = false;
    
    public Item NGOItem;
    public Item EcoItem;
    public Item GouvItem;
    public int nbForCraft;

    //For Glossary Overview
    public Sprite icon;
    public Sprite ecoIcon;
    public Sprite hiddenIcon;
    public Sprite ecoHiddenIcon;

    public string glossaryDesc;
}
