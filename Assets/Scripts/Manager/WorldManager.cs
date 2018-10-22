using Assets.Scripts.Game;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public enum SpawnValue { UP, DOWN }

	/// <summary>
	/// 
	/// </summary>
	public class WorldManager : MonoBehaviour
	{
		public static List<PolutionArea> PolutionsWay = new List<PolutionArea>();
		public static List<DeforestationArea> DeforestationAreas = new List<DeforestationArea>();

        private int _deforestionStartID = 13;
        private int _deforestionStartID2 = 72;

        [Header("Culling")]
		[SerializeField]
		protected bool activeCulling;
		public bool ActiveCulling { get { return activeCulling; } }

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de WorldManager alors que c'est un singleton.");
			}
			_instance = this;
		}

		protected void OnEnable()
		{
			Events.Instance.AddListener<OnEndPlanetCreation>(Init);
			Events.Instance.AddListener<OnNewYear>(OnYearPassed);
			Events.Instance.AddListener<OnNewMonth>(OnMonthPassed);
			Events.Instance.AddListener<OnUpdateForest>(HandleForestUpdate);
			Events.Instance.AddListener<OnUpdateGround>(HandleGroundUpdate);
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<OnEndPlanetCreation>(Init);
			Events.Instance.RemoveListener<OnNewYear>(OnYearPassed);
			Events.Instance.RemoveListener<OnNewMonth>(OnMonthPassed);
            Events.Instance.RemoveListener<OnUpdateForest>(HandleForestUpdate);
            Events.Instance.RemoveListener<OnUpdateGround>(HandleGroundUpdate);
        }

		protected void Init(OnEndPlanetCreation e)
		{
			if (PlanetMaker.instance.edit != EditState.INACTIVE) return;
            PoolTree.ForestCells.CleanDoublon();
			StartDeforestationZone(_deforestionStartID);
			StartDeforestationZone(_deforestionStartID2);
            StartPolutionWay(106, new List<CellState>() { CellState.SNOW });
            StartPolutionWay(66, new List<CellState>() { CellState.MOSS, CellState.GRASS });
		}

		public void SetCulling(bool toActive)
		{
			if (toActive)
			{
				StartCoroutine(CullingCoroutine());
				activeCulling = true;
			}
			else
			{
				StopAllCoroutines();
				activeCulling = false;
			}
		}

		protected IEnumerator CullingCoroutine()
		{
			while (true)
			{
                /*Culling<Cell>.Instance.Update();
                Culling<BaseObject>.Instance.Update();*/
				yield return null;
			}
		}

		protected void OnYearPassed(OnNewYear e)
		{
            if (ResourcesManager.instance) ResourcesManager.instance.UpdateBudget();
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
                newWay.Start();
            }
        }

		public void StartDeforestationZone(int ID)
		{
			Cell refCell = EarthManager.Instance.Cells.Find(c => c.ID == ID);
			if (!refCell) return;

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
                newWay.Start();
            }
		}

		protected void OnDestroy()
		{
			_instance = null;
		}

        public void Clear()
        {
            DeforestationAreas.Clear();
            PolutionsWay.Clear();
        }

		#region Instance
		private static WorldManager _instance;
		public static WorldManager instance
		{
			get
			{
				return _instance;
			}
		}
		#endregion
	}
}