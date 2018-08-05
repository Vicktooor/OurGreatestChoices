using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_CERN : InteractablePNJ {
    private Material _normalTexture;

    public override void Start() {
        base.Start();
        _normalTexture = GetComponent<Renderer>().material;
    }
    public override void OnReceiveBudget(OnReceiveBudget e) {
        if (budgetComponent.budget < budgetComponent.targetBudget) {
            GetComponent<Renderer>().material = _materialRed;
        }
        if (budgetComponent.budget >= budgetComponent.targetBudget) {
            GetComponent<Renderer>().material = _normalTexture;
        }
    }
}
