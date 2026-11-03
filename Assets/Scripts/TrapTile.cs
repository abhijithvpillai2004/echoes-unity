using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A floor spike trap tile that damages units when stepped on while active.
/// If marked as poisonous, features a pulsating toxic green venom sheen,
/// a sharp thrust punch on impact, and a procedural toxic venom droplet splash burst.
/// </summary>
[ExecuteAlways]
public class TrapTile : MonoBehaviour
{
    [Header("Grid Position")]
    public Vector2Int gridPos;
    public int damage = 1;

    [Header("Sprites")]
    public Sprite activeSpikesSprite;   // tilemap_packed_69 (extended)
    public Sprite retractedSpikesSprite; // tilemap_packed_68 (flat/safe)

    [Header("State")]
    public bool isRetracted = false;

    [Header("Poison Visual Settings")]
    public bool isPoisonous = true;
    public Color poisonBaseColor = new Color(0.38f, 0.95f, 0.42f, 1f);
    public Color poisonHighlightColor = new Color(0.70f, 1.0f, 0.65f, 1f);
    public float sheenSpeed = 2.4f;

    private SpriteRenderer spriteRenderer;
    private float sheenTimer = 0f;
    private bool isThrusting = false;
    private Vector3 baseScale = new Vector3(0.46f, 0.46f, 1f);
    private static Sprite cachedDropletSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (transform.localScale != Vector3.zero)
        {
            baseScale = transform.localScale;
        }
        SnapToGrid();
    }

    private void Start()
    {
        ResetTrap();
    }

    public void SnapToGrid()
    {
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(gridPos);
        }
    }

    public void ResetTrap()
    {
        isRetracted = false;
        transform.localScale = baseScale;
        isThrusting = false;
        sheenTimer = Random.Range(0f, Mathf.PI * 2f); // Desynchronize adjacent traps
        UpdateVisual();
    }

    public void SetRetracted(bool retracted)
    {
        isRetracted = retracted;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (isRetracted)
            {
                if (retractedSpikesSprite != null) spriteRenderer.sprite = retractedSpikesSprite;
                spriteRenderer.color = new Color(0.65f, 0.65f, 0.65f, 0.5f);
            }
            else
            {
                if (activeSpikesSprite != null) spriteRenderer.sprite = activeSpikesSprite;
                spriteRenderer.color = isPoisonous ? poisonBaseColor : Color.white;
            }
        }
    }

    private void Update()
    {
        if (isRetracted) return;

        // 1. Poisonous Sheen Animation
        if (isPoisonous && !isThrusting)
        {
            sheenTimer += Time.deltaTime * sheenSpeed;
            float wave = (Mathf.Sin(sheenTimer) + 1f) * 0.5f; // 0..1
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(poisonBaseColor, poisonHighlightColor, wave);
            }
        }
    }

    public void CheckUnitTrigger(Unit unit)
    {
        if (unit == null || !unit.gameObject.activeInHierarchy) return;
        if (isRetracted) return;

        if (unit.GridPosition == gridPos)
        {
            if (!unit.isEcho)
            {
                Debug.Log("<color=red>Live player stepped on active toxic spikes! Inflicting poison...</color>");
                if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.InflictPoison();
                }

                // Trigger punch & venom splash
                TriggerThrustPunch();
                TriggerVenomBurst();
            }
            else
            {
                Debug.Log("<color=yellow>Echo triggered spikes.</color>");
                TriggerThrustPunch();
                TriggerVenomBurst();
            }
        }
    }

    public void TriggerThrustPunch()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ThrustPunchRoutine());
        }
    }

    private IEnumerator ThrustPunchRoutine()
    {
        isThrusting = true;

        // 1. Instant jab upward + bright flash (scaled relative to baseScale)
        transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 1.35f, baseScale.z);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.92f, 1.0f, 0.82f, 1f); // Bright impact flash
        }

        yield return new WaitForSeconds(0.06f);

        // 2. Smooth recoil & settle back down to baseScale
        float elapsed = 0f;
        float duration = 0.20f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = baseScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = baseScale;
        isThrusting = false;
    }

    public void TriggerVenomBurst()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(VenomBurstRoutine());
    }

    private IEnumerator VenomBurstRoutine()
    {
        if (cachedDropletSprite == null) cachedDropletSprite = CreateDropletSprite();

        GameObject fxParent = new GameObject("VenomSplashFX");
        fxParent.transform.position = transform.position + new Vector3(0f, 0.1f, 0f);
        Destroy(fxParent, 0.5f);

        int count = 9;
        List<Transform> droplets = new List<Transform>();
        List<Vector3> velocities = new List<Vector3>();

        Color[] acidColors = new Color[]
        {
            new Color(0.35f, 1.0f, 0.35f, 1f), // Neon lime
            new Color(0.20f, 0.90f, 0.25f, 1f), // Toxic emerald
            new Color(0.60f, 1.0f, 0.45f, 1f)  // Acid pale green
        };

        for (int i = 0; i < count; i++)
        {
            GameObject drop = new GameObject($"Droplet_{i}");
            drop.transform.SetParent(fxParent.transform, false);
            drop.transform.localPosition = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.1f, 0.1f), 0f);

            float initialScale = Random.Range(0.25f, 0.45f);
            drop.transform.localScale = Vector3.one * initialScale;

            SpriteRenderer sr = drop.AddComponent<SpriteRenderer>();
            sr.sprite = cachedDropletSprite;
            sr.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder + 3 : 15);
            sr.color = acidColors[Random.Range(0, acidColors.Length)];

            droplets.Add(drop.transform);

            // Outward spraying velocity arc
            float angle = Random.Range(35f, 145f) * Mathf.Deg2Rad;
            float speed = Random.Range(1.8f, 3.2f);
            Vector3 vel = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f);
            velocities.Add(vel);
        }

        float life = 0.32f;
        float timer = 0f;
        float gravity = -9.0f;

        while (timer < life)
        {
            timer += Time.deltaTime;
            float t = timer / life;

            for (int i = 0; i < droplets.Count; i++)
            {
                if (droplets[i] == null) continue;

                Vector3 vel = velocities[i];
                vel.y += gravity * Time.deltaTime;
                velocities[i] = vel;

                droplets[i].position += vel * Time.deltaTime;

                float shrink = Mathf.Lerp(1f, 0.1f, t);
                droplets[i].localScale = Vector3.one * shrink * 0.4f;

                SpriteRenderer sr = droplets[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f - t;
                    sr.color = c;
                }
            }

            yield return null;
        }

        Destroy(fxParent);
    }

    private static Sprite CreateDropletSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.45f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float factor = 1f - (dist / radius);
                    float alpha = Mathf.Pow(factor, 0.6f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
