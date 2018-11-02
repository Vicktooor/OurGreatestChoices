public class InteractablePNJ_OilPlant : InteractablePNJ
{
    protected override void Awake()
    {
        base.Awake();
        Events.Instance.AddListener<OnCleanVehicles>(CleanZone);
    }

    protected void CleanZone(OnCleanVehicles e)
    {
        if (e.companyName == "CarsCompanyForest" && IDname == "OilPlanForest")
        {
            budgetComponent.SetNewImpact(EWorldImpactType.None);
        }
        else if (e.companyName == "CarsCompanyDesert" && IDname == "OilPlanDesert")
        {
            budgetComponent.SetNewImpact(EWorldImpactType.None);
        }
        else if (e.companyName == "CarsCompanyMountain" && IDname == "OilPlanMountain")
        {
            budgetComponent.SetNewImpact(EWorldImpactType.None);
        }
        Events.Instance.RemoveListener<OnCleanVehicles>(CleanZone);
    }
}
