using Assets.Scripts.Game.Save;
using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Items {
    public class CitizenProp : ItemProp
	{
        public static float talkDistance = 0.15f;

        public int ID;

        [Header("STATES")]
        [SerializeField]
        GameObject unHappyState;
        [SerializeField]
        GameObject fruitsMarketState;

        private bool _isHappy = true;
        private bool _isFruitsMarket = false;

        protected override void Awake()
        {
            EarthManager.citizens.Add(this);
            base.Awake();
        }

        public override void Init()
        {
            PositionID found = PlanetSave.CitizensID.Find(c => new Vector3(c.x, c.y, c.z) == transform.position);
            ID = found.ID;
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
            base.OnDestroy();
        }
    }
}
