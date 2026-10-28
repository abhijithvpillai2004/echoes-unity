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
    public bool isAlerted = false;
    [Header("Shield Config")]
    public bool isShielded = false;
    private static readonly Color ShieldColor = new Color(0.4f, 0.85f, 1f, 1f); // Magic cyan
    private static readonly Color NormalColor = Color.white;
    private static float lastShieldDownSFXTime = -10f;

    private GameObject shieldVisual;
    private SpriteRenderer barrierBubbleRenderer;

    private Vector2Int currentPos;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;

    public bool IsDead => isDead;
    public Vector2Int GetGridPosition() => currentPos;

    public void SetShielded(bool shielded)
    {
        if (isDead) return;
        bool wasShielded = isShielded;
        isShielded = shielded;
        EnsureShieldVisual();

        if (wasShielded && !shielded)
        {
            // Trigger shield shatter / glass scattering animation
            PlayShieldShatter();
        }
        else
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(isShielded);
            }
        }

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isShielded ? ShieldColor : NormalColor;
        }
    }

    private void PlayShieldShatter()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ShieldShatterRoutine());
        }
        else
        {
            if (shieldVisual != null) shieldVisual.SetActive(false);
        }
    }

    private System.Collections.IEnumerator ShieldShatterRoutine()
    {
        // 1. Brief soft flash on the honeycomb barrier before it dissolves
        if (barrierBubbleRenderer != null)
        {
            barrierBubbleRenderer.color = new Color(1.3f, 1.3f, 1.3f, 1f);
        }
        yield return new WaitForSeconds(0.04f);

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        // 2. Spawn crystal shards that gently float and scatter outward
        GameObject fxParent = new GameObject("ShieldShatterFX");
        fxParent.transform.position = transform.position;

        // Play shield down SFX (debounced so simultaneous guards don't stack volume)
        if (Time.time - lastShieldDownSFXTime > 0.15f)
        {
            lastShieldDownSFXTime = Time.time;
            AudioClip sfxClip = Resources.Load<AudioClip>("Audio/SFX/SFX_ShieldDown");
            if (sfxClip != null)
            {
                AudioSource.PlayClipAtPoint(sfxClip, transform.position, 0.85f);
            }
        }

        Sprite shardSprite = Resources.Load<Sprite>("shield_shard");
        int shardCount = 12;
        List<Transform> shards = new List<Transform>();
        List<Vector2> velocities = new List<Vector2>();
        List<float> rotSpeeds = new List<float>();
        List<Vector3> initialScales = new List<Vector3>();

        for (int i = 0; i < shardCount; i++)
        {
            GameObject s = new GameObject("Shard_" + i);
            s.transform.SetParent(fxParent.transform, false);

            float startAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float startDist = Random.Range(0.03f, 0.20f);
            s.transform.localPosition = new Vector3(Mathf.Cos(startAngle) * startDist, Mathf.Sin(startAngle) * startDist, 0f);

            Vector3 initScale = Vector3.one * Random.Range(0.22f, 0.38f);
            s.transform.localScale = initScale;
            initialScales.Add(initScale);

            SpriteRenderer sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = shardSprite;
            sr.sortingOrder = 14;
            sr.color = new Color(0.25f, 0.85f, 1f, 0.95f);

            // Gentle, smooth outward drift
            float speed = Random.Range(0.6f, 1.3f);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            velocities.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed);
            rotSpeeds.Add(Random.Range(-120f, 120f));
            shards.Add(s.transform);
        }

        // 3. Smoothly animate scattering shards: gentle drift, subtle damping, and soft fade
        float duration = 0.60f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth ease-out fade
            float currentAlpha = Mathf.SmoothStep(0.95f, 0f, t);

            for (int i = 0; i < shards.Count; i++)
            {
                if (shards[i] == null) continue;
                Vector3 pos = shards[i].localPosition;
                Vector2 vel = velocities[i];

                // Gentle air resistance / damping for smooth floating motion
                vel *= Mathf.Clamp01(1f - 0.8f * Time.deltaTime);
                velocities[i] = vel;

                pos += (Vector3)vel * Time.deltaTime;
                shards[i].localPosition = pos;
                shards[i].Rotate(0f, 0f, rotSpeeds[i] * Time.deltaTime);
                shards[i].localScale = initialScales[i] * Mathf.Lerp(1f, 0.45f, t);

                SpriteRenderer sr = shards[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = currentAlpha;
                    sr.color = c;
                }
            }
            yield return null;
        }

        Destroy(fxParent);
    }

    private void EnsureShieldVisual()
    {
        if (shieldVisual == null)
        {
            Transform existing = transform.Find("ShieldVisual");
            if (existing != null) shieldVisual = existing.gameObject;
        }

        if (shieldVisual == null)
        {
            shieldVisual = new GameObject("ShieldVisual");
            shieldVisual.transform.SetParent(transform, false);
            shieldVisual.transform.localPosition = Vector3.zero;

            // Honeycomb Forcefield enclosing the body
            GameObject bubble = new GameObject("BarrierBubble");
            bubble.transform.SetParent(shieldVisual.transform, false);
            bubble.transform.localPosition = Vector3.zero;
            bubble.transform.localScale = Vector3.one * 1.0f;

            barrierBubbleRenderer = bubble.AddComponent<SpriteRenderer>();
            Sprite wardSp = Resources.Load<Sprite>("honeycomb_shield");
            if (wardSp == null)
            {
                Sprite[] allSp = Resources.LoadAll<Sprite>("honeycomb_shield");
                if (allSp != null && allSp.Length > 0) wardSp = allSp[0];
            }
            if (wardSp == null) wardSp = Resources.Load<Sprite>("ward_diamond");
            barrierBubbleRenderer.sprite = wardSp;
            barrierBubbleRenderer.color = Color.white;
            barrierBubbleRenderer.sortingOrder = 12;
        }

        // Clean up any old ShieldIcon if present from previous iterations
        if (shieldVisual != null)
        {
            Transform oldIcon = shieldVisual.transform.Find("ShieldIcon");
            if (oldIcon != null)
            {
                Destroy(oldIcon.gameObject);
            }
        }

        if (barrierBubbleRenderer == null && shieldVisual != null)
        {
            barrierBubbleRenderer = shieldVisual.transform.Find("BarrierBubble")?.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (isShielded && shieldVisual != null && shieldVisual.activeSelf && !isDead)
        {
            // Distinct energy breathing & humming pulse on the honeycomb barrier
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * 3.0f);
            float shimmer = Mathf.Sin(Time.time * 12.0f) * 0.04f;

            // Modulate brightness from calm energy (0.72x) to radiant electric surge (1.35x)
            float glow = Mathf.Lerp(0.72f, 1.35f, wave) + shimmer;
            float alpha = Mathf.Lerp(0.65f, 1.00f, wave);

            // Rich cyan surge at peak brightness
            float r = glow * 0.88f;
            float g = glow * 1.05f;
            float b = glow * 1.22f;

            if (barrierBubbleRenderer != null)
            {
                barrierBubbleRenderer.color = new Color(r, g, b, alpha);
                barrierBubbleRenderer.transform.localScale = Vector3.one;
            }
        }
    }

    public void OnShieldDeflect()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FlashShieldDeflect());
        }
    }

    private System.Collections.IEnumerator FlashShieldDeflect()
    {
        EnsureShieldVisual();
        if (barrierBubbleRenderer != null)
        {
            barrierBubbleRenderer.color = new Color(1.5f, 1.5f, 1.5f, 1f);
            barrierBubbleRenderer.transform.localScale = Vector3.one;
        }
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(0.14f);

        if (barrierBubbleRenderer != null)
        {
            barrierBubbleRenderer.color = Color.white;
            barrierBubbleRenderer.transform.localScale = Vector3.one * 1.0f;
        }
        if (spriteRenderer != null && !isDead)
        {
            spriteRenderer.color = isShielded ? ShieldColor : NormalColor;
        }
    }

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
        StopAllCoroutines();
        isDead = false;
        isAlerted = false;
        isShielded = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
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

    public void Alert()
    {
        if (isDead) return;
        isAlerted = true;
    }

    public static void AlertAll()
    {
        CombatEnemy[] all = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        foreach (var e in all)
        {
            if (e != null) e.Alert();
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
            if (isRowPatroller && !isAlerted && u.GridPosition.y != currentPos.y) continue;
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

        // 2. If configured to patrol only in its row alone (and not currently alerted)
        if (isRowPatroller && !isAlerted)
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
                    PlaySlimeMoveSound();
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

            // Score: shorter path is prioritized; when alerted, strongly prioritize live armed player
            int score = path.Count * 2 - (u.isEcho ? 6 : 0);
            if (isAlerted && !u.isEcho)
            {
                score -= 30; // Strongly chase the live armed player!
            }

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
            PlaySlimeMoveSound();
        }
    }

    private static AudioClip[] slimeMoveClips = null;
    private static float lastSlimeMoveSoundTime = -10f;

    private void PlaySlimeMoveSound()
    {
        if (slimeMoveClips == null || slimeMoveClips.Length == 0)
        {
            slimeMoveClips = new AudioClip[] {
                Resources.Load<AudioClip>("Audio/SFX/SFX_SlimeMove_1"),
                Resources.Load<AudioClip>("Audio/SFX/SFX_SlimeMove_2"),
                Resources.Load<AudioClip>("Audio/SFX/SFX_SlimeMove_3"),
                Resources.Load<AudioClip>("Audio/SFX/SFX_SlimeMove_4")
            };
        }

        if (Time.time - lastSlimeMoveSoundTime > 0.08f)
        {
            lastSlimeMoveSoundTime = Time.time;
            int rIdx = Random.Range(0, slimeMoveClips.Length);
            AudioClip clip = slimeMoveClips[rIdx];
            if (clip == null) clip = Resources.Load<AudioClip>("Audio/SFX/SFX_SlimeMove");
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, 0.70f);
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
            if (shieldVisual != null) shieldVisual.SetActive(false);
            Debug.Log("<color=green>Enemy was slain by the player's weapon!</color>");
            StartCoroutine(DeathFadeOut());
        }
        else
        {
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
