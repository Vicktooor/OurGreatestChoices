using UnityEngine.UI;

public class UIGameButton : Button
{
    protected override void Awake()
    {
        base.Awake();
        Events.Instance.AddListener<OnUpdateMainMenu>(HandleGameButton);
    }

    protected void HandleGameButton(OnUpdateMainMenu e)
    {

    }

    protected override void OnDestroy()
    {
        Events.Instance.RemoveListener<OnUpdateMainMenu>(HandleGameButton);
        base.OnDestroy();
    }
}
