using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles procedural animations for the Mage (Player & Echoes):
/// Ultra-Smooth Upright Arcane Hover-Slide with Ground Rune Sparks.
/// Features:
/// 1. Harmonic Sine Easing: 0.20s butter-smooth straight-line glide between tiles with zero jerk.
/// 2. Seamless Idle Hover Blending: Transitions seamlessly into and out of stationary hovering with no 1-frame pops.
/// 3. Interruption Continuity: Chained movement commands interpolate smoothly from the character's live position.
/// 4. Ground Rune Sparks: Kicks up a wake of twinkling diamond rune sparks along the floor beneath the robes.
/// 5. Wall Bump / Impact Rebound: Tactile feedback when walking into walls or obstacles.
/// </summary>
public class MageAnimation : MonoBehaviour
{
    [Header("Hover-Slide Settings")]
    [SerializeField] private float slideDuration = 0.20f; // Butter-smooth glide duration

    [Header("Ground Spark Settings")]
    [SerializeField] private Color playerSparkColor = new Color(0.25f, 0.90f, 1.00f, 0.95f); // Radiant Arcane Cyan
    [SerializeField] private Color echoSparkColor = new Color(0.60f, 0.75f, 1.00f, 0.80f);   // Arcane Lavender
    [SerializeField] private int sparksPerSlide = 5;
    [SerializeField] private float sparkLifetime = 0.24f;

    [Header("Idle Hover Settings")]
    [SerializeField] private float hoverFrequency = 3.5f;
    [SerializeField] private float hoverAmplitude = 0.04f;
    [SerializeField] private float tiltFrequency = 2.5f;
    [SerializeField] private float tiltAngle = 1.5f;

    [Header("Wall Bump Settings")]
    [SerializeField] private float bumpDistance = 0.22f;
    [SerializeField] private float bumpForwardDuration = 0.05f;
    [SerializeField] private float bumpReboundDuration = 0.08f;

    private SpriteRenderer spriteRenderer;
    private Transform weaponVisual;
    private Unit unit;
    private Vector3 initialScale = Vector3.zero;
    private Vector3 baseWorldPosition;
    private bool isAnimating = false;
    private float idleHoverWeight = 0f;
    private Coroutine activeAnimCoroutine;

    private static Sprite cachedSparkSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        unit = GetComponent<Unit>();
        GetBaseScale();
        baseWorldPosition = transform.position;

        Transform weapon = transform.Find("WeaponVisual");
        if (weapon != null) weaponVisual = weapon;
    }

    private Vector3 GetBaseScale()
    {
        if (initialScale.x <= 0.001f || initialScale.y <= 0.001f)
        {
            if (transform.localScale.x > 0.001f && transform.localScale.y > 0.001f)
            {
                initialScale = transform.localScale;
            }
            else
            {
                initialScale = new Vector3(0.75f, 0.75f, 1f);
            }
        }
        return initialScale;
    }

    private void Update()
    {
        if (!isAnimating)
        {
            // Smoothly blend idle hovering back in over 0.25 seconds so there is never a 1-frame pop
            idleHoverWeight = Mathf.MoveTowards(idleHoverWeight, 1f, Time.deltaTime * 4.0f);

            float hoverY = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude * idleHoverWeight;
            float tiltZ = Mathf.Sin(Time.time * tiltFrequency) * tiltAngle * idleHoverWeight;

            transform.position = baseWorldPosition + new Vector3(0, hoverY, 0);
            transform.rotation = Quaternion.Euler(0, 0, tiltZ);
        }
        else
        {
            idleHoverWeight = 0f;
        }
    }

    /// <summary>
    /// Snaps immediately to base position (used on loop resets / grid snaps).
    /// </summary>
    public void SnapToBase(Vector3 newWorldPos)
    {
        if (activeAnimCoroutine != null)
        {
            StopCoroutine(activeAnimCoroutine);
            activeAnimCoroutine = null;
        }

        isAnimating = false;
        idleHoverWeight = 0f;
        baseWorldPosition = newWorldPos;
        transform.position = newWorldPos;
        transform.localScale = GetBaseScale();
        transform.rotation = Quaternion.identity;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = (unit != null && unit.isEcho) ? 0.5f : 1.0f;
            spriteRenderer.color = c;
        }
    }

    /// <summary>
    /// Executes the ultra-smooth Arcane Hover-Slide with Ground Rune Sparks.
    /// </summary>
    public void PlayHoverSlide(Vector3 startPos, Vector3 targetPos, Vector2Int direction)
    {
        // Smoothly continue from current position if a previous move is interrupted
        Vector3 actualStart = isAnimating ? transform.position : startPos;

        if (activeAnimCoroutine != null)
        {
            StopCoroutine(activeAnimCoroutine);
            activeAnimCoroutine = null;
        }

        UpdateFacingDirection(direction);
        activeAnimCoroutine = StartCoroutine(SlideCoroutine(actualStart, targetPos, direction));
    }

    // Aliases for compatibility with Unit.cs
    public void PlayHop(Vector3 startPos, Vector3 targetPos, Vector2Int direction) => PlayHoverSlide(startPos, targetPos, direction);
    public void PlayGlide(Vector3 startPos, Vector3 targetPos, Vector2Int direction) => PlayHoverSlide(startPos, targetPos, direction);
    public void PlayDash(Vector3 startPos, Vector3 targetPos, Vector2Int direction) => PlayHoverSlide(startPos, targetPos, direction);
    public void PlayBlink(Vector3 startPos, Vector3 targetPos, Vector2Int direction) => PlayHoverSlide(startPos, targetPos, direction);

    private IEnumerator SlideCoroutine(Vector3 start, Vector3 target, Vector2Int direction)
    {
        isAnimating = true;
        idleHoverWeight = 0f;
        baseWorldPosition = target;

        Vector3 baseScale = GetBaseScale();
        transform.rotation = Quaternion.identity;
        transform.localScale = baseScale;

        float sparkInterval = slideDuration / (sparksPerSlide + 1);
        float nextSparkTime = sparkInterval;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float rawT = Mathf.Clamp01(elapsed / slideDuration);

            // Harmonic Sine Ease In-Out: 0.5 * (1 - cos(pi * t))
            // Continuous zero-acceleration derivative at both ends = completely silky smooth
            float t = 0.5f * (1f - Mathf.Cos(rawT * Mathf.PI));

            // Ultra-smooth straight-line translation between tiles
            transform.position = Vector3.Lerp(start, target, t);

            // Strictly upright
            transform.rotation = Quaternion.identity;
            transform.localScale = baseScale;

            // Emit ground rune sparks beneath the hem of the robe onto the floor
            if (elapsed >= nextSparkTime && nextSparkTime < slideDuration)
            {
                SpawnGroundSpark(direction);
                nextSparkTime += sparkInterval;
            }

            yield return null;
        }

        transform.position = target;
        transform.localScale = baseScale;
        transform.rotation = Quaternion.identity;
        isAnimating = false;
        activeAnimCoroutine = null;
    }

    private void SpawnGroundSpark(Vector2Int direction)
    {
        if (!Application.isPlaying) return;
        if (cachedSparkSprite == null) cachedSparkSprite = CreateSparkSprite();

        // Position spark right at floor level beneath the robes
        Vector3 sparkPos = transform.position + new Vector3(
            Random.Range(-0.10f, 0.10f) - direction.x * 0.15f,
            -0.22f + Random.Range(-0.03f, 0.03f),
            0f
        );

        GameObject sparkObj = new GameObject("RuneSparkMote");
        sparkObj.transform.position = sparkPos;
        sparkObj.transform.localScale = Vector3.zero;

        SpriteRenderer sr = sparkObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedSparkSprite;
        sr.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder - 1 : 8);

        bool isAnEcho = (unit != null && unit.isEcho);
        Color baseCol = isAnEcho ? echoSparkColor : playerSparkColor;
        sr.color = baseCol;

        // Upward floating & swirling velocity with slight backward drift
        Vector3 velocity = new Vector3(
            -direction.x * Random.Range(0.2f, 0.5f) + Random.Range(-0.25f, 0.25f),
            Random.Range(0.25f, 0.6f),
            0f
        );
        float rotSpeed = Random.Range(-220f, 220f);

        StartCoroutine(AnimateGroundSpark(sparkObj, sr, velocity, rotSpeed, baseCol, sparkLifetime));
    }

    private IEnumerator AnimateGroundSpark(GameObject obj, SpriteRenderer sr, Vector3 velocity, float rotSpeed, Color baseCol, float duration)
    {
        float timer = 0f;
        float maxScale = Random.Range(0.30f, 0.45f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (obj != null && sr != null)
            {
                obj.transform.position += velocity * Time.deltaTime;
                obj.transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

                // Flare up quickly then shrink
                float scale = Mathf.Sin(t * Mathf.PI) * maxScale;
                obj.transform.localScale = Vector3.one * scale;

                Color c = baseCol;
                c.a = Mathf.Lerp(baseCol.a, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (obj != null)
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// Bumps forward into a wall/obstacle and rebounds back.
    /// </summary>
    public void PlayWallBump(Vector2Int direction)
    {
        if (activeAnimCoroutine != null)
        {
            StopCoroutine(activeAnimCoroutine);
        }

        UpdateFacingDirection(direction);
        activeAnimCoroutine = StartCoroutine(BumpCoroutine(direction));
    }

    private IEnumerator BumpCoroutine(Vector2Int direction)
    {
        isAnimating = true;
        Vector3 start = baseWorldPosition;
        Vector3 bumpTarget = start + new Vector3(direction.x, direction.y, 0) * bumpDistance;
        Vector3 baseScale = GetBaseScale();

        // 1. Lunge into obstacle
        float elapsed = 0f;
        while (elapsed < bumpForwardDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bumpForwardDuration);
            transform.position = Vector3.Lerp(start, bumpTarget, t);
            yield return null;
        }

        // 2. Elastic rebound back to original tile
        elapsed = 0f;
        while (elapsed < bumpReboundDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bumpReboundDuration);
            transform.position = Vector3.Lerp(bumpTarget, start, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        transform.position = start;
        transform.localScale = baseScale;
        transform.rotation = Quaternion.identity;
        isAnimating = false;
        activeAnimCoroutine = null;
    }

    private void UpdateFacingDirection(Vector2Int direction)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (direction.x < 0)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = true;
            if (weaponVisual != null)
            {
                weaponVisual.localPosition = new Vector3(-Mathf.Abs(weaponVisual.localPosition.x), weaponVisual.localPosition.y, weaponVisual.localPosition.z);
                SpriteRenderer wsr = weaponVisual.GetComponent<SpriteRenderer>();
                if (wsr != null) wsr.flipX = true;
            }
        }
        else if (direction.x > 0)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = false;
            if (weaponVisual != null)
            {
                weaponVisual.localPosition = new Vector3(Mathf.Abs(weaponVisual.localPosition.x), weaponVisual.localPosition.y, weaponVisual.localPosition.z);
                SpriteRenderer wsr = weaponVisual.GetComponent<SpriteRenderer>();
                if (wsr != null) wsr.flipX = false;
            }
        }
    }

    private static Sprite CreateSparkSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.48f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - center.x);
                float dy = Mathf.Abs(y - center.y);

                float nx = dx / radius;
                float ny = dy / radius;
                float val = Mathf.Sqrt(nx) + Mathf.Sqrt(ny);

                if (val <= 1.0f)
                {
                    float alpha = Mathf.Pow(1.0f - val, 0.75f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
