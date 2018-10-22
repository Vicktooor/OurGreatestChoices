using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {

    new public string name = "New Item";
    public GameObject prefab = null; //A METTRE DANS PICK UP

    public List<Item> itemsLinked;
    public EPlayer type;
    public bool isPrimary = false;
    
    public Item NGOItem;
    public Item EcoItem;
    public Item GouvItem;
    public int nbForCraft;

    public Item originalItem;

    //For Glossary Overview
    public Sprite icon;
    public Sprite hiddenIcon;

    public string glossaryDesc;
}
