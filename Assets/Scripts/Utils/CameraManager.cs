using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace Assets.Scripts.Utils
{
	public enum ECamera { NONE, ORBITAL, CIRCULAR, GAME }
	public enum ECameraTargetType { MAP, ZOOM, BOTH, NONE }

	public class CameraManager : MonoBehaviour
	{
        public static EQuality QUALITY;

        private GameCamera _gc;
        public GameCamera GameCamera { get { return _gc; } }

        public ECamera startCamera;

		private float MinZoomDistance = 0.35f;
		[SerializeField]
		private float MaxZoomDistance = 1.2f;
		[SerializeField]
		private float MinMapDistance = 7;
		private float MaxMapDistance = 15;
		private float MinStepheight = 0.05f;
		private float MaxStepheight = 1.5f;

		[SerializeField]
		protected float _mapViewDistance = 10f;
		public float MapViewDistance {
			get { return Mathf.Clamp(_mapViewDistance, MinMapDistance, MaxMapDistance); }
			set { _mapViewDistance = Mathf.Clamp(value, MinMapDistance, MaxMapDistance); }
		}

		[SerializeField]
		protected float _swipeSpeed = 10f;
		public float SwipeSpeed
		{
			get { return Mathf.Clamp(_swipeSpeed, 2f, 50f); }
		}

		[SerializeField]
		protected float _slideSpeed = 10f;
		public float SlideSpeed
		{
			get { return Mathf.Clamp(_slideSpeed, 2f, 50f); }
		}

		[Header("Zoom Camera Properties")]
		public float elevation = 25f;
		protected float Elevation {
			get { return Mathf.Clamp(elevation, 5f, 89f) * Mathf.Deg2Rad; }
			set { elevation = Mathf.Clamp(value, 5f, 89f) * Mathf.Deg2Rad; }
		}

		[SerializeField]
		protected float _distanceToplayer = 0.85f;
		public float DistanceToplayer
		{
			get { return Mathf.Clamp(_distanceToplayer, MinZoomDistance, MaxZoomDistance); }
			set { _distanceToplayer = Mathf.Clamp(value, MinZoomDistance, MaxZoomDistance); }
		}

		[SerializeField]
		protected float _stepHeight = 0.35f;
		public float StepHeight
		{
			get { return Mathf.Clamp(_stepHeight, MinStepheight, MaxStepheight); }
			set { _stepHeight = Mathf.Clamp(value, MinStepheight, MaxStepheight); }
		}

		protected float _actualPolar;
		protected float ActualPolar { get { return _actualPolar % (Mathf.PI * 2); } }

		protected static CameraManager _instance;
		public static CameraManager Instance
		{
			get { return _instance; }
		}
		
		protected CameraCC[] cameras;
		protected Dictionary<ECamera, CameraCC> typedCameras = new Dictionary<ECamera, CameraCC>();
		protected ECamera activeCamera;

		protected bool _transitionRunning = false;

        private bool _endCalled = false;

        // Atmosphere
        [SerializeField]
		private PostProcessingProfile _postProcess;
		[SerializeField]
		private GameObject _atmospherePlane;

		protected void Awake()
		{
			InitInstance();

			cameras = Camera.main.GetComponents<CameraCC>();
			typedCameras = new Dictionary<ECamera, CameraCC>();
			foreach (CameraCC cam in cameras) typedCameras.Add(cam.type, cam);

            _gc = GetGameCamera();
			ActiveCamera(startCamera);

            SetQuality(EQuality.Standard);
		}

		protected void OnEnable()
		{
			Events.Instance.AddListener<OnPinch>(Pinch);
		}

		protected void OnDisable()
		{
			Events.Instance.RemoveListener<OnPinch>(Pinch);
        }

        public void HandleZoomEnd()
        {
            _endCalled = false;
            _mapViewDistance = 10f;
            _distanceToplayer = 0.85f;
        }

		protected void Update()
		{
			if (Input.GetKey(KeyCode.S)) Events.Instance.Raise(new OnEdtionInputKeyEvent(KeyCode.S));
			if (Input.GetMouseButton(1)) Events.Instance.Raise(new OnEditionRightClick(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
			if (Input.GetMouseButton(0)) Events.Instance.Raise(new OnEditionLeftClick(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
			if (Input.GetAxis("Mouse ScrollWheel") != 0) Events.Instance.Raise(new OnEditionMouseScroll(Input.GetAxis("Mouse ScrollWheel")));
			if (Input.GetKeyDown(KeyCode.Q)) ActiveCamera(ECamera.ORBITAL);
		}

		public void ShowAtmosphere(bool toShow)
		{
			if (toShow && QUALITY == EQuality.HD)
			{
				_postProcess.depthOfField.enabled = true;
				_atmospherePlane.SetActive(true);
			}
            else if (toShow && QUALITY == EQuality.Standard)
            {
                _atmospherePlane.SetActive(true);
            }
			else
			{
				_postProcess.depthOfField.enabled = false;
				_atmospherePlane.SetActive(false);
			}
		}

        public void SetQuality(EQuality quality)
        {
            QUALITY = quality;
            if (QUALITY == EQuality.Standard)
            {
                _postProcess.depthOfField.enabled = false;
                _postProcess.ambientOcclusion.enabled = false;
                _postProcess.vignette.enabled = false;           
            }
            else
            {
                _postProcess.depthOfField.enabled = true;
                _postProcess.ambientOcclusion.enabled = true;
                _postProcess.vignette.enabled = true;
            }
            Events.Instance.Raise(new OnChangeQuality());
        }

		protected void Pinch(OnPinch e)
		{
			GameCamera gCam = GetGameCamera();
            if (gCam.TransitionRunning) return;  
            if (GameManager.Instance.LoadedScene == SceneString.MapView)
            {
                MapViewDistance += e.value / 100f;
                if (_mapViewDistance <= MinMapDistance && !_endCalled)
                {
                    _endCalled = true;
                    Events.Instance.Raise(new OnPinchEnd());
                }
            }
            else
            {
                if (e.value > 0)
                {
                    DistanceToplayer += e.value / 1000f;
                    if (_distanceToplayer >= MaxZoomDistance && !_endCalled)
                    {
                        _endCalled = true;
                        Events.Instance.Raise(new OnPinchEnd());
                    }
                }
            }
        }

		public void GetActiveCameraType(out ECamera outType)
		{
			outType = ECamera.NONE;
			foreach (CameraCC cam in cameras) {
				if (cam.isActiveAndEnabled) outType = cam.type;
			}
		}

		public void ActiveCamera(ECamera type)
		{
			activeCamera = type;
			foreach (CameraCC cam in cameras)
			{
				if (cam.isActiveAndEnabled) {
					if (cam.type != activeCamera) cam.enabled = false;
				}	
				else {
					if (cam.type == activeCamera) cam.enabled = true;
				}		
			}
		}

		public void GetActiveCameraScript(ECamera type, out CameraCC outCam)
		{
			outCam = typedCameras[type];
		}

		public void TransformCameraTarget(Transform targetTransform, out Vector3 outPosition, out Quaternion outRotation, ECameraTargetType view)
		{
			if (view == ECameraTargetType.MAP)
			{
				Vector3 lpos = targetTransform.position;
				outPosition = lpos + (lpos.normalized * MapViewDistance);

				Vector3 upVector = lpos.normalized;
				Vector3 rightVector = Vector3.Cross(upVector, Vector3.up).normalized;
				Vector3 forwardVector = MathCustom.GetFaceNormalVector(Vector3.zero, rightVector, upVector);
				outRotation = Quaternion.LookRotation(-upVector, forwardVector);
			}
			else if (view == ECameraTargetType.ZOOM)
			{
				Vector3 lpos = targetTransform.position;

				Vector3 newPosition;
				MathCustom.SphericalToCartesian(DistanceToplayer, -Mathf.PI / 2, Elevation, out newPosition);
				outPosition = targetTransform.TransformPoint(newPosition + Vector3.up * StepHeight);

				Vector3 newPos = targetTransform.TransformPoint(newPosition);
				Vector3 cross = Vector3.Cross(newPos, targetTransform.forward);
				outRotation = Quaternion.LookRotation(lpos - newPos, Vector3.Cross(cross, -(lpos - newPos)));
			}
			else
			{
				outPosition = Vector3.zero;
				outRotation = Quaternion.identity;
			}
		}

		private GameCamera GetGameCamera()
		{
            if (!typedCameras.ContainsKey(ECamera.GAME)) return null;
			GameCamera gameCam = (GameCamera)typedCameras[ECamera.GAME];
			return gameCam;
		}

        public void Reset()
        {
            GameCamera.Reset();
            ActiveCamera(startCamera);
        }

        private void InitInstance()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(this);
				throw new Exception("An instance of CameraManager already exists.");
			}
			else _instance = this;
		}
	}
}
