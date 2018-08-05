using UnityEngine;

public class MetroProp : ItemProp {

    [Header("STATES")]
    [SerializeField]
    GameObject MetroState;

    public void SetMetroState() {
        MetroState.SetActive(true);
    }
}
