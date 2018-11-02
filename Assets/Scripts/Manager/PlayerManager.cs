using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{

    /// <summary>
    /// 
    /// </summary>

    public class PlayerManager : MonoSingleton<PlayerManager> {

        #region Public Variable
        public Player player {
            get { return _player; }
        }

        public EPlayer playerType {
            get { return _playerType; }
        }

        #endregion

        #region Private Variable
        // Array of all player
		[SerializeField]
        private List<Player> _playersArray;

        Player _player;

        // Type of the Player who is played
        EPlayer _playerType = EPlayer.NGO;

        // Tag of Player
        const string PLAYER_TAG = "Player";
        #endregion

        bool _init = false;
        protected override void Awake()
        {
            base.Awake();
            Events.Instance.AddListener<OnSceneLoaded>(Init);
        }

        protected void OnEnable()
		{
			Events.Instance.AddListener<SelectPlayer>(OnSetPlayer);
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<SelectPlayer>(OnSetPlayer);
		}

        void Init(OnSceneLoaded e) {
            if (!_init)
            {
                _init = true;
                ArrayExtensions.ToList(FindObjectsOfType<Player>(), out _playersArray);
                _player = GetPlayer();
                if (GameManager.Instance.LoadedScene == SceneString.ZoomView)
                {
                    StudioListener listener = _player.GetComponent<StudioListener>();
                    if (listener) listener.enabled = true;
                }
            }
            else
            {
                ArrayExtensions.ToList(FindObjectsOfType<Player>(), out _playersArray);
                _player = GetPlayer();
                if (GameManager.Instance.LoadedScene == SceneString.ZoomView)
                {
                    StudioListener listener = _player.GetComponent<StudioListener>();
                    if (listener) listener.enabled = true;
                }
            }
            Events.Instance.Raise(new OnPlayerInitFinish());
        }

        public void SetPlayer(EPlayer playerType)
        {
            Player player = _playersArray.Find(p => p.playerType == playerType);
            _player = player;
            _playerType = player.playerType;
        }

        public void OnSetPlayer(SelectPlayer e)
		{
			_player = e.player;
			_playerType = e.player.playerType;
		}

        public Player GetPlayer()
        {
            return _playersArray.Find(p => p.playerType == _playerType);
        }

		public Player GetPlayerByType(EPlayer playerType)
		{
			foreach (Player p in _playersArray)
			{
                if (p.playerType == playerType) return p;
            }
			return null;
		}

        public void Clear()
        {
            _init = false;
        }

        protected void OnDestroy() {
            Events.Instance.RemoveListener<OnSceneLoaded>(Init);
            _instance = null;
        }
    }
}