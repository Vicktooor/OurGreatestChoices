using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {

    new public string name = "New Item";
    public GameObject prefab = null; //A METTRE DANS PICK UP

    public List<Item> itemsLinked;
    //public List<Item> itemsGiven;
    //public GameObject assetEvolution;
    //public ItemPickUp itemPickUp; //Item produced by an item Prop
    public EPlayer type;
    public bool isPrimary = false;

    
    public Item NGOItem;
    public Item EcoItem;
    public Item GouvItem;

    public Item originalItem;

    //For Glossary Overview
    public Sprite icon;
    public Sprite hiddenIcon;
}
