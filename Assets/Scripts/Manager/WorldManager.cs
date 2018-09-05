using UnityEngine;
using System;
using Assets.Scripts.Game;
using System.Collections.Generic;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Utils;
using System.Collections;
using Assets.Script;

namespace Assets.Scripts.Manager
{
	public enum SpawnValue { UP, DOWN }

	public class PolutionArea
	{
		public Cell initialCell;

        private CellState polutionState;
		private int cellIterator = 0;
		private int iteration = 0;
		private List<ObjectArray<Cell>> iterationCells = new List<ObjectArray<Cell>>();

		public PolutionArea(Cell refCell, CellState polutionType)
		{
			ObjectArray<Cell> newArray = new ObjectArray<Cell>();
            polutionState = polutionType;
			initialCell = refCell;
			newArray.Add(refCell);
			iterationCells.Add(newArray);
		}

		public void Start()
		{
			if (iteration == 0 && cellIterator == 0)
			{
				iterationCells.Clear();
				ObjectArray<Cell> newArray = new ObjectArray<Cell>();
				newArray.Add(initialCell);
				iterationCells.Add(newArray);
			}			
		}

		public void Action()
		{
			if (WorldValues.STATE_CLEANLINESS == 0) return;
			bool toPoluted = WorldValues.STATE_CLEANLINESS < 0;

			ObjectArray<Cell> iArray = iterationCells[iteration];
			if (toPoluted)
			{
				if (cellIterator < iArray.Length && cellIterator >= 0)
				{
					iArray.Objs[cellIterator].obj.SetPolution(toPoluted);
					cellIterator++;
				}
				else ExtendPolution(iArray.Objs);
			}
			else {
				if (cellIterator <= iArray.Length && cellIterator > 0)
				{
					cellIterator--;
					iArray.Objs[cellIterator].obj.SetPolution(toPoluted);					
				}
				else RetractPolution();
			}
		}

		protected void ExtendPolution(DataStruct<Cell>[] tCells)
		{
			ObjectArray<Cell> newArray = new ObjectArray<Cell>();

			int nbCell = tCells.Length;
			for (int i = 0; i < nbCell; i++)
			{
				int nbICell = tCells[i].obj.Neighbors.Count;
				for (int j = 0; j < nbICell; j++)
				{
					Cell neighbor = tCells[i].obj.Neighbors[j];
					if (!neighbor.Poluted && !newArray.Contains(neighbor) && neighbor.State == polutionState)
						newArray.Add(neighbor);
				}
			}
			
			if (newArray.Length > 0)
			{
				iteration++;
				cellIterator = 0;
				newArray.CheckIt();
				iterationCells.Add(newArray);
			}
		}

		protected void RetractPolution()
		{
			if (iteration > 0)
			{
				iterationCells.Remove(iterationCells[iteration]);
				iteration--;
				cellIterator = iterationCells[iteration].Length;
			}
		}
	}

	public class DeforestationArea
	{
        private static List<Cell> CELLS_USED = new List<Cell>();
        public static int NB_FOREST_CUT = 0;

		public Cell initialCell;

		private int cellIterator = 0;
		private int iteration = 0;
		private List<ObjectArray<Cell>> iterationCells = new List<ObjectArray<Cell>>();

		public DeforestationArea(int cID)
		{
			ObjectArray<Cell> newArray = new ObjectArray<Cell>();
			initialCell = EarthManager.Instance.Cells.Find(c => c.ID == cID);
			newArray.Add(initialCell);
			iterationCells.Add(newArray);
		}

		public void Start()
		{
			if (iteration == 0 && cellIterator == 0)
			{
				iterationCells.Clear();
				ObjectArray<Cell> newArray = new ObjectArray<Cell>();
				newArray.Add(initialCell);
				iterationCells.Add(newArray);
			}
		}

		public void Action()
		{
			bool toCut = WorldValues.STATE_FOREST < 0;

			ObjectArray<Cell> iArray = iterationCells[iteration];
			if (toCut)
			{
                if (WorldValues.STATE_FOREST >= -1.5f && NB_FOREST_CUT >= EarthManager.NB_FOREST_PROPS * 0.75f) return;
                if (cellIterator < iArray.Length && cellIterator >= 0)
				{
					foreach (KeyValuePair<Props, string> p in iArray.Objs[cellIterator].obj.Props)
					{
						if (p.Key.GetType() == typeof(PoolTree))
						{
							PoolTree pt = (PoolTree)p.Key;
							pt.SetDeforestation(toCut);
						}
					}
					cellIterator++;
				}
				else ExtendDeforestation(iArray.Objs);
			}
			else
			{
                if (WorldValues.STATE_FOREST <= 1.5f && NB_FOREST_CUT <= EarthManager.NB_FOREST_PROPS * 0.25f) return;
                if (cellIterator <= iArray.Length && cellIterator > 0)
				{
					cellIterator--;
					foreach (KeyValuePair<Props, string> p in iArray.Objs[cellIterator].obj.Props)
					{
						if (p.Key.GetType() == typeof(PoolTree))
						{
							PoolTree pt = (PoolTree)p.Key;
							pt.SetDeforestation(toCut);
						}
					}
				}
				else RetractDeforestation();
			}
		}

		protected void ExtendDeforestation(DataStruct<Cell>[] tCells)
		{
			ObjectArray<Cell> newArray = new ObjectArray<Cell>();

			int nbCell = tCells.Length;
			for (int i = 0; i < nbCell; i++)
			{
				int nbICell = tCells[i].obj.Neighbors.Count;
				for (int j = 0; j < nbICell; j++)
				{
					Cell neighbor = tCells[i].obj.Neighbors[j];
					if (!newArray.Contains(neighbor) && PoolTree.ForestCells.Contains(neighbor) && !CELLS_USED.Contains(neighbor))
					{
						newArray.Add(neighbor);
                        CELLS_USED.Add(neighbor);
                    }
				}
			}

			if (newArray.Length > 0)
			{
				iteration++;
				cellIterator = 0;
				newArray.CheckIt();
				iterationCells.Add(newArray);
			}
            else if (NB_FOREST_CUT < EarthManager.NB_FOREST_PROPS) ForceExtend();
		}

        protected void ForceExtend()
        {
            List<Cell> catchCells = new List<Cell>();   
            foreach (DataStruct<Cell> c in PoolTree.ForestCells.Objs)
            {
                if (!CELLS_USED.Contains(c.obj))
                {
                    bool containUncutAsset = false;
                    List<PoolTree> treesAsset = c.obj.GetProps<PoolTree>();
                    foreach (PoolTree pt in treesAsset)
                    {
                        if (!pt.IsCut)
                        {
                            containUncutAsset = true;
                            break;
                        }
                    }
                    if (containUncutAsset)
                    {
                        catchCells.Add(c.obj);
                        CELLS_USED.Add(c.obj);
                    } 
                }
            }

            if (catchCells.Count > 0)
            {
                iteration++;
                cellIterator = 0;
                ObjectArray<Cell> newArray = new ObjectArray<Cell>();
                int lLength = catchCells.Count;
                for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
                iterationCells.Add(newArray);
            }
        }

		protected void RetractDeforestation()
		{
			if (iteration > 0)
			{
                int lLength = iterationCells[iteration].Length;
                for (int i = 0; i < lLength; i++) CELLS_USED.Remove(iterationCells[iteration].Objs[i].obj);

				iterationCells.Remove(iterationCells[iteration]);
                iteration--;
				cellIterator = iterationCells[iteration].Length;
			}
            else if (NB_FOREST_CUT > 0) ForceRetract();
		}

        private void ForceRetract()
        {
            List<Cell> catchCells = new List<Cell>();
            foreach (DataStruct<Cell> c in PoolTree.ForestCells.Objs)
            {
                if (!CELLS_USED.Contains(c.obj))
                {
                    bool containCutAsset = false;
                    List<PoolTree> treesAsset = c.obj.GetProps<PoolTree>();
                    foreach (PoolTree pt in treesAsset)
                    {
                        if (pt.IsCut)
                        {
                            containCutAsset = true;
                            break;
                        }
                    }
                    if (containCutAsset)
                    {
                        catchCells.Add(c.obj);
                        CELLS_USED.Add(c.obj);
                    }
                }
            }

            if (catchCells.Count > 0)
            {
                iteration++;
                cellIterator = 0;
                ObjectArray<Cell> newArray = new ObjectArray<Cell>();
                int lLength = catchCells.Count;
                for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
                iterationCells.Add(newArray);
            }
        }
	}

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
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<OnEndPlanetCreation>(Init);
			Events.Instance.RemoveListener<OnNewYear>(OnYearPassed);
			Events.Instance.RemoveListener<OnNewMonth>(OnMonthPassed);
            Events.Instance.RemoveListener<OnUpdateForest>(HandleForestUpdate);
        }

		protected void Init(OnEndPlanetCreation e)
		{
			if (PlanetMaker.instance.edit != EditState.INACTIVE) return;
            PoolTree.ForestCells.CleanDoublon();
			StartDeforestationZone(_deforestionStartID);
			StartDeforestationZone(_deforestionStartID2);
			SetCulling(activeCulling);
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

		protected void Display<T>(float refWorldValue) where T : BaseObject
		{
			Type type = typeof(T);
			if (PoolingManager.Instance.PoolArrays.ContainsKey(type))
			{
				refWorldValue = Mathf.Clamp01(refWorldValue);
				PoolingManager IPool = PoolingManager.Instance;
				System.Random rnd = new System.Random();				
				while (IPool.GetTypeActivePrct<T>() < refWorldValue)
				{
					List<BaseObject> inactiveList = IPool.PoolArrays[type].FindAll(obj => (obj.gameObject.activeSelf == false));
					int index = rnd.Next(0, inactiveList.Count);
					int pIndex = IPool.PoolArrays[type].IndexOf(inactiveList[index]);
					IPool.PoolArrays[type][pIndex].gameObject.SetActive(true);
				}
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

		public void StartPolutionWay(Cell refCell)
		{
			PolutionArea area = PolutionsWay.Find(pa => pa.initialCell == refCell);
			if (area != null)
			{
				if (!refCell.Poluted) area.Start(); 
				return;
			}

			if (refCell && !refCell.Poluted)
			{
                if (PolutionsWay.Count <= 1)
                {
                    PolutionArea newWay = new PolutionArea(refCell, refCell.State);
                    PolutionsWay.Add(newWay);
                    newWay.Start();
                }
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

			DeforestationArea newWay = new DeforestationArea(ID);
			DeforestationAreas.Add(newWay);
			newWay.Start();
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