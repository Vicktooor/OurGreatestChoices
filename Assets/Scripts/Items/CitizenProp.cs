using Assets.Scripts.Game.Save;
using Assets.Scripts.Manager;
using Assets.Scripts.PNJ;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Items {
    public class CitizenProp : ItemProp
	{
        public static float talkDistance = 0.15f;

        [Header("STATES")]
        [SerializeField]
        GameObject unHappyState;
        [SerializeField]
        GameObject fruitsMarketState;

        public List<SimpleLocalisationText> dialogueText;

        private bool _isHappy = true;
        private bool _isFruitsMarket = false;

        protected override void Awake()
        {
            EarthManager.citizens.Add(this);
            base.Awake();
            Events.Instance.AddListener<OnDialoguesLoaded>(OnLoadDialogue);
        }

        protected void OnLoadDialogue(OnDialoguesLoaded e)
        {
            dialogueText = new List<SimpleLocalisationText>();
            Events.Instance.RemoveListener<OnDialoguesLoaded>(OnLoadDialogue);
            foreach (CitizenDialogueSave cs in PlanetSave.CitizenTexts)
            {

                Vector3 pos = new Vector3(cs.x, cs.y, cs.z);
                if (pos.normalized == transform.position.normalized)
                {
                    for (int i = 0; i < cs.texts.Length; i++) dialogueText.Add(cs.texts[i]);
                }
            }
        }

        public void SetMood(bool pIsHappy) {
            if (pIsHappy) {
                if (_isHappy) return;
                else {
                    unHappyState.SetActive(false);
                    _isHappy = true;
                }
            }

            else {
                if (_isHappy) {
                    unHappyState.SetActive(true);
                    _isHappy = false;
                }
                else return;
            }
        }

        public void DisplayFruitsMarketState() {
            if (_isFruitsMarket) return;

            _isFruitsMarket = true;
            fruitsMarketState.SetActive(true);
        }

        protected override void OnDestroy()
        {
            Events.Instance.RemoveListener<OnDialoguesLoaded>(OnLoadDialogue);
            base.OnDestroy();
        }
    }
}
