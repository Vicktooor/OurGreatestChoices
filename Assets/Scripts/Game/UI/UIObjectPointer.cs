using Assets.Script;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.UI
{
	/// <summary>
	/// 
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class UIObjectPointer : MonoBehaviour
	{
        internal class CallBackPos
        {
            public Action<Vector3> method;
            public Vector3 param;
        }

        public static float INTERACT_DISTANCE = 0.2f;

		public float pointRotation;
		public bool autoRotation;
        public bool autoDestroy;
		protected RectTransform rect;

        protected string _displayerInstanceName;

        [SerializeField]
		protected Transform uiTarget = null;
		protected Vector3 worldTarget = Vector3.zero;

        protected List<Action> callbacks = new List<Action>();
        private List<CallBackPos> callbacksPos = new List<CallBackPos>();

		[SerializeField]
		protected float _pointDistance;
		public float PointDistance {
			get { return Mathf.Clamp(_pointDistance, 0, 1500f); }
			set { _pointDistance = Mathf.Clamp(value, 0, 1500f); }
		}

        private float _activePointDistance;

		protected virtual void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

        public virtual void SetProperties(float pDistance, float pAngle, bool autoRot, Vector3 worldPos, string diName, bool pautoDestroy)
        {
            _displayerInstanceName = diName;
            autoDestroy = pautoDestroy;
            PointDistance = pDistance;
            _activePointDistance = pDistance;
            pointRotation = pAngle;
            autoRotation = autoRot;
            worldTarget = worldPos;
            uiTarget = null;
            FollowTarget();
        }

        public virtual void SetProperties(float pDistance, float pAngle, bool autoRot, Transform pUITarget, string diName, bool pautoDestroy)
        {
            _displayerInstanceName = diName;
            autoDestroy = pautoDestroy;
            PointDistance = pDistance;
            _activePointDistance = pDistance;
            pointRotation = pAngle;
            autoRotation = autoRot;
            worldTarget = Vector3.zero;
            uiTarget = pUITarget;
            FollowTarget();
        }

        protected virtual void FollowTarget()
		{
			if (uiTarget) Point(uiTarget.position);
			else if (worldTarget != Vector3.zero)
			{
				Vector3 objScreenPos = Camera.main.WorldToScreenPoint(worldTarget);
				if (objScreenPos.z < 0)
				{
                    objScreenPos.z = 0;
                    objScreenPos.y = -objScreenPos.y;
                    objScreenPos.x = -objScreenPos.x;
                    Point(objScreenPos);
				}
				else Point(Camera.main.WorldToScreenPoint(worldTarget));
			}

            Player playerTransform = PlayerManager.Instance.player;
            if (playerTransform != null && autoDestroy)
            {
                if (Vector3.Distance(worldTarget, playerTransform.transform.position) <= INTERACT_DISTANCE / 2f)
                {
                    ActiveCallBacks();
                    ArrowDisplayer.Instances(_displayerInstanceName).DestroyArrow(this);
                }
            }
        }

		protected void Point(Vector2 targetScreenPos)
		{
			float yRatio = 1080f / Screen.width;
			float xRatio = 1920f / Screen.height;

			Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2f);
			Vector2 lTargetPos = targetScreenPos - screenCenter;

			if (autoRotation) pointRotation = Mathf.Atan2(-Mathf.Abs(lTargetPos.y), lTargetPos.x) * Mathf.Rad2Deg;

			float cos = Mathf.Cos(pointRotation * Mathf.Deg2Rad);
			float sin = Mathf.Sin(pointRotation * Mathf.Deg2Rad);
			Vector2 dir = new Vector2(cos, sin);

			Vector2 textureSize = new Vector2(rect.sizeDelta.x / 2f, rect.sizeDelta.y / 2f);

			lTargetPos -= dir * (_activePointDistance / (xRatio * yRatio));
			if (targetScreenPos.x > screenCenter.x * 2f) lTargetPos.x = (screenCenter.x * xRatio) - textureSize.x;
			if (targetScreenPos.x < 0) lTargetPos.x = -(screenCenter.x * xRatio) + textureSize.x;
			if (targetScreenPos.y > screenCenter.y * 2f) lTargetPos.y = (screenCenter.y * yRatio) - textureSize.y;
			if (targetScreenPos.y < 0) lTargetPos.y = -(screenCenter.y * yRatio) + textureSize.y;

			Vector3 newPos = lTargetPos;
			newPos.z = 0;
            if (!autoRotation) transform.localPosition = new Vector2(newPos.x * xRatio, newPos.y * yRatio);
            else transform.localPosition = new Vector2(newPos.x, newPos.y);

            transform.localRotation = Quaternion.Euler(0f, 0f, pointRotation);
		}

        public void AddCallBack(Action callBack)
        {
            if (!callbacks.Contains(callBack)) callbacks.Add(callBack);
        }

        public void AddCallBackPos(Action<Vector3> callBack, Vector3 param)
        {
            CallBackPos cb = new CallBackPos();
            cb.method = callBack;
            cb.param = param;
            if (callbacksPos.Find(m => m.method == callBack) != null) callbacksPos.Add(cb);
        }

        protected void ActiveCallBacks()
        {
            foreach (Action method in callbacks) method();
            foreach (CallBackPos cb in callbacksPos) cb.method(cb.param);
            callbacks.Clear();
            callbacksPos.Clear();
        }

        public void Update()
        {
            MoveAnimation();
            FollowTarget();
        }

        public void SetAnimProperties(float animTime, float pAnimDist)
        {
            tAnim = 0f;
            tAnimTime = animTime;
            animDist = pAnimDist;
        }

        private float tAnim = 0f;
        private float tAnimTime = 0.75f;
        private float animDist = 25f;
        private void MoveAnimation()
        {
            if (tAnimTime > 0f) tAnim = (tAnim + Mathf.Clamp01(Time.deltaTime * (1f / tAnimTime))) % 1f;
            float x = Easing.Arch(tAnim);
            _activePointDistance = PointDistance + (animDist * x);
        }

        public Vector3 GetWorldTargetPos()
		{
			return worldTarget;
		}
	}
}