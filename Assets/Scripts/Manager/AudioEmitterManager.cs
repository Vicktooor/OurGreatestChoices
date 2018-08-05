using UnityEngine;
using System;
using FMODUnity;
using Assets.Scripts.Game;
using Assets.Script;
using System.Collections.Generic;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Items;

namespace Assets.Scripts.Manager {

    /// <summary>
    /// 
    /// </summary>
    public class AudioEmitterManager : MonoBehaviour {

        private static List<Type> TYPES_BUILDINGS = new List<Type>()
        {
            typeof(HouseProp),
            typeof(MetroProp),
            typeof(MainItemProp)
        };

        #region Private Variable
        // Reference of transform
        Transform _transform;

        // Array of all Emitter
        StudioEventEmitter[] _emitterArray;

        // Snapshot STRING
        const string ZOOM_VIEW_SNAPSHOT_STRING = "Vue_zoom";
        const string MAP_VIEW_SNAPSHOT_STRING = "Vue_dézoom";
        
        // CONST EVENT
        const string SFX_OBJ_PICKUP_STRING = "SFX_obj_pickup";
        const string SFX_OBJ_TRANSFORM_STRING = "SFX_obj_transform";
        const string SFX_OBJ_FUSION_STRING = "SFX_obj_fusion";
        const string SFX_OBJ_GIVE_STRING = "SFX_obj_give";
        const string SFX_OBJ_DROP_STRING = "SFX_obj_drop";

        //UI
        const string SFX_CLICK_GLOSSARY_STRING = "SFX_Clickglossary";
        const string SFX_SWIPE_STRING = "SFX_swipe";
        const string SFX_CLICK_TRANSFORM_STRING = "SFX_click_transform";
        const string SFX_TRANSFORM_REWARD_STRING = "SFX_transform_reward";
        const string SFX_TRANSFORM_REWARD_NEW_STRING = "SFX_transform_reward_new";
        const string SFX_NEW_OBJECT_STRING = "SFX_new_object";
        const string SFX_WRONG_OBJECT_STRING = "SFX_wrong_object";
        const string SFX_GOOD_OBJECT_STRING = "SFX_good_object";
        const string SFX_TAKE_MONEY_STRING = "SFX_take_money";
        const string SFX_GIVE_MONEY_STRING = "SFX_give_money";
        const string SFX_POP_UP_STRING = "SFX_Popup";
        const string SFX_NPC_MAN_STRING = "SFX_npc_man";
        const string SFX_NPC_WOMAN_STRING = "SFX_npc_woman";
        const string SFX_ALARM_STRING = "SFX_alarmjauge";
        const string SFX_GAMEOVER_STRING = "SFX_gameover";
        const string SFX_WIN_STRING = "SFX_win";


        //AMBIANCES
        const string AMB_COAST_STRING = "AMB_Coast";
        const string AMB_FIELD_STRING = "AMB_Field";
        const string AMB_FOREST_STRING = "AMB_Forest";
        const string AMB_CITY_STRING = "AMB_City";
        const string AMB_MOUNTAIN_STRING = "Amb_Mountain";
        const string AMB_DESERT_STRING = "Amb_desert";
        const string AMB_PLANET_STRING = "Amb_vueplanete";

        const string PARAM_FOREST_STRING = "Foret";
        const int POSITIVE_FOREST_VALUE = 2;
        const int NEGATIVE_FOREST_VALUE = 7;

        // bool for AMB Planet
        bool _onMenu = true;

        // Bool for AMB
        bool _inTown = false;
        bool _inForest = false;

        // Bool for Alarm SFX
        bool _alarmDone = false;

        // MUSIC
        [SerializeField]
        StudioEventEmitter _musicEmitter;

        const int NEUTRAL_MUSIC_VALUE = 5;
        const int NEGATIVE_MUSIC_VALUE = 15;
        const int POSITIVE_MUSIC_VALUE = 25;

        const string PARAM_MUSIC_STRING = "music";

        const float NEGATIVE_VALUE = -1.5f;
        const float NEGATIVE_AVERAGE2_VALUE = -1f;
        const float NEGATIVE_AVERAGE4_VALUE = 0f;
        const float NEUTRAL_AVERAGE4_VALUE = 1f;
        
        // Average World Values 
        float _averageValues;

        // TRANSITION

        [SerializeField]
        StudioEventEmitter _cloudInEmitter;
        [SerializeField]
        StudioEventEmitter _cloudOutEmitter;

        // CLICK

        [SerializeField]
        StudioEventEmitter _clickEmitter;
        
        #endregion

        private static AudioEmitterManager _instance;

        /// <summary>
        /// instance unique de la classe     
        /// </summary>
        public static AudioEmitterManager instance {
            get {
                return _instance;
            }
        }

        protected void Awake() {
            if (_instance != null) {
                throw new Exception("Tentative de création d'une autre instance de AudioEmitterManager alors que c'est un singleton.");
            }
            _instance = this;
        }

        protected void Start() {
            _transform = transform;
            _emitterArray = _transform.GetComponents<StudioEventEmitter>();

            Events.Instance.AddListener<OnPickUp>(ObjectPickUp);
            Events.Instance.AddListener<OnTransform>(ObjectTransform);
            Events.Instance.AddListener<OnFusion>(ObjectFusion);
            Events.Instance.AddListener<OnGive>(ObjectGive);
            Events.Instance.AddListener<OnDrop>(ObjectDrop);

            Events.Instance.AddListener<OnGrass>(PlayOnGrass);
            Events.Instance.AddListener<OnMoss>(PlayOnMoss);
            Events.Instance.AddListener<OnTown>(PlayOnTown);
            Events.Instance.AddListener<OnCoast>(PlayOnCoast);
            Events.Instance.AddListener<OnMountain>(PlayOnMountain);
            Events.Instance.AddListener<OnDesert>(PlayOnDesert);
            
            Events.Instance.AddListener<OnClickGlossary>(OpenGlossary);
            Events.Instance.AddListener<OnSwipe>(Swipe);
            Events.Instance.AddListener<OnClickTransform>(ClickTransform);
            Events.Instance.AddListener<OnTransformReward>(TransformReward);
            Events.Instance.AddListener<OnTransformRewardNew>(TransformRewardNew);
            Events.Instance.AddListener<OnNewObject>(NewObject);
            Events.Instance.AddListener<OnWrongObject>(WrongObject);
            Events.Instance.AddListener<OnGoodObject>(OnGoodObject);
            Events.Instance.AddListener<OnGiveBudget>(GiveMoney);
            Events.Instance.AddListener<OnPopUp>(PopUp);
            Events.Instance.AddListener<OnNPCDialogueSFX>(NPCDialogue);

            Events.Instance.AddListener<OnTapStepFTUE>(OnClickFTUE);

            Events.Instance.AddListener<CloudIn>(CloudInSFX);
            Events.Instance.AddListener<CloudOut>(CloudOutSFX);

			Events.Instance.AddListener<OnSceneLoaded>(SwitchSnapshot);
            
            _musicEmitter.Params[0].Value = NEUTRAL_MUSIC_VALUE;
            _musicEmitter.SetParameter(PARAM_MUSIC_STRING, NEUTRAL_MUSIC_VALUE);
        }

        void Update() {
            UpdateWorldValuesAverage();
            if (GameManager.Instance.LoadedScene == SceneString.ZoomView) {
                _alarmDone = false;

                CheckTownAMB();
                if (!_inTown) CheckForestAMB();
            }
            else {
                if (_onMenu) return;

                StopOtherAMBEmitterExcept(AMB_PLANET_STRING);
                PlayEmitter(AMB_PLANET_STRING);
                
                if (!_alarmDone) PlayAlarm();
            }
        }

        void PlayEmitter(string pEvent) {
            StudioEventEmitter lEmitter;

            int lLength = _emitterArray.Length;
            int lIndex;

            for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                lEmitter = _emitterArray[lIndex];
                if (lEmitter.Event.Contains(pEvent)) {
                    if (lEmitter.IsPlaying()) break;
                    else
                    {
                        lEmitter.Play();
                    }
                    break;
                }
            }
        }

        public void StopAllEmitter() {

            GameObject[] l3DEmitterArray = GameObject.FindGameObjectsWithTag("3DEmitter");

            StudioEventEmitter lEmitter;

            int lLength = _emitterArray.Length;
            int lIndex;

            for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                lEmitter = _emitterArray[lIndex];
                lEmitter.Stop();
            }

            lLength = l3DEmitterArray.Length;

            for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                Destroy(l3DEmitterArray[lIndex]);
            }

            SwitchSnapshot(null);

            _musicEmitter.Stop();
            _cloudInEmitter.Stop();
            _cloudOutEmitter.Stop();
        }

        void StopOtherAMBEmitterExcept(string pEvent) {
            StudioEventEmitter lEmitter;

            int lLength = _emitterArray.Length;
            int lIndex;

            for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                lEmitter = _emitterArray[lIndex];
                if(pEvent != null) {
                    if (lEmitter.Event.Contains(pEvent)) continue;
                }
                if(lEmitter.Event.Contains("AMB") || lEmitter.Event.Contains("Amb")) lEmitter.Stop();
            }
        }

        bool EmitterIsPlaying(string pEvent) {
            StudioEventEmitter lEmitter;

            int lLength = _emitterArray.Length;
            int lIndex;

            for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                lEmitter = _emitterArray[lIndex];
                if (lEmitter.Event.Contains(pEvent)) return lEmitter.IsPlaying();
            }

            return false;
        }

        StudioEventEmitter GetEmitter(string pEvent) {
            StudioEventEmitter lEmitter;

            int lLength = _emitterArray.Length;
            int lIndex;

            for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                lEmitter = _emitterArray[lIndex];
                if (lEmitter.Event.Contains(pEvent)) return lEmitter;
            }
            
            return null;
        }

        #region ObjectEmitter

        void ObjectPickUp(OnPickUp pEvent) {
            PlayEmitter(SFX_OBJ_PICKUP_STRING);
        }

        void ObjectTransform(OnTransform pEvent) {
            PlayEmitter(SFX_OBJ_TRANSFORM_STRING);
        }

        void ObjectFusion(OnFusion pEvent) {
            PlayEmitter(SFX_OBJ_FUSION_STRING);
        }

        void ObjectGive(OnGive pEvent) {
            PlayEmitter(SFX_OBJ_GIVE_STRING);
        }

        void ObjectDrop(OnDrop pEvent) {
            PlayEmitter(SFX_OBJ_DROP_STRING);
        }

        #endregion

        #region UI Emitter
        void OpenGlossary(OnClickGlossary pEvent) {
            PlayEmitter(SFX_CLICK_GLOSSARY_STRING);
        }

        void Swipe(OnSwipe pEvent) {
            PlayEmitter(SFX_SWIPE_STRING);
        }

        void ClickTransform(OnClickTransform pEvent) {
            PlayEmitter(SFX_CLICK_TRANSFORM_STRING);
        }

        void TransformReward(OnTransformReward pEvent) {
            PlayEmitter(SFX_TRANSFORM_REWARD_STRING);
        }

        void TransformRewardNew(OnTransformRewardNew pEvent) {
            PlayEmitter(SFX_TRANSFORM_REWARD_NEW_STRING);
        }

        void NewObject(OnNewObject pEvent) {
            PlayEmitter(SFX_NEW_OBJECT_STRING);
        }

        void WrongObject(OnWrongObject pEvent) {
            PlayEmitter(SFX_WRONG_OBJECT_STRING);
        }

        void OnGoodObject(OnGoodObject pEvent) {
            PlayEmitter(SFX_GOOD_OBJECT_STRING);
        }

        void GiveMoney(OnGiveBudget pEvent) {
            PlayEmitter(SFX_GIVE_MONEY_STRING);
        }

        void PopUp(OnPopUp pEvent) {
            PlayEmitter(SFX_POP_UP_STRING);
        }

        void NPCDialogue(OnNPCDialogueSFX pEvent) {
            if (pEvent.gender) PlayEmitter(SFX_NPC_MAN_STRING);
            else PlayEmitter(SFX_NPC_WOMAN_STRING);
        }

        void OnClickFTUE(OnTapStepFTUE pEvent) {
            if (!_clickEmitter.IsPlaying()) _clickEmitter.Play();
        }
        #endregion

        #region CellEmitter

        void PlayOnGrass(OnGrass pEvent) {
            if (EmitterIsPlaying(AMB_FIELD_STRING) || _inForest || _inTown) return;

            StopOtherAMBEmitterExcept(AMB_FIELD_STRING);
            PlayEmitter(AMB_FIELD_STRING);
        }

        void PlayOnMoss(OnMoss pEvent) {
            if (EmitterIsPlaying(AMB_FOREST_STRING) || _inTown) return;

            StopOtherAMBEmitterExcept(AMB_FOREST_STRING);
            PlayEmitter(AMB_FOREST_STRING);
        }

        void PlayOnTown(OnTown pEvent) {
            if (EmitterIsPlaying(AMB_CITY_STRING) || _inForest) return;

            StopOtherAMBEmitterExcept(AMB_CITY_STRING);
            PlayEmitter(AMB_CITY_STRING);
        }

        void PlayOnCoast(OnCoast pEvent) {
            if (EmitterIsPlaying(AMB_COAST_STRING) || _inForest || _inTown) return;

            StopOtherAMBEmitterExcept(AMB_COAST_STRING);
            PlayEmitter(AMB_COAST_STRING);
        }

        void PlayOnMountain(OnMountain pEvent) {
            if (EmitterIsPlaying(AMB_MOUNTAIN_STRING) || _inForest || _inTown) return;

            StopOtherAMBEmitterExcept(AMB_MOUNTAIN_STRING);
            PlayEmitter(AMB_MOUNTAIN_STRING);
        }

        void PlayOnDesert(OnDesert pEvent) {
            if (EmitterIsPlaying(AMB_DESERT_STRING) || _inForest || _inTown) return;

            StopOtherAMBEmitterExcept(AMB_DESERT_STRING);
            PlayEmitter(AMB_DESERT_STRING);
        }

        void CheckForestAMB() {
            StudioEventEmitter emitter = GetEmitter(AMB_FOREST_STRING);

            int forestNumber = 0;
            int deforestationNumber = 0;

            GameObject p = PlayerManager.instance.player;
            if (p)
            {
                Player lPlayer = p.GetComponent<Player>();
                foreach (KeyValuePair<Props, string> item in lPlayer.AssociateCell.Props)
                {
                    if (item.Key.GetType() == typeof(PoolTree))
                    {
                        PoolTree lForest = item.Key as PoolTree;
                        if (lForest.cutModel)
                        {
                            if (lForest.opositeAsset.activeSelf) forestNumber++;
                            else deforestationNumber++;
                        }
                        else
                        {
                            if (lForest.opositeAsset.activeSelf) deforestationNumber++;
                            else forestNumber++;
                        }
                    }
                }

                foreach (Cell c in lPlayer.AssociateCell.Neighbors)
                {
                    foreach (KeyValuePair<Props, string> item in c.Props)
                    {
                        if (item.Key.GetType() == typeof(PoolTree))
                        {
                            PoolTree lForest = item.Key as PoolTree;
                            if (lForest.cutModel)
                            {
                                if (lForest.opositeAsset.activeSelf) forestNumber++;
                                else deforestationNumber++;
                            }
                            else
                            {
                                if (lForest.opositeAsset.activeSelf) deforestationNumber++;
                                else forestNumber++;
                            }
                        }
                    }
                }
            }

            emitter.SetParameter(PARAM_FOREST_STRING, forestNumber > deforestationNumber ? POSITIVE_FOREST_VALUE : NEGATIVE_FOREST_VALUE);
            if (forestNumber + deforestationNumber > 1)
            {
                _inForest = true;
                PlayOnMoss(null);
            }
            else
            {
                _inForest = false;
                emitter.Stop();
            }
        }
        
        void CheckTownAMB() {
            StudioEventEmitter emitter = GetEmitter(AMB_CITY_STRING);

            GameObject p = PlayerManager.instance.player;
            if (p)
            {
                Player lPlayer = p.GetComponent<Player>();
                foreach (KeyValuePair<Props, string> item in lPlayer.AssociateCell.Props)
                {
                    if (TYPES_BUILDINGS.Contains(item.Key.GetType()))
                    {
                        PlayTown(emitter, true);
                        return;
                    }
                }

                foreach (Cell c in lPlayer.AssociateCell.Neighbors)
                {
                    foreach (KeyValuePair<Props, string> item in c.Props)
                    {
                        if (TYPES_BUILDINGS.Contains(item.Key.GetType()))
                        {
                            PlayTown(emitter, true);
                            return;
                        }
                    }
                }
            }

            PlayTown(emitter, false);
        }

        void PlayTown(StudioEventEmitter emitter, bool isInTown)
        {
            if (isInTown)
            {
                _inTown = true;
                _inForest = false;
                PlayOnTown(null);
            }
            else
            {
                _inTown = false;
                emitter.Stop();
            }
        }

        int GetVisibleNumber(VisibleObject[] pArray) {
            VisibleObject lObject;

            int length = pArray.Length;
            int index;

            int count = 0;

            for (index = 0; index < length; index++) {
                lObject = pArray[index];
                if (lObject.visible) count++;
            }

            return count;
        }

        #endregion

        #region Snapshot

        void SwitchSnapshot(OnSceneLoaded e) {

            StudioEventEmitter lEmitter;

            int lLength = _emitterArray.Length;
            int lIndex;

            string lStringStart;
            string lStringStop;

            if (e == null) {
                for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                    lEmitter = _emitterArray[lIndex];
                    if (lEmitter.Event.Contains(MAP_VIEW_SNAPSHOT_STRING) || lEmitter.Event.Contains(ZOOM_VIEW_SNAPSHOT_STRING)) lEmitter.Stop();
                }

                _onMenu = true;
            }
            else {
                lStringStart = e.scene == SceneString.MapView ? MAP_VIEW_SNAPSHOT_STRING : ZOOM_VIEW_SNAPSHOT_STRING;
                lStringStop = e.scene == SceneString.MapView ? ZOOM_VIEW_SNAPSHOT_STRING : MAP_VIEW_SNAPSHOT_STRING;

                for (lIndex = lLength - 1; lIndex >= 0; lIndex--) {
                    lEmitter = _emitterArray[lIndex];
                    if (lEmitter.Event.Contains(lStringStart)) lEmitter.Play();
                    else if (lEmitter.Event.Contains(lStringStop)) lEmitter.Stop();
                }

                _onMenu = false;
            }
        }

        #endregion

        #region Music

        void UpdateWorldValuesAverage() {

            float _cleanValue = WorldValues.STATE_CLEANLINESS;
            float _npcValue = WorldValues.STATE_NPC;
            float _economyValue = WorldValues.STATE_ECONOMY;
            float _forestValue = WorldValues.STATE_FOREST;

            _averageValues = (_cleanValue + _npcValue + _economyValue + _forestValue) / 4;
            
            if (_averageValues < NEGATIVE_AVERAGE4_VALUE ||
                AverageWorldValues(_cleanValue, _npcValue) <= NEGATIVE_AVERAGE2_VALUE ||
                AverageWorldValues(_cleanValue, _economyValue) <= NEGATIVE_AVERAGE2_VALUE ||
                AverageWorldValues(_cleanValue, _forestValue) <= NEGATIVE_AVERAGE2_VALUE ||
                AverageWorldValues(_npcValue, _economyValue) <= NEGATIVE_AVERAGE2_VALUE ||
                AverageWorldValues(_npcValue, _forestValue) <= NEGATIVE_AVERAGE2_VALUE ||
                AverageWorldValues(_economyValue, _forestValue) <= NEGATIVE_AVERAGE2_VALUE ||
                _cleanValue <= NEGATIVE_VALUE || _npcValue <= NEGATIVE_VALUE ||
                _economyValue <= NEGATIVE_VALUE || _forestValue <= NEGATIVE_VALUE) {
                _musicEmitter.SetParameter(PARAM_MUSIC_STRING, NEGATIVE_MUSIC_VALUE);
            }
            else if(_averageValues <= NEUTRAL_MUSIC_VALUE) _musicEmitter.SetParameter(PARAM_MUSIC_STRING, NEUTRAL_MUSIC_VALUE);
            else _musicEmitter.SetParameter(PARAM_MUSIC_STRING, POSITIVE_MUSIC_VALUE);
        }

        float AverageWorldValues(float pValue1, float pValue2) {
            return (pValue1+pValue2) / 2;
        }

        #endregion

        #region Transition

        void CloudInSFX(CloudIn pEvent) {
            _cloudInEmitter.Play();
        }

        void CloudOutSFX(CloudOut pEvent) {
            _cloudOutEmitter.Play();
        }

        #endregion

        #region Alarm

        void PlayAlarm() {
            if(UIManager.instance.CheckRedGauges()) {
                PlayEmitter(SFX_ALARM_STRING);
                _alarmDone = true;
            }
        }
        #endregion

        #region Win / GameOver

        void PlayOnGameover() {
            StopAllEmitter();
            PlayEmitter(SFX_GAMEOVER_STRING);
        }

        void PlayOnWin() {
            StopAllEmitter();
            PlayEmitter(SFX_WIN_STRING);
        }

        #endregion

        protected void OnDestroy() {
            _instance = null;
        }
    }
}