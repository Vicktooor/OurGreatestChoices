using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelButton : MonoBehaviour
{
    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        Events.Instance.AddListener<OnPanelBtnState>(Enable);
    }

    private void Enable(OnPanelBtnState e)
    {
        _btn.interactable = e.interactable;
    }

    private void OnDestroy()
    {
        Events.Instance.RemoveListener<OnPanelBtnState>(Enable);
    }
}
