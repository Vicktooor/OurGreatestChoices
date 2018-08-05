using UnityEngine;

namespace Assets.Scripts.Game.UI
{
	[ExecuteInEditMode]
	public class LayerRenderer : MonoBehaviour
	{
		protected string targetLayer = "UIText";
		protected Renderer _renderer;

		protected void Start()
		{
			_renderer = GetComponent<Renderer>();
			_renderer.sortingLayerName = targetLayer;
		}
	}
}
