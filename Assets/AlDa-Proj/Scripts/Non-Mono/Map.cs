using UnityEngine;

namespace HexSnake
{
	public abstract class Map
	{
		protected int MapRadius;
		protected float CellRadius;

		public Octree<Cell> Cells;
		public abstract Vector3 AxisToWorld(int pR, int pQ);
		public abstract Cell CellAtLocation(Vector3 pLocation);
		public abstract int DistanceBetweenCells(Cell lhs, Cell rhs);
	}
}

