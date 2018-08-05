using UnityEngine;
using System;
using Assets.Scripts.Manager;
using System.Collections.Generic;
using Assets.Scripts.Utils.Budget;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Game.UI.Global
{
	public class UIBudgetMap : MonoBehaviour
	{
		[SerializeField]
		private BudgetToolTip toolTipModel;
		private List<BudgetToolTip> toolTips = new List<BudgetToolTip>();

		private bool _toggle = false;
		private Dictionary<BudgetComponent, GameObject> budgetsProps = new Dictionary<BudgetComponent, GameObject>();

		#region Instance
		private static UIBudgetMap _instance;
		public static UIBudgetMap instance
		{
			get
			{
				return _instance;
			}
		}
		#endregion

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de UIBudgetMap alors que c'est un singleton.");
			}
			_instance = this;
			ForceToggle(false);
		}

        public void Open(Dictionary<BudgetComponent ,GameObject> objs)
		{
			Toggle();
			budgetsProps.Clear();
			budgetsProps = objs;
			foreach (KeyValuePair<BudgetComponent, GameObject> kpv in budgetsProps)
			{
				BudgetToolTip toolTip = Instantiate(
					toolTipModel,
					Camera.main.WorldToScreenPoint(kpv.Value.transform.position),
					Quaternion.identity,
					transform
					);

				toolTip.SetState(ResourcesManager.instance.GetState(kpv.Key), kpv.Value.transform, kpv.Key.budget);
				toolTips.Add(toolTip);
			}
		}

		public void Close()
		{
			Clear();
			Toggle();
        }

		public void Clear()
		{
			foreach (BudgetToolTip btt in toolTips)
			{
				Destroy(btt.gameObject);
			}
			toolTips.Clear();
		}

		protected void ForceToggle(bool force)
		{
			gameObject.SetActive(force);
			_toggle = force;
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

		protected void OnDestroy()
		{
			_instance = null;
		}
	}
}