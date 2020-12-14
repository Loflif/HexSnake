using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadtree<T> : IEnumerable<T>
{
	private readonly Node RootNode;
	private List<T> Nodes = new List<T>();

	public int Count => Nodes.Count;

	public Quadtree(Vector2 pTreeOrigin, float pTreeWidth, int pMaxDivisions)
	{
		RootNode = new Node(pTreeOrigin, pTreeWidth, pMaxDivisions, 1);
	}
	
	public Quadtree(Vector2 pTreeOrigin, float pTreeWidth, int pMaxDivisions, int pNodeCapacity = 1)
	{
		RootNode = new Node(pTreeOrigin, pTreeWidth, pMaxDivisions, pNodeCapacity);
	}

	public void Insert(T pItem, Vector2 pPosition)
	{
		RootNode.Insert(pItem, pPosition, ref Nodes);
	}

	public void Insert(T pItem, (float, float) pPosition)
	{
		Vector2 pos = new Vector2(pPosition.Item1, pPosition.Item2);
		Insert(pItem, pos);
	}

	public void Insert(T pItem, (int, int) pPosition)
	{
		Vector2 pos = new Vector2(pPosition.Item1, pPosition.Item2);
		Insert(pItem, pos);
	}

	public List<T> GetDataInsideArea(Bounds pArea)
	{
		List<T> dataInsideArea = new List<T>();
		RootNode.QueryData(pArea, ref dataInsideArea);
		return dataInsideArea;
	}
	
	public List<T> GetDataInsideArea(Vector2 pAreaCenter, Vector2 pAreaRadius)
	{
		Bounds area = new Bounds(pAreaCenter, pAreaRadius);
		return GetDataInsideArea(area);
	}
	
	public List<T> GetDataInsideArea((int, int) pAreaCenter, (int, int) pAreaRadius)
	{
		Vector2 pos = new Vector2(pAreaCenter.Item1, pAreaCenter.Item2);
		Vector2 size = new Vector2(pAreaRadius.Item1, pAreaRadius.Item2);
		Bounds area = new Bounds(pos, size);
		return GetDataInsideArea(area);
	}

	private class Node
	{
		private readonly struct DataPoint
		{
			public DataPoint(T pData, Vector2 pPoint)
			{
				Data = pData;
				Point = pPoint;
			}

			public T Data { get; }
			public Vector2 Point { get; }
		}
		
		private readonly Vector2 Position;
		private readonly float Width;
		private readonly int Capacity;
		private readonly int CurrentDivision;
		private readonly List<DataPoint> DataPoints = new List<DataPoint>();
		private Node[] SubNodes;
		private bool IsDivided = false;
		public bool IsEmpty => DataPoints.Count <= 0;

		public Node(Vector2 pPosition, float pRootWidth, int pCurrentDivision, int pNodeCapacity)
		{
			Width = pRootWidth * 0.5f;
			Position = pPosition;
			CurrentDivision = pCurrentDivision;
			Capacity = pNodeCapacity;
		}

		public void Insert(T pData, Vector2 pPoint, ref List<T> pNodes)
		{
			if (!Contains(pPoint))
				return;

			if(!IsDivided 
			   && DataPoints.Count >= Capacity
			   && CurrentDivision > 0)
			{
				Subdivide();
			}
			if (IsDivided)
			{
				foreach (Node n in SubNodes)
				{
					if (n.Contains(pPoint))
					{
						n.Insert(pData, pPoint, ref pNodes);
						return;
					}
				}
			}
			else
			{
				DataPoints.Add(new DataPoint(pData, pPoint));
				pNodes.Add(pData);
			}
		}
		
		public void QueryData(Bounds pCheckBounds, ref List<T> pDataList)
		{
			if (IsEmpty)
				return;

			foreach (DataPoint d in DataPoints)
			{
				if (pCheckBounds.Contains(d.Point))
				{
					pDataList.Add(d.Data);
				}
			}

			if (!IsDivided) 
				return;
			foreach (Node n in SubNodes)
			{
				n.QueryData(pCheckBounds, ref pDataList);
			}
		}

		private bool Contains(Vector2 pPoint)
		{
			return  pPoint.x >= Position.x - Width &&
			        pPoint.x <= Position.x + Width &&
			        pPoint.y >= Position.y - Width &&
			        pPoint.y <= Position.y + Width;
		}
		
		private void Subdivide()
		{
			Vector2[] newBoxPositions = 
			{
				new Vector2(Position.x + Width * 0.5f, Position.y + Width * 0.5f),
				new Vector2(Position.x + Width * 0.5f, Position.y - Width * 0.5f),
				new Vector2(Position.x - Width * 0.5f, Position.y + Width * 0.5f),
				new Vector2(Position.x - Width * 0.5f, Position.y - Width * 0.5f),
			};
			
			SubNodes = new Node[4];

			for (int i = 0; i < newBoxPositions.Length; i++)
			{
				SubNodes[i] = new Node(newBoxPositions[i], Width, CurrentDivision-1, Capacity);
			}
			
			IsDivided = true;
		}

		public void DebugGetPositionsAndWidths(ref List<Vector2> pPositions, ref List<float> pWidths, ref List<Vector2> pPoints)
		{
			pPositions.Add(Position);
			pWidths.Add(Width);
			if (DataPoints.Count > 0)
			{
				foreach (DataPoint d in DataPoints)
				{
					pPoints.Add(d.Point);
				}
			}

			if (SubNodes == null)
				return;
			
			foreach (Node n in SubNodes)
			{
				n?.DebugGetPositionsAndWidths(ref pPositions, ref pWidths, ref pPoints);
			}
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return Nodes[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void DebugDrawCubes(Color pPointColor, Color pCubeColor)
	{
		List<Vector2> positions = new List<Vector2>();
		List<float> widths = new List<float>();
		List<Vector2> points = new List<Vector2>();
		RootNode.DebugGetPositionsAndWidths(ref positions, ref widths, ref points);

		for (int i = 0; i < positions.Count; i++)
		{
			Gizmos.color = pCubeColor;
			Gizmos.DrawWireCube(positions[i], new Vector2(widths[i]*2, widths[i]*2));
		}

		foreach (var point in points)
		{
			Gizmos.color = pPointColor;
			Gizmos.DrawSphere(point, 0.1f);
		}
	}
}
