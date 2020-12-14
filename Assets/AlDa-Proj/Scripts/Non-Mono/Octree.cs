using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree<T> : IEnumerable<T>
{
	private readonly Node RootNode;
	private List<T> Nodes = new List<T>();

	public int Count => Nodes.Count;

	public Octree(Vector3 pTreeOrigin, float pTreeWidth, int pMaxDivisions)
	{
		RootNode = new Node(pTreeOrigin, pTreeWidth, pMaxDivisions, 1);
	}
	
	public Octree(Vector3 pTreeOrigin, float pTreeWidth, int pMaxDivisions, int pNodeCapacity = 1)
	{
		RootNode = new Node(pTreeOrigin, pTreeWidth, pMaxDivisions, pNodeCapacity);
	}

	public void Insert(T pItem, Vector3 pPosition)
	{
		RootNode.Insert(pItem, pPosition, ref Nodes);
	}

	public void Insert(T pItem, (float, float, float) pPosition)
	{
		Vector3 pos = new Vector3(pPosition.Item1, pPosition.Item2, pPosition.Item3);
		Insert(pItem, pos);
	}

	public void Insert(T pItem, (int, int, int) pPosition)
	{
		Vector3 pos = new Vector3(pPosition.Item1, pPosition.Item2, pPosition.Item3);
		Insert(pItem, pos);
	}

	public List<T> GetDataInsideArea(Bounds pArea)
	{
		List<T> dataInsideArea = new List<T>();
		RootNode.QueryData(pArea, ref dataInsideArea);
		return dataInsideArea;
	}
	
	public List<T> GetDataInsideArea(Vector3 pAreaCenter, Vector3 pAreaRadius)
	{
		Bounds area = new Bounds(pAreaCenter, pAreaRadius);
		return GetDataInsideArea(area);
	}
	
	public List<T> GetDataInsideArea((int, int, int) pAreaCenter, (int, int, int) pAreaRadius)
	{
		Vector3 pos = new Vector3(pAreaCenter.Item1, pAreaCenter.Item2, pAreaCenter.Item3);
		Vector3 size = new Vector3(pAreaRadius.Item1, pAreaRadius.Item2, pAreaRadius.Item3);
		Bounds area = new Bounds(pos, size);
		return GetDataInsideArea(area);
	}

	private class Node
	{
		private readonly struct DataPoint
		{
			public DataPoint(T pData, Vector3 pPoint)
			{
				Data = pData;
				Point = pPoint;
			}

			public T Data { get; }
			public Vector3 Point { get; }
		}
		
		private readonly Vector3 Position;
		private readonly float Width;
		private readonly int Capacity;
		private readonly int CurrentDivision;
		private readonly List<DataPoint> DataPoints = new List<DataPoint>();
		private Node[] SubNodes;
		private bool IsDivided = false;
		public bool IsEmpty => DataPoints.Count <= 0;

		public Node(Vector3 pPosition, float pRootWidth, int pCurrentDivision, int pNodeCapacity)
		{
			Width = pRootWidth * 0.5f;
			Position = pPosition;
			CurrentDivision = pCurrentDivision;
			Capacity = pNodeCapacity;
		}

		public void Insert(T pData, Vector3 pPoint, ref List<T> pNodes)
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

		private bool Contains(Vector3 pPoint)
		{
			return  pPoint.x >= Position.x - Width &&
			        pPoint.x <= Position.x + Width &&
			        pPoint.y >= Position.y - Width &&
			        pPoint.y <= Position.y + Width &&
			        pPoint.z >= Position.z - Width &&
			        pPoint.z <= Position.z + Width;
		}
		
		private void Subdivide()
		{
			Vector3[] newBoxPositions = 
			{
				new Vector3(Position.x + Width * 0.5f, Position.y + Width * 0.5f, Position.z + Width * 0.5f),
				new Vector3(Position.x + Width * 0.5f, Position.y - Width * 0.5f, Position.z - Width * 0.5f),
				new Vector3(Position.x + Width * 0.5f, Position.y + Width * 0.5f, Position.z - Width * 0.5f),
				new Vector3(Position.x + Width * 0.5f, Position.y - Width * 0.5f, Position.z + Width * 0.5f),
				new Vector3(Position.x - Width * 0.5f, Position.y + Width * 0.5f, Position.z + Width * 0.5f),
				new Vector3(Position.x - Width * 0.5f, Position.y - Width * 0.5f, Position.z + Width * 0.5f),
				new Vector3(Position.x - Width * 0.5f, Position.y + Width * 0.5f, Position.z - Width * 0.5f),
				new Vector3(Position.x - Width * 0.5f, Position.y - Width * 0.5f, Position.z - Width * 0.5f),
			};
			
			SubNodes = new Node[8];

			for (int i = 0; i < newBoxPositions.Length; i++)
			{
				SubNodes[i] = new Node(newBoxPositions[i], Width, CurrentDivision-1, Capacity);
			}

			
			IsDivided = true;
		}

		public void DebugGetPositionsAndWidths(ref List<Vector3> pPositions, ref List<float> pWidths, ref List<Vector3> pPoints)
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
		List<Vector3> positions = new List<Vector3>();
		List<float> widths = new List<float>();
		List<Vector3> points = new List<Vector3>();
		RootNode.DebugGetPositionsAndWidths(ref positions, ref widths, ref points);

		for (int i = 0; i < positions.Count; i++)
		{
			Gizmos.color = pCubeColor;
			Gizmos.DrawWireCube(positions[i], new Vector3(widths[i]*2, widths[i]*2, widths[i]*2));
		}

		foreach (var point in points)
		{
			Gizmos.color = pPointColor;
			Gizmos.DrawSphere(point, 0.1f);
		}
	}
}
