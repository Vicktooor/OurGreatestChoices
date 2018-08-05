using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneString { MapView, ZoomView }
public enum EQuality { Standard, HD }

public class EnumClass : MonoBehaviour {

    //Enum for scene
    //ENUM FOR PLAYERS TYPE
    public enum TYPE { NGO, GOV, BUISNESS };
    //Enum for buildings
    public enum TypeBuilding{ None, TownHall, Factory, NormalHouse, VegetableBuilding, WindTurbine, Cars, Animals, Citizens };

    //Enum for Buildings States
    public enum Trees { NO, YES, Dead};
    public enum Clean { NO, YES};
    public enum RoofGarden { NO, YES};

    //Enum for Cars Company
    public enum CarsCompany { closed, open, electric, greenElectric};

    //Enum for Gauges
    public enum Gauges { economy, mood, forest, cleanliness};

    //Enum for notifications
    public enum NotificationsType { sdg, inventory, glossary};

    public enum Language { en, fr }
}