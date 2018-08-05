using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.Objects
{
	/// <summary>
	/// 
	/// </summary>
	public class PoolTree : Props
	{
        public static ObjectArray<Cell> ForestCells = new ObjectArray<Cell>();

        public bool cutModel;
		public GameObject opositeAsset;

        private bool _modified = false;
        public bool Modified { get { return _modified; } }

        public static void IncrementDeforestation(bool oldCutState, bool newCutState)
        {
            if (oldCutState != newCutState)
            {
                DeforestationArea.NB_FOREST_CUT += newCutState ? 1 : -1;
            }
        }

        public bool IsCut {
            get {
                if (cutModel && !_modified) return true;
                else if (!cutModel && _modified) return true;
                else return false;
            }
        }

        protected void Start()
        {
            if (opositeAsset != null)
            {
                if (cutModel) IncrementDeforestation(false, true);
                ForestCells.Add(associateCell);
                opositeAsset.transform.parent = associateCell.transform;
            }
        }

        public void SetDeforestation(bool deforest)
		{
			if (opositeAsset == null) return;
            if (deforest && IsCut) return;
            if (!deforest && !IsCut) return;

            bool oldIsCut = IsCut;
			if (cutModel)
			{
				if (!deforest && !_modified)
				{
                    opositeAsset.SetActive(true);
					gameObject.SetActive(false);
				}
				else if (_modified)
				{
                    opositeAsset.SetActive(false);
					gameObject.SetActive(true);
				}
			}
			else
			{
				if (deforest && !_modified)
				{
                    opositeAsset.SetActive(true);
					gameObject.SetActive(false);
				}
				else if (_modified)
				{
                    opositeAsset.SetActive(false);
					gameObject.SetActive(true);
				}
			}
            IncrementDeforestation(oldIsCut, IsCut);
		}

        public override void Display(OnZoomFinish e)
        {
        }
    }
}