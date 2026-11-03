using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds dynamic magical rune illumination, distinct color themes, and tactile mechanical feedback to Pressure Plates.
/// When multiple plates exist in a level, each plate receives a distinct jewel/rune color theme:
/// 1. Golden Amber (Sunstone)
/// 2. Arcane Cyan (Sapphire)
/// 3. Mystic Violet (Amethyst)
/// 4. Verdant Emerald (Jade)
/// 5. Blazing Ruby (Crimson Flame)
/// 
/// Runs in Edit Mode ([ExecuteAlways]) for live preview.
/// </summary>
[ExecuteAlways]
public class PressurePlateGlow : MonoBehaviour
{
    public enum PlateColorTheme
    {
        Auto = 0,
        GoldenAmber = 1,
        ArcaneCyan = 2,
        MysticViolet = 3,
        VerdantEmerald = 4,
        BlazingRuby = 5
    }

    [Header("Color Theme")]
    public PlateColorTheme theme = PlateColorTheme.Auto;
    public Color activeColor = new Color(0.25f, 1.00f, 0.85f, 0.95f);
    public Color dormantColor = new Color(0.25f, 1.00f, 0.85f, 0.25f);
    public Color idleSpriteColor = Color.white;

    [Header("Scale & Brightness")]
    public float activeScale = 1.30f;
    public float dormantScale = 0.95f;
    public float pulseSpeed = 3.5f;

    [Header("Mechanical Sinking")]
    public float depressionDistance = 0.08f; // ~1.5 pixel sink into stone bevel
    public float squashFactor = 0.88f;       // Y compression when stepped on
    public float depressionSpeed = 16f;     // Snappy click down & smooth pop-up

    private PressurePlate pressurePlate;
    private SpriteRenderer plateRenderer;
    private GameObject runeAuraObj;
    private SpriteRenderer runeRenderer;
    private Light plateLight;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private bool hasCachedBaseTransform = false;
    private float currentDepression = 0f;
    private bool lastPressedState = false;

    private static Sprite cachedPlateAuraSprite;
    private static Sprite cachedRuneRingSprite;
    private static Sprite cachedSparkleSprite;

    private void Awake()
    {
        pressurePlate = GetComponent<PressurePlate>();
        plateRenderer = GetComponent<SpriteRenderer>();
        CacheBaseTransform();
        ConfigureColorTheme();
        SetupAura();
    }

    private void OnEnable()
    {
        if (pressurePlate == null) pressurePlate = GetComponent<PressurePlate>();
        if (plateRenderer == null) plateRenderer = GetComponent<SpriteRenderer>();
        CacheBaseTransform();
        ConfigureColorTheme();
        SetupAura();
    }

    private void Start()
    {
        CacheBaseTransform();
        ConfigureColorTheme();
        SetupAura();
    }

    private void OnDisable()
    {
        if (hasCachedBaseTransform)
        {
            transform.localPosition = baseLocalPosition;
            transform.localScale = baseLocalScale;
            if (plateRenderer != null) plateRenderer.color = Color.white;
        }
    }

    private void CacheBaseTransform()
    {
        if (!hasCachedBaseTransform && (pressurePlate == null || !pressurePlate.IsPressed))
        {
            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;
            hasCachedBaseTransform = true;
        }
    }

    public void ConfigureColorTheme()
    {
        PlateColorTheme selected = theme;

        if (selected == PlateColorTheme.Auto)
        {
            // Determine plate index from GameObject name (e.g. "PressurePlate", "PressurePlate2", "PressurePlate3"...)
            int index = GetPlateIndexFromName(gameObject.name);

            // Check how many plates exist in this scene
            PressurePlate[] allPlates = Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
            int scenePlateCount = 0;
            foreach (var p in allPlates)
            {
                if (p.gameObject.scene == gameObject.scene) scenePlateCount++;
            }

            if (scenePlateCount <= 1)
            {
                // Single plate level (Level 1, Level 2) uses classic Radiant Golden Amber
                selected = PlateColorTheme.GoldenAmber;
            }
            else
            {
                // Multi-plate level: assign distinct theme per index
                switch (index)
                {
                    case 1: selected = PlateColorTheme.GoldenAmber; break;
                    case 2: selected = PlateColorTheme.ArcaneCyan; break;
                    case 3: selected = PlateColorTheme.MysticViolet; break;
                    case 4: selected = PlateColorTheme.VerdantEmerald; break;
                    case 5: selected = PlateColorTheme.BlazingRuby; break;
                    default:
                        // Wrap around for any higher numbers
                        int mod = ((index - 1) % 5) + 1;
                        selected = (PlateColorTheme)mod;
                        break;
                }
            }
        }

        ApplyThemeColors(selected);
    }

    private int GetPlateIndexFromName(string name)
    {
        if (string.IsNullOrEmpty(name)) return 1;

        // Try extracting ending digit
        int digitIndex = -1;
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(name[i])) digitIndex = i;
            else break;
        }

        if (digitIndex >= 0)
        {
            string numStr = name.Substring(digitIndex);
            if (int.TryParse(numStr, out int val)) return val;
        }

        return 1; // Default to plate 1
    }

    private void ApplyThemeColors(PlateColorTheme t)
    {
        switch (t)
        {
            case PlateColorTheme.GoldenAmber:
                activeColor = new Color(1.00f, 0.72f, 0.12f, 0.95f);   // Radiant Sunstone Gold
                dormantColor = new Color(1.00f, 0.72f, 0.12f, 0.25f);
                idleSpriteColor = new Color(1.00f, 0.94f, 0.82f, 1.0f); // Warm rune glow
                break;

            case PlateColorTheme.ArcaneCyan:
                activeColor = new Color(0.10f, 0.82f, 1.00f, 0.95f);   // Sapphire Cyan
                dormantColor = new Color(0.10f, 0.82f, 1.00f, 0.25f);
                idleSpriteColor = new Color(0.82f, 0.95f, 1.00f, 1.0f); // Ice rune glow
                break;

            case PlateColorTheme.MysticViolet:
                activeColor = new Color(0.82f, 0.25f, 1.00f, 0.95f);   // Amethyst Violet
                dormantColor = new Color(0.82f, 0.25f, 1.00f, 0.25f);
                idleSpriteColor = new Color(0.95f, 0.84f, 1.00f, 1.0f); // Arcane rune glow
                break;

            case PlateColorTheme.VerdantEmerald:
                activeColor = new Color(0.15f, 0.95f, 0.50f, 0.95f);   // Jade Emerald
                dormantColor = new Color(0.15f, 0.95f, 0.50f, 0.25f);
                idleSpriteColor = new Color(0.84f, 1.00f, 0.90f, 1.0f); // Verdant rune glow
                break;

            case PlateColorTheme.BlazingRuby:
                activeColor = new Color(1.00f, 0.22f, 0.35f, 0.95f);   // Crimson Ruby
                dormantColor = new Color(1.00f, 0.22f, 0.35f, 0.25f);
                idleSpriteColor = new Color(1.00f, 0.84f, 0.86f, 1.0f); // Flame rune glow
                break;

            default:
                activeColor = new Color(1.00f, 0.72f, 0.12f, 0.95f);
                dormantColor = new Color(1.00f, 0.72f, 0.12f, 0.25f);
                idleSpriteColor = Color.white;
                break;
        }

        if (plateLight != null)
        {
            plateLight.color = activeColor;
        }
    }

    public void SetupAura()
    {
        Transform existing = transform.Find("PlateRuneAura");
        if (existing != null) runeAuraObj = existing.gameObject;
        else
        {
            runeAuraObj = new GameObject("PlateRuneAura");
            runeAuraObj.transform.SetParent(transform, false);
            runeAuraObj.transform.localPosition = Vector3.zero;
        }

        runeRenderer = runeAuraObj.GetComponent<SpriteRenderer>();
        if (runeRenderer == null) runeRenderer = runeAuraObj.AddComponent<SpriteRenderer>();

        if (cachedPlateAuraSprite == null) cachedPlateAuraSprite = CreateRuneGlowSprite();
        runeRenderer.sprite = cachedPlateAuraSprite;
        runeRenderer.sortingOrder = 2; // Sits right above floor plate (order 1), under units (order 9/10)

        // Point light for atmospheric illumination
        plateLight = GetComponent<Light>();
        if (plateLight == null) plateLight = gameObject.AddComponent<Light>();
        plateLight.type = LightType.Point;
        plateLight.range = 2.5f;
        plateLight.intensity = 0.5f;
        plateLight.color = activeColor;
        plateLight.renderMode = LightRenderMode.ForcePixel;
    }

    private void Update()
    {
        if (runeAuraObj == null || runeRenderer == null)
        {
            SetupAura();
            return;
        }

        bool isPressed = (pressurePlate != null && pressurePlate.IsPressed);

        // Detect instant of depression to trigger shockwave ripple & sparks
        if (isPressed && !lastPressedState)
        {
            TriggerActivationFX();
        }
        lastPressedState = isPressed;

        float dt = Application.isPlaying ? Time.deltaTime : 0.033f;

        // Mechanical sinking & squish animation
        if (hasCachedBaseTransform)
        {
            float targetDepression = isPressed ? 1f : 0f;
            currentDepression = Mathf.MoveTowards(currentDepression, targetDepression, dt * depressionSpeed);

            float sinkY = -currentDepression * depressionDistance;
            transform.localPosition = baseLocalPosition + new Vector3(0f, sinkY, 0f);
            transform.localScale = new Vector3(
                baseLocalScale.x * Mathf.Lerp(1f, 1.03f, currentDepression),
                baseLocalScale.y * Mathf.Lerp(1f, squashFactor, currentDepression),
                baseLocalScale.z
            );

            // Keep the floor rune aura anchored to the floor plane as the stone plate depresses
            runeAuraObj.transform.localPosition = new Vector3(0f, -sinkY, 0f);

            // Recessed bevel shading when pressed into the floor
            if (plateRenderer != null)
            {
                Color recessedColor = idleSpriteColor * new Color(0.80f, 0.80f, 0.85f, 1f);
                plateRenderer.color = Color.Lerp(idleSpriteColor, recessedColor, currentDepression);
            }
        }

        // Lighting & Pulsing Rune Aura
        float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
        float pulse = (Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f;

        if (isPressed)
        {
            // Bright, surging energy while an Echo or Player holds the switch
            float targetScale = Mathf.Lerp(activeScale, activeScale * 1.15f, pulse);
            runeAuraObj.transform.localScale = Vector3.one * targetScale;

            Color c = activeColor;
            c.a = Mathf.Lerp(0.75f, 0.95f, pulse);
            runeRenderer.color = c;

            if (plateLight != null)
            {
                plateLight.intensity = Mathf.Lerp(2.2f, 3.0f, pulse);
                plateLight.color = activeColor;
            }
        }
        else
        {
            // Gentle, dormant resting glow
            float targetScale = Mathf.Lerp(dormantScale * 0.92f, dormantScale, pulse);
            runeAuraObj.transform.localScale = Vector3.one * targetScale;

            Color c = dormantColor;
            c.a = Mathf.Lerp(dormantColor.a * 0.7f, dormantColor.a, pulse);
            runeRenderer.color = c;

            if (plateLight != null)
            {
                plateLight.intensity = 0.4f;
                plateLight.color = dormantColor;
            }
        }
    }

    public void TriggerActivationFX()
    {
        if (!Application.isPlaying || !gameObject.activeInHierarchy) return;

        StartCoroutine(ActivationRippleRoutine());
        StartCoroutine(SparkBurstRoutine());
    }

    private IEnumerator ActivationRippleRoutine()
    {
        if (cachedRuneRingSprite == null) cachedRuneRingSprite = CreateRuneRingSprite();

        GameObject rippleObj = new GameObject("PlateRuneRipple");
        rippleObj.transform.position = transform.position;
        Destroy(rippleObj, 0.40f);

        SpriteRenderer sr = rippleObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedRuneRingSprite;
        sr.sortingOrder = 2; // Floor plane above plate
        sr.color = activeColor;

        float duration = 0.32f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = Mathf.Lerp(0.5f, 2.2f, Mathf.Sqrt(t));
            rippleObj.transform.localScale = Vector3.one * scale;

            Color c = activeColor;
            c.a = Mathf.Lerp(0.95f, 0.0f, t);
            sr.color = c;

            yield return null;
        }
    }

    private IEnumerator SparkBurstRoutine()
    {
        if (cachedSparkleSprite == null) cachedSparkleSprite = CreateSparkleSprite();

        GameObject sparkContainer = new GameObject("PlateSparks");
        sparkContainer.transform.position = transform.position;
        Destroy(sparkContainer, 0.45f);

        int sparkCount = 8;
        List<Transform> sparks = new List<Transform>();
        List<Vector3> velocities = new List<Vector3>();
        List<float> rotSpeeds = new List<float>();

        for (int i = 0; i < sparkCount; i++)
        {
            GameObject sp = new GameObject($"Spark_{i}");
            sp.transform.SetParent(sparkContainer.transform, false);
            sp.transform.localPosition = new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.1f, 0.1f), 0f);

            SpriteRenderer sr = sp.AddComponent<SpriteRenderer>();
            sr.sprite = cachedSparkleSprite;
            sr.sortingOrder = (plateRenderer != null ? plateRenderer.sortingOrder + 3 : 12);
            sr.color = activeColor;

            sparks.Add(sp.transform);

            // Radial burst with slight upward vertical bias
            float baseAngle = (i * (360f / sparkCount) + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            float speed = Random.Range(1.6f, 2.6f);
            Vector3 vel = new Vector3(Mathf.Cos(baseAngle) * speed, Mathf.Sin(baseAngle) * speed * 0.75f + 0.5f, 0f);
            velocities.Add(vel);
            rotSpeeds.Add(Random.Range(-180f, 180f));
        }

        float duration = 0.35f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            for (int i = 0; i < sparks.Count; i++)
            {
                if (sparks[i] == null) continue;

                sparks[i].position += velocities[i] * Time.deltaTime;
                sparks[i].Rotate(0f, 0f, rotSpeeds[i] * Time.deltaTime);

                float scale = Mathf.Lerp(0.55f, 0.05f, t);
                sparks[i].localScale = Vector3.one * scale;

                SpriteRenderer sr = sparks[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = activeColor;
                    c.a = Mathf.Lerp(1.0f, 0f, t);
                    sr.color = c;
                }
            }

            yield return null;
        }
    }

    private static Sprite CreateRuneGlowSprite()
    {
        int size = 96;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.48f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float factor = 1f - (dist / radius);
                    float alpha = Mathf.Pow(factor, 1.25f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 96f);
    }

    private static Sprite CreateRuneRingSprite()
    {
        int size = 96;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float outerRadius = size * 0.46f;
        float innerRadius = size * 0.30f;
        float ringWidth = (outerRadius - innerRadius) * 0.5f;
        float ringCenter = (outerRadius + innerRadius) * 0.5f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist >= innerRadius && dist <= outerRadius)
                {
                    float dCenter = Mathf.Abs(dist - ringCenter);
                    float factor = 1f - (dCenter / ringWidth);
                    float alpha = Mathf.SmoothStep(0f, 1f, factor);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 96f);
    }

    private static Sprite CreateSparkleSprite()
    {
        int size = 48;
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
                    float alpha = Mathf.Pow(1.0f - val, 0.7f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48f);
    }
}
