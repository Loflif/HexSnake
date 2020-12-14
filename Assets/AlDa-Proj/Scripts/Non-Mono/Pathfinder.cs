using System.Collections.Generic;

namespace HexSnake
{
	public class Pathfinder
	{
		private class Node : IHeapItem<Node>
		{
			public int g;
			public int h;
			public Node Parent = null;

			public Cell Cell;

			public int f
			{
				get { return g + h; }
			}

			public int CompareTo(Node pOther)
			{
				int compare = f.CompareTo(pOther.f);
				if (compare == 0)
				{
					compare = h.CompareTo(pOther.h);
				}
				return -compare;
			}

			public bool Equals(Cell pOther)
			{
				return Cell.Equals(pOther);
			}

			public int Index { get; set; }
		}

		public static List<Cell> FindPath(Cell pStartNode, Cell pEndNode, Map pMap)
		{
			Heap<Node> openSet = new Heap<Node>(pMap.Cells.Count);

			Dictionary<Cell, Node> checkedSet = new Dictionary<Cell, Node>();
			
			HashSet<Cell> closedSet = new HashSet<Cell>();
			
			Node start = NewNode(pStartNode, 0);

			List<Cell> path = new List<Cell>();

			openSet.Add(start);
			

			while (openSet.Count > 0)
			{
				Node current = openSet.PopFirst();

				closedSet.Add(current.Cell);

				if (current.h == 0)
				{
					RetracePath(start, current);
					return path;
				}

				foreach (Cell neighbour in current.Cell.Neighbours)
				{
					if (!neighbour.IsTraversable || closedSet.Contains(neighbour))
						continue;

					int newCostToNeighbour = current.g + 1;

					ValidateNeighbour(neighbour, current, newCostToNeighbour);
				}
			}

			Node NewNode(Cell pCell, int pG)
			{
				int h = pMap.DistanceBetweenCells(pCell, pEndNode);
				Node newNode = new Node {g = pG, h = h, Cell = pCell};
				checkedSet.Add(pCell, newNode);
				return newNode;
			}

			void ValidateNeighbour(Cell pNeighbour, Node pCurrent, int pNewCostToTile)
			{
				if (checkedSet.ContainsKey(pNeighbour))
				{
					Node checkedNeighbour = checkedSet[pNeighbour];
					if (checkedNeighbour.g > pNewCostToTile)
					{
						checkedNeighbour.g = pNewCostToTile;
						checkedNeighbour.Parent = pCurrent;
						openSet.UpdateItem(checkedNeighbour);
					}

					return;
				}
				foreach (Node n in openSet)
				{
					if (!n.Equals(pNeighbour)) 
						continue;
					if (pNewCostToTile > n.g)
					{
						n.g = pNewCostToTile;
						openSet.UpdateItem(n);
						return;
					}
				}

				Node newNode = NewNode(pNeighbour, pNewCostToTile);
				newNode.Parent = pCurrent;
				openSet.Add(newNode);
			}

			void RetracePath(Node pPathStartNode, Node pPathEndNode)
			{
				Node currentNode = pPathEndNode;

				while (!currentNode.Equals(pPathStartNode))
				{
					path.Add(currentNode.Cell);
					currentNode = currentNode.Parent;
				}

				path.Reverse();
			}

			return path;
		}
	}
}