using Assets.Script;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Manager;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utils
{

	/// <summary>
	/// 
	/// </summary>
	public class GameCamera : CameraCC
	{
		public ECameraTargetType mode = ECameraTargetType.MAP;

		[SerializeField]
		protected bool _blocked = false;
		public bool Blocked { get { return _blocked; } }

		[SerializeField]
		protected Transform planetTransform;

		protected Player player;
		protected Vector3 targetPosition;
		protected Quaternion targetRotation;

		protected Vector2 _oldTouchPosition;
		protected Vector2 _oldHoldPosition;
		protected bool _holding = false;

		protected bool _transitionRunning = false;
		public bool TransitionRunning { get { return _transitionRunning; } }

        protected override void OnEnable()
		{
			base.OnEnable();
			Events.Instance.AddListener<SelectPlayer>(SetPos);
			Events.Instance.AddListener<OnPinchEnd>(ChangeSceneTransition);
			Events.Instance.AddListener<OnHold>(Hold);
			Events.Instance.AddListener<OnRemove>(Remove);
			Events.Instance.AddListener<OnPlayerInitFinish>(Init);
			Events.Instance.AddListener<OnStartSpeakingNPC>(ZoomOnPlayer);
			Events.Instance.AddListener<OnEndSpeakingNPC>(ZoomOutPlayer);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Events.Instance.RemoveListener<SelectPlayer>(SetPos);
            Events.Instance.RemoveListener<OnPinchEnd>(ChangeSceneTransition);
            Events.Instance.RemoveListener<OnHold>(Hold);
			Events.Instance.RemoveListener<OnRemove>(Remove);
			Events.Instance.RemoveListener<OnPlayerInitFinish>(Init);
			Events.Instance.RemoveListener<OnStartSpeakingNPC>(ZoomOnPlayer);
			Events.Instance.RemoveListener<OnEndSpeakingNPC>(ZoomOutPlayer);
		}

		protected void Init(OnPlayerInitFinish e)
		{
            planetTransform = EarthManager.Instance.planetLink.transform;
            player = PlayerManager.Instance.GetPlayer();
            CameraManager.Instance.TransformCameraTarget(player.transform, out targetPosition, out targetRotation, mode);
			transform.position = targetPosition.normalized * CameraManager.Instance.MapViewDistance;
			transform.rotation = targetRotation;
			_oldHoldPosition = Vector2.zero;
		}

		protected void Remove(OnRemove e)
		{
			_holding = false;
			_oldHoldPosition = Vector2.zero;
		}

		public void ChangeSceneTransition(OnPinchEnd e)
		{
			ChangeScene();
		}

		protected void ZoomOnPlayer(OnStartSpeakingNPC e)
		{
            StopAllCoroutines();
            StartCoroutine(ZoomCoroutine(0.15f));
		}

		protected void ZoomOutPlayer(OnEndSpeakingNPC e)
		{
            StopAllCoroutines();
            StartCoroutine(ZoomCoroutine(0.35f));
		}

		protected void LateUpdate()
		{
            if (_transitionRunning) return;
			Vector3 targetPosition;
			Quaternion targetRotation;
			if (mode == ECameraTargetType.ZOOM)
			{
				if (_blocked) return;
                else if (player)
                {
                    CameraManager.Instance.TransformCameraTarget(player.transform, out targetPosition, out targetRotation, mode);
                    transform.position = targetPosition;
                    transform.rotation = targetRotation;
                }			
			}
			else
			{
				if (_blocked)
                {
                    if (!player) return;
                    Vector3 nPos = transform.position.normalized;
                    transform.position = (nPos * player.transform.position.magnitude) + nPos * CameraManager.Instance.MapViewDistance;
                }
                else if (player != null)
				{
                    transform.position = player.transform.position + player.transform.position.normalized * CameraManager.Instance.MapViewDistance;
                    transform.LookAt(player.transform);
				}
			}
		}

		protected void SetPos(SelectPlayer e)
		{
			if (player == null) player = e.player;
			else
			{
				if (_blocked)
				{
					if (player.playerType != e.player.playerType)
					{
						PlayerToPlayer(e.player.gameObject);
						player = e.player;
					}
				}
				else
				{
					PlayerToPlayer(e.player.gameObject);
					player = e.player;
				}
			}
		}

		protected void OrbitalMove()
		{
            if (planetTransform == null) return;

			Vector3 lastBlockedPos = transform.position;
			Quaternion lastBlockedRot = transform.rotation;
			Vector2 mousePos = ControllerInput.instance.touchCenterPosition;
			if (_oldHoldPosition == Vector2.zero) _oldHoldPosition = mousePos;
			
			transform.RotateAround(planetTransform.position, transform.up, (mousePos.x - _oldHoldPosition.x) * CameraManager.Instance.SlideSpeed);
			transform.RotateAround(planetTransform.position, transform.right, -(mousePos.y - _oldHoldPosition.y) * CameraManager.Instance.SlideSpeed);
			_oldHoldPosition = mousePos;

			float angle = Vector3.Angle(Vector3.up, transform.position);
			if (angle >= 10f && angle <= 170f)
			{
				Reoriente();
			}
			else
			{
				transform.position = lastBlockedPos;
				transform.rotation = lastBlockedRot;
			}
		}

		protected void Hold(OnHold e)
		{
            if (_rolling || mode != ECameraTargetType.MAP) return;		
			Block();
			_holding = true;
			StartCoroutine(OrbitalCoroutine());			
		}

		public void Block()
		{
			_blocked = true;
		}

		public void Unblock()
		{
			_blocked = false;
		}

		protected void PlayerToPlayer(GameObject go)
		{
			StartCoroutine(TransitionCoroutine(ECameraTargetType.MAP, go.GetComponent<Player>()));
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360F)
				angle += 360F;
			if (angle > 360F)
				angle -= 360F;
			return Mathf.Clamp(angle, min, max);
		}

		protected void ChangeScene()
		{
			if (mode == ECameraTargetType.MAP) StartCoroutine(TransitionCoroutine(ECameraTargetType.ZOOM, player));
			else StartCoroutine(TransitionCoroutine(ECameraTargetType.MAP, player));
		}

        private bool _rolling = false;
		protected IEnumerator OrbitalCoroutine()
		{
            _rolling = true;
			while (_blocked && _holding && !_transitionRunning)
			{
				OrbitalMove();
				yield return null;
			}
            _rolling = false;
		}

        /// <summary>
        /// Camera transition coroutine
        /// </summary>
        /// <param name="transitionType"></param>
        /// <param name="targetView"></param>
        /// <param name="centerPos">Use only with circle transition</param>
        /// <returns></returns>
        private ECameraTargetType _targetMode = ECameraTargetType.NONE;
		protected IEnumerator TransitionCoroutine(ECameraTargetType targetView, Player targetPlayer)
		{
            ControllerInput.instance.ControlEnable = EControlEnable.Off;
            UIManager.instance.PNJState.Active(false);
			if (FtueManager.instance.active && targetView != mode) Events.Instance.Raise(new OnInputFtuePinch());

			_transitionRunning = true;
			Block();
			float t = 0;
			Vector3 basePosition = transform.position;
			Quaternion baseRotation = transform.rotation;

			CameraManager.Instance.TransformCameraTarget(targetPlayer.transform, out targetPosition, out targetRotation, targetView);

			while (t < 1)
			{
                switch (targetView)
                {
                    case ECameraTargetType.MAP:
                        if (mode == ECameraTargetType.MAP)
                        {
                            t += Time.deltaTime;
                            if (t > 1) t = 1;
                            float x = Easing.CrossFade(Easing.SmoothStart, 2, Easing.SmoothStop, 2, t);
                            transform.position = Vector3.Slerp(basePosition, targetPosition, x);
                            Reoriente();
                            yield return null;
                        }
                        else
                        {
                            t += Time.deltaTime * (1f / SwitchPanel.TIME_TRANSITION);
                            if (t > 1) t = 1;
                            transform.position += (transform.position.normalized - transform.forward) * (Time.deltaTime / 4f);
                        }
                        break;
                    case ECameraTargetType.ZOOM:
                        t += Time.deltaTime  * (1f / SwitchPanel.TIME_TRANSITION);
                        if (t > 1) t = 1;
                        transform.position -= transform.position.normalized * Time.deltaTime;
                        break;
                    default:
                        break;
                }
                yield return null;
            }   

            _targetMode = targetView;
            if (targetView == mode)
            {
                ControllerInput.instance.ControlEnable = EControlEnable.On;
                player = targetPlayer;
                Events.Instance.Raise(new OnEndSwitchedPlayer());
                Unblock();
                _transitionRunning = false;
                StopAllCoroutines();
            }
            else
            {
                if (FtueManager.instance.active) Events.Instance.Raise(new OnEndFtuePinch());
                Events.Instance.AddListener<OnTransitionEnd>(FinishTransition);
            } 
		}

        protected void FinishTransition(OnTransitionEnd e)
        {
            player = PlayerManager.Instance.GetPlayer();
            CameraManager.Instance.TransformCameraTarget(player.transform, out targetPosition, out targetRotation, _targetMode);
            Events.Instance.RemoveListener<OnTransitionEnd>(FinishTransition);
            if (_targetMode == ECameraTargetType.MAP)
            {
                if (mode == ECameraTargetType.ZOOM)
                {
                    mode = _targetMode;
                    StartCoroutine(DezoomIntroCoroutine());
                }
            }
            if (_targetMode == ECameraTargetType.ZOOM)
            {
                mode = _targetMode;
                StartCoroutine(ZoomIntroCoroutine());
            }
        }

        private IEnumerator ZoomIntroCoroutine()
        {
            float t = 0f;
            float x = 0;
            float xr = 0;
            transform.LookAt(player.transform);
            Quaternion r = transform.rotation;
            Vector3 initPos = transform.position;
            while (t < 1f)
            {
                t = Mathf.Clamp(t + (Time.deltaTime * (1f / 2f)), 0f, 1f);
                x = Easing.CrossFade(Easing.SmoothStart, 2, Easing.SmoothStop, 2, t);
                xr = Easing.Mix(Easing.SmoothStop, 2, Easing.SmoothStart, 4, 0.3f, t);
                transform.position = Vector3.Lerp(initPos, targetPosition, x);
                transform.rotation = Quaternion.Lerp(r, targetRotation, xr);
                yield return null;
            }

            ControllerInput.instance.ControlEnable = EControlEnable.On;
            CameraManager.Instance.HandleZoomEnd();
            Unblock();
            _transitionRunning = false;
            StopAllCoroutines();
        }

        private IEnumerator DezoomIntroCoroutine()
        {
            Vector3 initPos = transform.position;
            float t = 0f;
            float x = 0;
            while (t < 1f)
            {
                t = Mathf.Clamp(t + (Time.deltaTime * (1f / 2f)), 0f, 1f);
                x = Easing.CrossFade(Easing.SmoothStart, 2, Easing.SmoothStop, 2, t);
                transform.position = Vector3.Lerp(initPos, targetPosition, x);
                transform.LookAt(planetTransform);
                yield return null;
            }

            ControllerInput.instance.ControlEnable = EControlEnable.On;
            CameraManager.Instance.HandleZoomEnd();
            Unblock();
            _transitionRunning = false;
            StopAllCoroutines();
        }

		protected IEnumerator ZoomCoroutine(float targetValue)
		{
			int zoom = 0;
			if (targetValue > CameraManager.Instance.StepHeight) zoom = 1;
			else if (targetValue < CameraManager.Instance.StepHeight) zoom = -1;

			while (CameraManager.Instance.StepHeight != targetValue)
			{
				if (zoom > 0)
				{
					CameraManager.Instance.StepHeight += Time.deltaTime / 5f;
					if (CameraManager.Instance.StepHeight > targetValue) CameraManager.Instance.StepHeight = targetValue;
				}
				else
				{
					CameraManager.Instance.StepHeight -= Time.deltaTime / 5f;
					if (CameraManager.Instance.StepHeight < targetValue) CameraManager.Instance.StepHeight = targetValue;
				}

				if(PlayerManager.Instance.playerType == EPlayer.NGO) PointingBubble.instance.Point();

				yield return null;
			}

            Events.Instance.Raise(new OnEndPanelZoom());
		}

		protected void Reoriente()
		{
			Vector3 forwardVector = -transform.position.normalized;
			Vector3 rightVector = Vector3.Cross(forwardVector, Vector3.up).normalized;
			Vector3 upVector = MathCustom.GetFaceNormalVector(transform.position, transform.position + rightVector, transform.position + forwardVector);
			transform.rotation = Quaternion.LookRotation(forwardVector, -upVector);
		}

        public override void Reset()
        {
            if (player != null)
            {
                transform.position = player.transform.position + player.transform.position.normalized * CameraManager.Instance.MapViewDistance;
                transform.LookAt(player.transform);
                player = null;
            }

            planetTransform = null;
            mode = ECameraTargetType.MAP;
            _blocked = false;           
            _holding = false;
            _transitionRunning = false;
        }
    }
}