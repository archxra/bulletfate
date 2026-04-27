using System.Collections.Generic;
using UnityEngine;

public class AStar2D : MonoBehaviour
{
    public static AStar2D Instance;

    [Header("=== РАЗМЕР КАРТЫ ===")]
    [Tooltip("Гарантированный размер сетки (даже если нет стен)")]
    public Vector2 minMapSize = new Vector2(50f, 50f);

    [Header("=== ТОЧНОСТЬ ===")]
    public float nodeRadius = 0.25f; // 0.25 - идеально для кубиков
    public float wallBuffer = 0.3f;  // Отступ от стен
    public LayerMask obstacleLayer;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    private Vector3 worldBottomLeft;

    void Awake()
    {
        Instance = this;
        nodeDiameter = nodeRadius * 2;
        CalculateMapBounds();
        GenerateGrid();
    }

    // 1. ВЫЧИСЛЯЕМ ГРАНИЦЫ
    void CalculateMapBounds()
    {
        Bounds bounds = new Bounds(transform.position, minMapSize); // Базовый размер (50х50)

        // Ищем все стены. Если они выходят за 50х50 - расширяем карту!
        Collider2D[] allWalls = FindObjectsOfType<Collider2D>();
        foreach (var wall in allWalls)
        {
            if (((1 << wall.gameObject.layer) & obstacleLayer) != 0)
            {
                bounds.Encapsulate(wall.bounds);
            }
        }

        // Центрируем Мозг по найденным границам
        transform.position = bounds.center;

        // Считаем количество клеток
        gridSizeX = Mathf.RoundToInt(bounds.size.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(bounds.size.y / nodeDiameter);

        // Левый нижний угол карты
        worldBottomLeft = transform.position - Vector3.right * bounds.size.x / 2 - Vector3.up * bounds.size.y / 2;
    }

    // 2. СТРОИМ СЕТКУ
    public void GenerateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

                // Проверка на стену
                bool isWalkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius + wallBuffer, obstacleLayer));

                grid[x, y] = new Node(isWalkable, worldPoint, x, y);
            }
        }
        Debug.Log($"[AStar] Карта построена: {gridSizeX}x{gridSizeY} клеток.");
    }

    // 3. ПОИСК ПУТИ
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);

        // Если цель за картой или в стене - пути нет
        if (startNode == null || targetNode == null || !targetNode.walkable) return null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost || (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode) return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                int moveCost = current.gCost + GetDistance(current, neighbor);
                if (moveCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = moveCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    List<Vector3> RetracePath(Node start, Node end)
    {
        List<Vector3> path = new List<Vector3>();
        Node curr = end;
        while (curr != start) { path.Add(curr.worldPosition); curr = curr.parent; }
        path.Reverse();
        return path;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return (dx > dy) ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
    }

    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x - worldBottomLeft.x) / (gridSizeX * nodeDiameter);
        float percentY = (worldPos.y - worldBottomLeft.y) / (gridSizeY * nodeDiameter);

        if (percentX < 0 || percentX > 1 || percentY < 0 || percentY > 1) return null;

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    List<Node> GetNeighbors(Node n)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int cx = n.gridX + x; int cy = n.gridY + y;
                if (cx >= 0 && cx < gridSizeX && cy >= 0 && cy < gridSizeY) neighbors.Add(grid[cx, cy]);
            }
        }
        return neighbors;
    }

    // РИСУЕМ КАРТУ В РЕДАКТОРЕ
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, minMapSize); // Показываем базовый размер

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? new Color(1, 1, 1, 0.05f) : new Color(1, 0, 0, 0.4f);
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
            }
        }
    }

    public class Node
    {
        public bool walkable; public Vector3 worldPosition; public int gridX, gridY, gCost, hCost; public Node parent;
        public Node(bool w, Vector3 p, int x, int y) { walkable = w; worldPosition = p; gridX = x; gridY = y; }
        public int fCost => gCost + hCost;
    }
}