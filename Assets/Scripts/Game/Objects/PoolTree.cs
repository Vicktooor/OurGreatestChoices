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
        public static List<Cell> ForestCells = new List<Cell>();

        public bool cutModel;
        public GameObject opositeAsset;

        public bool _modified = false;

        public bool IsCut
        {
            get
            {
                if (cutModel)
                {
                    if (_modified) return false;
                    else return true;
                }
                else {
                    if (_modified) return true;
                    else return false;
                }
            }
        }

        override public void Init()
        {
            if (opositeAsset != null)
            {
                if (cutModel) DeforestationArea.NB_FOREST_CUT++;
                ForestCells.Add(associateCell);
                opositeAsset.transform.parent = associateCell.transform;
            }
        }

        public void SetDeforestation(bool deforest)
        {
            if (opositeAsset == null) return;
            if (deforest && IsCut) return;
            if (!deforest && !IsCut) return;

            if (cutModel)
            {
                if (!deforest && IsCut)
                {
                    DeforestationArea.NB_FOREST_CUT--;
                    _modified = true;
                    opositeAsset.SetActive(true);
                    gameObject.SetActive(false);
                }
                else if (!IsCut)
                {
                    DeforestationArea.NB_FOREST_CUT++;
                    _modified = false;
                    opositeAsset.SetActive(false);
                    gameObject.SetActive(true);
                }
            }
            else
            {
                if (deforest && !IsCut)
                {
                    _modified = true;
                    DeforestationArea.NB_FOREST_CUT++;
                    opositeAsset.SetActive(true);
                    gameObject.SetActive(false);
                }
                else if (IsCut)
                {
                    DeforestationArea.NB_FOREST_CUT--;
                    _modified = false;
                    opositeAsset.SetActive(false);
                    gameObject.SetActive(true);
                }
            }
        }

        public override void Display(OnSwitchScene e)
        {
        }
    }
}