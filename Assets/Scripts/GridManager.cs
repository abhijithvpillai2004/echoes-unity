using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance;
    public static GridManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<GridManager>();
                if (_instance != null) _instance.InitializeGrid();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Grid Dimensions")]
    public int width = 8;
    public int height = 8;
    public Vector2Int originOffset = new Vector2Int(-4, -4);
    public float cellSize = 1f;

    [Header("Walls & Obstacles")]
    public List<Vector2Int> wallPositions = new List<Vector2Int>();

    private HashSet<Vector2Int> unwalkableCells = new HashSet<Vector2Int>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            InitializeGrid();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeGrid()
    {
        unwalkableCells.Clear();
        foreach (var pos in wallPositions)
        {
            unwalkableCells.Add(pos);
        }

        PermanentWall[] permWalls = Object.FindObjectsByType<PermanentWall>(FindObjectsSortMode.None);
        foreach (var pw in permWalls)
        {
            if (pw != null && pw.wallCells != null)
            {
                foreach (var cell in pw.wallCells)
                    unwalkableCells.Add(cell);
            }
        }

        ToggleWall[] toggleWalls = Object.FindObjectsByType<ToggleWall>(FindObjectsSortMode.None);
        foreach (var tw in toggleWalls)
        {
            if (tw != null && !tw.IsOpen() && tw.wallCells != null)
            {
                foreach (var cell in tw.wallCells)
                    unwalkableCells.Add(cell);
            }
        }
    }

    public void SetWalkable(Vector2Int cell, bool walkable)
    {
        if (walkable)
            unwalkableCells.Remove(cell);
        else
            unwalkableCells.Add(cell);
    }

    public bool IsWalkable(Vector2Int cell)
    {
        int minX = originOffset.x;
        int maxX = originOffset.x + width - 1;
        int minY = originOffset.y;
        int maxY = originOffset.y + height - 1;

        if (cell.x < minX || cell.x > maxX || cell.y < minY || cell.y > maxY)
            return false;

        return !unwalkableCells.Contains(cell);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize + cellSize * 0.5f, gridPos.y * cellSize + cellSize * 0.5f, 0f);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / cellSize), Mathf.FloorToInt(worldPos.y / cellSize));
    }
}