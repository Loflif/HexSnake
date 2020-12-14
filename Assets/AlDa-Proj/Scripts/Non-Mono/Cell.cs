using System;

namespace HexSnake
{
	public enum CellState
	{
		DEFAULT,
		WALL,
		FOOD
	}
	
	public class Cell : IComparable <Cell>
	{
		public Cell(int pR, int pQ)
		{
			r = pR;
			q = pQ;
		}
		
		public Cell[] Neighbours;
		public int r { get; }
		public int q { get; }
		public int s => -r - q;

		public CellState State { get; private set; } = CellState.DEFAULT;

		public bool IsTraversable { get; private set; } = true;
		public bool IsFood { get; private set; }

		public delegate void StateChange(Cell pCell);

		public StateChange StateChanged;
		
		public void ChangeState(CellState pNewState)
		{
			State = pNewState;

			StateChanged?.Invoke(this);
			
			switch (State)
			{
				case CellState.DEFAULT:
					IsTraversable = true;
					IsFood = false;
					break;
				case CellState.WALL:
					IsTraversable = false;
					IsFood = false;
					break;
				case CellState.FOOD:
					IsTraversable = true;
					IsFood = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		public string GetPositionalData()
		{
			return "" + r + "\n" + q + "\n" + s;
		}

		public int CompareTo(Cell pOther)
		{
			return r.CompareTo(pOther.r) + q.CompareTo(pOther.q) + s.CompareTo(pOther.s);
		}

		public bool Equals(Cell pOther)
		{
			return (r == pOther.r) && (q == pOther.q) && (s == pOther.s);
		}
	}
}

