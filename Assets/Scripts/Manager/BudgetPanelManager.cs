using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts.Game.UI;
using Assets.Script;
using Assets.Scripts.Game;
using UnityEngine.UI;
using Assets.Scripts.Game.UI.Ftue;

namespace Assets.Scripts.Manager
{

	/// <summary>
	/// 
	/// </summary>
	public class BudgetPanelManager : MonoBehaviour
	{
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

        public Tweener tweener;

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de BudgetPanelManager alors que c'est un singleton.");
			}
			_instance = this;
            TweenerLead.Instance.NewTween(GetComponent<RectTransform>(), tweener);
            Events.Instance.AddListener<OnPopupBuilding>(Open);
            gameObject.SetActive(false);
        }

        protected void OnEnable()
        {
            ControllerInput.AddScreen(transform);
        }

        protected void OnDisable()
        {
            ControllerInput.RemoveScreen(transform);
            if (ControllerInput.instance) ControllerInput.instance.ResetDatasTouch();
        }

        protected void CheckFtue()
        {
            if (FtueManager.instance.active)
            {
                if (FtueManager.instance.currentStep.waitOpeningPanel == transform) FtueManager.instance.ValidStep();
            }
        }

        private void Opening()
        {
            float x1 = Easing.Scale(Easing.SmoothStop, tweener.t, 2, 2f);
            float x2 = Easing.FlipScale(Easing.SmoothStart, tweener.t, 2, 2f);
            float x = Easing.Mix(x1, x2, 0.5f, tweener.t);
            tweener.SetScale(x);
        }

        private void Set()
        {
            Active();
            buildingIcon.texture = _npc.pictoHead.texture;
            buildingMoney.text = (buildingBudget.budget * WorldValues.PLAYER_MONEY_MULTIPLICATOR).ToString("#0") + "/ -";
            stockMoney.text = (InventoryPlayer.Instance.moneyStock * WorldValues.PLAYER_MONEY_MULTIPLICATOR).ToString("#0") + "/" + (InventoryPlayer.Instance.maxStock * WorldValues.PLAYER_MONEY_MULTIPLICATOR);
        }

        public void ClickGive()
        {
            if (buildingBudget.GiveBudget())
            {
                _npc.ReceiveBudget();
                Events.Instance.Raise(new OnUpdateNPCInfo());
                if (FtueManager.instance.active)
                {
                    if (FtueManager.instance.currentStep.targetBudget != 0)
                    {
                        FtueManager.instance.currentStep.GetBudget();
                        if (FtueManager.instance.currentStep.HaveBudget())
                            FtueManager.instance.ValidStep();
                    }
                }
            }
            Set();
        }

        public void ClickTake()
        {
            if (buildingBudget.TakeBudget())
            {
                _npc.SendBudget();
                Events.Instance.Raise(new OnUpdateNPCInfo());
                if (FtueManager.instance.active)
                {
                    if (FtueManager.instance.currentStep.targetBudget != 0)
                    {
                        FtueManager.instance.currentStep.GetBudget();
                        if (FtueManager.instance.currentStep.HaveBudget())
                            FtueManager.instance.ValidStep();
                    }
                }
            }
            Set();
        }

        private void Active()
        {
            gameObject.SetActive(true);
        }

		protected void Open(OnPopupBuilding e)
		{
            _npc = e.npc;
			buildingBudget = e.buildingbudget;
            UIName.text = TextManager.GetText(_npc.IDname);
            tweener.SetMethods(Opening, Set, CheckFtue, null);
            TweenerLead.Instance.StartTween(tweener);
        }

        private void TweenClose()
        {
            PlayerManager.Instance.player.ShowNPCState();
            gameObject.SetActive(false);
        }

		public void Close()
		{
            if (tweener.Opened)
            {
                tweener.SetMethods(Opening, null, null, TweenClose);
                TweenerLead.Instance.StartTween(tweener);
            }
        }

		protected void OnDestroy()
		{
			Events.Instance.RemoveListener<OnPopupBuilding>(Open);
			_instance = null;
		}
	}
}