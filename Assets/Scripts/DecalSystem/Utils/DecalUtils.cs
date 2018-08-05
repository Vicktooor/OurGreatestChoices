using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.DecalSystem.Utils
{
	public static class DecalUtils
	{
		public static MeshFilter[] GetAffectedMeshFilter(Decal decal, GameObject[] targets)
		{
			List<MeshRenderer> toDecalMesh = new List<MeshRenderer>();
			Bounds bounds = GetBounds(decal.transform);

			foreach (GameObject go in targets)
			{
				MeshRenderer goMeshRender = go.GetComponent<MeshRenderer>();
				toDecalMesh.Add(goMeshRender);
			}

			return toDecalMesh.ToArray()
				.Where(obj => HasLayer(LayerMask.GetMask(new string[2] { "Cell", "Water" }), obj.gameObject.layer))
				.Where(obj => obj.GetComponent<Decal>() == null)
				.Where(obj => bounds.Intersects(obj.bounds))
				.Select(obj => obj.GetComponent<MeshFilter>())
				.Where(obj => obj != null && obj.sharedMesh != null)
				.ToArray();
		}

		private static bool HasLayer(LayerMask mask, int layer)
		{
			return (mask.value & 1 << layer) != 0;
		}

		private static Bounds GetBounds(Transform transform)
		{
			Vector3 size = transform.lossyScale;
			Vector3 min = -size / 2f;
			Vector3 max = size / 2f;

			Vector3[] vts = new Vector3[] {
			new Vector3(min.x, min.y, min.z),
			new Vector3(max.x, min.y, min.z),
			new Vector3(min.x, max.y, min.z),
			new Vector3(max.x, max.y, min.z),

			new Vector3(min.x, min.y, max.z),
			new Vector3(max.x, min.y, max.z),
			new Vector3(min.x, max.y, max.z),
			new Vector3(max.x, max.y, max.z),
		};

			vts = vts.Select(transform.TransformDirection).ToArray();
			min = vts.Aggregate(Vector3.Min);
			max = vts.Aggregate(Vector3.Max);

			return new Bounds(transform.position, max - min);
		}

	}
}
