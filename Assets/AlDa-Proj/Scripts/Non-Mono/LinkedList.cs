public class LinkedList<T>
{
	private Node Head = null;
	private Node Tail = null;

	public T First => Head.Data;
	public T Last => Tail.Data;

	public int Count { get; private set; } = 0;
	
	public void Push(T pData)
	{
		Node newNode = new Node(pData);

		if (Head == null)
		{
			Head = newNode;
			Tail = newNode;
		}
		else
		{
			Head.Next = newNode;
			newNode.Previous = Head;
			Head = newNode;
			Head.Next = null;
		}
		
		Count++;
	}

	public void Pop()
	{
		if (Head == null)
			return;
		
		if (Head != Tail)
		{
			Tail = Tail.Next;
			Tail.Previous = null;
		}
		else
		{
			Head = Tail = null;
		}
		
		Count--;
	}
	
	private class Node
	{
		public Node(T pData)
		{
			Data = pData;
		}
		
		public T Data;
		public Node Next = null;
		public Node Previous = null;
	}
}
