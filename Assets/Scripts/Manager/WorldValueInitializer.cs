using Assets.Scripts.Game;
using Assets.Scripts.Game.Save;
using UnityEngine;

public class WorldValueInitializer : MonoSingleton<WorldValueInitializer>
{
    [Header("Economic values")]
    public float boostTargetValue = 2f;
    public float transfertValue = 0.1f;
    public int moneyMultiplicator = 2000;

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

    public void Init()
    {
        WorldValues.BOOST_TARGET_VALUE = boostTargetValue;
        WorldValues.TRANSFERT_VALUE = transfertValue;
        WorldValues.PLAYER_MONEY_MULTIPLICATOR = moneyMultiplicator;

        WorldValues.I_STATE_FOREST = forestState;
        WorldValues.I_STATE_CLEANLINESS = planetCleanliness;
        WorldValues.I_STATE_NPC = npcMood;
        WorldValues.I_STATE_ECONOMY = economy;

        if (GameManager.PARTY_TYPE == EPartyType.SAVE)
        {
            WorldValues.STATE_FOREST = PlanetSave.GameStateSave.WorldSave.forestState;
            WorldValues.STATE_CLEANLINESS = PlanetSave.GameStateSave.WorldSave.cleanliness;
            WorldValues.STATE_NPC = PlanetSave.GameStateSave.WorldSave.npcState;
            WorldValues.STATE_ECONOMY = PlanetSave.GameStateSave.WorldSave.economyState;
        }
        else
        {
            WorldValues.STATE_FOREST = forestState;
            WorldValues.STATE_CLEANLINESS = planetCleanliness;
            WorldValues.STATE_NPC = npcMood;
            WorldValues.STATE_ECONOMY = economy;
        }
        Events.Instance.Raise(new OnChangeGauges());
    }

    protected void Update()
    {
        actualEconomy = WorldValues.STATE_ECONOMY;
        actualPlanetCleanliness = WorldValues.STATE_CLEANLINESS;
        actualNpcMood = WorldValues.STATE_NPC;
        actualForestState = WorldValues.STATE_FOREST;
    }
}
