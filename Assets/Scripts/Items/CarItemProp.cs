using UnityEngine;

namespace Assets.Scripts.Items
{
	class CarItemProp : ItemProp
	{
        [SerializeField]
        MeshFilter electricCarAsset;

        [SerializeField]
        MeshFilter greenElectricCarAsset;

        [SerializeField]
        MeshFilter pollutedCarAsset;

        public void SetElectricityMode() {
            Mesh mesh = electricCarAsset.sharedMesh;
            Mesh mesh2 = Instantiate(mesh);
            GetComponent<MeshFilter>().sharedMesh = mesh2;
        }

        public void SetGreenElectricityMode() {
            Mesh mesh = greenElectricCarAsset.sharedMesh;
            Mesh mesh2 = Instantiate(mesh);
            GetComponent<MeshFilter>().sharedMesh = mesh2;
        }

    }
}
