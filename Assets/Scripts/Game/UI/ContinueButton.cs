using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContinueButton : MonoBehaviour
{
    public void OnEnable()
    {
        Events.Instance.AddListener<PartyLoaded>(HandleLoadedParty);   
    }

    public void HandleLoadedParty(PartyLoaded e)
    {
        gameObject.SetActive(e.loaded);
        GetComponent<Button>().interactable = e.loaded;
    }

    public void OnDisable()
    {
        Events.Instance.RemoveListener<PartyLoaded>(HandleLoadedParty);
    }
}
