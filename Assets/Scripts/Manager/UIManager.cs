using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using Assets.Script;
using UnityEngine.SceneManagement;
using FMODUnity;
using Assets.Scripts.Manager;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Game;
using Assets.Scripts.Utils;
using System.Collections.Generic;
using Assets.Scripts.Game.UI.Global;

[System.Serializable]
public class SwitchEvent : UnityEvent<GameObject> {

}

[System.Serializable]
public class SceneEvent : UnityEvent<bool> {

}

namespace Assets.Script {

    /// <summary>
    /// 
    /// </summary>
    public class UIManager : MonoBehaviour {

        #region Public Variable
        public ObjectUIPerceptor inventoryTarget;

        [SerializeField]
        GameObject GlossaryPanel;
        [SerializeField]
        GameObject GlobalGaugesPanel;
        [SerializeField]
        GameObject _cloudPanel;
        
        [Header("Gauges")]
        [SerializeField]
        Slider _economyGauge;
        [SerializeField]
        Slider _moodGauge;
        [SerializeField]
        Slider _forestGauge;
        [SerializeField]
        Slider _cleanlinessGauge;

        #endregion

        #region Private Variable
        [Header("Notifications Buttons")]

        // Debug Input Feedback
        [SerializeField]
        GameObject _UIPointer;

        // Panel of UI_InGame
        GameObject _UIInGamePanel;

        // Panel of Switch Scene
        [SerializeField]
        GameObject _switchScenePanel;

        // Panel of Loading
        GameObject _loadingPanel;

        // Array of Button
        Button[] _switchButtonArray;

        // Old press Button
        Button _switchButton;

        // Current press Button
        Button _currentSwitchButton;

        // Bag Button
        Image _bagButton;

        // Quit Button
        Button _quitButton;

        // Vector2 for the Top Position of the News Box
        Vector2 _newsBoxTopPosition = Vector2.zero;

        // Vector2 for the Bottom Position of the News Box
        Vector2 _newsBoxBottomPosition = Vector2.zero;

        // Speed For the Lerp
        [SerializeField]
        float _speedNewsBox = 6f;

        // Life Time of the News Box
        float _newsBoxLifeTime = 5f;

        // DEBUG BUTTON FOR NEWS BOX
        Button _fakeNewsButton;

        // Emitter for SceneButton
        [SerializeField]
        StudioEventEmitter _fmodEmitterZoom;
        [SerializeField]
        StudioEventEmitter _fmodEmitterDezoom;

        // Bool return true if the active scene is MapView
        bool _sceneToLoadIsZoom = false;

        // Bool to know if a clickable button is pressed
        bool _press = false;

        // Bool to know if the 2 panel of transition have finish
        bool _secondPanelFinish = false;

        // Bool to know if the news box is lerping
        bool _newsBoxLerp = false;

        // Tag of UI InGame Panel
        const string UI_PANEL_TAG = "UI_InGame_Panel";

        // Tag of Loading Panel
        const string LOADING_SCENE_TAG = "Loading_Panel";

        // Tag of Switch Button
        const string SWITCH_BUTTON_TAG = "SwitchButton";

        // Tag of Scene Button
        const string SCENE_BUTTON_TAG = "SceneButton";

        // Tag of Focus Button
        const string FOCUS_BUTTON_TAG = "FocusButton";

        // Tag of Bag Button
        const string BAG_BUTTON_TAG = "BagButton";

        // Tag of Quit Button
        const string QUIT_BUTTON_TAG = "QuitButton";

        // Tag of Quit Button
        const string NEWS_BOX_TAG = "NewsBox";

        // Tag of Player Budget
        const string PLAYER_BUDGET_TAG = "PlayerBudget";

        // Text in Focus Button
        const string SCENE_BUTTON_TEXT_ZOOMVIEW = "MAP";
        const string SCENE_BUTTON_TEXT_MAPVIEW = "ZOOM";

        // Values for Switch Player Button
        const float PLANET_RADIUS = 2;

        float _planetRadiusOnScreen = 0;

        private ObjectArray<Sprite> _sdgIconQueue;
        #endregion

        #region Event
        [HideInInspector]
        public SwitchEvent switchButtonEvent;

        [HideInInspector]
        public UnityEvent pressButtonEvent;
        #endregion

        [Header("SDGs")]
        [SerializeField]
        private SDGDatabase _sdgSpriteDatabase;
        public SDGNotification SDGNotification;

        private static UIManager _instance;

        public static UIManager instance {
            get {
                return _instance;
            }
        }

        protected void Awake() {
            if (_instance != null) {
                throw new Exception("Tentative de création d'une autre instance de PlayerManager alors que c'est un singleton.");
            }
            _instance = this;

            _sdgIconQueue = new ObjectArray<Sprite>();

            switchButtonEvent = new SwitchEvent();
        }

		protected void OnEnable()
		{
			Events.Instance.AddListener<OnPinchEnd>(OnClickOnSceneButton);
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<OnPinchEnd>(OnClickOnSceneButton);
		}

		protected void Start() {
            InitButtonArray();
            InitQuitButton();
            InitBagButton();
            InitGauges();

            _UIInGamePanel = GameObject.FindGameObjectWithTag(UI_PANEL_TAG);
            _switchScenePanel.SetActive(false);
            _loadingPanel = GameObject.FindGameObjectWithTag(LOADING_SCENE_TAG);

            Events.Instance.AddListener<OnSceneLoaded>(SceneLoaded);
            Events.Instance.AddListener<OnNotifications>(UpdateNotificationButton);
            Events.Instance.AddListener<OnChangeGauges>(UpdateGauges);
            Events.Instance.AddListener<PanelLerpEnd>(PanelTransitionEnd);
            Events.Instance.AddListener<OnPlayerInitFinish>(InitSwitchButton);
            Events.Instance.AddListener<OnRemove>(UnPressClickableButtons);
            Events.Instance.AddListener<LerpEnd>(EnableSwitchButton);
            Events.Instance.AddListener<ZoomEndUI>(ChangeScene);
            Events.Instance.AddListener<OnPinchEnd>(OnClickOnSceneButton);
            Events.Instance.AddListener<OnUpdateInventory>(InventoryScreen.Instance.MajInventory);
            Events.Instance.AddListener<OnHold>(OnHoldMovement);
        }

        #region Init

        protected void OnHoldMovement(OnHold e)
        {
            _bagButton.raycastTarget = false;
        }

        void InitButtonArray() {
            int i;
            int j;
            int lLength;

            Button lButton;
            GameObject[] lButtonArray;

            string lName;

            lButtonArray = GameObject.FindGameObjectsWithTag(SWITCH_BUTTON_TAG);

            lLength = lButtonArray.Length;
            _switchButtonArray = new Button[lLength];

            for (i = lLength - 1; i >= 0; i--) {
                lButton = lButtonArray[i].GetComponent<Button>();
                lName = lButton.name;

                for (j = 1; j <= lLength; j++) if (lName.Contains(j.ToString())) _switchButtonArray[j - 1] = lButton;
            }
        }

        IEnumerator InitRadiusOnScreen() {
            yield return null;
            _planetRadiusOnScreen = Camera.main.WorldToScreenPoint(Vector3.up * PLANET_RADIUS).y - Screen.height / 2;
            yield break;
        }

        void InitBagButton() {
            _bagButton = GameObject.FindGameObjectWithTag(BAG_BUTTON_TAG).GetComponent<Image>();
            _bagButton.gameObject.SetActive(false);
        }

        void InitSwitchButton(OnPlayerInitFinish e) {
            _switchButton = _switchButtonArray[PlayerManager.instance.player.GetComponent<Player>().index - 1];
            _switchButton.interactable = false;
        }


        void InitGauges() {
            UpdateEconomyGauge();
            UpdateForestGauge();
            UpdateMoodGauge();
            UpdateCleanlinessGauge();
        }

        void InitQuitButton() {
            //_quitButton = GameObject.FindGameObjectWithTag(QUIT_BUTTON_TAG).GetComponent<Button>();
        }

#endregion

        protected void Update() {
            if (Input.touchCount != 0) {
                _UIPointer.SetActive(true);
            }
            else if (Input.touchCount == 0) {
                _UIPointer.SetActive(false);
            }
        }

        public void MoveObjectToInventory(Transform obj, Action callBack)
        {
            inventoryTarget.AttractObject(obj, callBack);
        }

        #region SceneLoaded
        void SceneLoaded(OnSceneLoaded e) {
			if (e.scene == SceneString.MapView)
			{
				ChangeAllSceneButtons(false);
				EnableAllButton();
				GlobalGaugesPanel.SetActive(true);
                if(_planetRadiusOnScreen == 0) StartCoroutine(InitRadiusOnScreen());
            }
			else
			{
				ChangeAllSceneButtons(true);
				DisableAllButton();
				GlobalGaugesPanel.SetActive(false);
			}
			_UIInGamePanel.SetActive(true);
			_loadingPanel.SetActive(false);
			//_switchScenePanel.SetActive(false);
		}

        #endregion
        
        #region ButtonUtils

        public void OnPressClickableButtons() {
			if (_press) return;

            /* Event for CameraComplement and Player */
            pressButtonEvent.Invoke();
            _press = true;
        }

        void UnPressClickableButtons(OnRemove e) {
            _bagButton.raycastTarget = true;
            _press = false;
        }

        void EnableAllButton() {
            foreach (Button lButton in _switchButtonArray) lButton.interactable = true;
        }

        void DisableAllButton() {
            foreach (Button lButton in _switchButtonArray) lButton.interactable = false;
        }

        #endregion

        #region SwitchButton

        public void OnClickOnSwitchButton(Button pButton) {
			if (FtueManager.instance.active) return;
			int i = 0;
            int lIndex = 0;

            _currentSwitchButton = pButton;
            DisableAllButton();
            GlobalGaugesPanel.SetActive(false);

            foreach (Button lButton in _switchButtonArray) {
                if (lButton == _currentSwitchButton) lIndex = i;
                i++;
            }

            /* Event for CameraComplement */
            switchButtonEvent.Invoke(PlayerManager.instance.GetPlayerByIndex(lIndex));
			Events.Instance.Raise(new SelectPlayer(PlayerManager.instance.GetPlayerByIndex(lIndex).GetComponent<Player>()));
		}

        void EnableSwitchButton(LerpEnd e = null) {
            GlobalGaugesPanel.SetActive(true);

            _switchButton = _currentSwitchButton;
            foreach(Button button in _switchButtonArray) {
                if (button != _switchButton) button.interactable = true;
            }
        }

        #endregion

        #region SwitchScene

        public void OnClickOnSceneButton() {
            if (GameManager.Instance.LoadedScene == SceneString.MapView) _fmodEmitterZoom.Play();
            else _fmodEmitterDezoom.Play();

            _switchScenePanel.SetActive(true);
            GlobalGaugesPanel.SetActive(false);

			Events.Instance.Raise(new OnSwitchScene(GameManager.Instance.LoadedScene));
        }

		public void OnClickOnSceneButton(OnPinchEnd e)
		{
			OnClickOnSceneButton();
		}

		public void ChangeAllSceneButtons(bool pZoomView) {
            /* Button in MapView */
            foreach (Button lButton in _switchButtonArray) lButton.gameObject.SetActive(!pZoomView);

            /* Button in ZoomView */
            if (pZoomView && PlayerManager.instance.playerType == EPlayer.ECO) _bagButton.gameObject.SetActive(true);
            else _bagButton.gameObject.SetActive(false);

            _cloudPanel.SetActive(pZoomView);
        }

        void ChangeScene(ZoomEndUI e) {
            if (!_sceneToLoadIsZoom) GlobalGaugesPanel.SetActive(true);

            ChangeAllSceneButtons(_sceneToLoadIsZoom);
        }

        void PanelTransitionEnd(PanelLerpEnd e = null) {
            if (_secondPanelFinish) {
                _switchScenePanel.SetActive(false);
                _secondPanelFinish = false;
            }
            else _secondPanelFinish = true;
        }

        #endregion

        #region Gauges
        void UpdateGauges(OnChangeGauges e)
        {
            UpdateEconomyGauge();
            UpdateMoodGauge();
            UpdateForestGauge();
            UpdateCleanlinessGauge();
        }

        void UpdateEconomyGauge() {
            _economyGauge.value = WorldValues.STATE_ECONOMY;
            UpdateGaugeColor(_economyGauge);
        }

        void UpdateMoodGauge() {
            _moodGauge.value = WorldValues.STATE_NPC;
            UpdateGaugeColor(_moodGauge);
        }

        void UpdateForestGauge() {
            _forestGauge.value = WorldValues.STATE_FOREST;
            UpdateGaugeColor(_forestGauge);
        }

        void UpdateCleanlinessGauge() {
            _cleanlinessGauge.value = WorldValues.STATE_CLEANLINESS;
            UpdateGaugeColor(_cleanlinessGauge);
        }

        void UpdateGaugeColor(Slider pGauge) {
            if (pGauge.value <= WorldValues.BAD_STEP) {
                pGauge.fillRect.GetComponent<Image>().color = Color.red;
                pGauge.GetComponent<Blink>().canBlink = true;
            }
            else if (pGauge.value >= WorldValues.GOOD_STEP) {
                pGauge.fillRect.GetComponent<Image>().color = Color.green;
                pGauge.GetComponent<Blink>().canBlink = false;
            }
            else {
                pGauge.fillRect.GetComponent<Image>().color = Color.yellow;
                pGauge.GetComponent<Blink>().canBlink = false;
            }
        }

        public bool CheckRedGauges() {
            return (_economyGauge.value <= WorldValues.BAD_STEP
                || _moodGauge.value <= WorldValues.BAD_STEP
                || _forestGauge.value <= WorldValues.BAD_STEP
                || _cleanlinessGauge.value <= WorldValues.BAD_STEP);
        }

        #endregion

        #region Notifications Buttons
        void UpdateNotificationButton(OnNotifications e) {
            switch (e.notificationType) {
                //case EnumClass.NotificationsType.glossary;
            }
        }
        #endregion

        private bool _sdgRunning = false;
        private void ShowSDGs()
        {
            if (_sdgIconQueue.Length > 0)
            {
                _sdgRunning = true;
                Sprite spr = _sdgIconQueue.Objs[0].obj;
                _sdgIconQueue.Remove(spr);
                SDGNotification.Init(spr);
                SDGNotification.ShowSDG(ShowSDGs);
            }
            else
            {
                _sdgRunning = false;
                return;
            }
        }

        public void AddSDGNotification(int[] pSDGs)
        {
            Events.Instance.Raise(new OnShowPin(EPin.Sdg, true));
            if (_sdgRunning) return;
            for (int i = 0; i < pSDGs.Length; i++)
            {
                Sprite spr = _sdgSpriteDatabase.sdgSprites[pSDGs[i] - 1 ];
                if (!_sdgIconQueue.Contains(spr)) _sdgIconQueue.Add(spr);
            }
            ShowSDGs();
        }

        protected void OnDestroy() {
            Events.Instance.RemoveListener<OnSceneLoaded>(SceneLoaded);
            Events.Instance.RemoveListener<OnNotifications>(UpdateNotificationButton);
            Events.Instance.RemoveListener<OnChangeGauges>(UpdateGauges);
            Events.Instance.RemoveListener<PanelLerpEnd>(PanelTransitionEnd);
            Events.Instance.RemoveListener<OnPlayerInitFinish>(InitSwitchButton);
            Events.Instance.RemoveListener<OnRemove>(UnPressClickableButtons);
            Events.Instance.RemoveListener<LerpEnd>(EnableSwitchButton);
            Events.Instance.RemoveListener<ZoomEndUI>(ChangeScene);
            Events.Instance.RemoveListener<OnPinchEnd>(OnClickOnSceneButton);
            Events.Instance.RemoveListener<OnUpdateInventory>(InventoryScreen.Instance.MajInventory);
            Events.Instance.RemoveListener<OnHold>(OnHoldMovement);
            _instance = null;
        }
    }
}