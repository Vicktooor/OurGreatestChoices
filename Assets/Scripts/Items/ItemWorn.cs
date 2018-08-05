using UnityEngine;

public class ItemWorn : Interactable {

    public Vector3 sourcePosition;

    public override void Start() {
        base.Start();
        radius = 0.4f;
        _materialRed = new Material(Shader.Find("Specular"));
        _materialRed.color = Color.red;
        _materialYellow = new Material(Shader.Find("Specular"));
        _materialYellow.color = Color.yellow;
        _materialGreen = new Material(Shader.Find("Specular"));
        _materialGreen.color = Color.green;
    }

    /*public override void InteractMode(OnInteraction e) {
        base.InteractMode(e);
        if (gameObject == PlayerManager.instance.player.GetComponent<InventoryPlayer>().assetWorn) {

            Material[] intMaterials = new Material[_materialsLenght];

            for (int i = 0; i < intMaterials.Length; i++) {
                intMaterials[i] = _materialGreen;
            }
            _meshRenderer.materials = intMaterials;
        }
    }*/

    /*public override void DesinteractMode(OnDesinteraction e) {
        base.DesinteractMode(e);
        //_renderer.material.color = _colorDefault;
    }*/

    public override void TransitionMode(OnTransition e) {
        base.TransitionMode(e);
        //_renderer.material.color = _colorDefault;
    }

    /*public override void ActionMode(OnAction e) {
        base.ActionMode(e);
        if (HasInteractionBetweenObjects()) {
            if (InteractableManager.instance.canTake(gameObject)) {				
				//Fusion();
            }
        }
    }*/

    /*void Fusion() {

        Events.Instance.Raise(new OnFusion());

        PlayerManager.instance.player.GetComponent<InventoryPlayer>().RemoveObjectWorn();
        PlayerManager.instance.RemoveObjectWorn(gameObject.transform.parent.transform.parent.gameObject);
        Destroy(gameObject);    
        PlayerManager.instance.player.GetComponent<InventoryPlayer>().Add((ItemPickUp)item.itemsFusion[0]);
    }*/
}
