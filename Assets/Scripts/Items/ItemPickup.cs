using Assets.Script;
using Assets.Scripts.Game.UI;
using Assets.Scripts.Utils;
using UnityEngine;

public class ItemPickup : Interactable {

	public float rotationSpeed = 30f;
	public float floatingSpeed = 1f;
	protected float floatingDistance = 0.025f;

	[HideInInspector]
    public Vector3 sourcePosition;

   /* [SerializeField]*/
    GameObject particlesSystem;

    public override void Start() {
        base.Start();
        sourcePosition = transform.position;
        particlesSystem = gameObject.transform.GetChild(0).gameObject;
    }

    public override void TransitionMode(OnTransition e) {
        base.TransitionMode(e);
    }

    public void PickUp() {
        if (!InteractableManager.instance.canTake(gameObject)) return;
        NotePad.Instance.CleanBillboard(transform.position); 

        //Son
        Events.Instance.Raise(new OnPickUp());
        particlesSystem.transform.parent = gameObject.transform.parent;
        ParticleSystem ps = particlesSystem.GetComponent<ParticleSystem>();
        ps.Play();

        if (item.isPrimary)
        {
            UIManager.instance.MoveObjectToInventory(transform, OnInventory);
        }
        else
        {
            InventoryPlayer.instance.Add(item);
            Destroy(gameObject);
        }
    }

    private void OnInventory()
    {
        InventoryPlayer.instance.Add(item);
        if (associateCell) associateCell.DestroyProps(this);
        Destroy(gameObject);
    }
}
