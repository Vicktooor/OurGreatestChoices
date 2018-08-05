using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Game;
using System.Collections;
using Assets.Scripts.Utils;
using Assets.Scripts.Items;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Game.Objects;

public enum EPlayer { NONE, ECO, GOV, NGO }

namespace Assets.Scripts.Manager
{
	public class EarthManager : MonoBehaviour
	{
        public static int NB_FOREST_PROPS = 0;

        public static List<CitizenProp> citizens = new List<CitizenProp>();

        public HoneycombPlanetSmooth _planetLink;

		public string[] assetsFolder;

		protected List<Cell> _cells = new List<Cell>();
		public List<Cell> Cells
		{
			get { return _cells; }
			set { _cells = value; }
		}

		protected List<GameObject> _cellsObj = new List<GameObject>();
		public List<GameObject> CellsObj
		{
			get { return _cellsObj; }
			set { _cellsObj = value; }
		}

		[SerializeField]
		protected GameObject _cellPrefab;
        [Header("World bubble prefab")]
        public BillboardBubble bubblePrefab;
        [Header("Help Sprite prefab")]
        public BillboardHelp helpSpritePrefab;

        public Dictionary<EPlayer, KeyValuePair<int, Vector3>> playerPosition = new Dictionary<EPlayer, KeyValuePair<int, Vector3>>();

		protected float _planetRadius;
		public float PlanetRadius { get { return _planetRadius; } }
       
        private bool _loaded = false;
        public bool Loaded { get { return _loaded; } }

        protected static EarthManager _instance;
		public static EarthManager Instance
		{
			get { return _instance; }
		}

		protected void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(this);
				throw new Exception("An instance of EarthManager already exists.");
			}
			else _instance = this;
		}

		public void Start()
		{
            if (PlanetMaker.instance.edit != EditState.INACTIVE)
            {
                PlanetSave.LoadPlanet(_planetLink.name);
                CreatePlanet();
            }
            else CreatePlanet();
		}

		public void CreatePlanet()
		{
			StartCoroutine(LoadCoroutine());
		}

		protected bool ResourcesLoaded()
		{
			int nbFolder = assetsFolder.Length;
			int nbLoaded = 0;
			for (int i = 0; i < assetsFolder.Length; i++)
			{
				if (ResourceLoader.GetProgress(assetsFolder[i]) >= 100) nbLoaded++;
			}

			if (nbLoaded >= nbFolder) return true;
			else return false;
		}

		protected IEnumerator LoadCoroutine()
		{
			/*for (int i = 0; i < assetsFolder.Length; i++)
			{
				ResourceLoader.LoadFolderAsync(assetsFolder[i]);
			}

			while (!ResourcesLoaded())
			{
				yield return null;
			}*/
			yield return null;
			_planetLink.StartGeneration(CreateFullPlanet);
		}

		public void CreateProps(GameObject model, Vector3 pos, Cell associateCell)
		{
			Quaternion orientation = Quaternion.FromToRotation(Vector3.up, associateCell.GetCenterPosition());
			Props newGo = Instantiate(model.GetComponent<Props>(), pos, orientation, associateCell.transform);
			newGo.associateCell = associateCell;
			newGo.transform.position += pos.normalized * newGo.offsetY;
            associateCell.Props.Add(newGo, model.name);
			associateCell.PropsCollider.Add(newGo, newGo.GetCollider());

            CatchTree(newGo, associateCell);
        }

		public GameObject CreateSavedProps(GameObject model, Vector3 pos, Cell associateCell, Quaternion orientation)
		{
			if (model == null) return null;
			Type subType = model.GetType();
			Props modelProps = model.GetComponent<Props>();
			if (modelProps == null) return null;
			Props newGo = Instantiate(modelProps, pos, orientation, associateCell.transform);
			newGo.associateCell = associateCell;
			newGo.transform.position += pos.normalized * newGo.offsetY;
            associateCell.Props.Add(newGo, model.name);
			associateCell.PropsCollider.Add(newGo, newGo.GetCollider());

            CatchTree(newGo, associateCell);

            PoolingManager.Instance.CreatePoolObject(newGo);
            return newGo.gameObject;
		}

		public void CreateSavedProps(GameObject model, Vector3 pos, Cell associateCell, Quaternion orientation, BuildingLink link)
		{
			Type subType = model.GetType();
			LinkObject item;
			Props modelProps = model.GetComponent<Props>();
			if (modelProps == null) return;
			Props newGo = Instantiate(modelProps, pos, orientation, associateCell.transform);
			newGo.associateCell = associateCell;
			newGo.transform.position += pos.normalized * newGo.offsetY;
			if (newGo.GetComponent<LinkObject>())
			{
				item = newGo.GetComponent<LinkObject>();
				item.BuildingLink = link;
				item.Creation();
			}
			associateCell.Props.Add(newGo, model.name);
			associateCell.PropsCollider.Add(newGo, newGo.GetCollider());

			PoolingManager.Instance.CreatePoolObject(newGo);
		}

		protected void CreateCells()
		{
            foreach (GroundMesh ground in _planetLink.allGroundMesh)
            {
                GameObject newGo = Instantiate(_cellPrefab, Vector3.zero, Quaternion.identity, _planetLink.transform);
                _cellsObj.Add(newGo);

                Cell newCell = newGo.GetComponent<Cell>();
                newCell.SetID();
                newCell.Init(ground);
                _cells.Add(newCell);
            }

            _planetLink.GenerateNeighborLinks(_planetLink.GetMinimalNeighborDistance(), _cells);

            foreach (Cell cCell in _cells)
			{
				Mesh finalMesh = cCell.gameObject.GetComponent<MeshFilter>().mesh;

				if (PlanetSave.SavedCells.Count > 0)
				{
					foreach (SaveCell savedCell in PlanetSave.SavedCells)
					{
						if (savedCell.ID == cCell.ID)
						{
							cCell.Init(savedCell);
							break;
						}
					}
				}

				finalMesh.vertices = cCell.GroundMesh.smoothVertex;
				finalMesh.triangles = cCell.GroundMesh.smoothTriangles;
				finalMesh.normals = cCell.GroundMesh.smoothNormals;
				finalMesh.uv = cCell.GroundMesh.UVs;

				cCell.gameObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
				finalMesh.RecalculateBounds();
			}		
		}

		public void CreateFullPlanet()
		{
            if (GameManager.Instance)
            {
                if (GameManager.PARTY_TYPE == EPartyType.NEW) PlanetSave.LoadPlanet(_planetLink.name);
                else if (GameManager.PARTY_TYPE == EPartyType.NEW || GameManager.PARTY_TYPE == EPartyType.SAVE)
                {
                    for (int i = 0; i < PlanetSave.GameStateSave.SavedCells.Length; i++)
                        PlanetSave.SavedCells[i] = PlanetSave.GameStateSave.SavedCells[i];
                }
                else return;
            }           

            CreateCells();
            foreach (Cell c in _cells)
            {
                foreach (KeyValuePair<Props, string> p in c.Props)
                {
                    p.Key.UpdatePosition();
                }
            }
            RecalculateUVMap();
            if (!_loaded)
            {
                LoadDialogues();
                LoadBudget();
                _loaded = true;
            }

            Events.Instance.Raise(new OnEndPlanetCreation());
        }

		protected void RandomizeNewPolution(bool pPoluted)
		{
			/// do stuff here
		}

		public void RecalculateUVMap()
		{
			foreach (Cell lcell in _cells) lcell.InitColor();
			foreach (Cell lcell in _cells) lcell.SetVertexColors();
		}

		public void SavePlanet()
		{
			PlanetSave.SavePlanet(_cells, _planetLink.name);
		}

		public void SavePlayer()
		{
			PlanetSave.SavePlayer(_planetLink.name);
		}

		public void SaveDialogues()
		{
			PlanetSave.SavePNJDialogues(_planetLink.name);
		}

		public void SaveBudget()
		{
			PlanetSave.SaveBudgetConfiguration(_planetLink.name);
		}

		protected void OnDestroy()
		{
			_instance = null;
		}

		public void LoadDialogues()
		{
			if (PlanetSave.LoadPNJDialogues(_planetLink.name)) Events.Instance.Raise(new OnDialoguesLoaded());
		}

		public void LoadBudget()
		{
			if (PlanetSave.LoadBudget(_planetLink.name)) Events.Instance.Raise(new OnBudgetLoaded());
		}

        public void CatchTree(Props prop, Cell aCell)
        {
            if (prop.GetType() == typeof(PoolTree)) NB_FOREST_PROPS++;
        }

        public void Clear()
        {
            _cellsObj.Clear();
            _cells.Clear();
            playerPosition.Clear();
            citizens.Clear();
        }
	}
}
