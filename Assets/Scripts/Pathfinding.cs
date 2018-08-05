using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	public static class Pathfinding
	{
		public static List<Vector3> GetPathFromTo(Cell startCell, Cell destinationCell)
		{
			bool havePath = false;
			List<Vector3> path = new List<Vector3>();
			path.Add(startCell.GetCenterPosition());

			CheckNeighbor(startCell, destinationCell);
		
			return path;
		}

		private static void CheckNeighbor(Cell startCell, Cell destinationCell)
		{
			foreach(Cell cell in startCell.Neighbors)
			{
				/*if (cell.State != CellState.SEA)
				{
					//Debug.Log(Vector3.Distance(destinationCell.GetCenterNormalizedPosition(), cell.GetCenterNormalizedPosition()));
				}*/
			}
		}

		private static Cell ClosestNeighbor(Cell cell)
		{
			Cell closestCell = null;
			float lowerDist = Vector3.Distance(cell.GetCenterPosition(), -cell.GetCenterPosition());
			foreach (Cell lCell in cell.Neighbors)
			{
				if (Vector3.Distance(cell.GetCenterPosition(), lCell.GetCenterPosition()) < lowerDist)
				{
					closestCell = lCell;
				}
			}
			return closestCell;
		}
	}
}