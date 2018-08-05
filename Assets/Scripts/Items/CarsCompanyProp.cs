using UnityEngine;

namespace Assets.Scripts.Items {
    class CarsCompanyProp : MainItemProp {
        [SerializeField]
        GameObject inactivedState;

        public void SetActivated() {
            //inactivedState.SetActive(false);
        }
    }
}
