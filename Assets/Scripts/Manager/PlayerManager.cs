using FMODUnity;
using System;
using UnityEngine;

namespace Assets.Script
{

    /// <summary>
    /// 
    /// </summary>

    public class PlayerManager : MonoBehaviour {


        #region Public Variable
        public GameObject playerAsset;

        public GameObject player {
            get { return _player; }
        }

        public EPlayer playerType {
            get { return _playerType; }
        }

        #endregion

        #region Private Variable
        // Array of all player
		[SerializeField]
        private GameObject[] _playersArray;

        GameObject _player;

        ItemPickUp _bioManObjectWorn;
        ItemPickUp _socioManObjectWorn;
        ItemPickUp _ecoWomanObjectWorn;

        // Type of the Player who is played
        EPlayer _playerType = EPlayer.NGO;

        // Tag of Player
        const string PLAYER_TAG = "Player";
        #endregion

        private static PlayerManager _instance;

        /// <summary>
        /// instance unique de la classe     
        /// </summary>
        public static PlayerManager instance {
            get {
                return _instance;
            }
        }

        protected void Awake() {
            if (_instance != null) {
                throw new Exception("Tentative de création d'une autre instance de PlayerManager alors que c'est un singleton.");
            }
            _instance = this;
			Events.Instance.AddListener<OnSceneLoaded>(Init);
		}

		protected void OnEnable()
		{
			Events.Instance.AddListener<OnUpdateObject>(UpdateObjectWorn);
			Events.Instance.AddListener<SelectPlayer>(OnSetPlayer);
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<OnUpdateObject>(UpdateObjectWorn);
			Events.Instance.RemoveListener<SelectPlayer>(OnSetPlayer);
		}

		#region Init

        void Init(OnSceneLoaded e) {
			InitPlayerArray();
            SortPlayerArray();

            SelectPlayer();
            SetObjectWorn();

            if (GameManager.Instance.LoadedScene == SceneString.ZoomView) _player.GetComponent<StudioListener>().enabled = true;
            Events.Instance.Raise(new OnPlayerInitFinish());
        }

        void InitPlayerArray() {
            _playersArray = GameObject.FindGameObjectsWithTag(PLAYER_TAG);
        }

        void SortPlayerArray() {
            int i;
            int j;
            int lLength = _playersArray.Length;

            GameObject lPlayer;
            GameObject[] lArray = new GameObject[lLength];

            string lName;

            for (i = lLength - 1; i >= 0; i--) {
                lPlayer = _playersArray[i];
                lName = lPlayer.name;

                for (j = 1; j <= lLength; j++) {
                    if (lName.Contains(j.ToString())) {
                        lPlayer.GetComponent<Player>().index = j;
                        lArray[j - 1] = lPlayer;
                    }
                }
            }

            _playersArray = lArray;
        }

        void SetObjectWorn() {
            GameObject lPlayer;
            int lLenght = _playersArray.Length;
            //A CLEANER
            /*for (int i = 0; i < lLenght; i++) {
                lPlayer = _playersArray[i];

                if (lPlayer.GetComponent<Player>().playerDatas.type == EnumClass.TYPE.NGO && _bioManObjectWorn != null) lPlayer.GetComponent<InventoryPlayer>().Add(_bioManObjectWorn, true);
                else if (lPlayer.GetComponent<Player>().playerDatas.type == EnumClass.TYPE.gouvernment && _socioManObjectWorn != null) lPlayer.GetComponent<InventoryPlayer>().Add(_socioManObjectWorn, true);
                else if (lPlayer.GetComponent<Player>().playerDatas.type == EnumClass.TYPE.buisness && _ecoWomanObjectWorn != null) lPlayer.GetComponent<InventoryPlayer>().Add(_ecoWomanObjectWorn, true);
            }*/
        }

        public void RemoveObjectWorn(GameObject pPlayer) {
            if (pPlayer == GetPlayerByIndex(0)) _bioManObjectWorn = null;
            if (pPlayer == GetPlayerByIndex(1)) _ecoWomanObjectWorn = null;
            if (pPlayer == GetPlayerByIndex(2)) _socioManObjectWorn = null;
        }

        public void SelectPlayer() {
            foreach (GameObject lPlayer in _playersArray) {
                if (lPlayer.GetComponent<Player>().playerType == _playerType) {
                    _player = lPlayer;
					Events.Instance.Raise(new SelectPlayer(_player.GetComponent<Player>()));
				}
            }
        }

        #endregion

		public void OnSetPlayer(SelectPlayer e)
		{
			_player = e.player.gameObject;
			_playerType = e.player.playerType;
		}

        void UpdateObjectWorn(OnUpdateObject e) {
            
            switch (_player.GetComponent<Player>().playerDatas.type) {
                case EPlayer.NGO:
                    _bioManObjectWorn = e.itemWorn;
                    break;
                case EPlayer.ECO:
                    _ecoWomanObjectWorn = e.itemWorn;
                    break;
                case EPlayer.GOV:
                    _socioManObjectWorn = e.itemWorn;
                    break;
            }
        }

		public Player GetPlayerByType(EPlayer playerType)
		{
			foreach (GameObject obj in _playersArray)
			{
                if (obj != null)
                {
                    Player lPlayer = obj.GetComponent<Player>();
                    if (lPlayer != null)
                    {
                        if (lPlayer.playerType == playerType) return lPlayer;
                    }
                }
			}
			return null;
		}

		//Ordre 0 = BioMan, 1 = EcoWoman, 2 = SocioMan
		public GameObject GetPlayerByIndex(int pIndex) {
            if (_playersArray.Length == 0) return null;
            return _playersArray[pIndex];
        }

        protected void OnDestroy() {
            Events.Instance.RemoveListener<OnSceneLoaded>(Init);
            _instance = null;
        }

        private void Update() {
            if (_player != null) playerAsset = _player.gameObject;
        }
    }
}