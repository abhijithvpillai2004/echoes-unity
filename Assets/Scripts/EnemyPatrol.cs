using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Boundaries (Grid)")]
    public int minX = -2;
    public int maxX = 0;
    public int yRow = 1;

    [Header("Start Configuration")]
    public Vector2Int startPos = new Vector2Int(0, 1);
    public int startDirection = -1; // -1 = Left, 1 = Right

    private Vector2Int currentPos;
    private int currentDir;

    private void Start()
    {
        ResetToStart();
    }

    public void ResetToStart()
    {
        currentPos = startPos;
        currentDir = startDirection;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(currentPos);
        }
    }

    public Vector2Int GetGridPosition()
    {
        return currentPos;
    }

    public void StepTurn()
    {
        Vector2Int nextPos = currentPos + new Vector2Int(currentDir, 0);

        if (nextPos.x < minX || nextPos.x > maxX)
        {
            currentDir *= -1;
            nextPos = currentPos + new Vector2Int(currentDir, 0);
        }

        Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        bool blocked = false;
        foreach (Unit u in units)
        {
            if (u != null && u.GridPosition == nextPos)
            {
                blocked = true;
                break;
            }
        }

        if (!blocked && GridManager.Instance != null && !GridManager.Instance.IsWalkable(nextPos))
        {
            blocked = true;
        }

        if (blocked)
        {
            currentDir *= -1;
            return;
        }

        currentPos = nextPos;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(currentPos);
        }
    }
}
