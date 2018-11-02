using UnityEngine;
using System;
using Assets.Scripts.Game;
using System.Collections.Generic;
using Assets.Scripts.DecalSystem;
using Assets.Scripts.Planet;
using Assets.Scripts.Manager;
using Assets.Scripts.Save;
using Assets.Scripts.Game.Save;

// AUTHOR - Victor

namespace Assets.Scripts.Utils
{
	public enum Snap { ANYWHERE, CENTER, CORNER, EXTERN_EDGE, INTERN_EDGE }
	public enum EditState { INACTIVE, NOTHING, GROUND, PROPS, DECAL, PLAYER_POSITION }
	public enum GroundEditState { ELEVATION, BIOME }
	public enum PropsEditState { ADD, REMOVE, ROTATION, REPLACE, MULTI_ONE_BY_ONE, MULTI_BY_CELL }
	public enum DecalLayer { GROUND, SEA, BUILD }

	/// <summary>
	/// 
	/// </summary>
	public class PlanetMaker : MonoBehaviour
    {
        #region Instance
        private static PlanetMaker _instance;

        /// <summary>
        /// instance unique de la classe     
        /// </summary>
        public static PlanetMaker instance
        {
            get
            {
                return _instance;
            }
        }
		#endregion

		public bool showButtons = false;

		#region Edition properties
		[Header("Pre Compilation")]
		[SerializeField]
		protected bool _randomGroundColor = false;
		public bool RandomGroundColor { get { return _randomGroundColor; } }

		[Header("Runtime Edition")]
		public EditState edit = EditState.NOTHING;

		[SerializeField]
		protected float _smoothNormalFactor;
		public float SmoothNormalFactor { get { return _smoothNormalFactor; } }	

		[Header("Ground Edition")]
        [SerializeField]
        protected GroundEditState _groundEditState;
        public GroundEditState GroundEditState
        {
            get { return _groundEditState; }
        }
        public bool showUsingTiles = true;
        [SerializeField]
        protected AnimationCurve _elevationCurve;
        public AnimationCurve ElevationCurve
        {
            get { return _elevationCurve; }
        }
        [SerializeField]
        protected int _nbNeighbors;
        public int NbNeighbors
        {
            get { return _nbNeighbors; }
        }

		[SerializeField]
		protected List<BiomeColor> _colors;
		public List<BiomeColor> Colors { get { return _colors; } }

        [Header("Snapping")]
        public Snap snapTo = Snap.ANYWHERE;

        [Header("Props Edition")]
        public PropsEditState propsEditType = PropsEditState.ADD;
        public bool showUsingProps = true;
        [SerializeField]
        protected GameObject _propsModel;

        protected int _holdingTimer = 0;

		[Header("Decal Edition")]
		[SerializeField]
		protected DecalLayer _decalType;
		[SerializeField]
		protected Material _decalMaterial;
		[SerializeField]
		protected Sprite _decalSprite;
		[SerializeField]
		protected float _pushDistance = 0.1f;
		public float PushDistance { get { return _pushDistance; } }
		[SerializeField]
		protected string _decalName;

		[Header("Player Edition")]
		[SerializeField]
		protected EPlayer selectedPlayer;

		[Header("Pathfinding")]
		[SerializeField]
		protected bool _testPathfinding = false;
		#endregion

		protected Cell _selectedCell;
        protected Props _selectedProp;

        protected List<Cell> _selectedCells = new List<Cell>();
        protected List<Props> _selectedProps = new List<Props>();

        protected Vector3 _selectionMousePosition;
        protected float _selectionClickElevation;

        protected List<Cell> _movingCells = new List<Cell>();
		protected Cell _pathfindingTarget;

        protected void Awake()
        {
            if (_instance != null)
            {
                throw new Exception("Tentative de création d'une autre instance de PlanetMaker alors que c'est un singleton.");
            }
            _instance = this;
        }

        protected void Update()
        {        
			if (_testPathfinding)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				TestPathfinding(ray);
				return;
			}

			if (edit != EditState.NOTHING && edit != EditState.INACTIVE)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (edit == EditState.GROUND) EditGround(ray);
				else if (edit == EditState.PROPS) EditProps(ray);
				else if (edit == EditState.DECAL) EditDecal();
				else if (edit == EditState.PLAYER_POSITION) EditPlayer(ray);
			}
        }    

		/// <summary>
		/// Ground edition function
		/// </summary>
		/// <param name="ray">Mouse raycast</param>
		protected void EditGround(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
            {				
                if (Input.GetMouseButtonDown(0))
                {
					if (_selectedCell != hit.transform.gameObject)
					{
						if (_groundEditState != GroundEditState.BIOME) _selectedCells.Clear();

						_selectedCell = hit.transform.gameObject.GetComponent<Cell>();
						_selectionMousePosition = Input.mousePosition;
						_selectionClickElevation = _selectedCell.Elevation;
						_holdingTimer = 1;
						_movingCells = GetElevationCells();

						if (!_selectedCells.Contains(_selectedCell)) _selectedCells.Add(_selectedCell);
						
						if (_groundEditState == GroundEditState.BIOME)
						{
							foreach (Cell toMoveCell in _movingCells)
								if (!_selectedCells.Contains(toMoveCell)) _selectedCells.Add(toMoveCell);
						}
					}
				}              
            }

			if (Input.GetMouseButton(0) && _holdingTimer >= 1 && _groundEditState != GroundEditState.BIOME && !_testPathfinding)
			{
				float diff = (Input.mousePosition - _selectionMousePosition).normalized.y * (Time.deltaTime / 15f);
				_selectedCell.UpdateHeight(diff);
				_holdingTimer++;
				_selectionMousePosition = Input.mousePosition;

				foreach (Cell toMoveCell in _movingCells) toMoveCell.UpdateHeightFromCurve(_elevationCurve, diff);
				foreach (Cell c in EarthManager.Instance.Cells) c.UpdateCollider();
			}

			if (Input.GetMouseButtonUp(0))
            {
                if (_selectedCell != null && _selectedCell.gameObject.activeSelf)
                {
                    _selectedCell.SetElevationID(-1);
                    _selectedCell = null;
                    _holdingTimer = 0;
                }

                foreach (Cell lCell in _movingCells) lCell.SetElevationID(-1);
                _movingCells.Clear();
            }
        }

		/// <summary>
		/// Get all tiles to move
		/// </summary>
		/// <returns></returns>
		protected List<Cell> GetElevationCells()
		{
			List<Cell> toMoveCells = new List<Cell>();

			_selectedCell.SetElevationID(0);
			List<Cell> neighborsCellList = _selectedCell.SetNeighborsElevationID();
			foreach (Cell validCell in neighborsCellList) toMoveCells.Add(validCell);
			List<Cell> transitionNeighborsCellList = new List<Cell>();

			int neighborCounter = neighborsCellList.Count;
			while (neighborCounter > 0)
			{
				foreach (Cell neighbor in neighborsCellList)
				{
					List<Cell> lNeighborsCellList = neighbor.SetNeighborsElevationID();
					foreach (Cell lCell in lNeighborsCellList)
					{
						transitionNeighborsCellList.Add(lCell);
					}
				}

				neighborCounter = transitionNeighborsCellList.Count;
				List<Cell> newList = new List<Cell>();
				foreach (Cell transitionCell in transitionNeighborsCellList)
				{
					newList.Add(transitionCell);
				}
				neighborsCellList = newList;
				transitionNeighborsCellList.Clear();

				foreach (Cell validCell in neighborsCellList) toMoveCells.Add(validCell);
			}

			return toMoveCells;
		}

		/// <summary>
		/// Props edition function
		/// </summary>
		/// <param name="ray">Mouse raycast</param>
		protected void EditProps(Ray ray)
        {
            RaycastHit hit;
            if (propsEditType == PropsEditState.ADD && Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cell lCell = hit.transform.gameObject.GetComponent<Cell>();
                    switch (snapTo)
                    {
                        case Snap.ANYWHERE:
							EarthManager.Instance.CreateProps(_propsModel, hit.point, hit.transform.gameObject.GetComponent<Cell>());
                            break;
                        case Snap.CENTER:
							EarthManager.Instance.CreateProps(_propsModel, lCell.GetCenterPosition() * lCell.transform.localScale.x, lCell);
                            break;
                        case Snap.EXTERN_EDGE:
							EarthManager.Instance.CreateProps(_propsModel, Cell.GetNearestTypedVertex(lCell.GroundMesh.indexes.edgeVertexIndex, hit.point, lCell.GroundMesh) * lCell.transform.localScale.x, lCell);
                            break;
						case Snap.INTERN_EDGE:
							EarthManager.Instance.CreateProps(_propsModel, Cell.GetNearestTypedVertex(lCell.GroundMesh.indexes.internVertexIndex, hit.point, lCell.GroundMesh) * lCell.transform.localScale.x, lCell);
							break;
						case Snap.CORNER:
							EarthManager.Instance.CreateProps(_propsModel, Cell.GetNearestTypedVertex(lCell.GroundMesh.indexes.cornerVertexIndex, hit.point, lCell.GroundMesh) * lCell.transform.localScale.x, lCell);
                            break;
						default:
                            break;
                    }
                }
            }
            else if (propsEditType == PropsEditState.REMOVE && Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[3] { "Nature", "Interactable", "Building" })))
            {
                if (Input.GetMouseButtonDown(0))
                {
					_selectedProp = hit.transform.gameObject.GetComponent<Props>();
					if (!_selectedProp) _selectedProp = hit.transform.gameObject.GetComponentInParent<Props>();
                    _selectedProp.associateCell.RemoveProps(_selectedProp);
                    if (_selectedProps.Contains(_selectedProp)) _selectedProps.Remove(_selectedProp);
                    Destroy(_selectedProp.gameObject);
                }
            }
            else if (propsEditType == PropsEditState.MULTI_ONE_BY_ONE || propsEditType == PropsEditState.ROTATION)
            {
                if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[3] { "Nature", "Interactable", "Building" })))
                {
                    _selectedProp = hit.transform.gameObject.GetComponent<Props>();
					if (!_selectedProp) _selectedProp = hit.transform.gameObject.GetComponentInParent<Props>();

					if (_selectedProp != hit.transform.gameObject)
                    {
                        if (!_selectedProps.Contains(_selectedProp) && propsEditType == PropsEditState.ROTATION)
                        {
                            _holdingTimer = 1;
                            _selectedProps.Clear();
                            _selectedProps.Add(_selectedProp);
                        }
                        else if (!_selectedProps.Contains(_selectedProp) && propsEditType == PropsEditState.MULTI_ONE_BY_ONE)
                        {                         
                            _selectedProps.Add(_selectedProp);
                        }
                    }
                }

                if (Input.GetMouseButton(0) && _holdingTimer >= 1 && _selectedProp != null)
                {
                    float diff = (Input.mousePosition - _selectionMousePosition).normalized.x * (Time.deltaTime / 15f);
                    _holdingTimer++;
                    _selectionMousePosition = Input.mousePosition;

                    Vector3 rotateAxis = _selectedProp.associateCell.Axis;
					_selectedProp.transform.RotateAround(_selectedProp.transform.position, rotateAxis, diff * 360);
                }
            }
			else if (propsEditType == PropsEditState.MULTI_BY_CELL)
			{
				if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
				{
					Cell lClickedCell = hit.transform.gameObject.GetComponent<Cell>();
					foreach (KeyValuePair<Props, string> item in lClickedCell.Props)
					{
						if (!_selectedProps.Contains(_selectedProp)) _selectedProps.Add(item.Key);
					}
				}
			}
			else if (propsEditType == PropsEditState.REPLACE)
			{		
				if (_selectedProp != null)
				{
					Cell toUseCell = _selectedProp.associateCell;
					if (toUseCell.GetComponent<MeshCollider>().Raycast(ray, out hit, 100.0f))
					{
						_selectedProp.transform.position = hit.point + (hit.point.normalized * _selectedProp.offsetY);
						_selectedProp.transform.rotation = Quaternion.FromToRotation(Vector3.up, toUseCell.GetCenterPosition());
						if (Input.GetMouseButtonDown(0))
						{
							_selectedProp = null;
						}
					}
					else if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
					{
						_selectedProp.associateCell = hit.transform.GetComponent<Cell>();
						_selectedProp.transform.position = hit.point + (hit.point.normalized * _selectedProp.offsetY);
						_selectedProp.transform.rotation = Quaternion.FromToRotation(Vector3.up, toUseCell.GetCenterPosition());
						if (Input.GetMouseButtonDown(0))
						{
							_selectedProp.associateCell.Props.Remove(_selectedProp);
							hit.transform.GetComponent<Cell>().Props.Add(_selectedProp, _selectedProp.name);
							_selectedProp = null;
						}
					}
				}
				else
				{
					if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[3] { "Nature", "Interactable", "Building" })))
					{
						_selectedProp = hit.transform.gameObject.GetComponent<Props>();
						if (!_selectedProp) _selectedProp = hit.transform.gameObject.GetComponentInParent<Props>();
					}
				}			
			}
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _selectedProp = null;
                _selectedProps.Clear();
            }
        }

        protected void EditDecal()
		{
			Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[2] { "Cell", "Water" })))
			{
				if (Input.GetKeyDown(KeyCode.R))
				{
					if (_decalMaterial != null && _decalSprite!= null)
					{
						Quaternion orientation = Quaternion.LookRotation(ray.direction, -Camera.main.transform.right);
						GameObject newGo = Instantiate(Resources.Load("Decal"), hit.point, orientation) as GameObject;
						Decal newDecal = newGo.GetComponent<Decal>();
						newDecal.Init(_decalSprite, _decalMaterial, _decalType, _decalName);
					}
				}
			}
		}

		protected void EditPlayer(Ray ray)
		{
			RaycastHit hit;
			if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
			{
				PlayerEditorPos[] playersVisu = GetComponentsInChildren<PlayerEditorPos>();
				if (!EarthManager.Instance.playerPositions.ContainsKey(selectedPlayer))
				{
					KeyValuePair<int, Vector3> posInfo = new KeyValuePair<int, Vector3>(hit.transform.GetComponent<Cell>().ID, hit.point);
					EarthManager.Instance.playerPositions.Add(selectedPlayer, posInfo);
					foreach (PlayerEditorPos pv in playersVisu)
					{
						if (pv.associedPlayer == selectedPlayer) pv.gameObject.transform.position = hit.point;
					}
				}
				else
				{
					KeyValuePair<int, Vector3> posInfo = new KeyValuePair<int, Vector3>(hit.transform.GetComponent<Cell>().ID, hit.point);
					EarthManager.Instance.playerPositions[selectedPlayer] = posInfo;
					foreach (PlayerEditorPos pv in playersVisu)
					{
						if (pv.associedPlayer == selectedPlayer) pv.gameObject.transform.position = hit.point;
					}
				}
			}
		}

		/// <summary>
		/// Pathfinding function
		/// </summary>
		/// <param name="ray">Mouse raycast</param>
		protected void TestPathfinding(Ray ray)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
			{
				if (Input.GetMouseButtonDown(0))
				{
					if (_selectedCells.Count < 2)
					{
						Cell clickedCell = hit.transform.GetComponent<Cell>();
						if (!_selectedCells.Contains(clickedCell)) _selectedCells.Add(clickedCell);
					}
				}
			}

			if (_selectedCells.Count == 2)
			{
				Pathfinding.GetPathFromTo(_selectedCells[0], _selectedCells[1]);
			}
		}

		public void RecalulateNormals()
		{
			foreach (Cell lcell in EarthManager.Instance.Cells) lcell.RecalculateNormals();
		}

		#region OnGUI methods
#if UNITY_EDITOR
		/// <summary>
		/// Show editor buttons in game window
		/// </summary>
		public string biomeIDEdit = "";
		public void OnGUI()
        {
			if (!showButtons) return;

            if (GUI.Button(new Rect(10, 10, 50, 25), "Save")) PlanetSave.SaveCells(EarthManager.Instance.Cells, EarthManager.Instance.playingPlanetName);
            if (GUI.Button(new Rect(10, 45, 120, 25), "Save Player")) PlanetSave.SavePlayer(EarthManager.Instance.playingPlanetName);
			if (GUI.Button(new Rect(70, 10, 50, 25), "Light")) RecalulateNormals();
			if (GUI.Button(new Rect(130, 10, 50, 25), "UVs")) EarthManager.Instance.RecalculateUVMap();
            if (GUI.Button(new Rect(300, 10, 120, 25), "Save Pnjs")) PlanetSave.SavePnjs(EarthManager.Instance.playingPlanetName);
            if (GUI.Button(new Rect(430, 10, 120, 25), "Save Citizens")) PlanetSave.SaveCitizens(EarthManager.Instance.playingPlanetName);
            if (GUI.Button(new Rect(430, 70, 50, 25), "Load")) EarthManager.Instance.CreateOnlyPlanet();

			float y = 100;
            if (_selectedCells.Count > 0)
            {
                foreach (CellState state in Enum.GetValues(typeof(CellState)))
                {
                    if (GUI.Button(new Rect(10, y, 90, 25), state.ToString()))
                    {
                        foreach (Cell lCell in _selectedCells)
                        {
                            lCell.SetCellState(lCell.GroundMesh, state);
                        }
                    }

                    y += 35;
                }

				biomeIDEdit = GUI.TextField(new Rect(10, y, 20, 20), biomeIDEdit, 25);
				bool apply = GUI.Button(new Rect(35, y, 60, 20), "Apply");
				if (string.Empty != biomeIDEdit && apply)
				{
					foreach (Cell lCell in _selectedCells)
					{
						lCell.BiomeID = int.Parse(biomeIDEdit);
					}
				}

				if (GUI.Button(new Rect(10, y + 35, 20, 20), "X"))
                {
                    _selectedCells.Clear();
                    _selectedCell = null;
                }
            }

			if (_selectedProps.Count > 0)
			{
				if (GUI.Button(new Rect(10, 100, 70, 25), "DELETE"))
				{
					foreach (Props p in _selectedProps)
					{
						Destroy(p.transform.gameObject.GetComponent<Props>());
					}

					_selectedProp = null;
					_selectedProps.Clear();
				}

				if (GUI.Button(new Rect(10, 160, 20, 20), "X") || Input.GetKeyDown(KeyCode.Q))
				{
					_selectedProp = null;
					_selectedProps.Clear();
				}
			}
		}

        /// <summary>
        /// Show selected cell
        /// </summary>
        protected void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.8f, 0.8f, 0, 0.5f);
            if (_selectedCells.Count > 0)
            {
                if (!showUsingTiles) return;

                foreach (Cell lCell in _selectedCells)
                {
                    Gizmos.DrawWireMesh(lCell.GetComponent<MeshFilter>().mesh, lCell.transform.position, lCell.transform.rotation, lCell.transform.localScale);
                }

                foreach (Cell lCell in _movingCells)
                {
                    foreach (Vector3 v in lCell.GroundMesh.computedVertex)
                    {
                        if (v != lCell.GroundMesh.centerPosition && v != Vector3.zero)
                            Gizmos.DrawWireMesh(lCell.GetComponent<MeshFilter>().mesh, lCell.transform.position, lCell.transform.rotation, lCell.transform.localScale);
                    }
                }
            }

            if (_selectedProps.Count > 0)
            {
                if (!showUsingProps) return;

                foreach (Props p in _selectedProps)
                {
                    if (_selectedProp == null)
                    {
                        _selectedProps.Remove(p);
                        continue;
                    }
					MeshFilter[] objMeshes = p.GetComponentsInChildren<MeshFilter>();
					foreach (MeshFilter lMesh in objMeshes)
					{
						Gizmos.DrawWireMesh(lMesh.mesh, p.transform.position, p.transform.rotation, p.transform.localScale);
					}                  
                }
            }

			if (edit != EditState.NOTHING && edit != EditState.INACTIVE && edit != EditState.GROUND)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask(new string[1] { "Cell" })))
                    {
                        Cell lCell = hit.transform.gameObject.GetComponent<Cell>();
                        Gizmos.DrawWireMesh(hit.transform.gameObject.GetComponent<MeshFilter>().mesh, lCell.transform.position, lCell.transform.rotation, lCell.transform.localScale);
                    }
                }
            }
        }
#endif
		#endregion

		protected void OnDestroy()
		{
			_instance = null;
		}
	}
}