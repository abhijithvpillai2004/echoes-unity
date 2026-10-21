using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEnemy : MonoBehaviour
{
    [Header("Position Config")]
    public Vector2Int startPos;
    public int attackDamage = 1;
    public int hitCount = 0;
    public int maxHitsAgainstPlayer = 3;
    public int maxEnemyHP = 1;
    public int currentEnemyHP = 1;

    [Header("Patrol Config (Optional)")]
    public bool isRowPatroller = false;
    public int patrolDirection = 1; // 1 = moving right, -1 = moving left
    public int minPatrolX = -4;
    public int maxPatrolX = 3;

    private Vector2Int currentPos;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;

    public bool IsDead => isDead;
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
        isDead = false;
        hitCount = 0;
        currentEnemyHP = maxEnemyHP;
        currentPos = startPos;
        patrolDirection = 1;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
            transform.localScale = Vector3.one * 0.8f;
        }
        gameObject.SetActive(true);

        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(currentPos);
        }
    }

    public void StepTurn()
    {
        if (isDead) return;

        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        if (allUnits == null || allUnits.Length == 0) return;

        // 1. Check if already adjacent to any target
        Unit adjacentEcho = null;
        Unit adjacentPlayer = null;
        foreach (var u in allUnits)
        {
            if (u == null || !u.gameObject.activeInHierarchy) continue;
            int d = Mathf.Abs(u.GridPosition.x - currentPos.x) + Mathf.Abs(u.GridPosition.y - currentPos.y);
            if (d == 1)
            {
                if (u.isEcho) adjacentEcho = u;
                else adjacentPlayer = u;
            }
        }

        // Prioritize attacking Echo decoy if adjacent, else live player
        if (adjacentEcho != null)
        {
            AttackTarget(adjacentEcho);
            return;
        }
        if (adjacentPlayer != null)
        {
            AttackTarget(adjacentPlayer);
            return;
        }

        // 2. If configured to patrol only in its row alone
        if (isRowPatroller)
        {
            Vector2Int candidate = currentPos + new Vector2Int(patrolDirection, 0);
            if (candidate.x < minPatrolX || candidate.x > maxPatrolX || !IsTileWalkableForEnemy(candidate))
            {
                patrolDirection = -patrolDirection;
                candidate = currentPos + new Vector2Int(patrolDirection, 0);
            }

            if (candidate.x >= minPatrolX && candidate.x <= maxPatrolX && IsTileWalkableForEnemy(candidate))
            {
                bool isBlocked = false;
                CombatEnemy[] otherEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
                foreach (var e in otherEnemies)
                {
                    if (e != null && e != this && !e.IsDead && e.GetGridPosition() == candidate)
                    {
                        isBlocked = true;
                        break;
                    }
                }

                if (!isBlocked)
                {
                    currentPos = candidate;
                    if (GridManager.Instance != null)
                    {
                        transform.position = GridManager.Instance.GridToWorld(currentPos);
                    }
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayEnemyMove();
                    }
                }
            }
            return;
        }

        // 3. Evaluate all reachable targets using BFS pathfinding (for free-roaming combat enemy)
        Unit bestTarget = null;
        List<Vector2Int> bestPath = null;
        int bestScore = int.MaxValue;

        foreach (var u in allUnits)
        {
            if (u == null || !u.gameObject.activeInHierarchy) continue;
            List<Vector2Int> path = FindPath(currentPos, u.GridPosition);
            if (path == null || path.Count == 0) continue;

            // Score: shorter path is prioritized; Echo decoys act as strong sacrificial lures
            int score = path.Count * 2 - (u.isEcho ? 6 : 0);

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = u;
                bestPath = path;
            }
        }

        if (bestTarget == null || bestPath == null || bestPath.Count == 0) return;

        // 4. Take 1 step along the shortest path towards the chosen target
        Vector2Int nextStep = bestPath[0];

        // Check if nextStep is occupied by another non-dead enemy
        bool occupiedByOtherEnemy = false;
        CombatEnemy[] allCombatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        foreach (var e in allCombatEnemies)
        {
            if (e != null && e != this && !e.IsDead && e.GetGridPosition() == nextStep)
            {
                occupiedByOtherEnemy = true;
                break;
            }
        }

        if (!occupiedByOtherEnemy)
        {
            currentPos = nextStep;
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(currentPos);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyMove();
            }
        }
    }

    private bool IsTileWalkableForEnemy(Vector2Int cell)
    {
        if (GridManager.Instance == null || !GridManager.Instance.IsWalkable(cell))
            return false;

        // Enemies cannot enter tiles occupied by active, uncollected weapons (potions are walkable)
        CollectibleItem[] items = Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        foreach (var it in items)
        {
            if (it != null && it.gameObject.activeInHierarchy && !it.IsCollected && it.itemType == ItemType.Weapon && it.gridPos == cell)
            {
                return false;
            }
        }

        return true;
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (start == goal) return new List<Vector2Int>();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        bool directPathFound = false;
        Vector2Int closestTile = start;
        int minDistanceToGoal = Mathf.Abs(start.x - goal.x) + Mathf.Abs(start.y - goal.y);

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            if (curr == goal)
            {
                directPathFound = true;
                break;
            }

            int distToGoal = Mathf.Abs(curr.x - goal.x) + Mathf.Abs(curr.y - goal.y);
            if (distToGoal < minDistanceToGoal)
            {
                minDistanceToGoal = distToGoal;
                closestTile = curr;
            }

            foreach (var dir in dirs)
            {
                Vector2Int next = curr + dir;
                if (visited.Contains(next)) continue;

                // Target goal is allowed even if occupied by target unit; otherwise tile must be walkable and clear of items
                if (next == goal || IsTileWalkableForEnemy(next))
                {
                    visited.Add(next);
                    cameFrom[next] = curr;
                    queue.Enqueue(next);
                }
            }
        }

        Vector2Int endNode = directPathFound ? goal : closestTile;
        if (endNode == start) return new List<Vector2Int>();

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = endNode;
        while (step != start && cameFrom.ContainsKey(step))
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Reverse();
        return path;
    }

    private void AttackTarget(Unit target)
    {
        hitCount++;
        if (target != null && !target.isEcho)
        {
            Debug.Log($"<color=red>Enemy attacked live player! (Hit {hitCount}/{maxHitsAgainstPlayer})</color>");
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(attackDamage);
            }
        }
        else
        {
            Debug.Log($"<color=yellow>Enemy is distracted, relentlessly attacking the Echo decoy!</color>");
        }

        StartCoroutine(AttackLungeAnimation());
    }

    private IEnumerator AttackLungeAnimation()
    {
        Vector3 orig = transform.position;
        transform.localScale = Vector3.one * 1.05f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = Vector3.one * 0.8f;
    }

    public void Die()
    {
        if (isDead) return;
        currentEnemyHP--;
        if (currentEnemyHP <= 0)
        {
            isDead = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyDeath();
            }
            Debug.Log("<color=green>Enemy was slain by the player's weapon!</color>");
            StartCoroutine(DeathFadeOut());
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossHit();
            }
            Debug.Log($"<color=yellow>Boss took a hit! HP remaining: {currentEnemyHP}/{maxEnemyHP}</color>");
            StartCoroutine(DamageFlash());
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator DeathFadeOut()
    {
        float elapsed = 0f;
        float duration = 0.75f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = c;
            }
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
