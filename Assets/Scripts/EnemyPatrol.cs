using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Mode")]
    public bool isSentinel = false;

    [Header("Sentinel / Blocker Mode (Level 2)")]
    public Vector2Int homePos = new Vector2Int(0, 1);
    public int minYThreshold = -1; // Only react to units near the corridor (y >= -1)

    [Header("Patrol Boundaries (Grid)")]
    public Vector2Int startPos = new Vector2Int(0, 0);
    public int startDirection = -1; // -1 = Left, 1 = Right
    public int minX = -2;
    public int maxX = 2;

    private Vector2Int currentPos;
    private int currentDir = -1;
    private SpriteRenderer spriteRenderer;

    public Vector2Int GetGridPosition() => currentPos;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ResetToStart();
    }

    public void ResetToStart()
    {
        currentPos = isSentinel ? homePos : startPos;
        currentDir = startDirection;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(currentPos);
        }
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (currentDir > 0);
        }
    }

    public void StepTurn()
    {
        if (isSentinel)
        {
            StepSentinel();
        }
        else
        {
            StepPatrol();
        }
    }

    private void StepSentinel()
    {
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);

        // 1. Find the target to mirror / block
        Unit target = null;
        int highestPriority = int.MinValue;

        foreach (Unit u in allUnits)
        {
            if (u == null || !u.gameObject.activeInHierarchy) continue;

            if (u.GridPosition.y >= minYThreshold)
            {
                int priority = u.isEcho ? 100 : 10;
                priority += u.GridPosition.y;

                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    target = u;
                }
            }
        }

        // 2. Mirror target's X position to block them!
        if (target != null)
        {
            int targetX = Mathf.Clamp(target.GridPosition.x, minX, maxX);
            int dirX = System.Math.Sign(targetX - currentPos.x);

            if (dirX != 0)
            {
                Vector2Int candidate = currentPos + new Vector2Int(dirX, 0);
                if (IsTileWalkableAndFree(candidate, allUnits))
                {
                    currentPos = candidate;
                    UpdatePosition();
                    if (spriteRenderer != null) spriteRenderer.flipX = (dirX > 0);
                    return;
                }
            }

            if (spriteRenderer != null && dirX != 0)
            {
                spriteRenderer.flipX = (dirX > 0);
            }
            return;
        }

        // 3. No target near the corridor: stay at or return to homePos
        if (currentPos != homePos)
        {
            int dirToHome = System.Math.Sign(homePos.x - currentPos.x);
            Vector2Int candidate = currentPos + new Vector2Int(dirToHome, 0);
            if (IsTileWalkableAndFree(candidate, allUnits))
            {
                currentPos = candidate;
                UpdatePosition();
                if (spriteRenderer != null && dirToHome != 0)
                {
                    spriteRenderer.flipX = (dirToHome > 0);
                }
            }
        }
    }

    private void StepPatrol()
    {
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);

        Vector2Int nextPos = currentPos + new Vector2Int(currentDir, 0);

        if (nextPos.x < minX || nextPos.x > maxX)
        {
            currentDir *= -1;
            nextPos = currentPos + new Vector2Int(currentDir, 0);
        }

        if (IsTileWalkableAndFree(nextPos, allUnits))
        {
            currentPos = nextPos;
            UpdatePosition();
            if (spriteRenderer != null) spriteRenderer.flipX = (currentDir > 0);
        }
        else
        {
            // Bounced by unit or obstacle
            currentDir *= -1;
            if (spriteRenderer != null) spriteRenderer.flipX = (currentDir > 0);
        }
    }

    private bool IsTileWalkableAndFree(Vector2Int pos, Unit[] units)
    {
        if (GridManager.Instance != null && !GridManager.Instance.IsWalkable(pos))
            return false;

        if (units != null)
        {
            foreach (Unit u in units)
            {
                if (u != null && u.gameObject.activeInHierarchy && u.GridPosition == pos)
                    return false;
            }
        }
        return true;
    }

    private void UpdatePosition()
    {
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(currentPos);
        }
    }
}
