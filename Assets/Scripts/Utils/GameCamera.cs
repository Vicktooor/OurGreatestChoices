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
			StartCoroutine(TransitionCoroutine(ECameraTargetType.MAP, Vector3.zero, go.GetComponent<Player>()));
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
			if (mode == ECameraTargetType.MAP) StartCoroutine(TransitionCoroutine(ECameraTargetType.ZOOM, Vector3.zero, player));
			else StartCoroutine(TransitionCoroutine(ECameraTargetType.MAP, Vector3.zero, player));
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
		protected IEnumerator TransitionCoroutine(ECameraTargetType targetView, Vector3 centerPos, Player targetPlayer)
		{
			if (FtueManager.instance.active && targetView != mode) Events.Instance.Raise(new OnInputFtuePinch());

			_transitionRunning = true;
			Block();
			float t = 0;
			Vector3 targetPosition;
			Quaternion targetRotation;
			Vector3 basePosition = transform.position;
			Quaternion baseRotation = transform.rotation;

			CameraManager.Instance.TransformCameraTarget(targetPlayer.playerAsset.transform, out targetPosition, out targetRotation, mode);

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

			if (targetView == ECameraTargetType.MAP)
			{
				if (mode == ECameraTargetType.ZOOM)
				{
					transform.position = transform.position + transform.position.normalized * CameraManager.Instance.MapViewDistance;
					if (planetTransform != null) transform.LookAt(planetTransform);
                    CameraManager.Instance.HandleZoomEnd();
				}
                else Events.Instance.Raise(new OnEndSwitchedPlayer());
            }
			if (targetView == ECameraTargetType.ZOOM)
			{
                CameraManager.Instance.HandleZoomEnd();
            }

			player = targetPlayer;
			if (FtueManager.instance.active && targetView != mode) Events.Instance.Raise(new OnEndFtuePinch());
			mode = targetView;		
			Unblock();
			_transitionRunning = false;
			StopAllCoroutines();
		}

        private bool _zoomRunning = false;
		protected IEnumerator ZoomCoroutine(float targetValue)
		{
            _zoomRunning = true;
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

				if(PlayerManager.Instance.playerType != EPlayer.ECO) PointingBubble.instance.Point();

				yield return null;
			}
            _zoomRunning = false;
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