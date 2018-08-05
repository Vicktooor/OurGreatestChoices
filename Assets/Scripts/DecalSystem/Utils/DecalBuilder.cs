using Assets.Scripts.Utils;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.DecalSystem.Utils
{
	public static class DecalBuilder
	{
		private static readonly DecalMeshBuilder builder = new DecalMeshBuilder();

		public static GameObject[] BuildAndSetDirty(Decal decal, GameObject[] targets)
		{
			var affectedObjects = Build(builder, decal, targets);
			return affectedObjects;
		}

		private static GameObject[] Build(DecalMeshBuilder builder, Decal decal, GameObject[] targets)
		{
			MeshFilter filter = decal.meshFilter;
			MeshRenderer renderer = decal.meshRenderer;

			if (decal.Material == null || decal.sprite == null)
			{
				filter.mesh.Clear();
				renderer.material = null;
				return null;
			}

			var objects = DecalUtils.GetAffectedMeshFilter(decal, targets);
			foreach (var obj in objects) Build(builder, decal, obj);
			builder.Push(PlanetMaker.instance.PushDistance);

			if (filter.mesh == null)
			{
				filter.mesh = new Mesh();
				filter.mesh.name = "Decal";
			}

			builder.ToMesh(filter.sharedMesh);
			renderer.material = decal.Material;

			return objects.Select(i => i.gameObject).ToArray();
		}


		private static void Build(DecalMeshBuilder builder, Decal decal, MeshFilter pobject)
		{
			Matrix4x4 objToDecalMatrix = decal.transform.worldToLocalMatrix * pobject.transform.localToWorldMatrix;

			Mesh mesh = pobject.sharedMesh;
			Vector3[] vertices = mesh.vertices;
			int[] triangles = mesh.triangles;

			for (int i = 0; i < triangles.Length; i += 3)
			{
				int i1 = triangles[i];
				int i2 = triangles[i + 1];
				int i3 = triangles[i + 2];

				Vector3 v1 = objToDecalMatrix.MultiplyPoint(vertices[i1]);
				Vector3 v2 = objToDecalMatrix.MultiplyPoint(vertices[i2]);
				Vector3 v3 = objToDecalMatrix.MultiplyPoint(vertices[i3]);

				AddTriangle(builder, decal, v1, v2, v3);
			}
		}


		private static void AddTriangle(DecalMeshBuilder builder, Decal decal, Vector3 v1, Vector3 v2, Vector3 v3)
		{
			Rect uvRect = To01(decal.sprite.textureRect, decal.sprite.texture);
			Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

			if (Vector3.Angle(Vector3.forward, -normal) <= decal.MaxAngle)
			{
				var poly = PolygonClippingUtils.Clip(v1, v2, v3);
				if (poly.Length > 0)
				{
					builder.AddPolygon(poly, normal, uvRect);
				}
			}
		}

		private static Rect To01(Rect rect, Texture2D texture)
		{
			rect.x /= texture.width;
			rect.y /= texture.height;
			rect.width /= texture.width;
			rect.height /= texture.height;
			return rect;
		}

	}
}

