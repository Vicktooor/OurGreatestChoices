using TMPro;
using UnityEngine;

namespace Assets.Scripts.Game.UI.Ftue
{
	public class UIFtueText : UIFtueImageContainer
	{
		public TextMeshProUGUI textAsset;

        public void SetText(string txt)
        {
            if (textAsset) textAsset.text = txt;
        }

		protected override void ReveiveNextStep(OnFtueNextStep e)
		{

		}

        protected override void SetAlpha(float alpha)
        {
            base.SetAlpha(alpha);
            Color lColor = textAsset.color;
            lColor.a = Alpha;
            textAsset.color = lColor;
        }
	}
}
