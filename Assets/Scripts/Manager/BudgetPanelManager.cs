using UnityEngine;
using System;
using Assets.Scripts.Game.UI.Global;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts.Game.UI;
using Assets.Script;
using Assets.Scripts.Utils.Budget;

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
		private BudgetPanelDrag diagramPanel;
		[SerializeField]
		private BudgetComponent buildingBudget;
		public TextMeshProUGUI UIName;

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
            if (buildingBudget.name != string.Empty) diagramPanel.ConstructDiagram(buildingBudget);
            else Close();
        }

		protected void Toggle()
		{
			if (_toggle)
			{
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
			buildingBudget = e.buildingbudget;
            UIName.text = buildingBudget.name;      
			Toggle();
		}

		public void Close()
		{
			diagramPanel.Clear();
			Toggle();
		}

		protected void OnDestroy()
		{
			Events.Instance.RemoveListener<OnPopupBuilding>(Open);
			_instance = null;
		}

        public void Clear()
        {
            diagramPanel.Clear();
        }
	}
}