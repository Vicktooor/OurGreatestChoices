using UnityEngine;

namespace Assets.Scripts.Game.UI.Ftue
{
	public enum EArrowSprite { STATIC, PICTO }

	public class ArrowTargetSprite : MonoBehaviour
	{
		public EArrowSprite pictoType;
		protected UIObjectPointer parent;

		public void Replace()
		{
			transform.localRotation = Quaternion.Euler(0f, 0f, -parent.pointRotation);
		}

		public void SetParent(UIObjectPointer pParent)
		{
			parent = pParent;
		}
	}
}