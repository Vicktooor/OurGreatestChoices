using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.UI
{

	/// <summary>
	/// 
	/// </summary>
	public class UIBudgetArrow : MonoBehaviour
	{
		protected Dictionary<EArrowPointer, UIArrowPointer> pointers = new Dictionary<EArrowPointer, UIArrowPointer>();

		public void Setup(BudgetComponent dragBudget, BudgetComponent mainBudget)
		{
			UIArrowPointer[] lPointers = GetComponentsInChildren<UIArrowPointer>();
			if (lPointers != null)
			{
				foreach (UIArrowPointer ap in lPointers)
				{
					if (!pointers.ContainsKey(ap.side)) pointers.Add(ap.side, ap);
				}

				foreach (string bt in dragBudget.budgetLinks)
				{
					if (mainBudget.name == bt) return;
				}
				if (pointers.ContainsKey(EArrowPointer.Left)) pointers[EArrowPointer.Left].gameObject.SetActive(false);
			}
			
		}
	}
}