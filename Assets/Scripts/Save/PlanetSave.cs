using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using Assets.Scripts.Manager;
using Assets.Scripts.Items;
using Assets.Scripts.Game.UI.Ftue;

[Serializable]
public struct GivedItemSave
{
    public string npcName;
    public EItemType[] givedItems;
    public int[] nbGived;
}

[Serializable]
public struct InventoryItemSave
{
    public EItemType type;
    public int nb;
}

namespace Assets.Scripts.Game.Save
{
	[Serializable]
	public struct PartySave
	{
        public string version;

        public float moneyStock;
        public EItemType[] knowsItem;
        public InventoryItemSave[] items;
        public SaveCell[] SavedCells;
        public SavePlayerPosition[] SavedPlayers;
        public TimeSave SavedTime;
        public WorldSave WorldSave;
        public GivedItemSave[] GivedItems;
        public BudgetsSave[] Budgets;

        public void Generate(
            List<Item> pList,
            List<SaveCell> cells, List<SavePlayerPosition> players,
            TimeSave timeSave, Dictionary<EItemType, int> inventory,
            WorldSave worldSave,
            Dictionary<string, Dictionary<EItemType, int>> givedItems,
            BudgetsSave[] budgetsSave
            )
        {
            SavedPlayers = players.ToArray();
            SavedCells = cells.ToArray();
            SavedTime = timeSave;
            WorldSave = worldSave;
            Budgets = budgetsSave;
            if (pList.Count > 0)
			{
                knowsItem = new EItemType[pList.Count];
				for (int i = 0; i < pList.Count; i++) knowsItem[0] = pList[i].itemType;
			}
            else knowsItem = new EItemType[0];
            items = new InventoryItemSave[inventory.Count];
            if (inventory.Count > 0)
            {
                InventoryItemSave newItem;
                int index = 0;
                foreach (KeyValuePair<EItemType, int> item in inventory)
                {
                    newItem = new InventoryItemSave();
                    newItem.nb = item.Value;
                    newItem.type = item.Key;
                    items[index] = newItem;
                    index++;
                }
            }
            GivedItems = new GivedItemSave[givedItems.Count];
            int k = 0;
            foreach (KeyValuePair<string, Dictionary<EItemType, int>> item in givedItems)
            {
                GivedItems[k].npcName = item.Key;
                List<EItemType> lGived = new List<EItemType>();
                List<int> lGivedNb = new List<int>();
                foreach (KeyValuePair<EItemType, int> ite in item.Value)
                {
                    lGived.Add(ite.Key);
                    lGivedNb.Add(ite.Value);
                }
                GivedItems[k].givedItems = lGived.ToArray();
                GivedItems[k].nbGived = lGivedNb.ToArray();
                k++;
            }
		}
	}

	[Serializable]
	public struct PositionID
	{
		public float x;
		public float y;
		public float z;
		public int ID;

		public PositionID(Vector3 pos, int pID = -1)
		{
			x = pos.x;
			y = pos.y;
			z = pos.z;
			ID = pID;
		}
	}

    [Serializable]
    public struct PositionKey
    {
        public float x;
        public float y;
        public float z;
        public string key;

        public PositionKey(Vector3 pos, string pKey)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
            key = pKey;
        }
    }

    #region Cell data structure
    [Serializable]
	public struct SaveCell
	{
		public int ID;
		public int biomeID;
		public CellState state;
		public float elevation;
        public bool walkable;

        public bool poluted;
        public bool deforested;

		public float[] propsX;
		public float[] propsY;
		public float[] propsZ;

		public float[] propsRotX;
		public float[] propsRotY;
		public float[] propsRotZ;
		public float[] propsRotW;

		public string[] Names;

		public BuildingLink[] buildingLinkedItems;

		public SaveCell(CellState cState, int bID, float cElevation, bool walk, List<KeyValuePair<string[], List<float[]>>> props, int cID, BuildingLink[] cBuilds, bool pPoluted, bool pDeforested)
		{
			ID = cID;
			biomeID = bID;

			state = cState;
			elevation = cElevation;
            walkable = walk;

            poluted = pPoluted;
            deforested = pDeforested;

			propsX = props[0].Value[0];
			propsY = props[0].Value[1];
			propsZ = props[0].Value[2];
			propsRotX = props[0].Value[3];
			propsRotY = props[0].Value[4];
			propsRotZ = props[0].Value[5];
			propsRotW = props[0].Value[6];
			Names = props[0].Key;

			buildingLinkedItems = cBuilds;
		}
	}

	[Serializable]
	public struct SavePlayerPosition
	{
		public int cellID;
		public EPlayer player;
		public float X;
		public float Y;
		public float Z;

		public SavePlayerPosition(int cID, EPlayer cPlayer, Vector3 cPos)
		{
			cellID = cID;
			player = cPlayer;
			X = cPos.x;
			Y = cPos.y;
			Z = cPos.z;
		}
	}
	#endregion

	public static class PlanetSave
	{
        #region Party
        public static PartySave GameStateSave = new PartySave();

        public static void SaveParty()
        {
            string path = PersistenDataManager.GetPersistentPath("/Save/PartySave.gd");
            GameStateSave.Generate(
                InventoryPlayer.Instance.knowsItems,
                ConvertPlanetToSerialiable(EarthManager.Instance.Cells),
                ConvertPlayerPositionToSerializable(EarthManager.Instance.playerPositions),
                TimeManager.Instance.GenerateSave(),
                InventoryPlayer.Instance.nbItems,
                WorldManager.Instance.GenerateSave(),
                InventoryPlayer.Instance.givedOject,
                InteractablePNJ.GenerateSave().ToArray()
                );
            GameStateSave.moneyStock = InventoryPlayer.Instance.moneyStock;
            GameStateSave.version = GameManager.VERSION;
            PersistenDataManager.Serialize(GameStateSave, path);
        }

        public static bool LoadParty()
        {
            string path = PersistenDataManager.GetPersistentPath("/Save/PartySave.gd");
            if (File.Exists(path))
            {
                GameStateSave = (PartySave)PersistenDataManager.Deserialize(path);
                return true;
            }
            else return false;
        }

        public static void DeleteParty()
        {
            string path = PersistenDataManager.GetPersistentPath("/Save/PartySave.gd");
            if (File.Exists(path)) File.Delete(path);
        }
        #endregion

        #region Planet
        public static List<SaveCell> BaseCells = new List<SaveCell>();

        public static void SaveCells(List<Cell> planetCells, string planetName)
        {
            string path = StreamingAssetAccessor.GetStreamingAssetPath() + "Save/" + planetName + "Save.gd";
            if (File.Exists(path)) FileManager.WriteFile(path, ConvertPlanetToSerialiable(planetCells));
            else FileManager.CreateFile(path, ConvertPlanetToSerialiable(planetCells));
        }

        public static bool LoadCells(string planetName)
        {
            BaseCells = (List<SaveCell>)StreamingAssetAccessor.Deserialize("Save/" + planetName + "Save.gd");
            return BaseCells != null;
        }
        #endregion

        #region Citizens
        public static List<PositionID> CitizensID = new List<PositionID>();

        public static void SaveCitizens(string planetName)
        {
            string path = StreamingAssetAccessor.GetStreamingAssetPath() + "Save/" + planetName + "CitizensSave.gd";
            List<PositionID> citizenSaves = new List<PositionID>();
            foreach (CitizenProp cp in EarthManager.citizens)
            {
                citizenSaves.Add(new PositionID(cp.transform.position, cp.ID));
            }
            if (File.Exists(path)) FileManager.WriteFile(path, citizenSaves);
            else FileManager.CreateFile(path, citizenSaves);
        }

        public static bool LoadCitizens(string planetName)
        {
            CitizensID = (List<PositionID>)StreamingAssetAccessor.Deserialize("Save/" + planetName + "CitizensSave.gd");
            return CitizensID != null;
        }
        #endregion

        #region PlayersPosition
        public static List<SavePlayerPosition> BasePlayerPos = new List<SavePlayerPosition>();

        public static void SavePlayer(string saveName)
        {
            string path = StreamingAssetAccessor.GetStreamingAssetPath() + "Save/" + saveName + "PlayerSave.gd";
            if (File.Exists(path)) FileManager.WriteFile(path, ConvertPlayerPositionToSerializable(EarthManager.Instance.playerPositions));
            else FileManager.CreateFile(path, ConvertPlayerPositionToSerializable(EarthManager.Instance.playerPositions));
        }

        public static bool LoadPlayer(string saveName)
        {
            BasePlayerPos.Clear();
            BasePlayerPos = (List<SavePlayerPosition>)StreamingAssetAccessor.Deserialize("Save/" + saveName + "PlayerSave.gd");
            foreach (SavePlayerPosition ps in BasePlayerPos)
            {
                if (!EarthManager.Instance.playerPositions.ContainsKey(ps.player)) EarthManager.Instance.playerPositions.Add(ps.player, new KeyValuePair<int, Vector3>(ps.cellID, new Vector3(ps.X, ps.Y, ps.Z)));
                else EarthManager.Instance.playerPositions[ps.player] = new KeyValuePair<int, Vector3>(ps.cellID, new Vector3(ps.X, ps.Y, ps.Z));
            }
            return BasePlayerPos != null;
        }

        public static List<SavePlayerPosition> ConvertPlayerPositionToSerializable(Dictionary<EPlayer, KeyValuePair<int, Vector3>> allPos)
        {
            List<SavePlayerPosition> posList = new List<SavePlayerPosition>();
            foreach (KeyValuePair<EPlayer, KeyValuePair<int, Vector3>> iPlayer in allPos)
            {
                posList.Add(new SavePlayerPosition(iPlayer.Value.Key, iPlayer.Key, iPlayer.Value.Value));
            }
            return posList;
        }

        public static Dictionary<EPlayer, KeyValuePair<int, Vector3>> DeserializePlayerPositions(List<SavePlayerPosition> allPosList)
        {
            Dictionary<EPlayer, KeyValuePair<int, Vector3>> posList = new Dictionary<EPlayer, KeyValuePair<int, Vector3>>();
            foreach (SavePlayerPosition iPlayer in allPosList)
            {
                posList.Add(iPlayer.player, new KeyValuePair<int, Vector3>(iPlayer.cellID, new Vector3(iPlayer.X, iPlayer.Y, iPlayer.Z)));
            }
            return posList;
        }
        #endregion

        #region PNJ
        public static List<PositionKey> PNJs = new List<PositionKey>();

        public static void SavePnjs(string planetName)
        {
            string path = StreamingAssetAccessor.GetStreamingAssetPath() + "Save/" + planetName + "PNJs.gd";
            List<PositionKey> pnjSaved = new List<PositionKey>();
            foreach (InteractablePNJ pnj in InteractablePNJ.PNJs)
            {
                pnjSaved.Add(new PositionKey(pnj.transform.position, pnj.IDname));
            }
            if (File.Exists(path)) FileManager.WriteFile(path, pnjSaved);
            else FileManager.CreateFile(path, pnjSaved);
        }

        public static bool LoadPNJs(string planetName)
        {
            PNJs = (List<PositionKey>)StreamingAssetAccessor.Deserialize("Save/" + planetName + "PNJs.gd");
            return PNJs != null;
        }
        #endregion

        public static List<SaveCell> ConvertPlanetToSerialiable(List<Cell> planetCells)
		{
			List<SaveCell> convertedCells = new List<SaveCell>();
			foreach (Cell lCell in planetCells)
			{
				List<KeyValuePair<string[], List<float[]>>> elements = new List<KeyValuePair<string[], List<float[]>>>();
				List<BuildingLink> links = new List<BuildingLink>();

				// naturals
				int nbProp = lCell.Props.Count;
				float[] posNatX = new float[nbProp];
				float[] posNatY = new float[nbProp];
				float[] posNatZ = new float[nbProp];
				float[] rotNatX = new float[nbProp];
				float[] rotNatY = new float[nbProp];
				float[] rotNatZ = new float[nbProp];
				float[] rotNatW = new float[nbProp];
				string[] propNames = new string[nbProp];

				Dictionary<Props, string> existingElement = ClearList(lCell.Props);

				int ite = 0;
				foreach (KeyValuePair<Props, string> p in existingElement)
				{
					posNatX[ite] = p.Key.transform.position.x;
					posNatY[ite] = p.Key.transform.position.y;
					posNatZ[ite] = p.Key.transform.position.z;
					rotNatX[ite] = p.Key.transform.rotation.x;
					rotNatY[ite] = p.Key.transform.rotation.y;
					rotNatZ[ite] = p.Key.transform.rotation.z;
					rotNatW[ite] = p.Key.transform.rotation.w;
					propNames[ite] = p.Value.Replace("(Clone)", "");
					ite++;					
				}

				links = GetBuildLinks(existingElement);

				elements.Add(new KeyValuePair<string[], List<float[]>>(propNames, new List<float[]>() { posNatX, posNatY, posNatZ, rotNatX, rotNatY, rotNatZ, rotNatW }));

				// Create save cell
				SaveCell newSaveCell = new SaveCell(lCell.State, lCell.BiomeID ,lCell.Elevation, lCell.walkable, elements, lCell.ID, links.ToArray(), lCell.Poluted, lCell.Deforested);
				convertedCells.Add(newSaveCell);
			}
			return convertedCells;
		}

		public static Dictionary<Props, string> ClearList(Dictionary<Props, string> plist)
		{
			Dictionary<Props, string> clearedList = new Dictionary<Props, string>();
			foreach (KeyValuePair<Props, string> p in plist)
			{
				if (p.Key) clearedList.Add(p.Key, p.Value);
			}
			return clearedList;
		}

		public static List<BuildingLink> GetBuildLinks(Dictionary<Props, string> list)
		{
			List<BuildingLink> lList = new List<BuildingLink>();
			int ite = 0;
			foreach (KeyValuePair<Props, string> p in list)
			{
				if (p.Key)
				{
					if (p.Key.GetComponent<LinkObject>() != null)
						lList.Add(p.Key.GetComponent<LinkObject>().BuildingLink);
					else
						lList.Add(new BuildingLink(EnumClass.TypeBuilding.None, -1));
					ite++;
				}	
			}
			return lList;
		}
	}
}
