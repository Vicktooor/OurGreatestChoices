using Assets.Scripts.Game.UI.Ftue;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
	public class UIObjectPointIcon : UIObjectPointer
	{
		protected ArrowTargetSprite[] sprites;

		protected override void Awake()
		{
			base.Awake();
			sprites = GetComponentsInChildren<ArrowTargetSprite>();
			foreach (ArrowTargetSprite fixSprite in sprites) fixSprite.SetParent(this);
		}

        protected override void FollowTarget()
		{
            foreach (ArrowTargetSprite fixSprite in sprites) fixSprite.Replace();
            base.FollowTarget();
		}

        public void SetProperties(float pDistance, float pAngle, bool autoRot, Transform uiTransform, Sprite pSprite, string diName, bool pautoDestroy)
		{
            SetSprite(pSprite);
            base.SetProperties(pDistance, pAngle, autoRot, uiTransform, diName, pautoDestroy);
		}

        public void SetProperties(float pDistance, float pAngle, bool autoRot, Vector3 pWorldPos, Sprite pSprite, string diName, bool pautoDestroy)
		{
            SetSprite(pSprite);
            base.SetProperties(pDistance, pAngle, autoRot, pWorldPos, diName, pautoDestroy);
		}

		private void SetSprite(Sprite pSprite)
		{
			foreach (ArrowTargetSprite sprite in sprites)
			{
				if (sprite.pictoType == EArrowSprite.PICTO) sprite.GetComponent<RawImage>().texture = pSprite.texture;
			}
		}
    }
}
