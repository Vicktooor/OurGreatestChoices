using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

public class RoadVertices
{
	public List<Triangle> triangles;
	public List<Vector3> vertex;
	public List<Vector3> normals;
	public List<Vector2> UV;

	public RoadVertices()
	{
		triangles = new List<Triangle>();
		vertex = new List<Vector3>();
		normals = new List<Vector3>();
		UV = new List<Vector2>();
	}

	public void AddTriangle(Vector3[] pVertex)
	{
		int[] newTriangle = new int[pVertex.Length];
		for (int i = 0; i < pVertex.Length; i++)
		{
			if (vertex.Contains(pVertex[i]))
			{
				newTriangle[i] = vertex.IndexOf(pVertex[i]);
			}
			else
			{
				vertex.Add(pVertex[i]);
				normals.Add(Vector3.zero);
				UV.Add(Vector2.zero);
				newTriangle[i] = vertex.IndexOf(pVertex[i]);
			}
		}
		triangles.Add(new Triangle( new int[3] { newTriangle[0], newTriangle[1], newTriangle[2] }));
	}
}

public struct RoadEdge
{
	public bool haveSwap;
	public Vector3[] points;
	public Vector3[] meshPoints;
	public Vector3 CenterPos { get { return (points[0] + points[1]) / 2f; } }

	public RoadEdge(Vector3 p1, Vector3 p2)
	{
		haveSwap = false;
		points = new Vector3[2] { p1 , p2};
		meshPoints = new Vector3[2];
	}
}

public enum EEditGround { Start, SelectEdge, Width, Path, CorrectUV }

namespace Assets.Scripts.Planet
{
	/// <summary>
	/// 
	/// </summary>
	public class RoadConstructor : MonoBehaviour
	{
		public bool isActive;

		public EEditGround edit;

		protected RoadEdge selectedEdge;
		protected Dictionary<RoadEdge, GameObject> choiceEdge = new Dictionary<RoadEdge, GameObject>();
		protected List<RoadEdge> edges = new List<RoadEdge>();

		[SerializeField]
		protected float _pushDistance;
		public float PushDistance { get { return _pushDistance / 100f; } }

		[SerializeField]
		protected float _roadWidth;
		public float RoadWidth {
			get {
				_roadWidth = Mathf.Clamp(_roadWidth, 0.25f, 0.95f);
				return _roadWidth;
			}
		}

		[SerializeField]
		protected GameObject constructorModelPoint;
		[SerializeField]
		protected GameObject widthModelPoint;
		[SerializeField]
		protected MeshFilter roadMeshObject;
		protected MeshCollider roadMeshCollider;
		protected RoadVertices roadVertice;

		[Header("Texture Properties")]
		[SerializeField]
		protected float atlasSize = 2048f;
		[SerializeField]
		protected float textureSize = 64f;

		protected Cell _targetCell;
		protected Triangle _targetTriangle;

        public RoadInfo roadInfo;

		protected GameObject[] sizeVisualizer = new GameObject[2];

		protected void Awake()
		{
			if (roadMeshObject) roadMeshCollider = roadMeshObject.GetComponent<MeshCollider>();
		}

		protected void Update()
		{
			if (!isActive) return;

			if (Input.GetMouseButtonDown(0))
			{
				bool triangleSelect = GetTargetInfo();
				bool edgeSelect = GetSelectedEdge();

				if (triangleSelect && edit == EEditGround.Start)
				{
                    roadInfo.tracks = new List<Vector3>();
					roadVertice = new RoadVertices();
					ShowPossibleEdge();
					edit = EEditGround.SelectEdge;
				}
				else if (edgeSelect && edit == EEditGround.SelectEdge)
				{
					for (int i = 0; i < 2; i++) sizeVisualizer[i] = Instantiate(widthModelPoint, Vector3.zero, Quaternion.identity);
					edges.Add(selectedEdge);
                    roadInfo.tracks.Add(selectedEdge.CenterPos);
					edit = EEditGround.Width;
				}
				else if (triangleSelect && edit == EEditGround.Width)
				{
					ShowPossibleEdge();
					edit = EEditGround.Path;
				}
				else if (edgeSelect && edit == EEditGround.Path)
				{
					PreviewWidth();
					AddRoadPart(edges[edges.Count - 1], selectedEdge);
					edges.Add(selectedEdge);
                    roadInfo.tracks.Add(selectedEdge.CenterPos);
                    edit = EEditGround.Width;
				}
			}
			else if (edit == EEditGround.Width)
			{
				PreviewWidth();
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				AddRoadPart(edges[edges.Count - 1], selectedEdge);
				SortPoints(edges[edges.Count - 1], selectedEdge);
				edges.Add(selectedEdge);
				SetUV();

                JsonManager.SaveRoad(roadInfo);
                roadInfo.name = string.Empty;
                roadInfo.tracks = new List<Vector3>();
			}

			if (edit == EEditGround.CorrectUV)
			{

			}
		}

		protected void SortPoints(RoadEdge startEdge, RoadEdge targetEdge)
		{
			Vector3 tmp;			
			if (Vector3.Angle((startEdge.CenterPos - targetEdge.CenterPos), (startEdge.meshPoints[0] - targetEdge.meshPoints[0])) > 10f)
			{
				targetEdge.haveSwap = true;
				tmp = targetEdge.meshPoints[0];
				targetEdge.meshPoints[0] = targetEdge.meshPoints[1];
				targetEdge.meshPoints[1] = tmp;
			}
		}

		protected void AddRoadPart(RoadEdge startEdge, RoadEdge targetEdge)
		{
            SortPoints(startEdge, targetEdge);

			List<Vector3[]> preTris = new List<Vector3[]>();
			preTris.Add(new Vector3[3] { startEdge.meshPoints[1], startEdge.meshPoints[0], targetEdge.meshPoints[0] });
			preTris.Add(new Vector3[3] { targetEdge.meshPoints[0], targetEdge.meshPoints[1], startEdge.meshPoints[1] });

			for (int i = 0; i < 2; i++)
			{
				if (Vector3.Angle(MathCustom.GetFaceNormalVector(preTris[i][0], preTris[i][1], preTris[i][2]).normalized, MathCustom.GetBarycenter(preTris[i]).normalized) < 90f)
				{
					roadVertice.AddTriangle(new Vector3[3] { preTris[i][2], preTris[i][1], preTris[i][0] });
				}
				else roadVertice.AddTriangle(preTris[i]);
			}
			preTris.Clear();

			int[] lTriangles = new int[roadVertice.triangles.Count * 3];
			for (int i = 0; i < lTriangles.Length / 3; i++)
			{
				lTriangles[i * 3] = roadVertice.triangles[i].verticesindex[0];
				lTriangles[(i * 3) + 1] = roadVertice.triangles[i].verticesindex[1];
				lTriangles[(i * 3) + 2] = roadVertice.triangles[i].verticesindex[2];
			}
				
			roadMeshObject.mesh.vertices = roadVertice.vertex.ToArray();
			roadMeshObject.mesh.triangles = lTriangles;			
			roadMeshObject.mesh.normals = roadVertice.normals.ToArray();

			SetUV();
			roadMeshObject.mesh.uv = roadVertice.UV.ToArray();
			roadMeshCollider.sharedMesh = roadMeshObject.mesh;
		}

		protected void SetUV()
		{
			for (int j = 0; j < edges.Count; j++)
			{
				RoadEdge re = edges[j];
				for (int i = 0; i < roadVertice.vertex.Count; i++)
				{
					if (roadVertice.vertex[i] == re.meshPoints[0])
					{
                        float uvx = (j % 2 != 0) ? 1f : 0f;
						if (re.haveSwap) roadVertice.UV[i] = new Vector2(1, uvx);
					    else roadVertice.UV[i] = new Vector2(0, uvx);
					}
					else if (roadVertice.vertex[i] == re.meshPoints[1])
					{
                        float uvx = (j % 2 != 0) ? 1f : 0f;
                        if (re.haveSwap) roadVertice.UV[i] = new Vector2(0, uvx);
                        else roadVertice.UV[i] = new Vector2(1, uvx);
                    }
				}
			}
		}

		protected void PreviewWidth()
		{
			for (int i = 0; i < 2; i++)
			{
				if (sizeVisualizer[i] != null)
				{
					Vector3 previewPos = selectedEdge.CenterPos + ((selectedEdge.points[i] - selectedEdge.CenterPos) * RoadWidth) + (selectedEdge.CenterPos.normalized * PushDistance);
					selectedEdge.meshPoints[i] = previewPos;
					sizeVisualizer[i].transform.position = previewPos;
				}				
			}
		}

		protected void ShowPossibleEdge()
		{
			RoadEdge re1 = new RoadEdge(
				_targetCell.GroundMesh.smoothVertex[_targetTriangle.verticesindex[0]],
				_targetCell.GroundMesh.smoothVertex[_targetTriangle.verticesindex[1]]);
			RoadEdge re2 = new RoadEdge(
				_targetCell.GroundMesh.smoothVertex[_targetTriangle.verticesindex[0]],
				_targetCell.GroundMesh.smoothVertex[_targetTriangle.verticesindex[2]]);
			RoadEdge re3 = new RoadEdge(
				_targetCell.GroundMesh.smoothVertex[_targetTriangle.verticesindex[1]],
				_targetCell.GroundMesh.smoothVertex[_targetTriangle.verticesindex[2]]);

			choiceEdge.Add(re1, Instantiate(constructorModelPoint, re1.CenterPos, Quaternion.identity));
			choiceEdge.Add(re2, Instantiate(constructorModelPoint, re2.CenterPos, Quaternion.identity));
			choiceEdge.Add(re3, Instantiate(constructorModelPoint, re3.CenterPos, Quaternion.identity));
		}

		public bool GetSelectedEdge()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool clickValid = false;

			List<RoadEdge> toDestroy = new List<RoadEdge>();
			foreach (KeyValuePair<RoadEdge, GameObject> item in choiceEdge)
			{
				if (item.Value.GetComponent<Collider>().Raycast(ray, out hit, 50f))
				{
					clickValid = true;
					selectedEdge = item.Key;
				}
				else toDestroy.Add(item.Key);
			}

			if (clickValid)
			{
				foreach (RoadEdge re in toDestroy)
				{
					Destroy(choiceEdge[re].gameObject);
					choiceEdge.Remove(re);
				}
			}
			return clickValid;
		}

		public bool GetTargetInfo()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 50f, LayerMask.GetMask(new string[1] { "Cell" })))
			{
				_targetCell = hit.transform.gameObject.GetComponent<Cell>();
				int triangleIndex = hit.triangleIndex;
				_targetTriangle = new Triangle(new int[3] {
					_targetCell.GroundMesh.smoothTriangles[triangleIndex * 3],
					_targetCell.GroundMesh.smoothTriangles[(triangleIndex * 3) + 1],
					_targetCell.GroundMesh.smoothTriangles[(triangleIndex * 3) + 2]
				});
				return true;
			}
			else return false;
		}

		public Vector3 GetTargetVertex()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 nearest = Vector3.zero;

			if (roadMeshCollider.Raycast(ray, out hit, 50f))
			{
				float dist = 0;
				int triangleIndex = hit.triangleIndex;
				for (int i = 0; i < 3; i++)
				{
					float tDist = Vector3.Distance(roadMeshObject.mesh.vertices[(triangleIndex * 3) + i], hit.point);
					if (dist != 0)
					{
						if (tDist < dist) nearest = roadMeshObject.mesh.vertices[(triangleIndex * 3) + i];
						else nearest = roadMeshObject.mesh.vertices[(triangleIndex * 3) + i - 1];
					}
					
					dist = tDist;
				}
			}

			return nearest;
		}

		public void OnDrawGizmos()
		{
			if (!isActive) return;
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 50f, LayerMask.GetMask(new string[1] { "Cell" })))
			{
				Cell c = hit.transform.gameObject.GetComponent<Cell>();
				Mesh lMesh = c.GetComponent<MeshFilter>().mesh;
				Gizmos.DrawWireMesh(lMesh, c.transform.position, c.transform.rotation, c.transform.localScale);
			}
		}
	}
}