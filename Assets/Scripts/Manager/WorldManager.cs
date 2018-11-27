using Assets.Scripts.Game;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldSave
{
    public float cleanliness;
    public float npcState;
    public float forestState;
    public float economyState;
    public List<int> idPoluted;
    public List<int> idDeforested;
}

namespace Assets.Scripts.Manager
{
    public enum SpawnValue { UP, DOWN }

	/// <summary>
	/// 
	/// </summary>
	public class WorldManager : MonoSingleton<WorldManager>
	{
		public static List<PolutionArea> PolutionsWay = new List<PolutionArea>();
		public static List<DeforestationArea> DeforestationAreas = new List<DeforestationArea>();

        [SerializeField]
        private int _deforestionStartID;
        [SerializeField]
        private int _deforestionStartID2;
        [SerializeField]
        private int _oilPlantDesertID;
        [SerializeField]
        private int _oilPlantForestID;
        [SerializeField]
        private int _oilPlantMoutainID;
        [SerializeField]
        private int _polutionID1;
        [SerializeField]
        private int _polutionID2;

        protected void OnEnable()
		{
			Events.Instance.AddListener<OnNewYear>(OnYearPassed);
			Events.Instance.AddListener<OnNewMonth>(OnMonthPassed);
			Events.Instance.AddListener<OnUpdateForest>(HandleForestUpdate);
			Events.Instance.AddListener<OnUpdateGround>(HandleGroundUpdate);
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<OnNewYear>(OnYearPassed);
			Events.Instance.RemoveListener<OnNewMonth>(OnMonthPassed);
            Events.Instance.RemoveListener<OnUpdateForest>(HandleForestUpdate);
            Events.Instance.RemoveListener<OnUpdateGround>(HandleGroundUpdate);
        }

        public void InitPolution()
        {
            if (GameManager.PARTY_TYPE == EPartyType.SAVE)
            {
                Cell cell;
                List<int> savePoluted = PlanetSave.GameStateSave.WorldSave.idPoluted;
                int lPoluted = PlanetSave.GameStateSave.WorldSave.idPoluted.Count;
                for (int i = 0; i < lPoluted; i++)
                {
                    cell = EarthManager.Instance.Cells.Find(c => c.ID == savePoluted[i]);
                    cell.ForcePolution(true);
                    PolutionArea.CELLS_USED.Add(cell);
                }
                List<int> saveDeforested = PlanetSave.GameStateSave.WorldSave.idDeforested;
                int lDeforested = PlanetSave.GameStateSave.WorldSave.idDeforested.Count;
                for (int i = 0; i < lDeforested; i++)
                {
                    cell = EarthManager.Instance.Cells.Find(c => c.ID == saveDeforested[i]);
                    cell.SetDeforestation(true);
                    DeforestationArea.CELLS_USED.Add(cell);
                }
            }

            StartDeforestationZone(_deforestionStartID);
            StartDeforestationZone(_deforestionStartID2);
            StartPolutionWay(_polutionID1, new List<CellState>() { CellState.SNOW });
            StartPolutionWay(_polutionID2, new List<CellState>() { CellState.MOSS, CellState.GRASS });
            StartPolutionWay(_oilPlantMoutainID, new List<CellState>() { CellState.SEA, });
            StartPolutionWay(_oilPlantForestID, new List<CellState>() { CellState.SEA, });
            StartPolutionWay(_oilPlantDesertID, new List<CellState>() { CellState.SEA });
        }

		protected void OnYearPassed(OnNewYear e)
		{
            ResourcesManager.Instance.UpdateBudget();
		}

		protected void OnMonthPassed(OnNewMonth e)
		{

		}

        protected void HandleForestUpdate(OnUpdateForest e)
        {
            foreach (DeforestationArea da in DeforestationAreas) da.Action();
        }

        protected void HandleGroundUpdate(OnUpdateGround e)
        {
            foreach (PolutionArea pa in PolutionsWay) pa.Action();
        }

        public void StartPolutionWay(int ID, List<CellState> polutionTypes)
		{
            Cell refCell = EarthManager.Instance.Cells.Find(c => c.ID == ID);
            if (!refCell) return;

            if (PolutionArea.CELLS_USED.Contains(refCell)) refCell.ForcePolution(true);

            PolutionArea area = PolutionsWay.Find(pa => pa.initialCell.ID == ID);
            if (area != null)
            {
                area.Start();
                return;
            }
            else
            {
                PolutionArea newWay = new PolutionArea(ID, polutionTypes);
                PolutionsWay.Add(newWay);
            }
        }

		public void StartDeforestationZone(int ID)
		{
			Cell refCell = EarthManager.Instance.Cells.Find(c => c.ID == ID);
			if (!refCell) return;

            if (DeforestationArea.CELLS_USED.Contains(refCell)) refCell.SetDeforestation(true);

            DeforestationArea area = DeforestationAreas.Find(pa => pa.initialCell.ID == ID);
			if (area != null)
			{
				area.Start();
				return;
			}
            else
            {
                DeforestationArea newWay = new DeforestationArea(ID);
                DeforestationAreas.Add(newWay);
            }
		}

        public WorldSave GenerateSave()
        {
            WorldSave newSave = new WorldSave();
            newSave.cleanliness = WorldValues.STATE_CLEANLINESS;
            newSave.npcState = WorldValues.STATE_NPC;
            newSave.forestState = WorldValues.STATE_FOREST;
            newSave.economyState = WorldValues.STATE_ECONOMY;

            newSave.idDeforested = new List<int>();
            newSave.idPoluted = new List<int>();
            foreach (Cell c in DeforestationArea.CELLS_USED) newSave.idDeforested.Add(c.ID);
            foreach (Cell c in PolutionArea.CELLS_USED) newSave.idPoluted.Add(c.ID);

            return newSave;
        }

		protected void OnDestroy()
		{
			_instance = null;
		}

        public void Clear()
        {
            PolutionArea.ClearStatic();
            DeforestationArea.ClearStatic();
            DeforestationAreas.Clear();
            PolutionsWay.Clear();
        }
	}
}