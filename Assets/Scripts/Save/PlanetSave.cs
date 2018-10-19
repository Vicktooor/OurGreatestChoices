using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Assets.Scripts.PNJ;
using Assets.Scripts.Game.NGO;
using Assets.Scripts.Manager;
using Assets.Scripts.Items;
using Assets.Scripts.Utils;
using Assets.Scripts.Game.UI.Ftue;

// AUTHOR - Victor

namespace Assets.Scripts.Game.Save
{
	[Serializable]
	public struct PartySave
	{
        public string version;
        public string[] itemsName;

        // Game save
        public SaveCell[] SavedCells;

        public void Generate(List<Item> pList, List<SaveCell> cells)
		{
            SavedCells = cells.ToArray();
            if (pList != null)
			{
				itemsName = new string[pList.Count];
				for (int i = 0; i < pList.Count; i++) itemsName[0] = pList[i].name;
			} else itemsName = new string[0];
		}
	}

	[Serializable]
	public struct BiomeSave
	{
		public int[,] biomes;
	}

	[Serializable]
	public struct SaveBudgetComponent
	{
		public float x;
		public float y;
		public float z;
		public BudgetComponent budgetComp;

		public SaveBudgetComponent(Vector3 worldPos, BudgetComponent cBudget)
		{
			x = worldPos.x;
			y = worldPos.y;
			z = worldPos.z;
			budgetComp = cBudget;
		}
	}

    [Serializable]
    public struct CitizenDialogueSave
    {
        public float x;
        public float y;
        public float z;
        public SimpleLocalisationText[] texts;

        public CitizenDialogueSave(Vector3 pos, SimpleLocalisationText[] txt)
        {
            texts = txt;
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }
    }

    [Serializable]
	public struct SaveDialogueNPC
	{
		public float x;
		public float y;
		public float z;
		public string[] govTopics;
		public string[] contTopics;
		public SimpleLocalisationText[] presentationTxts;
		public SimpleLocalisationText[] leavingTxts;

		public SaveDialogueNPC(Vector3 worldPos, List<SimpleLocalisationText> presTxts, List<SimpleLocalisationText> leaveTxts, List<GovernmentTopic> govs, List<ContractorTopic> conts)
		{
			x = worldPos.x;
			y = worldPos.y;
			z = worldPos.z;

			presentationTxts = presTxts.ToArray();
			leavingTxts = leaveTxts.ToArray();

			List<string> govList = new List<string>();
            foreach (GovernmentTopic so in govs)
            {
                if (so != null) govList.Add(so.name);
            }
			govTopics = govList.ToArray();

			List<string> contList = new List<string>();
            foreach (ContractorTopic so in conts)
            {
                if (so != null) contList.Add(so.name);
            }
			contTopics = contList.ToArray();
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

		public float[] propsX;
		public float[] propsY;
		public float[] propsZ;

		public float[] propsRotX;
		public float[] propsRotY;
		public float[] propsRotZ;
		public float[] propsRotW;

		public string[] Names;

		public BuildingLink[] buildingLinkedItems;

		public SaveCell(CellState cState, int bID, float cElevation, bool walk, List<KeyValuePair<string[], List<float[]>>> props, int cID, BuildingLink[] cBuilds)
		{
			ID = cID;
			biomeID = bID;

			state = cState;
			elevation = cElevation;
            walkable = walk;

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
        public static bool LOADED = false;

		public static PartySave GameStateSave = new PartySave();
		public static List<SaveDialogueNPC> PNJDialogues = new List<SaveDialogueNPC>();
		public static List<CitizenDialogueSave> CitizenTexts = new List<CitizenDialogueSave>();
		public static List<SaveBudgetComponent> BudgetElements = new List<SaveBudgetComponent>();
		public static List<SaveCell> SavedCells = new List<SaveCell>();
		public static List<SavePlayerPosition> PlayerPos = new List<SavePlayerPosition>();

		public static void SaveParty()
		{
			BinaryFormatter bf = new BinaryFormatter();
            var fileName = string.Format("{0}/{1}", Application.persistentDataPath, "PartySave.gd");

            GameStateSave.Generate(InventoryPlayer.instance.knowsItems, ConvertPlanetToSerialiable(EarthManager.Instance.Cells));
            GameStateSave.version = GameManager.VERSION;

            if (File.Exists(fileName))
            {
                FileStream file = File.OpenWrite(fileName);
                bf.Serialize(file, GameStateSave);
                file.Close();
            }
            else
            {
                FileStream file = File.Create(fileName);
                bf.Serialize(file, GameStateSave);
                file.Close();
            }
        }

		public static void LoadParty()
		{
			BinaryFormatter bf = new BinaryFormatter();

			try
			{
#if UNITY_STANDALONE
                string editorPath = Application.streamingAssetsPath + "/Save/PartySave.gd";
                if (File.Exists(editorPath))
                {
                    FileStream fileSave = File.OpenRead(editorPath);
                    GameStateSave = (PartySave)bf.Deserialize(fileSave);
                    fileSave.Close();

                    int length = GameStateSave.SavedCells.Length;
                    for (int i = 0; i < length; i++)
                    {
                        SavedCells.Add(GameStateSave.SavedCells[i]);
                    }

                    foreach (string str in GameStateSave.itemsName)
                    {
                        Item lItem = Resources.Load<Item>("Items/" + str);
                        if (lItem != null)
                        {
                            if (InventoryPlayer.instance) InventoryPlayer.instance.knowsItems.Add(lItem);
                            break;
                        }
                    }

                    LOADED = true;
                    Events.Instance.Raise(new PartyLoaded(LOADED));
                }               
#endif

#if UNITY_ANDROID
                string androidPath = "jar:file://" + Application.dataPath + "!/assets/Save/PartySave.gd";
                if (!PlanetMaker.instance.buildForMobile) androidPath = Application.streamingAssetsPath + "/Save/PartySave.gd";
                string fileName = string.Format("{0}/{1}", Application.persistentDataPath, "PartySave.gd");

                if (File.Exists(fileName))
                {
                    FileStream file = File.OpenRead(fileName);
                    GameStateSave = (PartySave)bf.Deserialize(file);
                    file.Close();

                    if (GameManager.VERSION == GameStateSave.version)
                    {
                        int length = GameStateSave.SavedCells.Length;
                        for (int i = 0; i < length; i++)
                        {
                            SavedCells.Add(GameStateSave.SavedCells[i]);
                        }

                        foreach (string str in GameStateSave.itemsName)
                        {
                            Item lItem = Resources.Load<Item>("Items/" + str);
                            if (lItem != null)
                            {
                                if (InventoryPlayer.instance) InventoryPlayer.instance.knowsItems.Add(lItem);
                                break;
                            }
                        }

                        LoadPlayer();

                        if (GameStateSave.SavedCells.Length == 162) LOADED = true;
                    }
                    else File.Delete(fileName);
                }

                Events.Instance.Raise(new PartyLoaded(LOADED));
#endif
            }
            catch (Exception err)
			{
				Debug.LogError("Load game state error : " + err.Message);
				throw;
			}
		}

		public static void SavePlanet(List<Cell> planetCells, string saveName)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.streamingAssetsPath + "/Save/" + saveName + "Save.gd");
			bf.Serialize(file, ConvertPlanetToSerialiable(planetCells));
			file.Close();
		}

		public static void SavePlayer(string saveName)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.streamingAssetsPath + "/Save/Player/" + saveName + "PlayerSave.gd");
			bf.Serialize(file, ConvertPlayerPositionToSerializable(EarthManager.Instance.playerPosition));
			file.Close();
		}

        public static void LoadPlayer()
        {
            BinaryFormatter bf = new BinaryFormatter();

            string androidPathPlayerPos = "jar:file://" + Application.dataPath + "!/assets/Save/Player/PlanetGoldTestPlayerSave.gd";
            if (!PlanetMaker.instance.buildForMobile) androidPathPlayerPos = Application.streamingAssetsPath + "/Save/Player/PlanetGoldTestPlayerSave.gd";
            string fileName = string.Format("{0}/{1}", Application.persistentDataPath, "PlanetGoldTestPlayerSave.gd");

            WWW wwwfilep = new WWW(androidPathPlayerPos);
            while (!wwwfilep.isDone) { }
            File.WriteAllBytes(fileName, wwwfilep.bytes);
            StreamReader wrp = new StreamReader(fileName);
            PlayerPos = (List<SavePlayerPosition>)bf.Deserialize(wrp.BaseStream);
            wrp.Close();

            foreach (SavePlayerPosition ps in PlayerPos)
            {
                if (!EarthManager.Instance.playerPosition.ContainsKey(ps.player))
                    EarthManager.Instance.playerPosition.Add(ps.player, new KeyValuePair<int, Vector3>(ps.cellID, new Vector3(ps.X, ps.Y, ps.Z)));
                else
                    EarthManager.Instance.playerPosition[ps.player] = new KeyValuePair<int, Vector3>(ps.cellID, new Vector3(ps.X, ps.Y, ps.Z));
            }
        }

		public static bool LoadPlanet(string targetSaveName)
		{
			BinaryFormatter bf = new BinaryFormatter();

#if UNITY_ANDROID // ANDROID PLANET LOAD FUNCTION
			string androidPath = "jar:file://" + Application.dataPath + "!/assets/Save/" + targetSaveName + "Save.gd";
			if (!PlanetMaker.instance.buildForMobile) androidPath = Application.streamingAssetsPath + "/Save/" + targetSaveName + "Save.gd";
			var fileName = string.Format("{0}/{1}", Application.persistentDataPath, targetSaveName + "Save.gd");

            if (File.Exists(fileName))
            {
                WWW wwwfile = new WWW(androidPath);
                while (!wwwfile.isDone) { }
                File.WriteAllBytes(fileName, wwwfile.bytes);
                StreamReader wr = new StreamReader(fileName);
                SavedCells = (List<SaveCell>)bf.Deserialize(wr.BaseStream);
                wr.Close();

                LoadPlayer();

                return true;
            }
            else return false;
#endif

#if UNITY_STANDALONE // DESKTOP PLANET LOAD FUNCTION
			string editorPath = Application.streamingAssetsPath + "/Save/" + targetSaveName + "Save.gd";
			string editorPathPlayer = Application.streamingAssetsPath + "/Save/Player/" + targetSaveName + "PlayerSave.gd";
			if (File.Exists(editorPath))
			{
				FileStream file = File.OpenRead(editorPath);
				SavedCells = (List<SaveCell>)bf.Deserialize(file);
				file.Close();
			}
			else return false;

			if (File.Exists(editorPath))
			{
				try
				{
					FileStream filePlayerPos = File.OpenRead(editorPathPlayer);
					PlayerPos = (List<SavePlayerPosition>)bf.Deserialize(filePlayerPos);
					filePlayerPos.Close();

					foreach (SavePlayerPosition ps in PlayerPos)
					{
						EarthManager.Instance.playerPosition.Add(ps.player, new KeyValuePair<int, Vector3>(ps.cellID, new Vector3(ps.X, ps.Y, ps.Z)));
					}
				}
				catch (Exception err)
				{
					Debug.LogError("Load players error : " + err.Message + " -> " + editorPath);
					return true;
					throw;
				}
			}
			else return false;
			return true;
#endif
		}

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
				SaveCell newSaveCell = new SaveCell(lCell.State, lCell.BiomeID ,lCell.Elevation, lCell.walkable, elements, lCell.ID, links.ToArray());
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

		public static List<SavePlayerPosition> ConvertPlayerPositionToSerializable(Dictionary<EPlayer, KeyValuePair<int, Vector3>> allPos)
		{
			List<SavePlayerPosition> posList = new List<SavePlayerPosition>();
			foreach (KeyValuePair<EPlayer, KeyValuePair<int, Vector3>> iPlayer in allPos)
			{
				posList.Add(new SavePlayerPosition(iPlayer.Value.Key, iPlayer.Key, iPlayer.Value.Value));
			}
			return posList;
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

		public static void SavePNJDialogues(string planetName)
		{
			List<SaveDialogueNPC> NPCdialogues = new List<SaveDialogueNPC>();
			foreach (Cell c in EarthManager.Instance.Cells)
			{
				foreach (KeyValuePair<Props, string> item in c.Props)
				{
					if (GameTypes.PNJ_TYPES.Contains(item.Key.GetType()))
					{
						InteractablePNJ pnj = (InteractablePNJ)item.Key;

						if (pnj.presentationTexts.Count > 0)
						{
							SaveDialogueNPC dialogueSave = new SaveDialogueNPC(
								pnj.transform.position,
								pnj.presentationTexts,
								pnj.leavingTexts,
								pnj.govTopics,
								pnj.contTopics
							);

							NPCdialogues.Add(dialogueSave);
						}						
					}
				}
			}

			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.streamingAssetsPath + "/Save/" + planetName + "WorldDialoguesSave.gd");
			bf.Serialize(file, NPCdialogues);
			file.Close();

            SaveCitizensDialogue(planetName);
		}

        private static void SaveCitizensDialogue(string planetName)
        {
            List<CitizenDialogueSave> citizenSaves = new List<CitizenDialogueSave>();
            foreach (CitizenProp cp in EarthManager.citizens)
            {
                citizenSaves.Add(new CitizenDialogueSave(cp.transform.position, cp.dialogueText.ToArray()));
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.streamingAssetsPath + "/Save/" + planetName + "CitizensSave.gd");
            bf.Serialize(file, citizenSaves);
            file.Close();
        }

        public static bool LoadPNJDialogues(string planetName)
		{			
			string targetSaveName = planetName + "WorldDialoguesSave.gd";
			string targetCitizenSaveName = planetName + "CitizensSave.gd";
			BinaryFormatter bf = new BinaryFormatter();

#if UNITY_ANDROID // ANDROID PLANET LOAD FUNCTION
			string androidPath = "jar:file://" + Application.dataPath + "!/assets/Save/" + targetSaveName;
			string androidCitizenPath = "jar:file://" + Application.dataPath + "!/assets/Save/" + targetCitizenSaveName;

			if (!PlanetMaker.instance.buildForMobile) androidPath = Application.streamingAssetsPath + "/Save/" + targetSaveName;
			if (!PlanetMaker.instance.buildForMobile) androidCitizenPath = Application.streamingAssetsPath + "/Save/" + targetCitizenSaveName;

			var fileName = string.Format("{0}/{1}", Application.persistentDataPath, targetSaveName);
			var fileCitizenName = string.Format("{0}/{1}", Application.persistentDataPath, targetCitizenSaveName);

			WWW wwwfile = new WWW(androidPath);
			while (!wwwfile.isDone) { }
			File.WriteAllBytes(fileName, wwwfile.bytes);
			StreamReader wr = new StreamReader(fileName);
			PNJDialogues = (List<SaveDialogueNPC>)bf.Deserialize(wr.BaseStream);
			wr.Close();

            WWW wwwCitizenfile = new WWW(androidCitizenPath);
			while (!wwwCitizenfile.isDone) { }
			File.WriteAllBytes(fileCitizenName, wwwCitizenfile.bytes);
			StreamReader wrCitizen = new StreamReader(fileCitizenName);
			CitizenTexts = (List<CitizenDialogueSave>)bf.Deserialize(wrCitizen.BaseStream);
			wrCitizen.Close();

			return true;
#endif

#if UNITY_STANDALONE // DESKTOP PLANET LOAD FUNCTION
            string editorPath = Application.streamingAssetsPath + "/Save/" +  targetSaveName;	
			string editorCitizenPath = Application.streamingAssetsPath + "/Save/" + targetCitizenSaveName;

            if (File.Exists(editorPath))
            {
                FileStream file = File.OpenRead(editorPath);
                PNJDialogues = (List<SaveDialogueNPC>)bf.Deserialize(file);
                file.Close();
                Debug.Log("PNJ dialogues : " + PNJDialogues.Count);
            }
            else
            {
                Debug.LogError("Pas de dialogues pour PNJ");
                return false;
            }

            if (File.Exists(editorCitizenPath))
            {
                FileStream file = File.OpenRead(editorCitizenPath);
                CitizenTexts = (List<CitizenDialogueSave>)bf.Deserialize(file);
                file.Close();
            }
            else Debug.LogError("Pas de dialogues pour les citizen");

			return true;
#endif
		}

		public static void SaveBudgetConfiguration(string planetName)
		{
			List<SaveBudgetComponent> budgetComps = new List<SaveBudgetComponent>();
			foreach (Cell c in EarthManager.Instance.Cells)
			{
				foreach (KeyValuePair<Props, string> item in c.Props)
				{
					if (GameTypes.PNJ_TYPES.Contains(item.Key.GetType()))
					{
						InteractablePNJ pnj = (InteractablePNJ)item.Key;

						if (pnj.budgetComponent.name != string.Empty)
						{
							SaveBudgetComponent budgetComp = new SaveBudgetComponent(
								pnj.transform.position,
								pnj.budgetComponent
							);

							budgetComps.Add(budgetComp);
						}
					}
				}
			}

			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.streamingAssetsPath + "/Save/" + planetName + "BudgetConfiguration.gd");
			bf.Serialize(file, budgetComps);
			file.Close();
		}

		public static bool LoadBudget(string planetName)
		{
			string targetBudgetName = planetName + "BudgetConfiguration.gd";
			BinaryFormatter bf = new BinaryFormatter();

#if UNITY_ANDROID
			string androidPath = "jar:file://" + Application.dataPath + "!/assets/Save/" + targetBudgetName;
			if (!PlanetMaker.instance.buildForMobile) androidPath = Application.streamingAssetsPath + "/Save/" + targetBudgetName;
			var fileName = string.Format("{0}/{1}", Application.persistentDataPath, targetBudgetName);

			WWW wwwfile = new WWW(androidPath);
			while (!wwwfile.isDone) { }
			File.WriteAllBytes(fileName, wwwfile.bytes);
			StreamReader wr = new StreamReader(fileName);
			BudgetElements = (List<SaveBudgetComponent>)bf.Deserialize(wr.BaseStream);
			wr.Close();

			return true;
#endif

#if UNITY_STANDALONE
			string editorBudgetPath = Application.streamingAssetsPath + "/Save/" + targetBudgetName;

			if (File.Exists(editorBudgetPath))
			{
				FileStream file = File.OpenRead(editorBudgetPath);
				BudgetElements = (List<SaveBudgetComponent>)bf.Deserialize(file);
				file.Close();
				Debug.Log("Budget number : " + BudgetElements.Count);
			}
			else return false;
			return true;
#endif
		}
	}
}
