using System.Collections.Generic;
using UnityEngine;

namespace HexSnake
{
	public class HexagonMap : Map
	{
		private readonly Dictionary<(int, int, int), Cell> CellsCube;

		public HexagonMap(int pMapRadius, float pCellRadius)
		{
			MapRadius = pMapRadius;
			CellRadius = pCellRadius;
			
			CellsCube = new Dictionary<(int, int, int), Cell>();
			Cells = new Octree<Cell>(Vector3.zero, pMapRadius * 2, 5);
			for (int q = -pMapRadius; q <= pMapRadius; q++)
			{
				int r1 = Mathf.Max(-pMapRadius, -q - pMapRadius);
				int r2 = Mathf.Min(pMapRadius, -q + pMapRadius);
				for (int r = r1; r <= r2; r++)
				{
					Cell newCell = new Cell(r, q);
					int s = -r - q;
					CellsCube.Add((r, q, s), newCell);
					Cells.Insert(newCell, (r, q, s));
				}
			}

			CalculateCellNeighbours();
		}

		private void CalculateCellNeighbours()
		{
			foreach (Cell c in Cells)
			{
				var neighbours = Cells.GetDataInsideArea((c.r, c.q, c.s), (2, 2, 2));
				neighbours.Remove(c);
				c.Neighbours = neighbours.ToArray();
			}
		}

		private (int, int, int) AddCoordinates((int, int, int) lhs, (int, int, int) rhs)
		{
			return (lhs.Item1 + rhs.Item1, lhs.Item2 + rhs.Item2, lhs.Item3 + rhs.Item3);
		}

		private bool IsCoordinateOnMap((int, int, int) pCoordinate)
		{
			int r = Mathf.Abs(pCoordinate.Item1);
			int q = Mathf.Abs(pCoordinate.Item2);
			int s = Mathf.Abs(pCoordinate.Item3);
			return (r <= MapRadius && q <= MapRadius && s <= MapRadius);
		}

		public override Vector3 AxisToWorld(int pR, int pQ)
		{
			Vector3 worldPos = Vector3.zero;

			worldPos.x = CellRadius * (Mathf.Sqrt(3) * pQ + Mathf.Sqrt(3) / 2 * pR);
			worldPos.y = CellRadius * (3.0f / 2 * pR);

			return worldPos;
		}

		public override Cell CellAtLocation(Vector3 pLocation)
		{
			int r = Mathf.RoundToInt((2.0f / 3 * pLocation.y) / CellRadius);
			int q = Mathf.RoundToInt((Mathf.Sqrt(3) / 3 * pLocation.x - 1.0f / 3 * pLocation.y) / CellRadius);
			int s = -r - q;

			return !CellsCube.ContainsKey((r, q, s)) ? null : CellsCube[(r, q, s)];
		}

		public override int DistanceBetweenCells(Cell lhs, Cell rhs)
		{
			int r = Mathf.Abs(lhs.r - rhs.r);
			int q = Mathf.Abs(lhs.q - rhs.q);
			int s = Mathf.Abs(lhs.s - rhs.s);
			return Mathf.Max(r, q, s);
		}
	}
}