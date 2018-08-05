using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIQuestMapButton : MonoBehaviour, IPointerDownHandler
{
    protected Image img;
    protected InteractablePNJ pnj;

    protected void Awake()
    {
        img = GetComponent<Image>();
    }

    public void SetPNJ(InteractablePNJ pPNJ)
    {
        pnj = pPNJ;
        if (pnj.pictoHead != null) img.sprite = pnj.pictoHead;
    }

    public void Update()
    {
        if (pnj != null) transform.position = Camera.main.WorldToScreenPoint(pnj.transform.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        QuestManager.Instance.SelectTarget(pnj);
    }
}
