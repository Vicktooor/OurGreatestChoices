using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{

	/// <summary>
	/// 
	/// </summary>
	public class BudgetPanelDrag : MonoBehaviour
	{
        public Transform panelCenter;
		public UIDraggableBudget draggableModel;
		public float budgetDistance;

		[SerializeField]
		private BudgetComponent _mainBudget;
		private List<UIDraggableBudget> _draggables = new List<UIDraggableBudget>();

		public Transform dragImageTransform;		
		private UIDraggableBudget _targetDraggable;
		private List<RaycastResult> _hitObjects = new List<RaycastResult>();
		private bool _dragging;

        private int _nbDraggable;
        private int _nbBudgetMax;

		protected void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				//_targetDraggable = GetDraggableTransformUnderMouse();
				if (_targetDraggable)
				{
					_dragging = true;
					dragImageTransform.gameObject.SetActive(true);
					dragImageTransform.SetAsLastSibling();
					_targetDraggable.GetComponent<Image>().raycastTarget = false;
				}
			}

			if (_dragging)
			{
				dragImageTransform.position = Input.mousePosition;

				if (Input.GetMouseButtonUp(0))
				{
					_dragging = false;
					//UIDraggableBudget UItargetBudget = GetDraggableTransformUnderMouse();
					/*if (UItargetBudget)
					{
						dragImageTransform.gameObject.SetActive(false);
                        FBX_Money.instance.Play(dragImageTransform.position);
						_targetDraggable.GetComponent<Image>().raycastTarget = true;

                        if (_targetDraggable.budgetComponent.name != UItargetBudget.budgetComponent.name)
                        {
                            if (_targetDraggable.budgetComponent.budget >= WorldValues.TRANSFERT_VALUE)
                            {
                                FBX_Money.instance.Play(dragImageTransform.position);
                                _targetDraggable.budgetComponent.SendBudget();
                                UItargetBudget.budgetComponent.ReceiveBudget();
                                UpdateDraggablesMoney();
                            }                            
                        }
					}
					else
					{
						dragImageTransform.gameObject.SetActive(false);
                        _targetDraggable.GetComponent<Image>().raycastTarget = true;					
					}*/
				}
			}
		}

		public void ConstructDiagram(BudgetComponent comp)
		{
            _mainBudget = comp;
            _nbBudgetMax = _mainBudget.budgetLinks.Length + 1;

            AddDraggable(_mainBudget.name);

            foreach (string bc in _mainBudget.budgetLinks) AddDraggable(bc);
		}

        private void AddDraggable(string budgetName)
        {
            foreach (InteractablePNJ fpnj in InteractablePNJ.PNJs)
            {
                if (fpnj.budgetComponent.name == budgetName)
                {
                    ContructDraggable(fpnj.budgetComponent, fpnj);
                    return;
                }
            }
        }

        private void ContructDraggable(BudgetComponent comp, InteractablePNJ npc)
        {
            Vector2 center = panelCenter.position;

            float x = Mathf.Cos((((360f / 4f) * _nbDraggable)) * Mathf.Deg2Rad);
            float y = Mathf.Sin((((360f / 4f) * _nbDraggable)) * Mathf.Deg2Rad);
            Vector2 targetPosition = new Vector2(x, y).normalized * (Screen.width / 6f);

            UIDraggableBudget lDraggable = Instantiate(draggableModel, Vector3.zero, Quaternion.identity, gameObject.transform);
            lDraggable.transform.position = center + targetPosition;
            lDraggable.budgetComponent = comp;

            if (lDraggable.img) lDraggable.img.sprite = npc.pictoHead;
            else
            {
                lDraggable.img = lDraggable.GetComponent<Image>();
                if (lDraggable.img) lDraggable.img.sprite = npc.pictoHead;
            }

            _draggables.Add(lDraggable);
            lDraggable.UpdateMoney();
            _nbDraggable++;
        }

        public void UpdateDraggablesMoney()
        {
            foreach (UIDraggableBudget bObj in _draggables)
            {
                bObj.UpdateMoney();
            }
        }

		public void Clear()
		{
            _nbBudgetMax = 0;
            _nbDraggable = 0;
			foreach (UIDraggableBudget d in _draggables)
			{
				Destroy(d.gameObject);
			}
			_draggables.Clear();
		}

		protected GameObject GetObjectUnderMouse()
		{
			Touch touch;
			if (Input.touchCount > 0) touch = Input.GetTouch(0);
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			pointer.position = Input.mousePosition;
			EventSystem.current.RaycastAll(pointer, _hitObjects);
			if (_hitObjects.Count <= 0) return null;
			else return _hitObjects[0].gameObject;
		}

		public T GetDraggableTransformUnderMouse<T>() where T : Component
		{
			GameObject go = GetObjectUnderMouse();
			if (!go) return null;
			T draggable = GetObjectUnderMouse().GetComponent<T>();
			if (draggable) return draggable;
			return null;
		}

        public void OnDestroy()
        {
            Clear();
        }
    }
}