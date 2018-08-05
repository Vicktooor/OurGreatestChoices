using Assets.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{

	/// <summary>
	/// 
	/// </summary>
	public class UIDraggableBudget : MonoBehaviour
	{
		public Image img;
		public BudgetComponent budgetComponent;
        private TextMeshProUGUI text;

		public void Awake()
		{
			img = GetComponent<Image>();
			text = GetComponentInChildren<TextMeshProUGUI>();
		}

        public void UpdateMoney()
        {
            text.text = (budgetComponent.budget * WorldValues.PLAYER_MONEY_MULTIPLICATOR).ToString("0.") + " $";
        }
    }
}