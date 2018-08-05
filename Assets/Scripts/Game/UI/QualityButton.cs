using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.Utils;

public class QualityButton : MonoBehaviour
{
    public EQuality qualityTarget;
    private Button btn;

    [SerializeField]
    private RawImage actifIcon;

    public void Awake()
    {
        actifIcon.gameObject.SetActive(false);
        btn = GetComponent<Button>();
        if (btn == null) return;
        if (CameraManager.QUALITY == qualityTarget)
        {
            btn.interactable = false;
            actifIcon.gameObject.SetActive(true);
        }
        Events.Instance.AddListener<OnChangeQuality>(HandleChangeQuality);
    }

    private void HandleChangeQuality(OnChangeQuality e)
    {
        if (btn == null) return;
        if (CameraManager.QUALITY == qualityTarget)
        {
            btn.interactable = false;
            actifIcon.gameObject.SetActive(true);
        }
        else
        {
            btn.interactable = true;
            actifIcon.gameObject.SetActive(false);
        }
    }

    public void OnDestroy()
    {
        Events.Instance.RemoveListener<OnChangeQuality>(HandleChangeQuality);
    }

    public void SetQuality()
    {
        if (CameraManager.Instance) CameraManager.Instance.SetQuality(qualityTarget);
    }
}
