using UnityEngine;
using System;
using Assets.Scripts.Game;
using System.Collections;

namespace Assets.Scripts.Utils
{

	/// <summary>
	/// 
	/// </summary>
	public class CameraZoomEditor : CameraCC
	{
		#region Instance
		private static CameraZoomEditor _instance;

		/// <summary>
		/// instance unique de la classe     
		/// </summary>
		public static CameraZoomEditor instance
		{
			get
			{
				return _instance;
			}
		}
		#endregion

		protected bool _controlEnable = true;

		public Transform baseTransform;
		public float elevation;
		protected float Elevation { get { return Mathf.Clamp(elevation, 5f, 89f) * Mathf.Deg2Rad; } }
		public float rotationSpeed;

		protected float _actualPolar;
		protected float ActualPolar { get { return _actualPolar % (Mathf.PI * 2); } }

		public float distanceMin = 2f;
		public float distanceMax = 15f;
		protected float movingDistance = 5f;

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de CameraZoomEditor alors que c'est un singleton.");
			}
			_instance = this;
			_actualPolar = 0;
			transform.rotation = Quaternion.FromToRotation(Vector3.up, transform.position);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SelectTarget();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Reset();
		}

		protected override void OnScroll(OnEditionMouseScroll e)
		{
			if (baseTransform.position != Vector3.zero && _controlEnable)
			{
				if (Input.GetKey(KeyCode.E)) elevation += e.value * 10f;
				else
				{
					movingDistance -= e.value * movingDistance;
					movingDistance = Mathf.Clamp(movingDistance, distanceMin, distanceMax);
				}
			}
		}

		protected override void OnRightClick(OnEditionRightClick e)
		{
			if (baseTransform.position != Vector3.zero && _controlEnable)
			{			
				_actualPolar -= Mathf.Clamp(Camera.main.ScreenToViewportPoint(Input.mousePosition).x - 0.5f, -1 * rotationSpeed * Time.deltaTime, 1 * rotationSpeed * Time.deltaTime);			
			}
		}

		protected override void OnLeftClick(OnEditionLeftClick e)
		{
			if (Input.GetKey(KeyCode.S) && _controlEnable) SelectTarget();
		}

		protected override void OnInputKey(OnEdtionInputKeyEvent e)
		{
			if (e.key == KeyCode.Q && _controlEnable) CameraManager.Instance.ActiveCamera(ECamera.ORBITAL);
		}

		protected void Update()
		{
			if (_controlEnable)
			{
				Vector3 newPosition;
				MathCustom.SphericalToCartesian(movingDistance, ActualPolar, Elevation, out newPosition);
				transform.position = baseTransform.TransformPoint(newPosition);
				Vector3 cross = Vector3.Cross(baseTransform.position - transform.position, baseTransform.up);
				transform.rotation = Quaternion.LookRotation(baseTransform.position - transform.position, Vector3.Cross(cross, baseTransform.position - transform.position));
			}
		}

		protected void SelectTarget()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (_controlEnable && Physics.Raycast(ray, out hit, movingDistance * 50f, LayerMask.GetMask(new string[2] { "Cell", "Nature" })))
			{
				Vector3 basePosition = transform.position;
				Quaternion baseRotation = transform.rotation;
				if (hit.collider.gameObject.GetComponent<Cell>()) baseTransform.position = hit.collider.gameObject.GetComponent<Cell>().GetCenterPosition();
				else baseTransform.position = hit.collider.gameObject.transform.position;
				baseTransform.rotation = Quaternion.FromToRotation(Vector3.up, baseTransform.position);
				StartCoroutine(MoveToTargetCell(basePosition, baseRotation));
			}
		}

		protected IEnumerator MoveToTargetCell(Vector3 basePosition, Quaternion baseRotation)
		{
			_controlEnable = false;
			float t = 0;		

			Vector3 targetPos;
			Quaternion targetRot;
			MathCustom.SphericalToCartesian(movingDistance, ActualPolar, Elevation, out targetPos);
			targetPos = baseTransform.TransformPoint(targetPos);
			Vector3 cross = Vector3.Cross(baseTransform.position - targetPos, baseTransform.up);
			targetRot = Quaternion.LookRotation(baseTransform.position - targetPos, Vector3.Cross(cross, baseTransform.position - targetPos));

			while (t < 1)
			{
				if (t > 1) t = 1;
				else t += Time.deltaTime * 1.25f;
				transform.position = Vector3.Slerp(basePosition, targetPos, t);
				transform.rotation = Quaternion.Slerp(baseRotation, targetRot, t);
				yield return null;
			}
			_controlEnable = true;
			StopAllCoroutines();
		}

		public override void Reset()
		{
			base.Reset();
			baseTransform.position = Vector3.zero;
			baseTransform.rotation = Quaternion.identity;
			movingDistance = 5f;
		}

		protected void OnDestroy()
		{
			_instance = null;
		}
	}
}