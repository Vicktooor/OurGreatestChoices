using Assets.Script;
using Assets.Scripts.Game;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using UnityEngine;

public class ItemPickup : Interactable {

	public float rotationSpeed = 30f;
	public float floatingSpeed = 1f;
	protected float floatingDistance = 0.025f;

	[HideInInspector]
    public Vector3 sourcePosition;

   /* [SerializeField]*/
    GameObject particlesSystem;

    public override void Init()
    {
        sourcePosition = transform.position;
        particlesSystem = gameObject.transform.GetChild(0).gameObject;
    }

    public override void TransitionMode(OnTransition e) {
        base.TransitionMode(e);
    }

    public void PickUp() {
        if (!FtueManager.instance.active) NotePad.Instance.CleanBillboard(transform.position); 
        Events.Instance.Raise(new OnPickUp());
        if (item.isPrimary)
        {
            particlesSystem.transform.parent = gameObject.transform.parent;
            ParticleSystem ps = particlesSystem.GetComponent<ParticleSystem>();
            ps.Play();
            UIManager.instance.MoveObjectToInventory(transform, OnInventory);
        }
        else
        {
            InventoryPlayer.Instance.Add(item);
            Destroy(gameObject);
        }
    }

    private void OnInventory()
    {
        InventoryPlayer.Instance.Add(item);
        if (associateCell) associateCell.RemoveProps(this);
        Destroy(gameObject, 0.2f);
    }
}
