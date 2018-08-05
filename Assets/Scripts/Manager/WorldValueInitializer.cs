using Assets.Scripts.Game;
using UnityEngine;

public class WorldValueInitializer : MonoBehaviour
{
    [Header("Economic values")]
    public float boostTargetValue = 2f;
    public float boostTime = 30f;
    public float transfertValue = 0.1f;
    public float moneyMultiplicator = 2500f;

    [Header("World values")]
    public float forestState;
    public float planetCleanliness;
    public float npcMood;
    public float economy;

    [Header("Actual values")]
    public float actualForestState;
    public float actualPlanetCleanliness;
    public float actualNpcMood;
    public float actualEconomy;

    protected void Awake()
    {
        WorldValues.BOOST_TARGET_VALUE = boostTargetValue;
        WorldValues.BOOST_ECONOMIC_TIME = boostTime;
        WorldValues.TRANSFERT_VALUE = transfertValue;
        WorldValues.PLAYER_MONEY_MULTIPLICATOR = moneyMultiplicator;

        WorldValues.I_STATE_FOREST = forestState;
        WorldValues.I_STATE_CLEANLINESS = planetCleanliness;
        WorldValues.I_STATE_NPC = npcMood;
        WorldValues.I_STATE_ECONOMY = economy;

        WorldValues.STATE_FOREST = forestState;
        WorldValues.STATE_CLEANLINESS = planetCleanliness;
        WorldValues.STATE_NPC = npcMood;
        WorldValues.STATE_ECONOMY = economy;
    }

    protected void Update()
    {
        actualEconomy = WorldValues.STATE_ECONOMY;
        actualPlanetCleanliness = WorldValues.STATE_CLEANLINESS;
        actualNpcMood = WorldValues.STATE_NPC;
        actualForestState = WorldValues.STATE_FOREST;
    }
}
