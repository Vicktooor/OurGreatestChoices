public class ItemProp : Interactable {

    protected override void OnEnable() {
        base.OnEnable();
        //add listener ici
    }

	protected override void OnDisable() {
        base.OnDisable();
        //remove listener here
    }

    // Use this for initialization
    public override void Start() {
        base.Start();
        return;
    }

    /*protected override void CheckDistance() {
        return;
    }*/

    //A MODIFIER
    /*public override void InteractMode(OnInteraction e) {
        return;
    }*/

    /*public override void DesinteractMode(OnDesinteraction e) {
        return;
    }*/

    public override void TransitionMode(OnTransition e) {
        return;
    }

    public override void ActionMode(OnAction e) {
        return;
    }
}
