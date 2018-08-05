using UnityEngine.UI;

namespace Assets.Scripts.Game.UI.Ftue
{
	public class UIFtueButton : UIFtueElement
	{
		protected Button btn;

		protected void Awake()
		{
			btn = GetComponent<Button>();
		}

		protected override void Start()
		{
			base.Start();
			if (btn && FtueManager.instance.active) btn.interactable = false;
		}

		protected override void ReveiveNextStep(OnFtueNextStep e)
		{
			if (btn) btn.interactable = false;
		}

		public void Active()
		{
			if (btn) btn.interactable = true;
		}
	}
}