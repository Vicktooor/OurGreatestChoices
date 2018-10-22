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
            if (buildingBudget.name != string.Empty) Set();
            else Close();
        }

        private void Set()
        {
            buildingIcon.texture = _npc.pictoHead.texture;
            buildingMoney.text = (buildingBudget.budget * WorldValues.PLAYER_MONEY_MULTIPLICATOR) + "/" + (buildingBudget.targetBudget * WorldValues.PLAYER_MONEY_MULTIPLICATOR);
            stockMoney.text = (InventoryPlayer.instance.moneyStock * WorldValues.PLAYER_MONEY_MULTIPLICATOR) + "/" + (InventoryPlayer.instance.maxStock * WorldValues.PLAYER_MONEY_MULTIPLICATOR);
        }

        public void ClickGive()
        {
            buildingBudget.GiveBudget();
            Set();
        }

        public void ClickTake()
        {
            buildingBudget.TakeBudget();
            Set();
        }

		protected void Toggle()
		{
			if (_toggle)
			{
                UIManager.instance.PNJState.Active(true);
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
            if (e.buildingbudget.targetBudget == 0) return;
            _npc = e.npc;
			buildingBudget = e.buildingbudget;
            UIName.text = buildingBudget.name;      
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