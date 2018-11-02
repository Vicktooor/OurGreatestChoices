using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using FMODUnity;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Game;
using System.Collections.Generic;
using Assets.Scripts.Utils;

[System.Serializable]
public class SwitchEvent : UnityEvent<Player> {

}

[System.Serializable]
public class SceneEvent : UnityEvent<bool> {

}

[Serializable]
public struct SwitchBtnStruct
{
    public EPlayer player;
    public Button btn;
}

namespace Assets.Script {

    /// <summary>
    /// 
    /// </summary>
    public class UIManager : MonoBehaviour {

        public ObjectUIPerceptor inventoryTarget;
        public SwitchPanel clouds;

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


        #region Private Variable
        [Header("Notifications Buttons")]

        // Debug Input Feedback
        [SerializeField]
        GameObject _UIPointer;

        // Panel of UI_InGame
        GameObject _UIInGamePanel;

        // Panel of Loading
        GameObject _loadingPanel;

        // Array of Button
        [SerializeField]
        List<SwitchBtnStruct> _switchButtonArray;

        // Bag Button
        Image _bagButton;

        // Emitter for SceneButton
        [SerializeField]
        StudioEventEmitter _fmodEmitterZoom;
        [SerializeField]
        StudioEventEmitter _fmodEmitterDezoom;

        // Bool return true if the active scene is MapView
        bool _sceneToLoadIsZoom = false;

        // Bool to know if a clickable button is pressed
        bool _press = false;

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

        private List<Sprite> _sdgIconQueue = new List<Sprite>();
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

        public BillboardNPCState PNJState;

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
            switchButtonEvent = new SwitchEvent();
        }

		protected void Start() {
            InitBagButton();
            InitGauges();

            _UIInGamePanel = GameObject.FindGameObjectWithTag(UI_PANEL_TAG);
            clouds.gameObject.SetActive(false);
            _loadingPanel = GameObject.FindGameObjectWithTag(LOADING_SCENE_TAG);

            Events.Instance.AddListener<OnPinchEnd>(OnClickOnSceneButton);
            Events.Instance.AddListener<OnSceneLoaded>(SceneLoaded);

            Events.Instance.AddListener<OnChangeGauges>(UpdateGauges);        
            Events.Instance.AddListener<OnUpdateInventory>(InventoryScreen.Instance.MajInventory);
            Events.Instance.AddListener<OnHold>(OnHoldMovement);

            CameraManager.Instance.ShowAtmosphere(false);
        }

        #region Init

        protected void OnHoldMovement(OnHold e)
        {
            _bagButton.raycastTarget = false;
            Events.Instance.AddListener<OnRemove>(RemoveHold);
        }

        protected void RemoveHold(OnRemove e)
        {
            _bagButton.raycastTarget = true;
            Events.Instance.RemoveListener<OnRemove>(RemoveHold);
        }

        void InitBagButton() {
            _bagButton = GameObject.FindGameObjectWithTag(BAG_BUTTON_TAG).GetComponent<Image>();
            _bagButton.gameObject.SetActive(false);
        }

        void InitGauges() {
            UpdateEconomyGauge();
            UpdateForestGauge();
            UpdateMoodGauge();
            UpdateCleanlinessGauge();
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
            _loadingPanel.SetActive(false);
            foreach (SwitchBtnStruct btn in _switchButtonArray)
            {
                if (btn.player != PlayerManager.Instance.playerType) btn.btn.interactable = e.scene == SceneString.MapView;
            }
            _switchButtonArray.Find(p => p.player == PlayerManager.Instance.playerType).btn.interactable = false;
            clouds.gameObject.SetActive(true);
            _bagButton.gameObject.SetActive(false);
            StartCoroutine(CloudCoroutine(true));          
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

        #endregion

        #region SwitchButton

        public void OnClickOnSwitchButton(Button pButton) {
			if (FtueManager.instance.active) return;

            SwitchBtnStruct btnStruct = _switchButtonArray.Find(e => e.btn.Equals(pButton));
            GlobalGaugesPanel.SetActive(false);
            foreach (SwitchBtnStruct btn in _switchButtonArray) btn.btn.interactable = false;

            Events.Instance.AddListener<OnEndSwitchedPlayer>(EndSwitchPlayer);
            switchButtonEvent.Invoke(PlayerManager.Instance.GetPlayerByType(btnStruct.player));
			Events.Instance.Raise(new SelectPlayer(PlayerManager.Instance.GetPlayerByType(btnStruct.player)));
        }

        private void EndSwitchPlayer(OnEndSwitchedPlayer e)
        {
            Events.Instance.RemoveListener<OnEndSwitchedPlayer>(EndSwitchPlayer);
            ChangeAllSceneButtons(true);
        }

        #endregion

        #region SwitchScene

        public void OnClickOnSceneButton() {
            if (GameManager.Instance.LoadedScene == SceneString.MapView) _fmodEmitterZoom.Play();
            else _fmodEmitterDezoom.Play();

            clouds.gameObject.SetActive(true);
            ChangeAllSceneButtons(false);
            _bagButton.gameObject.SetActive(false);
            _UIInGamePanel.SetActive(false);

            StartCoroutine(CloudCoroutine());
        }

        private float tTime = 0f;
        public IEnumerator CloudCoroutine(bool forceOpen = false)
        {
            if (tTime >= 1) Events.Instance.Raise(new CloudOut());
            else if (tTime <= 0) Events.Instance.Raise(new CloudIn());
            bool showMapUI = GameManager.Instance.LoadedScene == SceneString.MapView;

            if (forceOpen) tTime = 1f;

            if (tTime <= 0)
            {
                while (tTime < 1)
                {
                    tTime = Mathf.Clamp(tTime + (Time.deltaTime * (1f / SwitchPanel.TIME_TRANSITION)), 0f, 1f);
                    clouds.Move(Easing.SmoothStop(tTime, 2));
                    if (tTime >= 1)
                    {
                        CameraManager.Instance.ShowAtmosphere(showMapUI);
                        GameManager.Instance.ChangeScene();
                        CameraManager.Instance.StopAllCoroutines();
                    }
                    yield return null;
                }
            }
            else
            {
                while (tTime > 0)
                {
                    tTime = Mathf.Clamp(tTime - (Time.deltaTime * (1f / SwitchPanel.TIME_TRANSITION)), 0f, 1f);
                    clouds.Move(Easing.SmoothStop(tTime, 2));
                    if (tTime <= 0)
                    {
                        ChangeAllSceneButtons(showMapUI);                       
                        _UIInGamePanel.SetActive(true);
                    }
                    yield return null;
                }
            }
        }

		public void OnClickOnSceneButton(OnPinchEnd e)
		{
			OnClickOnSceneButton();
		}

        public void ChangeAllSceneButtons(bool mapView) {
            _bagButton.gameObject.SetActive(!mapView);
            GlobalGaugesPanel.SetActive(mapView);
            foreach (SwitchBtnStruct btn in _switchButtonArray)
            {
                if (btn.player != PlayerManager.Instance.playerType) btn.btn.interactable = mapView;
            }
            _switchButtonArray.Find(e => e.player == PlayerManager.Instance.playerType).btn.interactable = false;
            if (PlayerManager.Instance.playerType == EPlayer.ECO) _bagButton.gameObject.SetActive(!mapView);
            else _bagButton.gameObject.SetActive(false);
            _cloudPanel.SetActive(false);
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

        #region Menu btns

        public void ClickStart()
        {
            GameManager.Instance.OnClickOnStartButton();
        }

        public void ClickContinue()
        {
            GameManager.Instance.OnClickOnContinueButton();
        }

        public void ClickFR()
        {
            LocalizationManager.Instance.OnChangedLangageFr();
        }

        public void ClickEN()
        {
            LocalizationManager.Instance.OnChangedLangageEn();
        }

        #endregion

        private bool _sdgRunning = false;
        private void ShowSDGs()
        {
            if (_sdgIconQueue.Count > 0)
            {
                _sdgRunning = true;
                Sprite spr = _sdgIconQueue[0];
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

        public void Clear()
        {
            tTime = 0f;
        }

        protected void OnDestroy() {
            Events.Instance.RemoveListener<OnPinchEnd>(OnClickOnSceneButton);
            Events.Instance.RemoveListener<OnSceneLoaded>(SceneLoaded);

            Events.Instance.RemoveListener<OnChangeGauges>(UpdateGauges);
            Events.Instance.RemoveListener<OnRemove>(UnPressClickableButtons);
            Events.Instance.RemoveListener<OnUpdateInventory>(InventoryScreen.Instance.MajInventory);
            Events.Instance.RemoveListener<OnHold>(OnHoldMovement);
            _instance = null;
        }
    }
}