﻿using UnityEngine;

namespace Assets.Scripts.Items
{
	class HouseProp : ItemProp
	{
        [Header("STATES")]
        [SerializeField]
        GameObject NoTreeState;

        [SerializeField]
        GameObject TrashState;

        [SerializeField]
        GameObject DeadTreesState;

        [SerializeField]
        GameObject TreeState;

        [SerializeField]
        GameObject RoofGardenState;


        public bool isClean = true;

        //Variables to check the current state
        private EnumClass.Trees treeState = EnumClass.Trees.NO;
        private EnumClass.Clean cleanState = EnumClass.Clean.YES;
        private EnumClass.RoofGarden roofGardenState = EnumClass.RoofGarden.NO;

        public void SetRoofGardenState() {
            if (RoofGardenState) {
                RoofGardenState.SetActive(true);
                roofGardenState = EnumClass.RoofGarden.YES;

                if (treeState == EnumClass.Trees.Dead) {
                    DeadTreesState.SetActive(false);
                    TreeState.SetActive(true);

                    treeState = EnumClass.Trees.YES;
                }
            }
        }

        public void SetTrees() {
            if (TreeState) {
                if (treeState == EnumClass.Trees.YES) return;

                TreeState.SetActive(true);
                if (NoTreeState) NoTreeState.SetActive(false);
                if (DeadTreesState) DeadTreesState.SetActive(false);

                treeState = EnumClass.Trees.YES;
            }
        }

        public void SetTrash(bool pIsClean) {
            isClean = pIsClean;

            if (TrashState == null) return;

            if (pIsClean) {
                if (cleanState == EnumClass.Clean.YES) return;

                cleanState = EnumClass.Clean.YES;
                TrashState.SetActive(false);

                if (TreeState == null || DeadTreesState == null) return;

                if (treeState == EnumClass.Trees.Dead) {
                    treeState = EnumClass.Trees.YES;
                    DeadTreesState.SetActive(false);
                    TreeState.SetActive(true);
                }
            }
            else {
                if (cleanState == EnumClass.Clean.NO) return;

                cleanState = EnumClass.Clean.NO;
                TrashState.SetActive(true);

                if (TreeState == null || DeadTreesState == null) return;

                if (treeState == EnumClass.Trees.YES) {
                    treeState = EnumClass.Trees.Dead;
                    DeadTreesState.SetActive(true);
                    TreeState.SetActive(false);
                }
            }
        }
    }
}
