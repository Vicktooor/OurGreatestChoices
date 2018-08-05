using Assets.Scripts.Game;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EBudgetState { None, Good, Bad, Bonus }

namespace Assets.Scripts.Utils.Budget
{

	/// <summary>
	/// 
	/// </summary>
	public class BudgetToolTip : MonoBehaviour
	{
        public TextMeshProUGUI budgetText;
		private List<BudgetToolTipState> _toolsTips = new List<BudgetToolTipState>();
        private Transform _targetTransform;

		public void Awake()
		{
			BudgetToolTipState[] tips = GetComponentsInChildren<BudgetToolTipState>();
			for (int i = 0; i < tips.Length; i++) _toolsTips.Add(tips[i]);
		}

		public void SetState(EBudgetState pState, Transform tTranform, float budgetValue)
		{
            budgetText.text = (budgetValue * WorldValues.PLAYER_MONEY_MULTIPLICATOR).ToString("0.");
            _targetTransform = tTranform;
            foreach (BudgetToolTipState item in _toolsTips)
			{
				if (item.budgetState != pState) item.gameObject.SetActive(false);
			}
            StartCoroutine(FollowTarget());
		}

        private IEnumerator FollowTarget()
        {
            while (true)
            {
                transform.position = Camera.main.WorldToScreenPoint(_targetTransform.position);
                yield return null;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}