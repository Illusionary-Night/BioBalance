using UnityEngine;
using System.Collections.Generic;
using System;

public static class AStar
{
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(1, 0),   // Right
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(1, 1),   // Up-Right
        new Vector2Int(1, -1),  // Down-Right
        new Vector2Int(-1, -1), // Down-Left
        new Vector2Int(-1, 1)   // Up-Left
    };

    // �w���p�⪺�﨤�u�Z������
    private const float DiagonalCost = 1.4142136f; // sqrt(2)
    private const float StraightCost = 1f;

    // ���Ϊ����X�H��� GC ���O
    private static readonly BinaryHeapPriorityQueue<Node> OpenSet = new BinaryHeapPriorityQueue<Node>();
    private static readonly HashSet<Vector2Int> ClosedSet = new HashSet<Vector2Int>();
    private static readonly Dictionary<Vector2Int, Node> AllNodes = new Dictionary<Vector2Int, Node>();
    private static readonly List<Vector2Int> PathBuffer = new List<Vector2Int>();

    // ���Ҽ{�a���v��
    public static List<Vector2Int> FindPath(Vector2Int start)
    {
        return FindPath(start, goal, null);
    }

    // �䴩�a���v��
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, 
        System.Func<Vector2Int, float> getTerrainWeight = null)
    {
        // �M�z���Ϊ����X
        OpenSet.Clear();
        ClosedSet.Clear();
        AllNodes.Clear();
        PathBuffer.Clear();

        var startNode = new Node(start);
        startNode.GCost = 0;
        startNode.HCost = GetDistance(start, goal);
        AllNodes[start] = startNode;
        OpenSet.Enqueue(startNode, startNode.FCost);

        while (OpenSet.Count > 0)
        {
            var currentNode = OpenSet.Dequeue();
            if (currentNode.Position == goal)
                return RetracePath(startNode, currentNode);

            ClosedSet.Add(currentNode.Position);

            foreach (var direction in Directions)
            {
                var neighborPos = currentNode.Position + direction;
                if (ClosedSet.Contains(neighborPos))
                    continue;

                // �p���¦���ʦ����]���u�ι﨤�u�^
                var baseMoveCost = (direction.x != 0 && direction.y != 0) ? DiagonalCost : StraightCost;
                
                // ���Φa���v��
                var terrainWeight = getTerrainWeight?.Invoke(neighborPos) ?? 1f;
                var totalMoveCost = baseMoveCost * terrainWeight;
                
                var tentativeGCost = currentNode.GCost + totalMoveCost;

                if (!AllNodes.TryGetValue(neighborPos, out var neighborNode))
                {
                    neighborNode = new Node(neighborPos)
                    {
                        Parent = currentNode,
                        GCost = tentativeGCost,
                        HCost = GetDistance(neighborPos, goal)
                    };
                    AllNodes[neighborPos] = neighborNode;
                    OpenSet.Enqueue(neighborNode, neighborNode.FCost);
                }
                else if (tentativeGCost < neighborNode.GCost)
                {
                    neighborNode.GCost = tentativeGCost;
                    neighborNode.Parent = currentNode;
                    OpenSet.UpdatePriority(neighborNode, neighborNode.FCost);
                }
            }
        }
        return null; // No path found
    }

    private static List<Vector2Int> RetracePath(Node startNode, Node goalNode)
    {
        PathBuffer.Clear();
        var currentNode = goalNode;
        while (currentNode != startNode)
        {
            PathBuffer.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        PathBuffer.Reverse();
        
        // ��^�s�� List �H�קK���νw�İϳQ�ק�
        return new List<Vector2Int>(PathBuffer);
    }

    private static float GetDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        
        // �u�ƪ��﨤�u�Z���p��
        int min = Mathf.Min(dx, dy);
        int max = Mathf.Max(dx, dy);
        return DiagonalCost * min + StraightCost * (max - min);
    }
}

// ���į઺�G����n�u����C��@
public class BinaryHeapPriorityQueue<T> where T : class
{
    private readonly List<(T item, float priority)> heap = new List<(T, float)>();
    private readonly Dictionary<T, int> itemToIndex = new Dictionary<T, int>();

    public int Count => heap.Count;

    public void Clear()
    {
        heap.Clear();
        itemToIndex.Clear();
    }

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        itemToIndex[item] = heap.Count - 1;
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        var result = heap[0].item;
        itemToIndex.Remove(result);

        // �N�̫�@�Ӥ�������ڳ�
        var lastIndex = heap.Count - 1;
        if (lastIndex > 0)
        {
            heap[0] = heap[lastIndex];
            itemToIndex[heap[0].item] = 0;
        }
        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
            HeapifyDown(0);

        return result;
    }

    public void UpdatePriority(T item, float newPriority)
    {
        if (!itemToIndex.TryGetValue(item, out int index))
            return;

        var oldPriority = heap[index].priority;
        heap[index] = (item, newPriority);

        if (newPriority < oldPriority)
            HeapifyUp(index);
        else if (newPriority > oldPriority)
            HeapifyDown(index);
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (heap[index].priority >= heap[parentIndex].priority)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < heap.Count && heap[leftChild].priority < heap[smallest].priority)
                smallest = leftChild;

            if (rightChild < heap.Count && heap[rightChild].priority < heap[smallest].priority)
                smallest = rightChild;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;

        itemToIndex[heap[i].item] = i;
        itemToIndex[heap[j].item] = j;
    }
}

public class Node
{
    public Vector2Int Position { get; }
    public Node Parent { get; set; }
    public float GCost { get; set; } // �q�_�I���e�`�I������
    public float HCost { get; set; } // �q��e�`�I��ؼи`�I���w������
    public float FCost => GCost + HCost; // �`����
    
    public Node(Vector2Int position)
    {
        Position = position;
    }
}