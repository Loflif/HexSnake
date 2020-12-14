using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace HexSnake
{
	public class GameManager : MonoBehaviour
	{
		[Header("Grid")] 
		
		[Range(1, 40)] [SerializeField] private int MapRadius = 6;

		[Header("Cells")] 
		
		[SerializeField] private Shapes.ShapeRenderer CellRepresentation  ;
		[SerializeField] private Color UnoccupiedColor = new Color(1, 1, 1, 1);
		[SerializeField] private Color SnakeColor = new Color(0, 0, 0, 1);
		[SerializeField] private Color FoodColor = new Color(0, 1, 0, 1);
		[Range(0.0f, 10.0f)] [SerializeField] private float CellRadius = 1.0f;

		[Header("Snake")] 
		
		[SerializeField] private float SnakeMovementDelay = 0.1f;
		[SerializeField] private float SnakePathfindingDelay = 0.15f;

		[Range(0, 20)][SerializeField] private int SnakeCount = 1;

		[Header("Debug")] 
		
		[SerializeField] private bool DrawNeighbourDirections ;
		[SerializeField] private bool WritePositionalData ;
		[SerializeField] private bool DebugPathfinding ;
		[SerializeField] private bool DebugOctree ;
		[SerializeField] private Transform Seeker ;
		[SerializeField] private Transform Goal;

		private Map Map;
		private Transform OwnTransform;

		private Dictionary<(int, int, int), Shapes.ShapeRenderer> VisualTiles 
			= new Dictionary<(int, int, int), Shapes.ShapeRenderer>();

		private void Awake()
		{
			OwnTransform = transform;
			Map = new HexagonMap(MapRadius, CellRadius);
			CreateGridVisualRepresentation(Map);
		}

		private void Start()
		{
			for (int i = 0; i < SnakeCount; i++)
			{
				SpawnSnake(Map);
			}
		}

		private void SpawnSnake(Map pMap)
		{
			Cell randomCell = GetRandomUnoccupiedCell(pMap);
			if (randomCell == null)
				return;
			
			Snake newSnake = new Snake(pMap, randomCell);
			SpawnFood(newSnake);
			newSnake.FoodEaten += SpawnFood;
			StartCoroutine(SnakeMovement(newSnake));
			StartCoroutine(AllowSnakeNewPath(newSnake));
		}

		private void SpawnFood(Snake pEater)
		{
			Cell randomCell = GetRandomUnoccupiedCell(pEater.Map);
			if (randomCell == null)
				return;
			randomCell.ChangeState(CellState.FOOD);
			pEater.ShowFood(randomCell);
		}

		private Cell GetRandomUnoccupiedCell(Map pMap)
		{
			if (pMap == null)
			{
				Debug.LogError("Tried to get random UnoccupiedTile, on a null map!");
				return null;
			}

			List<Cell> mapCells = pMap.Cells.ToList();

			int cellsCount = mapCells.Count;
			int randomIndex = Random.Range(0, cellsCount);

			for (int i = randomIndex; i < cellsCount + randomIndex; i++)
			{
				Cell randomCell = mapCells[i % cellsCount];
				if (randomCell.IsTraversable)
					return randomCell;
			}

			return null;
		}

		private void ChangeCellColor(Cell pCell)
		{
			var cellID = (pCell.r, pCell.q, pCell.s);
			
			Shapes.ShapeRenderer renderer = VisualTiles[(cellID)];
			
			switch (pCell.State)
			{
				case CellState.DEFAULT:
					renderer.Color = UnoccupiedColor;
					break;
				case CellState.WALL:
					renderer.Color = SnakeColor;
					break;
				case CellState.FOOD:
					renderer.Color = FoodColor;
					break;
			}
			
		}

		private void CreateGridVisualRepresentation(Map pMap)
		{
			GameObject gridParent = new GameObject { name = "Grid" };
			gridParent.transform.parent = OwnTransform;
			foreach (Cell c in Map.Cells)
			{
				Vector3 cellPos = OwnTransform.position;
				Vector3 cellWorldPos = pMap.AxisToWorld(c.r, c.q);
				cellPos.x += cellWorldPos.x;
				cellPos.z += cellWorldPos.y;

				var newRenderer = Instantiate(CellRepresentation, cellPos, OwnTransform.rotation, gridParent.transform);
				if (WritePositionalData)
					newRenderer.GetComponentInChildren<TextMeshPro>().text = c.GetPositionalData();

				VisualTiles.Add((c.r, c.q, c.s), newRenderer);
				newRenderer.Color = UnoccupiedColor;
				c.StateChanged += ChangeCellColor;
			}
		}

		private IEnumerator SnakeMovement(Snake pSnake)
		{
			var snakeWait = new WaitForSeconds(SnakeMovementDelay);
			while (pSnake.IsAlive)
			{
				pSnake.Move();
				yield return snakeWait;
			}
		}

		private IEnumerator AllowSnakeNewPath(Snake pSnake)
		{
			var pathWait = new WaitForSeconds(SnakePathfindingDelay);
			while (pSnake.IsAlive)
			{
				pSnake.AllowedNewPath = true;
				yield return pathWait;
			}
		}
		

		private void OnDrawGizmos()
		{
			if (DrawNeighbourDirections)
				DrawNeighbourCones();

			if (DebugPathfinding)
				DrawDebugPath();

			if (DebugOctree)
				Map?.Cells.DebugDrawCubes(Color.red, Color.black);
			
			void DrawNeighbourCones()
			{
				if (Map == null)
					return;

				foreach (Cell c in Map.Cells)
				{
					Vector3 cellPos = Map.AxisToWorld(c.r, c.q);
					Vector3 flippedCell = new Vector3(cellPos.x, cellPos.z, cellPos.y) + OwnTransform.position;
					Cell[] neighbours = c.Neighbours;
					if (neighbours.Length <= 0)
						continue;
					foreach (Cell n in neighbours)
					{
						Vector3 neighbourPos = Map.AxisToWorld(n.r, n.q);
						Vector3 flippedPos = new Vector3(neighbourPos.x, neighbourPos.z, neighbourPos.y) + OwnTransform.position;
						Vector3 cellToNeighbour = (flippedCell - flippedPos).normalized;

						Color neighbourColor = new Color((float) n.r / MapRadius, (float) n.q / MapRadius,
							(float) n.s / MapRadius, 0.5f);

						Shapes.Draw.Color = neighbourColor;
						Shapes.Draw.Cone(flippedPos, Quaternion.LookRotation(cellToNeighbour, Vector3.up),
							CellRadius * 0.2f, CellRadius * 0.7f);
					}
				}
			}

			void DrawDebugPath()
			{
				if (Map == null || Seeker == null || Goal == null)
					return;

				var seekerPos = Seeker.position;
				Vector3 seekerFlipped = new Vector3(seekerPos.x, seekerPos.z, seekerPos.y);
				var goalPos = Goal.position;
				Vector3 goalFlipped = new Vector3(goalPos.x, goalPos.z, goalPos.y);
				
				Cell seekerCell = Map.CellAtLocation(seekerFlipped);
				Cell goalCell = Map.CellAtLocation(goalFlipped);
				
				List<Cell> path = Pathfinder.FindPath(seekerCell, goalCell, Map);
				
				Shapes.PolylinePath polyPath = new Shapes.PolylinePath();
				polyPath.AddPoint(seekerPos);
				Color pathColor = Color.cyan;

				foreach (Cell c in path)
				{
					Vector3 cellPos = Map.AxisToWorld(c.r, c.q);
					Vector3 flippedCellPos = new Vector3(cellPos.x, cellPos.z, cellPos.y) + OwnTransform.position;
					polyPath.AddPoint(flippedCellPos);
					Shapes.Draw.Sphere(flippedCellPos, 0.2f, pathColor);
				}
				polyPath.AddPoint(Goal.position);

				Shapes.Draw.Polyline(polyPath, false, 0.1f, pathColor);
			}
			
		}
	}
}