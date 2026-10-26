using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Guard Configuration")]
    public Vector2Int homePos = new Vector2Int(0, 1);
    public int minYThreshold = -1; // Only react to units near the corridor (y >= -1), ignore pressure plate at y = -2
    public int minX = -1;
    public int maxX = 1;

    private Vector2Int currentPos;
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
        currentPos = homePos;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(currentPos);
        }
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
    }

    public void StepTurn()
    {
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);

        // 1. Find the target to mirror / block
        // IMPORTANT: Ignore any unit at y < minYThreshold (e.g. Echo 1 on the pressure plate at y = -2)
        Unit target = null;
        int highestPriority = int.MinValue;

        foreach (Unit u in allUnits)
        {
            if (u == null || !u.gameObject.activeInHierarchy) continue;

            // Only react to intruders near the door / corridor
            if (u.GridPosition.y >= minYThreshold)
            {
                // Echo decoys get highest priority so the live player can slip through in Loop 3!
                int priority = u.isEcho ? 100 : 10;
                // Secondary factor: closer Y row has higher priority
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

            // Face the direction of the target if adjacent
            if (spriteRenderer != null && dirX != 0)
            {
                spriteRenderer.flipX = (dirX > 0);
            }
            return;
        }

        // 3. No target near the corridor: stay at or return to homePos (0, 1)
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
