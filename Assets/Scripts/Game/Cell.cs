using Assets.Script;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VerticesIndex
{
	public int[] edgeVertexIndex;
	public int[] cornerVertexIndex;
	public int[] internVertexIndex;

	public VerticesIndex(int nFace)
	{
		edgeVertexIndex = new int[nFace * 2];
		cornerVertexIndex = new int[nFace];
		internVertexIndex = new int[nFace];
	}
}

// AUTHOR - Victor
namespace Assets.Scripts.Game
{
    public enum CellState { GRASS, SAND, SNOW, MOSS, ROCK, TOWN, SEA }

	/// <summary>
	/// Cellule
	/// </summary>
	public class Cell : BaseObject
	{
        new public Vector3 Position { get { return GetCenterPosition(); } }

		protected static int ITERATOR_ID = 0;

        public Material matModel;

        public static int NB_CELL { get { return ITERATOR_ID; } }

		protected Mesh _personnalMesh;
		protected Color _color;
		public Color Color { get { return _color; } }
		protected int _colorIndex;

		protected VerticesIndex _indexes;
		public VerticesIndex Indexes { get { return _indexes; } }

		protected Vector3 _axis;
		public Vector3 Axis { get { return _axis; } }

        protected int _id;
        public int ID { get { return _id; } }

        [SerializeField]
        protected CellState _state;
        public CellState State { get { return _state; } }

		[SerializeField]
		protected int _biomeID;
		public int BiomeID { get { return _biomeID; } set { _biomeID = value; } }

		protected bool _poluted;
		public bool Poluted { get { return _poluted; } }

        protected bool _deforested;
        public bool Deforested { get { return _deforested; }  set { _deforested = value; } }

		protected float _stateHealth;
        protected bool _walkable;
        [SerializeField]
        protected int _neighborElevationID;
        public int NeighborElevationID { get { return _neighborElevationID; } }

        protected float _elevation;
        public float Elevation { get { return _elevation; } }

		protected GroundMesh _groundMesh;
        public GroundMesh GroundMesh { get { return _groundMesh; } }

		protected List<Cell> _neighbors = new List<Cell>();
        public List<Cell> Neighbors { get { return _neighbors; } }

        protected Dictionary<Props, string> _props = new Dictionary<Props, string>();
        public Dictionary<Props, string> Props { get { return _props; } }

		protected Dictionary<Props, Collider[]> _propsCollider = new Dictionary<Props, Collider[]>();
		public Dictionary<Props, Collider[]> PropsCollider { get { return _propsCollider; } }

		private Color _initialColor;

        public bool walkable = true;

        private BillboardNPCState _npcBillboardState;
        private BillboardBubble _citizenBubble;
        private BillboardHelp _pnjHelp;

		#region Edition protperties
		protected bool _isElevating;
		#endregion

		/// <summary>
		/// First init
		/// </summary>
		public void Init(GroundMesh meshObject)
        {
			meshObject.centerIndex = CustomGeneric.ArrayContain(meshObject.smoothVertex, meshObject.centerPosition);
			_indexes = meshObject.indexes;           
            _stateHealth = 1;			
            _walkable = true;
            _elevation = 1f;
            _isElevating = false;
            _neighborElevationID = -1;
            _groundMesh = meshObject;
            _poluted = false;
            _deforested = false;
            _axis = GetCenterPosition();
			_personnalMesh = GetComponent<MeshFilter>().mesh;
			_personnalCollider = GetComponents<MeshCollider>();
			_state = CellState.MOSS;
			UpdateHeight(0);
			InitColor();
		}

		/// <summary>
		/// Init saved properties
		/// </summary>
		/// <param name="savedProperties"></param>
        public void Init(SaveCell savedProperties)
        {
			_state = savedProperties.state;
            _walkable = savedProperties.walkable;

            if (savedProperties.state == CellState.SEA) _walkable = false;
            UpdateHeight(savedProperties.elevation - _groundMesh.centerPosition.magnitude);
            InitColor();

			if (savedProperties.Names != null)
				SpawnSavedProps(savedProperties);
        }

        public void ForcePolution(bool state)
        {
            _poluted = state;
            EarthManager.PoluteCell(_state, _poluted);
            ChangeColor(_poluted);
        }
		
		public void SetPolution(bool isPoluted)
		{
			bool lPoluted = _poluted;
			_poluted = isPoluted;
            if (lPoluted != isPoluted)
            {
                EarthManager.PoluteCell(_state, isPoluted);
                ChangeColor(isPoluted);
            }
		}

        public void SetDeforestation(bool toCut)
        {
            _deforested = toCut;
            foreach (KeyValuePair<Props, string> p in _props)
            {
                if (p.Key.GetType() == typeof(PoolTree))
                {
                    PoolTree pt = (PoolTree)p.Key;
                    pt.SetDeforestation(toCut);
                }
            }
        }

		protected void ChangeColor(bool poluted)
		{
            float polution = (poluted) ? 1f : 0f;
            _color = Color.Lerp(_initialColor, PlanetMaker.instance.Colors.Find(c => c.type == _state).PolutedColor, polution);
            SetVertexColors();
            foreach (Cell c in Neighbors) c.SetVertexColors();
        }

        /// <summary>
        /// Spawn saved Props
        /// </summary>
        /// <param name="savedItem"></param>
        protected void SpawnSavedProps(SaveCell savedItem)
		{
			int nbProps = savedItem.Names.Length;
			for (int i = 0; i < nbProps; i++)
			{
				if (savedItem.Names[i] == null || savedItem.Names[i] == string.Empty) continue;

                GameObject model = Resources.Load<GameObject>("Game/" + savedItem.Names[i]);
				if (savedItem.buildingLinkedItems[i].type != EnumClass.TypeBuilding.None && model)
				{
					EarthManager.Instance.CreateSavedProps(
						model,
						new Vector3(savedItem.propsX[i], savedItem.propsY[i], savedItem.propsZ[i]),
						this,
						new Quaternion(savedItem.propsRotX[i], savedItem.propsRotY[i], savedItem.propsRotZ[i], savedItem.propsRotW[i]),
						savedItem.buildingLinkedItems[i]
					);
				}
				else if (model)
				{
					EarthManager.Instance.CreateSavedProps(
						model,
						new Vector3(savedItem.propsX[i], savedItem.propsY[i], savedItem.propsZ[i]),
						this,
						new Quaternion(savedItem.propsRotX[i], savedItem.propsRotY[i], savedItem.propsRotZ[i], savedItem.propsRotW[i])
					);
				}
				else Debug.LogError("Modele de props non existant (" + savedItem.Names[i] + ") dans le dossier cible !");		
			}
		}

		/// <summary>
		/// Adjust mesh elevation
		/// </summary>
		/// <param name="inputValue"></param>
		public void UpdateHeight(float inputValue)
        {
			DeformMesh(inputValue);		
			_elevation = _groundMesh.centerPosition.magnitude;
			MeshCollider lSelfMeshCollider = (MeshCollider)_personnalCollider[0];
			lSelfMeshCollider.sharedMesh = _personnalMesh;
			foreach (KeyValuePair<Props, string> nat in _props) nat.Key.UpdatePosition();
		}

		/// <summary>
		/// Adjust mesh elevation from curve
		/// </summary>
		/// <param name="refCurve"></param>
		/// <param name="refDist"></param>
		public void UpdateHeightFromCurve(AnimationCurve refCurve, float refDist)
		{
			float step = refCurve.keys[refCurve.keys.Length - 1].time / (PlanetMaker.instance.NbNeighbors + 1);
			UpdateHeight(refCurve.Evaluate(step * _neighborElevationID) * refDist);
		}

		/// <summary>
		/// Defrom mesh from input value
		/// </summary>
		/// <param name="inputDist"></param>
		protected void DeformMesh(float inputDist)
		{
			_groundMesh.smoothVertex[_groundMesh.centerIndex] += _groundMesh.smoothVertex[_groundMesh.centerIndex].normalized * inputDist;
			_groundMesh.centerPosition = _groundMesh.smoothVertex[_groundMesh.centerIndex];
			foreach (int index in Indexes.internVertexIndex)
			{
				_groundMesh.smoothVertex[index] += _groundMesh.smoothVertex[index].normalized * inputDist;
			}
			_personnalMesh.vertices = _groundMesh.smoothVertex;

			UpdateCornerHeight();
			UpdateEdgeVertex();
			_personnalMesh.vertices = _groundMesh.smoothVertex;
		}

		/// <summary>
		/// Move corner vertices
		/// </summary>
		protected void UpdateCornerHeight()
		{
			foreach (int myindex in _groundMesh.indexes.cornerVertexIndex)
			{
				int iterator = 0;
				int[] internVertexIndexes = new int[2];
				int[] cornerVertexIndexes = new int[2];
				Cell[] linkedCells = new Cell[2];

				foreach (Cell fneighbor in _neighbors)
				{
					foreach (int index in fneighbor.Indexes.cornerVertexIndex)
					{
						Vector3 nVertex1 = _groundMesh.smoothVertex[myindex].normalized;
						Vector3 nVertex2 = fneighbor.GroundMesh.smoothVertex[index].normalized;

						if (nVertex1 == nVertex2)
						{
							List<int> internVertexIndex = GetLinkedInternVertexIndexes(fneighbor.GroundMesh, index);
							linkedCells[iterator] = fneighbor;
							internVertexIndexes[iterator] = internVertexIndex[0];
							cornerVertexIndexes[iterator] = index;
							iterator++;
						}
					}
				}

				if (iterator == 2)
				{
					List<int> myInternVertexIndex = GetLinkedInternVertexIndexes(_groundMesh, myindex);

					Vector3[] cloudOfPoint = new Vector3[3]
					{
						_groundMesh.smoothVertex[myInternVertexIndex[0]],
						linkedCells[0].GroundMesh.smoothVertex[internVertexIndexes[0]],
						linkedCells[1].GroundMesh.smoothVertex[internVertexIndexes[1]]
					};

					Vector3 G = MathCustom.GetBarycenter(cloudOfPoint);
					_groundMesh.smoothVertex[myindex] = G;

					linkedCells[0].UpdateVertexPos(cornerVertexIndexes[0], G);
					linkedCells[1].UpdateVertexPos(cornerVertexIndexes[1], G);
				}			
			}			
		}

		/// <summary>
		/// Move edge vertices
		/// </summary>
		protected void UpdateEdgeVertex()
		{
			foreach (int index in _groundMesh.indexes.edgeVertexIndex)
			{
				foreach (Cell fneighbor in _neighbors)
				{
					foreach (int fIndex in fneighbor.Indexes.edgeVertexIndex)
					{
						Vector3 nVertex1 = _groundMesh.smoothVertex[index].normalized;
						Vector3 nVertex2 = fneighbor.GroundMesh.smoothVertex[fIndex].normalized;

						if (nVertex1 == nVertex2)
						{
							List<int> myLinkedInternIndex = GetLinkedInternVertexIndexes(_groundMesh, index);
							List<int> linkedInternIndex = GetLinkedInternVertexIndexes(fneighbor.GroundMesh, fIndex);

							Vector3 internVertex = GetNearestTypedVertex(_groundMesh.indexes.internVertexIndex, nVertex1, _groundMesh);						
							Vector3 internNeighborVertex = GetNearestTypedVertex(fneighbor.GroundMesh.indexes.internVertexIndex, nVertex2, fneighbor.GroundMesh);

							Vector3[] cloudOfPoint = new Vector3[] { internVertex, internNeighborVertex };							
							Vector3 G = MathCustom.GetBarycenter(cloudOfPoint);

							_groundMesh.smoothVertex[index] = G;
							fneighbor.UpdateVertexPos(fIndex, G);
						}
					}
				}
			}
		}

		/// <summary>
		/// Return a list containing the internvertex linked to an edgevertex
		/// </summary>
		/// <param name="groundMesh"></param>
		/// <param name="edgeIndex"></param>
		/// <returns></returns>
		public static List<int> GetLinkedInternVertexIndexes(GroundMesh groundMesh, int edgeIndex)
		{
			int nbTriangle = groundMesh.smoothTriangles.Length / 3;
			List<int> linkedInternVertex = new List<int>();

			for (int i = 0; i < nbTriangle; i++)
			{
				Vector3 v1 = groundMesh.smoothVertex[groundMesh.smoothTriangles[i * 3]];
				Vector3 v2 = groundMesh.smoothVertex[groundMesh.smoothTriangles[i * 3 + 1]];
				Vector3 v3 = groundMesh.smoothVertex[groundMesh.smoothTriangles[i * 3 + 2]];

				if (v1 == groundMesh.smoothVertex[edgeIndex] || v2 == groundMesh.smoothVertex[edgeIndex] || v3 == groundMesh.smoothVertex[edgeIndex])
				{
					foreach (int finternIndex in groundMesh.indexes.internVertexIndex)
					{
						if (groundMesh.smoothVertex[finternIndex] == v1 || groundMesh.smoothVertex[finternIndex] == v2 || groundMesh.smoothVertex[finternIndex] == v3)
							linkedInternVertex.Add(finternIndex);
					}
				}
			}
			return linkedInternVertex;
		}

		/// <summary>
		/// Apply a new cell state
		/// </summary>
		/// <param name="groundMesh"></param>
		/// <param name="state"></param>
		public void SetCellState(GroundMesh groundMesh, CellState state)
        {
            _state = state;
			InitColor();
            int length = groundMesh.smoothVertex.Length;
            for (int i = 0; i < length; i++) {
				SetVertexColors();		
            }
        }

		/// <summary>
		/// Colors initiation
		/// </summary>
		public void InitColor()
		{
			if (!PlanetMaker.instance.RandomGroundColor) _color = PlanetMaker.instance.Colors.Find(c => c.type == _state).colors[0];
			else
			{
				int randInt = Mathf.RoundToInt(UnityEngine.Random.Range(0, PlanetMaker.instance.Colors.Find(c => c.type == _state).colors.Count));
				_color = PlanetMaker.instance.Colors.Find(c => c.type == _state).colors[randInt];
				_initialColor = _color;
			}
		}

		/// <summary>
		/// Update vertex colors
		/// </summary>
		public void SetVertexColors()
		{
			Color[] lcolors = new Color[_personnalMesh.vertices.Length];
			Color lColor = Color.white;

			for (int i = 0; i < _groundMesh.smoothVertex.Length; i++)
			{	
				if (CustomGeneric.IntArrayContain(_groundMesh.indexes.cornerVertexIndex, i) != -1)
				{
					lColor = _color;
					List<Cell> lCells = GetVertexCellNeighbors(_groundMesh.smoothVertex[i]);
					foreach (Cell lCell in lCells) {
						lColor += lCell.Color;
					}
					lColor *= 1 / 3f;
				}
				else if (CustomGeneric.IntArrayContain(_groundMesh.indexes.edgeVertexIndex, i) != -1)
				{
					lColor = _color;
					List<Cell> lCells = GetVertexCellNeighbors(_groundMesh.smoothVertex[i]);
                    foreach (Cell lCell in lCells)
                    {
                        lColor += lCell.Color;
                    }
					lColor *= 0.5f;
				}
				else lColor = _color;
				lcolors[i] = lColor;
			}

			_personnalMesh.colors = lcolors;
		}

		/// <summary>
		/// Return a list containing all the neighbors linked to a vertex
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		protected List<Cell> GetVertexCellNeighbors(Vector3 vertex)
		{
			List<Cell> foundCells = new List<Cell>();
			foreach (Cell neighbor in _neighbors)
			{
				foreach (Vector3 nV in neighbor.GroundMesh.smoothVertex)
				{
					if (nV == vertex) foundCells.Add(neighbor);
				} 
			}
			return foundCells;
		}

		/// <summary>
		/// Change elevation order (for curve elevation)
		/// </summary>
		/// <param name="pID"></param>
		public void SetElevationID(int pID)
        {
            _neighborElevationID = pID;
        }

		/// <summary>
		/// Set elevation id to neigbors
		/// </summary>
		/// <returns></returns>
        public List<Cell> SetNeighborsElevationID()
        {
            List<Cell> cellsList = new List<Cell>();
            foreach (Cell cell in _neighbors)
            {
                int elevationID = cell.NeighborElevationID;
                if (elevationID < 0)
                {
                    if (_neighborElevationID + 1 <= PlanetMaker.instance.NbNeighbors)
                    {
                        cell.SetElevationID(_neighborElevationID + 1);
                        cellsList.Add(cell);
                    }
                }
            }
            return cellsList;
        }

		/// <summary>
		/// Return an list containing all the face linked to a vertex
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		protected List<Vector3[]> GetLinkedFaces(Vector3 vertex)
		{
			List<Vector3[]> foundfaces = new List<Vector3[]>();
			int nbTriangle = _groundMesh.smoothTriangles.Length / 3;

			for (int i = 0; i < nbTriangle; i++)
			{
				Vector3[] face = new Vector3[3] { _groundMesh.smoothVertex[_groundMesh.smoothTriangles[i * 3]], _groundMesh.smoothVertex[_groundMesh.smoothTriangles[i * 3 + 1]], _groundMesh.smoothVertex[_groundMesh.smoothTriangles[i * 3 + 2]] };
				if (face[0] == vertex || face[1] == vertex || face[2] == vertex)
					foundfaces.Add(face);
			}

			foreach (Cell fneighbor in _neighbors)
			{
				int nbTriangleNeighbor = fneighbor.GroundMesh.smoothTriangles.Length / 3;

				for (int i = 0; i < nbTriangleNeighbor; i++)
				{
					Vector3[] face = new Vector3[3] { fneighbor.GroundMesh.smoothVertex[fneighbor.GroundMesh.smoothTriangles[i * 3]], fneighbor.GroundMesh.smoothVertex[fneighbor.GroundMesh.smoothTriangles[i * 3 + 1]], fneighbor.GroundMesh.smoothVertex[fneighbor.GroundMesh.smoothTriangles[i * 3 + 2]] };
					if (face[0] == vertex || face[1] == vertex || face[2] == vertex)
						foundfaces.Add(face);
				}
			}

			return foundfaces;
		}

		/// <summary>
		/// Get all the typed vertex link to a vertex
		/// </summary>
		/// <param name="indexArray">custom vertex type array</param>
		/// <param name="testPos"></param>
		/// <returns></returns>
        public static Vector3 GetNearestTypedVertex(int[] indexArray, Vector3 testPos, GroundMesh pGroundMesh)
        {
            Vector3 nearest = Vector3.zero;
            float dist = float.PositiveInfinity;

            foreach (int index in indexArray)
            {
				Vector3 vertex = pGroundMesh.smoothVertex[index];
				if (vertex != pGroundMesh.centerPosition)
                {
                    float lDist = Math.Abs(Vector3.Distance(testPos, vertex));
                    if (lDist < dist)
                    {
						nearest = vertex;
						dist = lDist;
                    }
                } 
            }
            return nearest;
        }

		/// <summary>
		/// Get player nearest cell
		/// </summary>
		public Cell[] GetNearestNeighborCells(Player player, int nbWanted)
		{
			int toReturnNb = Mathf.Clamp(nbWanted, 0, player.AssociateCell.Neighbors.Count - 1);
			Cell[] toReturnNeighbors = new Cell[toReturnNb];
			return toReturnNeighbors;
		}

		/// <summary>
		/// Move a specific vertex
		/// </summary>
		/// <param name="vertexIndex"></param>
		/// <param name="newPos"></param>
		public void UpdateVertexPos(int vertexIndex,  Vector3 newPos)
		{
			_groundMesh.smoothVertex[vertexIndex] = newPos;
			_personnalMesh.vertices = _groundMesh.smoothVertex;
			MeshCollider lSelfMeshCollider = (MeshCollider)_personnalCollider[0];
			lSelfMeshCollider.sharedMesh = _personnalMesh;
		}

		/// <summary>
		/// Return the center position of the game tile ("on the top of the rock")
		/// </summary>
		/// <returns></returns>
		public Vector3 GetCenterPosition()
        {
            return _groundMesh.centerPosition;
        }

        public void RemoveProps(Props prop)
        {
            if (!_props.Remove(prop))
            {
                foreach (KeyValuePair<Props, string> val in _props)
                {
                    if (val.Key == prop)
                    {
                        _props.Remove(val.Key);
						_propsCollider.Remove(val.Key);
                        break;
                    }
                }
            }           
        }

		public void AddProps(Props prop)
		{
			if (!_props.ContainsKey(prop))
			{
				_props.Add(prop, prop.gameObject.name.Replace("(Clone)", ""));
				_propsCollider.Add(prop, prop.GetCollider());
			}
		}

		/// <summary>
		/// Recalculate light normals
		/// </summary>
		public void RecalculateNormals()
		{
			int nLength = _groundMesh.smoothNormals.Length;
			for (int i = 0; i < nLength; i++)
			{
				Vector3 normal = _groundMesh.smoothVertex[i].normalized;
				List<Vector3[]> faces = GetLinkedFaces(_groundMesh.smoothVertex[i]);

				Vector3 vAdditionnal = Vector3.zero;
				foreach (Vector3[] face in faces)
				{
					vAdditionnal += Vector3.Cross(face[0] - face[1], face[0] - face[2]);
				}
				vAdditionnal = vAdditionnal.normalized;

				_groundMesh.smoothNormals[i] = (normal + vAdditionnal * PlanetMaker.instance.SmoothNormalFactor).normalized;
			}

			_personnalMesh.normals = _groundMesh.smoothNormals;
		}

		public void UpdateCollider()
		{
			MeshCollider lSelfMeshCollider = (MeshCollider)_personnalCollider[0];
			lSelfMeshCollider.sharedMesh = _personnalMesh;
		}

		public void AddNeighbor(Cell neighborCell)
        {
            _neighbors.Add(neighborCell);
        }

        public void UpdateProps()
        {
            foreach (KeyValuePair<Props, string> item in _props)
            {
                item.Key.UpdateInternProps();
            }
        }

        public void ShowBubble(Vector3 playerPos)
        {
            List<CitizenProp> citizens = GetProps<CitizenProp>();
            if (_citizenBubble == null && citizens.Count > 0)
            {
                CitizenProp tCitizen = citizens[0];
                CitizenProp testCitizen;

                float dist = Mathf.Abs(Vector3.Distance(tCitizen.transform.position, playerPos));
                int nb = citizens.Count;
                for (int i = 1; i < nb; i++)
                {
                    testCitizen = citizens[i];
                    float nDist = Vector3.Distance(testCitizen.transform.position, playerPos);
                    if (nDist < dist)
                    {
                        tCitizen = testCitizen;
                        dist = nDist;
                    }
                }

                if (dist < CitizenProp.talkDistance)
                {
                    string text = TextManager.GetText("citizen" + tCitizen.ID);
                    if (text != string.Empty)
                    {
                        _citizenBubble = Instantiate(EarthManager.Instance.bubblePrefab, tCitizen.transform.position + (tCitizen.transform.up.normalized) * 0.15f, Quaternion.identity) as BillboardBubble;
                        _citizenBubble.text.text = text;
                        _citizenBubble.citizen = tCitizen;
                        _citizenBubble.SetVisibility(dist, CitizenProp.talkDistance);
                    }
                }
            }   
            else if (_citizenBubble != null)
            {
                float dist = Mathf.Abs(Vector3.Distance(_citizenBubble.citizen.transform.position, playerPos));
                if (dist < CitizenProp.talkDistance) _citizenBubble.SetVisibility(dist, CitizenProp.talkDistance);
                else
                {
                    Destroy(_citizenBubble.gameObject);
                    _citizenBubble = null;
                }
            }
        }

        public void ShowHelp(Vector3 playerPos)
        {
            List<InteractablePNJ> pnjs = GetProps<InteractablePNJ>();
            if (_pnjHelp == null && pnjs.Count > 0)
            {
                InteractablePNJ tPnj = pnjs[0];
                InteractablePNJ testPnj;

                float dist = Mathf.Abs(Vector3.Distance(tPnj.transform.position, playerPos));
                for (int i = 1; i < pnjs.Count; i++)
                {
                    testPnj = pnjs[i];
                    float nDist = Vector3.Distance(testPnj.transform.position, playerPos);
                    if (nDist < dist)
                    {
                        tPnj = testPnj;
                        dist = nDist;
                    }
                }

                if (dist < InteractablePNJ.helpDistance)
                {
                    if (PlayerManager.Instance.playerType == EPlayer.GOV) {
                        if (tPnj.budgetComponent.initialBudget == 0) return;
                    }

                    _pnjHelp = Instantiate(EarthManager.Instance.helpSpritePrefab, tPnj.transform.position + (tPnj.transform.up.normalized) * 0.15f, Quaternion.identity) as BillboardHelp;
                    _pnjHelp.pnj = tPnj;
                    _pnjHelp.Init(PlayerManager.Instance.playerType);
                    _pnjHelp.SetVisibility(dist);
                }
            }
            else if (_pnjHelp != null)
            {
                float dist = Mathf.Abs(Vector3.Distance(_pnjHelp.pnj.transform.position, playerPos));
                if (dist < InteractablePNJ.helpDistance) _pnjHelp.SetVisibility(dist);
                else
                {
                    Destroy(_pnjHelp.gameObject);
                    _pnjHelp = null;
                }
            }
        }

        public List<T> GetProps<T>() where T : Props
        {
            List<T> catchList = new List<T>();
            foreach (KeyValuePair<Props, string> item in _props)
            {
                T castProp = item.Key as T;
                if (castProp != null)
                {
                    catchList.Add(item.Key as T);
                }
            }
            return catchList;
        }

        public static void ResetCells()
        {
            ITERATOR_ID = 0;
        }

        public void SetID() { _id = GetNewCellID(); }
        protected static int GetNewCellID()
        {
            return ITERATOR_ID++;
        }

        protected override void OnDestroy()
        {
            StopAllCoroutines();
            base.OnDestroy();
        }
    }
}

public static class CustomTranform
{
    public static Transform Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        return transform;
    }
}