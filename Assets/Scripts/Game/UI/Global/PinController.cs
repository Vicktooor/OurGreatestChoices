using UnityEngine;
using System.Collections;

public class PinController : MonoBehaviour
{
    protected Pin[] allPins;

    protected void Awake()
    {
        allPins = GetComponentsInChildren<Pin>(true);
        for (int i = 0; i < allPins.Length; i++) allPins[i].gameObject.SetActive(false);
        Events.Instance.AddListener<OnShowPin>(HandleShowPin);
    }

    protected void HandleShowPin(OnShowPin e)
    {
        for (int i = 0; i < allPins.Length; i++)
        {
            if (allPins[i].target == e.targetPin)
            {
                allPins[i].gameObject.SetActive(e.targetState);
                break;
            }
        }
    }

    protected void OnDestroy()
    {
        Events.Instance.RemoveListener<OnShowPin>(HandleShowPin);
    }
}
