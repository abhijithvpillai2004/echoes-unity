using System.Collections;
using UnityEngine;

/// <summary>
/// Handles procedural animations for the Mage (Player & Echoes):
/// 1. Arcane Glide & Lean with Directional Flip (sleek floating glide without jumping)
/// 2. Idle Hovering / Floating Float (subtle magical levitation)
/// 3. Wall Bump / Impact Rebound (tactile feedback when walking into a wall/obstacle)
/// </summary>
public class MageAnimation : MonoBehaviour
{
    [Header("Arcane Glide & Lean Settings")]
    [SerializeField] private float glideDuration = 0.14f;
    [SerializeField] private float leanMaxAngle = 7f;
    [SerializeField] private Vector3 horizontalGlideScale = new Vector3(1.08f, 0.94f, 1f);
    [SerializeField] private Vector3 verticalGlideScale = new Vector3(0.94f, 1.08f, 1f);

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
    private Vector3 initialScale = Vector3.zero;
    private Vector3 baseWorldPosition;
    private bool isAnimating = false;
    private Coroutine activeAnimCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetBaseScale();
        baseWorldPosition = transform.position;

        // Find optional WeaponVisual child (if equipped with staff/sword)
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
        // Only hover when not performing a movement glide or wall bump
        if (!isAnimating)
        {
            float hoverY = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            float tiltZ = Mathf.Sin(Time.time * tiltFrequency) * tiltAngle;

            transform.position = baseWorldPosition + new Vector3(0, hoverY, 0);
            transform.rotation = Quaternion.Euler(0, 0, tiltZ);
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
        baseWorldPosition = newWorldPos;
        transform.position = newWorldPos;
        transform.localScale = GetBaseScale();
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Executes the Arcane Glide & Lean from startPos to targetPos, facing the movement direction.
    /// (No jumping arc: glides level with smooth lean into the motion).
    /// </summary>
    public void PlayGlide(Vector3 startPos, Vector3 targetPos, Vector2Int direction)
    {
        if (activeAnimCoroutine != null)
        {
            StopCoroutine(activeAnimCoroutine);
        }

        // Apply directional flip
        UpdateFacingDirection(direction);

        activeAnimCoroutine = StartCoroutine(GlideCoroutine(startPos, targetPos, direction));
    }

    // Alias for backward compatibility
    public void PlayHop(Vector3 startPos, Vector3 targetPos, Vector2Int direction)
    {
        PlayGlide(startPos, targetPos, direction);
    }

    private IEnumerator GlideCoroutine(Vector3 start, Vector3 target, Vector2Int direction)
    {
        isAnimating = true;
        baseWorldPosition = target;

        Vector3 baseScale = GetBaseScale();

        // Calculate aerodynamic lean angle:
        // Moving Right: tilt forward (-Z)
        // Moving Left: tilt forward (+Z)
        float targetLean = 0f;
        if (direction.x > 0) targetLean = -leanMaxAngle;
        else if (direction.x < 0) targetLean = leanMaxAngle;

        Vector3 targetStretch = (direction.x != 0)
            ? Vector3.Scale(baseScale, horizontalGlideScale)
            : Vector3.Scale(baseScale, verticalGlideScale);

        float elapsed = 0f;
        while (elapsed < glideDuration)
        {
            elapsed += Time.deltaTime;
            float rawT = Mathf.Clamp01(elapsed / glideDuration);

            // SmoothStep curve for fluid floating glide
            float t = rawT * rawT * (3f - 2f * rawT);

            // Level straight glide (NO jumping arc!)
            transform.position = Vector3.Lerp(start, target, t);

            // Lean smoothly into the movement and recover
            float leanFactor = Mathf.Sin(rawT * Mathf.PI);
            transform.rotation = Quaternion.Euler(0, 0, targetLean * leanFactor);

            // Aerodynamic stretch along motion
            transform.localScale = Vector3.Lerp(baseScale, targetStretch, leanFactor);

            yield return null;
        }

        transform.position = target;
        transform.localScale = baseScale;
        transform.rotation = Quaternion.identity;
        isAnimating = false;
        activeAnimCoroutine = null;
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
}
