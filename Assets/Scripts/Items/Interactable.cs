using Assets.Scripts.Game;
using UnityEngine;

public class Interactable : LinkObject {

    #region Public Variables
    public float radius = 1f;
    public Item item;
    //public GameObject go;
    #endregion

    #region Private variables

    protected int _materialsLenght;
    [SerializeField]
    protected Renderer _meshRenderer;
    protected Material[] _baseMaterials;

    protected Material _materialRed;
    protected Material _materialYellow;
    protected Material _materialGreen;

    protected bool _isSelected;

    #endregion

    protected override void Awake() {
		base.Awake();
		if (_meshRenderer == null) {
            _meshRenderer = gameObject.GetComponentInChildren<Renderer>();
            _baseMaterials = _meshRenderer.materials;
            _materialsLenght = _baseMaterials.Length;
        }
    }

	protected override void OnEnable() {
        base.OnEnable();
        Events.Instance.AddListener<OnTransition>(TransitionMode);
        Events.Instance.AddListener<OnAction>(ActionMode);
    }

    protected override void OnDisable() {
        base.OnDisable();
        Events.Instance.RemoveListener<OnTransition>(TransitionMode);
        Events.Instance.RemoveListener<OnAction>(ActionMode);
    }

    public virtual void Start() {
        radius = 0.2f;
        if (GetComponent<shaderGlow>() != null && item != null) GetComponent<shaderGlow>().labelToDisplay = item.name;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public virtual void Interact() {

    }

    public virtual void TransitionMode(OnTransition e) {
        /*Material[] intMaterials = new Material[_materialsLenght];
        for (int i = 0; i < intMaterials.Length; i++) {
            intMaterials[i] = _baseMaterials[i];
        }
        if (GetComponent<shaderGlow>() != null) GetComponent<shaderGlow>().lightOff();
        _meshRenderer.materials = intMaterials;*/       
    }

    public virtual void ActionMode(OnAction e) {

    }
}
