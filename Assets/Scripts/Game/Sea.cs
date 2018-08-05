using UnityEngine;

// AUTHOR - Victor
namespace Assets.Scripts.Game
{
    class Sea : MonoBehaviour
    {
        protected Vector2 seaOffSet;
        protected MeshRenderer seaRenderer;

        protected void Start()
        {
            seaRenderer = GetComponent<MeshRenderer>();
            seaOffSet = GetComponent<MeshRenderer>().material.mainTextureOffset;
        }

        protected void Update()
        {
            seaOffSet = seaOffSet + new Vector2(0.007f, 0.007f) * Time.deltaTime;
            seaRenderer.material.mainTextureOffset = seaOffSet;
        }
    }
}
