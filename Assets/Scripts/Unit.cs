using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool isEcho = false;
    [SerializeField] private Vector2Int startGridPosition;
    [SerializeField] private Vector2Int gridPosition;

    public Vector2Int StartGridPosition { get => startGridPosition; set => startGridPosition = value; }
    public Vector2Int GridPosition => gridPosition;

    private void Awake()
    {
        gridPosition = startGridPosition;
    }

    public void SetStartGridPosition(Vector2Int newStart)
    {
        startGridPosition = newStart;
        gridPosition = newStart;
        SnapToGrid(gridPosition);
    }

    public void ResetUnit()
    {
        gridPosition = startGridPosition;
        SnapToGrid(gridPosition);
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        SnapToGrid(gridPosition);
    }

    public void SnapToGrid()
    {
        SnapToGrid(gridPosition);
    }

    public void SnapToGrid(Vector2Int targetPos)
    {
        gridPosition = targetPos;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(gridPosition);
        }
    }
}