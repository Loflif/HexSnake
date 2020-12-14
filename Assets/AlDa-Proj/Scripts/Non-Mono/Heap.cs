using System;
using System.Collections;
using System.Collections.Generic;

public class Heap<T> : IEnumerable<T> where T : IHeapItem<T>
{
	private T[] Items;
	
	public int Count { get; private set; } = 0;

	public Heap(int pMaxHeapSize)
	{
		Items = new T[pMaxHeapSize];
	}

	public void Add(T pItem)
	{
		pItem.Index = Count;
		Items[Count] = pItem;
		SortUp(pItem);
		Count++;
	}

	public T PopFirst()
	{
		T firstItem = Items[0];
		Count--;
		Items[0] = Items[Count];
		Items[0].Index = 0;
		SortDown(Items[0]);
		return firstItem;
	}

	public bool Contains(T pItem)
	{
		return Equals(Items[pItem.Index], pItem);
	}

	public void UpdateItem(T pItem)
	{
		SortUp(pItem);
		SortDown(pItem);
	}

	private void SortUp(T pItem)
	{
		while (true)
		{
			int parentIndex = (pItem.Index - 1) / 2;
			T parentItem = Items[parentIndex];

			if (pItem.CompareTo(parentItem) > 0)
				Swap(pItem, parentItem);
			else
				return;
		}
	}
	
	void SortDown(T item)
	{
		while (true)
		{
			int leftChildIndex = item.Index * 2 + 1;
			int rightChildIndex = item.Index * 2 + 2;

			if (leftChildIndex < Count)
			{
				var swapIndex = leftChildIndex;

				if (rightChildIndex < Count)
				{
					if (Items[leftChildIndex].CompareTo(Items[rightChildIndex]) < 0)
					{
						swapIndex = rightChildIndex;
					}
				}

				if (item.CompareTo(Items[swapIndex]) < 0)
				{
					Swap (item, Items[swapIndex]);
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}
	}

	private void Swap(T lhs, T rhs)
	{
		Items[lhs.Index] = rhs;
		Items[rhs.Index] = lhs;

		int tmp = lhs.Index;
		lhs.Index = rhs.Index;
		rhs.Index = tmp;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return Items[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

public interface IHeapItem<in T> : IComparable<T>
{
	int Index { get; set; }
}