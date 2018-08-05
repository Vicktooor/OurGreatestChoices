using Assets.Scripts.DecalSystem.Utils;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using UnityEngine;


namespace Assets.Scripts.DecalSystem
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class Decal : MonoBehaviour
	{
		[SerializeField]
		protected Material _material;
		public Material Material {
			get { return _material; }
			set { _material = value; }
		}

		public Sprite sprite;

		protected float _maxAngle = 360f;
		public float MaxAngle { get { return _maxAngle; } }

		protected LayerMask _affectedLayers = -1;
		public LayerMask AffectedLayers { get { return LayerMask.GetMask(new string[2] { "Cell", "Water" }); } }

		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;

		public DecalLayer layer;

		public Texture texture
		{
			get { return _material ? _material.mainTexture : null; }
		}

		protected void Start()
		{
			transform.hasChanged = false;
		}

		public void Init(Sprite iSprite, Material iMat, DecalLayer iType, string iName)
		{
			gameObject.name = iName;
			layer = iType;
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
			sprite = iSprite;
			_material = iMat;
			_material.SetTexture("_MainTex", iSprite.texture);
			meshRenderer.material = iMat;
		}

		public void Build()
		{
			if (layer == DecalLayer.GROUND) DecalBuilder.BuildAndSetDirty(this, EarthManager.Instance.CellsObj.ToArray());
			else if (layer == DecalLayer.SEA) DecalBuilder.BuildAndSetDirty(this, new GameObject[1] { GameObject.Find("Sea") });
		}

		public void Update()
		{
			if (transform.hasChanged) Build();
		}

		protected void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
