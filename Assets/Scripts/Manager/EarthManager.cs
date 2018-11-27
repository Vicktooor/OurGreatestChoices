using Assets.Scripts.Game;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Items;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayer { NONE, ECO, GOV, NGO }

namespace Assets.Scripts.Manager
{
    public class EarthManager : MonoSingleton<EarthManager>
    {
        public static int NB_FOREST_PROPS = 0;
        public static int NB_SNOW_GROUND = 0;
        public static int NB_FIELD_GROUND = 0;
        public static int NB_SEA_GROUND = 0;

        public static List<CitizenProp> citizens = new List<CitizenProp>();

        public string planetName;
        public string ftuePlanetName;
        [HideInInspector]
        public string playingPlanetName;
        public HoneycombPlanetSmooth planetModel;
        public HoneycombPlanetSmooth planetFtueModel;
        public HoneycombPlanetSmooth planetLink;

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

        public Dictionary<EPlayer, KeyValuePair<int, Vector3>> playerPositions = new Dictionary<EPlayer, KeyValuePair<int, Vector3>>();

        protected float _planetRadius;
        public float PlanetRadius { get { return _planetRadius; } }

        public void CreateOnlyPlanet()
        {
            StreamingAssetAccessor.Platform = RuntimePlatform.WindowsPlayer;
            TextManager.SetLanguage(SystemLanguage.English);
            ResourcesManager.Instance.Init();

            if (ftuePlanetName != string.Empty)
            {
                GameManager.PARTY_TYPE = EPartyType.NEW;
                playingPlanetName = ftuePlanetName;
                PlanetSave.LoadCitizens(playingPlanetName);
                PlanetSave.LoadPlayer(playingPlanetName);
                PlanetSave.LoadPNJs(playingPlanetName);
                CreatePlanet();
            }
            else if (planetName != string.Empty)
            {
                GameManager.PARTY_TYPE = EPartyType.NEW;
                playingPlanetName = planetName;
                PlanetSave.LoadCitizens(playingPlanetName);
                PlanetSave.LoadPlayer(playingPlanetName);
                PlanetSave.LoadPNJs(playingPlanetName);
                CreatePlanet();
            }
        }

        public void CreatePlanet()
        {
            if (playingPlanetName == planetName) planetLink = Instantiate(planetModel);
            else if (playingPlanetName == ftuePlanetName) planetLink = Instantiate(planetFtueModel);
            planetLink.transform.position = Vector3.zero;
            planetLink.transform.rotation = Quaternion.identity;
            planetLink.gameObject.name = playingPlanetName;
            planetLink.StartGeneration(CreateFullPlanet);
        }

        public GameObject CreateProps(GameObject model, Vector3 pos, Cell associateCell)
        {
            if (model == null) return null;
            Quaternion orientation = Quaternion.FromToRotation(Vector3.up, associateCell.GetCenterPosition());
            Props newGo = Instantiate(model.GetComponent<Props>(), pos, orientation, associateCell.transform);
            newGo.associateCell = associateCell;
            newGo.transform.position += pos.normalized * newGo.offsetY;
            associateCell.Props.Add(newGo, model.name);
            associateCell.PropsCollider.Add(newGo, newGo.GetCollider());
            CatchTree(newGo);
            newGo.Init();
            return newGo.gameObject;
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
            CatchTree(newGo);
            newGo.Init();
            return newGo.gameObject;
        }

        public GameObject CreateSavedProps(GameObject model, Vector3 pos, Cell associateCell, Quaternion orientation, BuildingLink link)
        {
            Type subType = model.GetType();
            LinkObject item;
            Props modelProps = model.GetComponent<Props>();
            if (modelProps == null) return null;
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
            newGo.Init();
            return newGo.gameObject;
        }

        protected void CreateCells(EPartyType partyType)
        {
            foreach (GroundMesh ground in planetLink.allGroundMesh)
            {
                GameObject newGo = Instantiate(_cellPrefab, Vector3.zero, Quaternion.identity, planetLink.transform);
                _cellsObj.Add(newGo);

                Cell newCell = newGo.GetComponent<Cell>();
                newCell.SetID();
                newCell.Init(ground);

                Mesh finalMesh = newCell.gameObject.GetComponent<MeshFilter>().mesh;
                finalMesh.vertices = ground.smoothVertex;
                finalMesh.triangles = ground.smoothTriangles;
                finalMesh.normals = ground.smoothNormals;
                finalMesh.uv = ground.UVs;
                newCell.gameObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
                finalMesh.RecalculateBounds();

                _cells.Add(newCell);
            }

            planetLink.GenerateNeighborLinks(planetLink.GetMinimalNeighborDistance(), _cells);

            if (partyType == EPartyType.SAVE)
            {
                int l = _cells.Count;
                Cell cCell;
                for (int i = 0; i < l; i++)
                {
                    cCell = _cells[i];
                    Mesh finalMesh = cCell.gameObject.GetComponent<MeshFilter>().mesh;
                    List<SaveCell> outCells;
                    ArrayExtensions.ToList(PlanetSave.GameStateSave.SavedCells, out outCells);
                    SaveCell sCell = outCells.Find(c => c.ID == cCell.ID);
                    cCell.Init(sCell);

                    cCell.SetPolution(sCell.poluted);
                    if (sCell.poluted) PolutionArea.CELLS_USED.Add(cCell);
                    cCell.SetDeforestation(sCell.deforested);
                    if (sCell.deforested) DeforestationArea.CELLS_USED.Add(cCell);

                    UpdateNbCell(cCell.State);

                    finalMesh.vertices = cCell.GroundMesh.smoothVertex;
                    finalMesh.triangles = cCell.GroundMesh.smoothTriangles;
                    finalMesh.normals = cCell.GroundMesh.smoothNormals;
                    finalMesh.uv = cCell.GroundMesh.UVs;

                    cCell.gameObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
                    finalMesh.RecalculateBounds();
                }
            }
            else if (partyType == EPartyType.NEW)
            {
                if (PlanetSave.LoadCells(playingPlanetName))
                {
                    foreach (Cell cCell in _cells)
                    {
                        Mesh finalMesh = cCell.gameObject.GetComponent<MeshFilter>().mesh;
                        SaveCell sCell = PlanetSave.BaseCells.Find(c => c.ID == cCell.ID);
                        cCell.Init(sCell);

                        UpdateNbCell(cCell.State);

                        finalMesh.vertices = cCell.GroundMesh.smoothVertex;
                        finalMesh.triangles = cCell.GroundMesh.smoothTriangles;
                        finalMesh.normals = cCell.GroundMesh.smoothNormals;
                        finalMesh.uv = cCell.GroundMesh.UVs;

                        cCell.gameObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
                        finalMesh.RecalculateBounds();
                    }
                }
            }
        }

        public void CreateFullPlanet()
        {
            if (GameManager.Instance) CreateCells(GameManager.PARTY_TYPE);
            else CreateCells(EPartyType.NEW);
            foreach (Cell c in _cells)
            {
                foreach (KeyValuePair<Props, string> p in c.Props)
                {
                    p.Key.UpdatePosition();
                }
            }
            RecalculateUVMap();
            Events.Instance.Raise(new OnEndPlanetCreation());
        }

        public void UpdateNbCell(CellState cState)
        {
            if (cState == CellState.GRASS || cState == CellState.MOSS) NB_FIELD_GROUND++;
            else if (cState == CellState.SEA) NB_SEA_GROUND++;
            else if (cState == CellState.SNOW) NB_SNOW_GROUND++;
        }

        public static void PoluteCell(CellState cState, bool add)
        {
            if (cState == CellState.GRASS || cState == CellState.MOSS) PolutionArea.FIELD_POLUTED += (add) ? 1 : -1;
            else if (cState == CellState.SEA) PolutionArea.SEA_POLUTED += (add) ? 1 : -1;
            else if (cState == CellState.SNOW) PolutionArea.SNOW_POLUTED += (add) ? 1 : -1;
        }

        public void RecalculateUVMap()
        {
            foreach (Cell lcell in _cells) lcell.InitColor();
            foreach (Cell lcell in _cells) lcell.SetVertexColors();
        }

        public void DestroyPlanet()
        {
            playingPlanetName = string.Empty;
            foreach (Cell c in _cells)
            {
                c.transform.Clear();
                Destroy(c.gameObject);
            }
            Clear();
            Cell.ResetCells();
            if (planetLink) Destroy(planetLink.gameObject);
        }

        public void Clear()
        {
            _cellsObj.Clear();
            _cells.Clear();
            playerPositions.Clear();
            playerPositions.Clear();
            citizens.Clear();
        }

        protected void OnDestroy()
        {
            DestroyPlanet();
            _instance = null;
        }

        public void CatchTree(Props prop)
        {
            if (prop.GetType() == typeof(PoolTree)) NB_FOREST_PROPS++;
        }
    }
}
