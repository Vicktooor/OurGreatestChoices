using Assets.Scripts.Items;
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_WaterDiversion : InteractablePNJ
{
    protected override void Awake()
    {
        base.Awake();
        Events.Instance.AddListener<OnCleanVehicles>(HandleCleanVehicle);
    }

    protected void HandleCleanVehicle(OnCleanVehicles e)
    {
        if (budgetComponent.name == "Oil Plant Land" && e.companyName == "Car Company Forest")
        {
            ResourcesManager.instance.RemoveBudgetComponent(budgetComponent);
        }
        else if (budgetComponent.name == "Oil Plant Desert" && e.companyName == "Car Company Desert")
        {
            ResourcesManager.instance.RemoveBudgetComponent(budgetComponent);
        }
        else if (budgetComponent.name == "Oil Plant Mountain" && e.companyName == "Car Company Mountain")
        {
            ResourcesManager.instance.RemoveBudgetComponent(budgetComponent);
        }
    }

    protected override void OnDestroy()
    {
        Events.Instance.RemoveListener<OnCleanVehicles>(HandleCleanVehicle);
        base.OnDestroy();
    }
}
