using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EScrollList { Glossary, Inventory }

public class LayoutElementDisplayCheck : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform layoutGroup;
    [SerializeField] private Vector3 objectScale;

    public EScrollList listType;

    private List<GameObject> objects = new List<GameObject>();
    public int currentObjectIndex = 0;
    public int newObjectIndex = 0;
    private GameObject currentObject = null;

    public static int currentIndex = 0;
	void Start ()
    {
        for(int i = 0; i < layoutGroup.childCount; ++i)
        {
            objects.Add(layoutGroup.GetChild(i).gameObject);
        }

        currentObject = objects[0];
        currentObject.transform.localScale = objectScale;
    }
	
	void Update ()
    {
        newObjectIndex = (int)(scrollRect.horizontalNormalizedPosition * objects.Count);
        newObjectIndex = Mathf.Clamp(newObjectIndex, 0, objects.Count -1);
   
        if (currentObjectIndex != newObjectIndex)
        {
            currentObject.transform.localScale = Vector3.one;
            currentObjectIndex = newObjectIndex;
            currentObject = objects[currentObjectIndex];
            currentObject.transform.localScale = objectScale;

            currentIndex = Mathf.Clamp(currentObjectIndex, 0, InventoryPlayer.instance.itemsWornArray.Count-1);

            Events.Instance.Raise(new OnScrolling(currentObjectIndex, listType));
            Events.Instance.Raise(new OnSwipe());
        }

        if (scrollRect.velocity.magnitude < 30)
        {
            scrollRect.velocity = Vector3.zero;          
        }
	}
}
