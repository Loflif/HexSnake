using System.Collections.Generic;
using UnityEngine;

namespace HexSnake
{
	public class Snake
	{
		public Map Map { get; private set; }
		private Cell Food = null;
		
		private LinkedList<Cell> Body = new LinkedList<Cell>();
		private List<Cell> Path = new List<Cell>();

		public bool IsAlive { get; private set; } = true;
		public bool AllowedNewPath = true;
		
		public delegate void foodEaten(Snake pEater);
		public foodEaten FoodEaten;
		
		public Snake(Map pMap, Cell pStart)
		{
			Map = pMap;
			TakeNewCell(pStart);
		}

		public void ShowFood(Cell pFood)
		{
			if(Food != null)
				Food.StateChanged -= FoodWasEaten;
			Food = pFood;
			Food.StateChanged += FoodWasEaten;
			AllowedNewPath = true;
		}

		public void Move()
		{
			if (!IsAlive)
				return;

			Cell next;
			
			if(AllowedNewPath)
				Path = Pathfinder.FindPath(Body.First, Food, Map);
			if (Path.Count > 0)
			{
				next = Path[0];
				Path.RemoveAt(0);
			}
			else
			{
				next = GetRandomCell();
			}
			
			TakeNewCell(next);
		}

		private void TakeNewCell(Cell pCell)
		{
			Body.Push(pCell);
			
			if (!pCell.IsTraversable)
				Die();

			if (pCell.IsFood)
			{
				// FoodWasEaten(pCell);
			}
			else if(Body.Count > 1)
			{
				Body.Last.ChangeState(CellState.DEFAULT);
				Body.Pop();
			}
			
			pCell.ChangeState(CellState.WALL);
		}
		
		private void FoodWasEaten(Cell pFood)
		{
			FoodEaten?.Invoke(this);
		}

		private void Die()
		{
			IsAlive = false;
		}

		private Cell GetRandomCell()
		{
			Cell[] headNeighbours = Body.First.Neighbours;
			int neighbourCount = headNeighbours.Length;
			int randomNeighbourIndex = Random.Range(0, headNeighbours.Length);
			Cell newCell = headNeighbours[0];
			for (int i = randomNeighbourIndex; i < randomNeighbourIndex + neighbourCount; i++)
			{
				newCell = headNeighbours[i % neighbourCount];
				if (newCell.IsTraversable)
					return newCell;
			}
			return newCell;
		}
	}   
}
