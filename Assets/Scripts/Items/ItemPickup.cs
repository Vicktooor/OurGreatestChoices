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

        bool wasPickedUp = false;

        //Son
        Events.Instance.Raise(new OnPickUp());
        particlesSystem.transform.parent = gameObject.transform.parent;
        ParticleSystem ps = particlesSystem.GetComponent<ParticleSystem>();
        ps.Play();

        if (item.isPrimary) {
            //A CLEANER
            wasPickedUp = InventoryPlayer.instance.Add(item);
            //SOLUTION DE SECOURS
			if (associateCell) associateCell.PropsCollider.Remove(this);
            if (wasPickedUp) {
                InteractableManager.instance.RecreatePrimaryPickUp(gameObject, item.prefab, associateCell);
            }
         }
        else InventoryPlayer.instance.Add(item);

		//PlayerManager.instance.player.GetComponent<Player>().AssociateCell.Props.Remove(this);
		//PlayerManager.instance.player.GetComponent<Player>().AssociateCell.PropsCollider.Remove(this);
        Destroy(gameObject);
    }

	private float t = 0;
	public void Update()
	{
		if (PlanetMaker.instance.edit != EditState.INACTIVE) return;

        //if (GameManager.Instance.LoadedScene == SceneString.ZoomView) CheckDistance();

        Transform lTransform = transform;
		lTransform.Rotate(transform.up, rotationSpeed * Time.deltaTime, Space.World);

		t += floatingSpeed * Time.deltaTime;
		t = Mathf.Clamp01(t);
		lTransform.position = Vector3.Lerp(sourcePosition, sourcePosition + (lTransform.up * floatingDistance), t);
		if (t == 1 || t == 0) floatingSpeed *= -1;		
	}

    public void FullDestroy()
    {
        associateCell.DestroyProps(this);
        Destroy(gameObject);
    }
}
