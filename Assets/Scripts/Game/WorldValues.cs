using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class WorldValues
	{
        public static float BOOST_TARGET_VALUE = 2f;
        public static float TRANSFERT_VALUE = 1f;
        public static int PLAYER_MONEY_MULTIPLICATOR = 2000;

        public static float STATE_FOREST = 0f;
        public static float STATE_CLEANLINESS = 0f;
        public static float STATE_NPC = 0f;
        public static float STATE_ECONOMY = 0f;

        public static float I_STATE_FOREST = 0f;
        public static float I_STATE_CLEANLINESS = 0f;
        public static float I_STATE_NPC = 0f;
        public static float I_STATE_ECONOMY = 0f;

        public static float BAD_STEP = -1f;
        public static float GOOD_STEP = 1f;

        public static void NewYear(BudgetGlobalImpact globalImpact)
        {
            globalImpact.forest += I_STATE_FOREST;
            globalImpact.cleanliness += I_STATE_CLEANLINESS;
            globalImpact.npc += I_STATE_NPC;
            globalImpact.economy += I_STATE_ECONOMY;

            ApplyImpact(globalImpact);
            Events.Instance.Raise(new OnChangeGauges());
        }

        public static void ApplyImpact(BudgetGlobalImpact values)
        {
            float lForest = STATE_FOREST;
            float lEconomy = STATE_ECONOMY;
            float lNpc = STATE_NPC;
            float lClean = STATE_CLEANLINESS;

            STATE_FOREST = Mathf.Clamp(values.forest - (lEconomy / 3f), -3f, 3f);
            STATE_CLEANLINESS = Mathf.Clamp(values.cleanliness + ((lForest - lEconomy) / 3f), -3f, 3f);
            STATE_NPC = Mathf.Clamp(values.npc + (lEconomy / 3f) + (1.5f * Mathf.Log(3.2f + lClean)) - 2f, -3f, 3f);
            STATE_ECONOMY = Mathf.Clamp(values.economy + (lNpc / 2f), -3f, 3f);
        }
	}
}
