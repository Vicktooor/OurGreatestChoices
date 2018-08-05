using Assets.Scripts.Game.UI.Ftue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
	public class UIObjectPointerText : UIObjectPointer
	{
		public RawImage sprite;
		public TextMeshPro textMesh;
		public ArrowTargetSprite[] sprites;

		protected static float fRotation = -90f;
		protected static float fDistance = 150f;

		protected override void Awake()
		{
			base.Awake();
			sprite = GetComponentInChildren<RawImage>();
			textMesh = GetComponentInChildren<TextMeshPro>();
			foreach (ArrowTargetSprite spr in sprites) spr.SetParent(this);
		}

		public void SetProperties(float pDistance, float pAngle, bool autoRot, Vector3 pWorldPos)
		{
			PointDistance = pDistance;
			pointRotation = pAngle;
			autoRotation = autoRot;
			worldTarget = pWorldPos;
		}

		public static UIObjectPointerText Build(Vector3 worldPos, string txt)
		{
			UIObjectPointerText infoBubble = new UIObjectPointerText();

			infoBubble.worldTarget = worldPos;
			infoBubble.textMesh.text = txt;
			infoBubble.SetProperties(fDistance, fRotation, false, worldPos);

			return infoBubble;
		}		
	}
}
