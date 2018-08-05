using Assets.Script;

public class PlayerInteractable : Interactable {

    public override void Start() {
        base.Start();
        radius = 0.2f;
    }

    /*protected override void CheckDistance() {
        if (PlayerManager.instance.player == null) return;
        if (PlayerManager.instance.player == gameObject) return;

        GameObject lPlayer = PlayerManager.instance.player;

        float distance = Vector3.Distance(lPlayer.transform.position, transform.position);
        if (distance <= radius) {
            if (InteractableManager.instance.isTransition) {
                if (InteractableManager.instance.canTake(gameObject)) {
                    Events.Instance.Raise(new OnClickInteractable(InteractableManager.instance.ACTION_TYPE));
                }
            }
        }
    }*/

    /*public override void InteractMode(OnInteraction e) {
        if (gameObject == PlayerManager.instance.player) return;
        base.InteractMode(e);
        //if (gameObject == InventoryPlayer.instance.assetWorn) _renderer.material.color = Color.green;
        //Player Manager Current Player
    }*/

    /*public override void DesinteractMode(OnDesinteraction e) {
        if (gameObject == PlayerManager.instance.player) return;
        base.DesinteractMode(e);
        //_renderer.material.color = _colorDefault;
    }*/

    public override void TransitionMode(OnTransition e) {
        if (gameObject == PlayerManager.instance.player) return;
        base.TransitionMode(e);
        //_renderer.material.color = _colorDefault;
    }

    /*public override void ActionMode(OnAction e) {
        if (gameObject == PlayerManager.instance.player) return;
        base.ActionMode(e);
        if (HasInteractionBetweenObjects()) {
            if (InteractableManager.instance.canTake(gameObject)) {
                //Exchange();
            }
        }
    }*/

    /*void Exchange() {
        int key = GetKey();

        Events.Instance.Raise(new OnTransform());

        PlayerManager.instance.player.GetComponent<InventoryPlayer>().RemoveObjectWorn();
        PlayerManager.instance.player.GetComponent<InventoryPlayer>().Add((ItemPickUp)item.itemsGiven[key]);		
	}*/

    /*int GetKey() {
        for (int i = 0; i < item.itemsLinked.Count; i++) {
            if (item.itemsLinked[i] == PlayerManager.instance.player.GetComponent<InventoryPlayer>().itemWorn) return i;
        }

        return 0;
    }*/

    //A SUPPRIMER
    /*public override bool HasInteractionBetweenObjects() {
        if (PlayerManager.instance.player == null || gameObject == null || item == null || item.itemsLinked == null) return false;
        //if (PlayerManager.instance.player.GetComponent<InventoryPlayer>().assetWorn == null) return false;
        //if (gameObject == PlayerManager.instance.player.GetComponent<InventoryPlayer>().assetWorn) return false;

        List<Item> itemsLinked = item.itemsLinked;
        Item itemCarried = PlayerManager.instance.player.GetComponent<InventoryPlayer>().itemWorn;

        for (int i = 0; i < itemsLinked.Count; i++) {
            if (itemsLinked[i] == itemCarried) return true;
        }

        return false;
    }*/
}
