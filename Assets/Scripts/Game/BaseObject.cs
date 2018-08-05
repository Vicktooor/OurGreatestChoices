using Assets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;

public interface ICullingObject
{
	void Cull(bool state);
}

namespace Assets.Scripts.Game
{
	public class BaseObject : MonoBehaviour, ICullingObject
	{
        public Vector3 Position { get { return transform.position; } }

        protected bool _visible = true;
        public bool Visible { get { return _visible; } }

		[SerializeField]
		protected ECameraTargetType displayView;

		protected Renderer _pRenderer;
		protected Renderer[] _pRenderers;
		public Renderer[] PersonnalRenderers {
			get {
				if (_pRenderers.Length > 1) return _pRenderers;
				else return new Renderer[1] { _pRenderer };
			}
		}
		public Renderer ParentRenderer
		{
			get {
				if (_pRenderers.Length > 1) return null;
				else return _pRenderer;
			}
		}

        protected Collider[] _personnalCollider = new Collider[0];
		public Collider[] SelfCollider { get { return _personnalCollider; } }

		protected virtual void Awake()
		{
			FindRenderers();
			Culling<BaseObject>.Instance.Add(this);

			_personnalCollider = GetCollider();

			Events.Instance.AddListener<OnZoomFinish>(Display);
			Events.Instance.AddListener<OnNewMonth>(OnNewMonthPassed);
			Events.Instance.AddListener<OnNewYear>(OnNewYearPassed);
		}

		protected void FindRenderers()
		{
			_pRenderer = GetComponent<Renderer>();
			
			_pRenderers = GetComponentsInChildren<Renderer>(true);
			List<Renderer> lRenderers = new List<Renderer>(_pRenderers);
			if (lRenderers.Contains(_pRenderer)) lRenderers.Remove(_pRenderer);
		}

		public virtual Collider[] GetCollider()
		{
			if (_personnalCollider.Length == 0)
			{
                _personnalCollider = GetComponentsInChildren<Collider>(true);
				return _personnalCollider;
			}
			else return _personnalCollider;
		}	

		protected virtual void OnNewMonthPassed(OnNewMonth e) { }

		protected virtual void OnNewYearPassed(OnNewYear e) { }

		public virtual void Display(OnZoomFinish e)
		{
			if (displayView == ECameraTargetType.NONE) return;
			if (displayView == ECameraTargetType.BOTH)
			{
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(true);
					return;
				}
				return;
			}
			if (e.view == displayView) gameObject.SetActive(true);
			else gameObject.SetActive(false);	
		}
		
		public void UpdateRenderer()
		{
			_pRenderer = GetComponent<Renderer>();
		}		

		// Interface methods
		public void Cull(bool displayState)
		{
            if (displayState != _visible)
            {
                _visible = displayState;
                if (_pRenderer) _pRenderer.enabled = displayState;

                int subCount = _pRenderers.Length;
                for (int i = 0; i < subCount; i++) _pRenderers[i].enabled = displayState;
            }          		
		}
        // -----------------

        protected virtual void OnDestroy()
		{
            Culling<BaseObject>.Instance.Remove(this);
			Events.Instance.RemoveListener<OnNewMonth>(OnNewMonthPassed);
			Events.Instance.RemoveListener<OnNewYear>(OnNewYearPassed);
			Events.Instance.RemoveListener<OnZoomFinish>(Display);
		}
	}
}