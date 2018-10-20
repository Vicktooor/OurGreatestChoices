using UnityEngine;
using DG.Tweening;
using System;

public class Dotween_TranslatBottom : MonoBehaviour
{
    public Vector3 move = new Vector3(0, 0, 0);
    public Vector3 punch = new Vector3(0, 0, 0);
    public float time1 = 1f;
    public float time2 = 1f;
    public int vibrato = 1;
    public float elasticity = 1f;
    /*
    public float debut  = 1f;
    public float fin  = 1f;
    Vector3 zero = new Vector3 (0,0,0);
    */
    public bool snapping = true;

    public GameObject bagButton;

    public Action callback;

    void OnEnable()
    {
        ControllerInput.OpenScreens.Add(transform);
        transform.DOLocalMove(move, time1, snapping).OnComplete(etape1);
    }

    public void closeInventaire()
    {
        /*transform.DOLocalMove(new Vector3(10.74f, -578f, 0), time2, snapping).OnComplete(
			()=>
			{

				transform.DOPunchPosition(punch, time1, vibrato, elasticity, snapping);
				Debug.Log("etape1");


			});*/

        transform.DOLocalMove(new Vector3(10.74f, -578f, 0), time1, snapping).OnComplete(disable);

    }

    void etape1()
    {
        if (GetComponent<InventoryScreen>() != null) callback = InventoryScreen.Instance.ActiveDrag;
        transform.DOPunchPosition(punch, time2, vibrato, elasticity, snapping).OnComplete(ActiveCallback);
    }

    void ActiveCallback()
    {
        if (callback != null) callback();
    }

    void disable()
    {
        callback = null;
        ControllerInput.OpenScreens.Remove(transform);
        gameObject.SetActive(false);
        bagButton.SetActive(true);
    }

    void OnDisable()
    {
        if (ControllerInput.OpenScreens.Contains(transform)) ControllerInput.OpenScreens.Remove(transform);
        //transform.localScale = zero;
    }

    public void closeNPC()
    {
        transform.DOLocalMove(new Vector3(10.74f, -1154f, 0), time1, snapping).OnComplete(disable);
    }
}
