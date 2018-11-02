using UnityEngine;
using System;
using Assets.Scripts.Game.UI.Global;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts.Game.UI;
using Assets.Script;
using Assets.Scripts.Game;
using UnityEngine.UI;

namespace Assets.Scripts.Manager
{

	/// <summary>
	/// 
	/// </summary>
	public class BudgetPanelManager : MonoBehaviour
	{
		private bool _toggle = false;

		#region Instance
		private static BudgetPanelManager _instance;

		/// <summary>
		/// instance unique de la classe     
		/// </summary>
		public static BudgetPanelManager instance
		{
			get
			{
				return _instance;
			}
		}
		#endregion

		[SerializeField]
		private BudgetComponent buildingBudget;
		public TextMeshProUGUI UIName;

        private InteractablePNJ _npc;

        public RawImage buildingIcon;
        public TextMeshProUGUI buildingMoney;
        public TextMeshProUGUI stockMoney;

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de BudgetPanelManager alors que c'est un singleton.");
			}
			_instance = this;

			Events.Instance.AddListener<OnPopupBuilding>(Open);
            gameObject.SetActive(false);
        }

        protected void OnEnable()
        {
            Events.Instance.AddListener<OnEndTween>(EndTween);
        }

        protected void OnDisable()
        {
            Events.Instance.RemoveListener<OnEndTween>(EndTween);
        }     

        protected void EndTween(OnEndTween e)
        {
            if (buildingBudget.initialBudget > 0) Set();
            else Close();
        }

        private void Set()
        {
            buildingIcon.texture = _npc.pictoHead.texture;
            buildingMoney.text = (buildingBudget.budget * WorldValues.PLAYER_MONEY_MULTIPLICATOR).ToString("#") + "/ -";
            stockMoney.text = (InventoryPlayer.Instance.moneyStock * WorldValues.PLAYER_MONEY_MULTIPLICATOR).ToString("#") + "/" + (InventoryPlayer.Instance.maxStock * WorldValues.PLAYER_MONEY_MULTIPLICATOR);
        }

        public void ClickGive()
        {
            if (buildingBudget.GiveBudget())
            {
                _npc.ReceiveBudget();
                Events.Instance.Raise(new OnUpdateNPCInfo());
            }
            Set();
        }

        public void ClickTake()
        {
            if (buildingBudget.TakeBudget())
            {
                _npc.SendBudget();
                Events.Instance.Raise(new OnUpdateNPCInfo());
            }
            Set();
        }

		protected void Toggle()
		{
			if (_toggle)
			{
                PlayerManager.Instance.player.ShowNPCState();
                gameObject.SetActive(false);
				_toggle = false;
			}
			else
			{
				gameObject.SetActive(true);
				_toggle = true;
			}
		}

		protected void Open(OnPopupBuilding e)
		{
            _npc = e.npc;
			buildingBudget = e.buildingbudget;
            UIName.text = TextManager.GetText(_npc.IDname);      
			Toggle();
		}

		public void Close()
		{
			Toggle();
		}

		protected void OnDestroy()
		{
			Events.Instance.RemoveListener<OnPopupBuilding>(Open);
			_instance = null;
		}
	}
}