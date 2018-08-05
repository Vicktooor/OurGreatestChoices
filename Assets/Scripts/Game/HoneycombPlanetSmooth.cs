using Assets.Scripts.Game;
using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

// AUTHOR - Victor

#region Custom Types
public struct Triangle
{
	public int[] verticesindex;

	public Triangle(int[] refVerticesIndex)
	{
		verticesindex = refVerticesIndex;
	}
}

public struct DataGround
{
	public Vector3 center;
	public Triangle[] triangles;
}

public struct VertexCouple
{
	public Vector3[] vertices;

	public VertexCouple(Vector3 one, Vector3 two)
	{
		vertices = new Vector3[2] { one, two };
	}
}

public class GroundMesh
{
	public Vector3 centerPosition;

	public int[] triangles;
	public Vector3[] vertex;
	public Vector3[] normals;
	public Triangle[] refTriangles;

	public int[] computedTriangles;
	public Vector3[] computedVertex;
	public Vector3[] computedNormals;

	public int[] smoothTriangles;
	public Vector3[] smoothVertex;
	public Vector3[] smoothNormals;
	public Vector2[] UVs;

	public int centerIndex;
	public VerticesIndex indexes;

	public GroundMesh(int lTri, int nbVer, Vector3 centerPos)
	{
		triangles = new int[lTri];
		vertex = new Vector3[nbVer];
		normals = new Vector3[nbVer];

		computedTriangles = new int[lTri * 2];
		computedVertex = new Vector3[nbVer + 1];
		computedNormals = new Vector3[nbVer + 1];

		smoothTriangles = new int[lTri * 5];
		smoothVertex = new Vector3[(nbVer * 4) + 1];
		smoothNormals = new Vector3[(nbVer * 4) + 1];
		UVs = new Vector2[(nbVer * 4) + 1];

		indexes = new VerticesIndex(nbVer - 1);

		centerPosition = centerPos;
	}
}
#endregion

public class HoneycombPlanetSmooth : MonoBehaviour
{
	[SerializeField]
	protected float _cellWidth = 0.65f;
	public float CellWidth
	{
		get { return Mathf.Clamp(_cellWidth, 0.05f, 0.95f); }
	}

	// Reference mesh
	protected Mesh referenceMesh;

    protected float radius;
    protected Triangle[] allTriangles;
    public List<GroundMesh> allGroundMesh;

    protected Vector3[] refVertices;
    protected int[] refTriangles;
    protected Vector3[] refNormals;

    protected float minimalNeighborDistance;

	private void Awake()
	{
		PoolingManager.Instance.Active();
		LinkDatabase.Instance.Init();
		radius = GetComponent<SphereCollider>().radius;
		referenceMesh = GetComponent<MeshFilter>().mesh;

		refVertices = referenceMesh.vertices;
		refTriangles = referenceMesh.triangles;
		refNormals = referenceMesh.normals;
	}

	public void StartGeneration(Action callback)
	{
		CreatePlanet();
		callback();
	}

    private void CreatePlanet()
    {
        GetAllReferenceTriangles();
        allGroundMesh = new List<GroundMesh>();
        
        List<DataGround> pentaGrounds = GetGroundsFromTriangleNumber(5);
        CreateGroundMesh(pentaGrounds);

        List<DataGround> hexaGrounds = GetGroundsFromTriangleNumber(6);
        CreateGroundMesh(hexaGrounds);
    }

    /// <summary>
    /// Create an Array with all the triangle of the reference mesh (have to be an Isocahedre)
    /// </summary>
    protected void GetAllReferenceTriangles()
    {
        allTriangles = new Triangle[referenceMesh.triangles.Length / 3];

        int ite = 0;
        for (int i = 0; i < refTriangles.Length; i++)
        {
            if (i % 3 == 0)
            {
                allTriangles[ite] = new Triangle(new int[3] { refTriangles[i], refTriangles[i + 1], refTriangles[i + 2] });
                ite++;
            }
        }
    }

	/// <summary>
	/// Return all the DataGround, based on their number of face
	/// </summary>
	/// <param name="groundNumberTriangle">Number of face</param>
	/// <returns></returns>
	protected List<DataGround> GetGroundsFromTriangleNumber(int groundNumberTriangle)
    {
        int currentNeighborIndex = 0;
        List<DataGround> groundsFind = new List<DataGround>();
        List<Vector3> usedCenters = new List<Vector3>();
        Triangle[] currentNeighbors = new Triangle[groundNumberTriangle];

        for (int i = 0; i < refVertices.Length; i++)
        {
            for (int j = 0; j < allTriangles.Length; j++)
            {
                Triangle currentTestTriangle = allTriangles[j];

                if (refVertices[currentTestTriangle.verticesindex[0]] == refVertices[i] || refVertices[currentTestTriangle.verticesindex[1]] == refVertices[i] || refVertices[currentTestTriangle.verticesindex[2]] == refVertices[i])
                {
                    currentNeighborIndex++;
                    if (currentNeighborIndex <= groundNumberTriangle) currentNeighbors[currentNeighborIndex - 1] = currentTestTriangle;
                }
            }

            if (currentNeighborIndex > groundNumberTriangle || currentNeighborIndex < groundNumberTriangle)
            {
                currentNeighborIndex = 0;
                currentNeighbors = new Triangle[groundNumberTriangle];
            }
            else
            {
                if (!usedCenters.Contains(refVertices[i]))
                {
                    currentNeighborIndex = 0;
                    DataGround newGroundData = new DataGround();
                    newGroundData.center = refVertices[i];
                    newGroundData.triangles = currentNeighbors;             
                    currentNeighbors = new Triangle[groundNumberTriangle];			
					usedCenters.Add(refVertices[i]);
					groundsFind.Add(newGroundData);
				}
                else
                {
                    currentNeighborIndex = 0;
                    currentNeighbors = new Triangle[groundNumberTriangle];
                }
            }
        }
        return groundsFind;
    }

	/// <summary>
	/// Create the GameObject and the mesh of a DatagroundList
	/// </summary>
	/// <param name="groundMeshs">DataGround list</param>
	protected void CreateGroundMesh(List<DataGround> groundMeshs)
    {
        Vector3 origin = gameObject.transform.position;
        int index = 0;
        foreach (DataGround ground in groundMeshs)
        {
            GroundMesh newGround = TrianglesToGroundMesh(ground);
            DualTransformation(newGround);
            RecalculatePeak(newGround, origin);
			PrepareMeshForSmooth(newGround);
			GenerateTextureMap(newGround);
            allGroundMesh.Add(newGround);
            index++;
        }
    }

	/// <summary>
	/// Generate UV mapping
	/// </summary>
	/// <param name="groundMesh"></param>
	protected void GenerateTextureMap(GroundMesh groundMesh)
	{
		Vector3 cross = Vector3.Cross(Vector3.up, groundMesh.centerPosition);
		Vector3 py = groundMesh.centerPosition + cross;
		Vector3 pz = groundMesh.centerPosition + Vector3.Cross(cross, groundMesh.centerPosition);
		Vector4 cellPlan = MathCustom.GetPlanValueWithThreePoints(groundMesh.centerPosition, py, pz);

		for (int i = 0; i < groundMesh.smoothVertex.Length; i++)
		{	
			Vector3 vertex = groundMesh.smoothVertex[i];
			if (vertex == groundMesh.centerPosition)
			{
				groundMesh.UVs[i] = new Vector2(0.5f, 0.5f);
			}
			else
			{
				Vector3 planeCoord = MathCustom.GetCoordinatesWhereLineCutPlan(Vector3.zero, vertex, cellPlan);
				Vector3 cartesianPos = Quaternion.FromToRotation(groundMesh.centerPosition, Vector3.up) * planeCoord;
				float lRadius;
				float lPolar;
				float lElevation;
				MathCustom.CartesianToSpherical(cartesianPos, out lRadius, out lPolar, out lElevation);
				float xAngle = Vector3.SignedAngle(groundMesh.centerPosition, vertex, groundMesh.centerPosition);
				float yAngle = Vector3.SignedAngle(Vector3.Cross(groundMesh.centerPosition, -Vector3.up), vertex, groundMesh.centerPosition);

				if (CustomGeneric.IntArrayContain(groundMesh.indexes.internVertexIndex, i) != -1)
					groundMesh.UVs[i] = new Vector2(0.5f + (Mathf.Cos(lPolar) * 0.9f), 0.5f + (Mathf.Sin(lPolar) * 0.9f));
				else groundMesh.UVs[i] = new Vector2(0.5f + Mathf.Cos(lPolar), 0.5f + Mathf.Sin(lPolar));
			}			
		}
	}

	/// <summary>
	/// Convert DataGrounds to a GroundMesh
	/// </summary>
	/// <param name="dataMesh">Dataground</param>
	/// <returns></returns>
	protected GroundMesh TrianglesToGroundMesh(DataGround dataMesh)
    {
        int nbTriangle = dataMesh.triangles.Length;
        GroundMesh groundMesh = new GroundMesh(nbTriangle * 3, nbTriangle + 1, dataMesh.center);
        groundMesh.refTriangles = dataMesh.triangles;

        int[] lTriangles = new int[groundMesh.triangles.Length];
        Vector3[] lVertex = new Vector3[nbTriangle + 1];
        Vector3[] lNormals = new Vector3[nbTriangle + 1];
        Vector2[] lUvs = new Vector2[nbTriangle + 1];

        int ite = 0;
        for (int i = 0; i < nbTriangle; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int index = CustomGeneric.ArrayContain(lVertex, refVertices[dataMesh.triangles[i].verticesindex[j]]);
                if (index < 0)
                {
                    lVertex[ite] = refVertices[dataMesh.triangles[i].verticesindex[j]];
                    lNormals[ite] = refNormals[dataMesh.triangles[i].verticesindex[j]];
                    ite++;
                }
            }
        }

        for (int i = 0; i < nbTriangle; i++)
        {
            Vector3 center = groundMesh.centerPosition;
            for (int j = 0; j < 3; j++)
            {
                lTriangles[i * 3 + j] = CustomGeneric.ArrayContain(lVertex, refVertices[dataMesh.triangles[i].verticesindex[j]]);
            }
        }

        groundMesh.vertex = lVertex;
        groundMesh.normals = lNormals;
        groundMesh.triangles = lTriangles;

        return groundMesh;
    }

    /// <summary>
    /// Get the dual transformation of a GroundMesh
    /// </summary>
    /// <param name="groundMesh">GroundMesh</param>
    protected void DualTransformation(GroundMesh groundMesh)
    {
		Vector3[] computedVertices = new Vector3[groundMesh.computedVertex.Length];
		Vector3[] computedNormals = new Vector3[groundMesh.computedVertex.Length];

        for (int i = 0; i < groundMesh.triangles.Length / 3; i++)
        {
            Vector3 addVector = Vector3.zero + groundMesh.centerPosition;
            if (groundMesh.vertex[groundMesh.triangles[i * 3]] != groundMesh.centerPosition) addVector += groundMesh.vertex[groundMesh.triangles[i * 3]];
            if (groundMesh.vertex[groundMesh.triangles[i * 3 + 1]] != groundMesh.centerPosition) addVector += groundMesh.vertex[groundMesh.triangles[i * 3 + 1]];
            if (groundMesh.vertex[groundMesh.triangles[i * 3 + 2]] != groundMesh.centerPosition) addVector += groundMesh.vertex[groundMesh.triangles[i * 3 + 2]];

            computedVertices[i] = addVector.normalized * radius;
            computedNormals[i] = computedVertices[i].normalized;
        }

        computedVertices[computedVertices.Length - 2] = Vector3.zero;
        computedNormals[computedNormals.Length - 2] = -groundMesh.centerPosition.normalized;

        computedVertices[computedVertices.Length - 1] = groundMesh.centerPosition;
        computedNormals[computedNormals.Length - 1] = groundMesh.centerPosition.normalized;

        groundMesh.computedVertex = computedVertices;
        groundMesh.computedNormals = computedNormals;

        // Creation of the dual triangles array

        int nbTriangle = groundMesh.triangles.Length / 3;
        VertexCouple[] couples = new VertexCouple[nbTriangle];

        for (int i = 0; i < nbTriangle; i++)
            couples[i] = new VertexCouple(Vector3.zero, Vector3.zero);

        int ite = 0;
        for (int j = 0; j < groundMesh.computedVertex.Length; j++)
        {
            for (int k = 0; k < groundMesh.computedVertex.Length; k++)
            {
                if (groundMesh.computedVertex[j] != groundMesh.centerPosition && groundMesh.computedVertex[k] != groundMesh.centerPosition && groundMesh.computedVertex[j] != Vector3.zero && groundMesh.computedVertex[k] != Vector3.zero)
                {
                    float angle = Vector3.Angle(groundMesh.centerPosition - groundMesh.computedVertex[j], groundMesh.centerPosition - groundMesh.computedVertex[k]);
                    if (angle > 2 && angle < 70)
                    {
                        VertexCouple newCouple = new VertexCouple(groundMesh.computedVertex[j], groundMesh.computedVertex[k]);
                        if (!CoupleExist(couples, newCouple))
                        {
                            couples[ite] = newCouple;
                            ite++;
                        }
                    }
                }
            }
        }

        int[] lTriangles = new int[groundMesh.triangles.Length * 2];
        for (int i = 0; i < couples.Length; i++)
        {
            Vector3 surfaceNormal = Vector3.Cross(groundMesh.centerPosition + couples[i].vertices[0], groundMesh.centerPosition + couples[i].vertices[1]);
            float angle = Vector3.Angle(gameObject.transform.position + groundMesh.centerPosition, surfaceNormal);

            if (angle < 90)
            {
                lTriangles[i * 3] = CustomGeneric.ArrayContain(groundMesh.computedVertex, groundMesh.centerPosition);
                lTriangles[i * 3 + 1] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[0]);
                lTriangles[i * 3 + 2] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[1]);                
            }
            else
            {
                lTriangles[i * 3] = CustomGeneric.ArrayContain(groundMesh.computedVertex, groundMesh.centerPosition);
                lTriangles[i * 3 + 1] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[1]);
                lTriangles[i * 3 + 2] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[0]);  
            }

            Vector4 face = MathCustom.GetPlanValueWithThreePoints(Vector3.zero, couples[i].vertices[1], couples[i].vertices[0]);
            if (MathCustom.GetDistanceToPlan(groundMesh.centerPosition, face) > 0)
            {
                lTriangles[groundMesh.triangles.Length + i * 3] = CustomGeneric.ArrayContain(groundMesh.computedVertex, Vector3.zero);
                lTriangles[groundMesh.triangles.Length + i * 3 + 1] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[0]);
                lTriangles[groundMesh.triangles.Length + i * 3 + 2] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[1]);
            }  
            else
            {
                lTriangles[groundMesh.triangles.Length + i * 3] = CustomGeneric.ArrayContain(groundMesh.computedVertex, Vector3.zero);
                lTriangles[groundMesh.triangles.Length + i * 3 + 1] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[1]);
                lTriangles[groundMesh.triangles.Length + i * 3 + 2] = CustomGeneric.ArrayContain(groundMesh.computedVertex, couples[i].vertices[0]);
            }
        }		
        groundMesh.computedTriangles = lTriangles;
	}

	/// <summary>
	/// Recalculate peak to make ground flat
	/// </summary>
	/// <param name="groundMesh">GroundMesh</param>
	protected void RecalculatePeak(GroundMesh groundMesh, Vector3 origin)
	{
		int nbVertex = groundMesh.computedVertex.Length;
		Vector3[] planPoints = new Vector3[3];
		int ite = 0;
		for (int i = 0; i < nbVertex; i++)
		{
			if (groundMesh.computedVertex[i] != groundMesh.centerPosition)
			{
				planPoints[ite] = groundMesh.computedVertex[i];
				ite++;
				if (ite >= 3) break;
			}
		}

		Vector4 groundPlan = MathCustom.GetPlanValueWithThreePoints(planPoints[0], planPoints[1], planPoints[2]);
		Vector3 newCenterPosition = MathCustom.GetCoordinatesWhereLineCutPlan(origin, groundMesh.centerPosition * int.MaxValue, groundPlan);

		for (int i = 0; i < nbVertex; i++)
		{
			if (groundMesh.computedVertex[i] == groundMesh.centerPosition)
			{
				groundMesh.centerPosition = newCenterPosition;
				groundMesh.computedVertex[i] = newCenterPosition;
			}
		}
	}

	/// <summary>
	/// Cut the mesh to create a smoothable cell
	/// </summary>
	/// <param name="groundMesh">GroundMesh</param>
	protected void PrepareMeshForSmooth(GroundMesh groundMesh)
	{
		Vector3[] internVertex = new Vector3[2];
		Vector3 midEdgeVertex = Vector3.zero;
		Vector3[] edgeVertex = new Vector3[2];

		Vector3[] smoothVertex = new Vector3[(groundMesh.triangles.Length * 4) + groundMesh.triangles.Length];
		int iterator = 0;
		for (int i = 0; i < groundMesh.computedTriangles.Length / 3; i++)
		{
			if (groundMesh.computedVertex[groundMesh.computedTriangles[i * 3]] == Vector3.zero) continue;

			Vector3 ver1 = groundMesh.computedVertex[groundMesh.computedTriangles[(i * 3) + 1]];
			Vector3 ver2 = groundMesh.computedVertex[groundMesh.computedTriangles[(i * 3) + 2]];
			internVertex[0] = (groundMesh.centerPosition + ver1) / 2f;
			internVertex[1] = (groundMesh.centerPosition + ver2) / 2f;

			Vector3 vDir1 = (internVertex[0] - groundMesh.centerPosition);
			Vector3 vDir2 = (internVertex[1] - groundMesh.centerPosition);
			internVertex[0] = internVertex[0] + (vDir1 * CellWidth);
			internVertex[1] = internVertex[1] + (vDir2 * CellWidth);

			float d = Vector3.Distance(internVertex[0], internVertex[1]) / 2f;
			midEdgeVertex = (ver1 + ver2) / 2f;
			edgeVertex[0] = midEdgeVertex + (midEdgeVertex - ver1).normalized * d;
			edgeVertex[1] = midEdgeVertex + (midEdgeVertex - ver2).normalized * d;

			for (int j = 0; j < 5; j++)
			{
				switch (j)
				{
					case 0:
						smoothVertex[iterator * 3] = groundMesh.centerPosition;
						smoothVertex[(iterator * 3) + 1] = internVertex[0];
						smoothVertex[(iterator * 3) + 2] = internVertex[1];
						break;
					case 1:
						smoothVertex[iterator * 3] = internVertex[0];
						if (Vector3.Distance(ver1, internVertex[0]) > Vector3.Distance(ver2, internVertex[0])) smoothVertex[iterator * 3 + 1] = ver2;
						else smoothVertex[iterator * 3 + 1] = ver1;
						if (Vector3.Distance(edgeVertex[0], internVertex[0]) > Vector3.Distance(edgeVertex[1], internVertex[0])) smoothVertex[iterator * 3 + 2] = edgeVertex[1];
						else smoothVertex[iterator * 3 + 2] = edgeVertex[0];
						break;
					case 2:
						smoothVertex[iterator * 3] = internVertex[1];
						if (Vector3.Distance(edgeVertex[0], internVertex[1]) > Vector3.Distance(edgeVertex[1], internVertex[1])) smoothVertex[iterator * 3 + 1] = edgeVertex[1];
						else smoothVertex[iterator * 3 + 1] = edgeVertex[0];
						if (Vector3.Distance(ver1, internVertex[1]) > Vector3.Distance(ver2, internVertex[1])) smoothVertex[iterator * 3 + 2] = ver2;
						else smoothVertex[iterator * 3 + 2] = ver1;					
						break;
					case 3:
						smoothVertex[iterator * 3] = internVertex[1];
						smoothVertex[iterator * 3 + 1] = internVertex[0];
						if (Vector3.Distance(edgeVertex[0], internVertex[1]) > Vector3.Distance(edgeVertex[1], internVertex[1])) smoothVertex[iterator * 3 + 2] = edgeVertex[1];
						else smoothVertex[iterator * 3 + 2] = edgeVertex[0];
						break;
					case 4:
						smoothVertex[iterator * 3] = internVertex[0];
						smoothVertex[iterator * 3 + 1] = edgeVertex[1];
						smoothVertex[iterator * 3 + 2] = edgeVertex[0];
						break;
				}

				iterator++;
			}		
		}

		iterator = 0;
		int coordNumber = smoothVertex.Length;
		for (int i = 0; i < coordNumber; i++)
		{
			int found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, smoothVertex[i]);
			if (found == -1)
			{
				groundMesh.smoothVertex[iterator] = smoothVertex[i];
				groundMesh.smoothNormals[iterator] = smoothVertex[i].normalized;
				groundMesh.smoothTriangles[i] = CustomGeneric.ArrayContain(groundMesh.smoothVertex, smoothVertex[i]);
				iterator++;
			}
			else groundMesh.smoothTriangles[i] = found;
		}

		for (int i = 0; i < groundMesh.computedTriangles.Length / 3; i++)
		{
			if (groundMesh.computedVertex[groundMesh.computedTriangles[i * 3]] == Vector3.zero) continue;

			Vector3 ver1 = groundMesh.computedVertex[groundMesh.computedTriangles[(i * 3) + 1]];
			Vector3 ver2 = groundMesh.computedVertex[groundMesh.computedTriangles[(i * 3) + 2]];
			internVertex[0] = (groundMesh.centerPosition + ver1) / 2f;
			internVertex[1] = (groundMesh.centerPosition + ver2) / 2f;

			Vector3 vDir1 = (internVertex[0] - groundMesh.centerPosition);
			Vector3 vDir2 = (internVertex[1] - groundMesh.centerPosition);
			internVertex[0] = internVertex[0] + (vDir1 * CellWidth);
			internVertex[1] = internVertex[1] + (vDir2 * CellWidth);

			float d = Vector3.Distance(internVertex[0], internVertex[1]) / 2f;
			midEdgeVertex = (ver1 + ver2) / 2f;
			edgeVertex[0] = midEdgeVertex + (midEdgeVertex - ver1).normalized * d;
			edgeVertex[1] = midEdgeVertex + (midEdgeVertex - ver2).normalized * d;

			int found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, edgeVertex[0]);
			if (found != -1) groundMesh.indexes.edgeVertexIndex[i * 2] = found;

			found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, edgeVertex[1]);
			if (found != -1) groundMesh.indexes.edgeVertexIndex[i * 2 + 1] = found;

			found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, ver1);
			if (found != -1) groundMesh.indexes.cornerVertexIndex[i] = found;

			found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, ver2);
			if (found != -1) groundMesh.indexes.cornerVertexIndex[i] = found;

			found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, internVertex[0]);
			if (found != -1) groundMesh.indexes.internVertexIndex[i] = found;

			found = CustomGeneric.ArrayContain(groundMesh.smoothVertex, internVertex[1]);
			if (found != -1) groundMesh.indexes.internVertexIndex[i] = found;
		}
	}

	/// <summary>
	/// Link all the tile to her neighors
	/// </summary>
	/// <param name="neighborRefDistance"></param>
	public void GenerateNeighborLinks(float neighborRefDistance, List<Cell> cells)
	{
		foreach (Cell iCell in cells)
		{
			Vector3 tilePosition = iCell.GetCenterPosition();
			foreach (Cell jCell in cells)
			{
				Vector3 jTileCenter = jCell.GetCenterPosition();
				if (tilePosition != jTileCenter)
				{
					float dist = Vector3.Distance(tilePosition, jTileCenter);
					if (dist < neighborRefDistance * 1.55f)
					{
						iCell.AddNeighbor(jCell);
					}
				}
			}
		}
	}

	/// <summary>
	/// Return the lowest value of neighbor distance
	/// </summary>
	/// <returns></returns>
	public float GetMinimalNeighborDistance()
	{
		float minimalDistance = float.MaxValue;
		int nbGround = allGroundMesh.Count;
		for (int i = 0; i < nbGround; i++)
		{
			Vector3 currentTilePosition = allGroundMesh[i].centerPosition;
			foreach (GroundMesh ground in allGroundMesh)
			{
				if (currentTilePosition != ground.centerPosition)
				{
					float lDistance = Vector3.Distance(currentTilePosition, ground.centerPosition);
					if (lDistance < minimalDistance)
						minimalDistance = lDistance;
				}
			}
		}
		return minimalDistance;
	}

    /// <summary>
    /// Return if a couple of vertices already exist
    /// </summary>
    /// <param name="couples">Array to check on</param>
    /// <param name="testCouple">Couple to test</param>
    /// <returns></returns>
    protected bool CoupleExist(VertexCouple[] couples, VertexCouple testCouple)
    {
        foreach (VertexCouple couple in couples)
        {
            if (testCouple.vertices[0] == couple.vertices[0] && testCouple.vertices[1] == couple.vertices[1] ||
                testCouple.vertices[0] == couple.vertices[1] && testCouple.vertices[1] == couple.vertices[0])
                return true;
        }
        return false;
    }

	public float GetRadius()
    {
        return radius;
    }
}
