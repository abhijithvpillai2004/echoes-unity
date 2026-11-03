using UnityEngine;

/// <summary>
/// Controls the visual radiance of the Golden Treasure Chest.
/// Features a gentle breathing golden aura, treasure surface shimmer,
/// and animated twinkling diamond star glints/sparkles dancing across the chest.
/// </summary>
[ExecuteAlways]
public class GoldenChestGlow : MonoBehaviour
{
    [Header("Glow Aura Settings")]
    public Color glowColor = new Color(1.0f, 0.84f, 0.1f, 0.7f); // Rich golden aura
    public float pulseSpeed = 2.8f;
    public float minScale = 1.1f;
    public float maxScale = 1.45f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 0.85f;

    [Header("Sparkle Settings")]
    public bool enableSparkles = true;
    public float globalSparkleSpeed = 1.0f;

    private SpriteRenderer chestRenderer;
    private GameObject glowAuraObj;
    private SpriteRenderer glowRenderer;
    private float timer = 0f;

    // Structured Sparkles
    private struct SparkleInstance
    {
        public GameObject obj;
        public SpriteRenderer renderer;
        public Vector3 baseLocalPos;
        public float interval;
        public float offset;
        public float maxScale;
        public float rotationSpeed;
        public Color tint;
    }

    private SparkleInstance[] sparkles;
    private static Sprite cachedSparkleSprite;

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        chestRenderer = GetComponent<SpriteRenderer>();
        SetupGlowAura();
        SetupSparkles();
    }

    public void SetupGlowAura()
    {
        Transform existing = transform.Find("GlowAura");
        if (existing != null) glowAuraObj = existing.gameObject;
        else
        {
            glowAuraObj = new GameObject("GlowAura");
            glowAuraObj.transform.SetParent(transform, false);
            glowAuraObj.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        }

        glowRenderer = glowAuraObj.GetComponent<SpriteRenderer>();
        if (glowRenderer == null) glowRenderer = glowAuraObj.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = CreateSoftGlowSprite();
        glowRenderer.sortingOrder = (chestRenderer != null ? chestRenderer.sortingOrder - 1 : 5);
        glowRenderer.color = glowColor;
    }

    public void SetupSparkles()
    {
        if (!enableSparkles) return;

        if (cachedSparkleSprite == null) cachedSparkleSprite = CreateSparkleSprite();

        Transform container = transform.Find("SparkleContainer");
        if (container == null)
        {
            GameObject containerObj = new GameObject("SparkleContainer");
            containerObj.transform.SetParent(transform, false);
            container = containerObj.transform;
            container.localPosition = Vector3.zero;
        }

        int baseOrder = (chestRenderer != null) ? chestRenderer.sortingOrder + 2 : 12;

        // 4 strategic sparkle locations:
        // 1. Golden latch / keyhole (center-low)
        // 2. Top-right lid corner
        // 3. Top-left lid corner
        // 4. Floating gleam above chest lid
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0.0f, -0.05f, 0f),
            new Vector3(0.24f, 0.22f, 0f),
            new Vector3(-0.22f, 0.20f, 0f),
            new Vector3(0.06f, 0.32f, 0f)
        };

        float[] intervals = new float[] { 1.6f, 2.0f, 2.3f, 1.85f };
        float[] offsets = new float[] { 0.0f, 0.55f, 1.15f, 1.70f };
        float[] scales = new float[] { 0.65f, 0.55f, 0.50f, 0.45f };
        float[] rotSpeeds = new float[] { 90f, -80f, 110f, -60f };
        Color[] colors = new Color[]
        {
            new Color(1.0f, 0.85f, 0.12f, 1f), // Deep brilliant gold
            new Color(1.0f, 0.90f, 0.25f, 1f), // Radiant yellow gold
            new Color(1.0f, 0.80f, 0.10f, 1f), // Rich warm amber gold
            new Color(1.0f, 0.88f, 0.20f, 1f)  // Luminous sun gold
        };

        sparkles = new SparkleInstance[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            string childName = $"ChestSparkle_{i}";
            Transform spTr = container.Find(childName);
            GameObject spObj;
            if (spTr != null) spObj = spTr.gameObject;
            else
            {
                spObj = new GameObject(childName);
                spObj.transform.SetParent(container, false);
            }

            spObj.transform.localPosition = positions[i];
            spObj.transform.localScale = Vector3.zero;

            SpriteRenderer sr = spObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = spObj.AddComponent<SpriteRenderer>();
            sr.sprite = cachedSparkleSprite;
            sr.sortingOrder = baseOrder + i;
            sr.color = Color.clear;

            sparkles[i] = new SparkleInstance
            {
                obj = spObj,
                renderer = sr,
                baseLocalPos = positions[i],
                interval = intervals[i],
                offset = offsets[i],
                maxScale = scales[i],
                rotationSpeed = rotSpeeds[i],
                tint = colors[i]
            };
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float curTime = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;

        timer += dt * pulseSpeed;
        float wave = (Mathf.Sin(timer) + 1f) * 0.5f; // 0..1

        // 1. Glow Aura pulse
        if (glowAuraObj != null && glowRenderer != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, wave);
            glowAuraObj.transform.localScale = new Vector3(scale * 1.3f, scale * 1.15f, 1f);

            Color c = glowColor;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
            glowRenderer.color = c;
        }

        // 2. Chest sprite breathing shimmer
        if (chestRenderer != null)
        {
            float shimmer = Mathf.Lerp(0.92f, 1.18f, wave);
            chestRenderer.color = new Color(shimmer, shimmer * 0.98f, shimmer * 0.85f, 1f);
        }

        // 3. Twinkling Glints / Star Sparkles
        if (enableSparkles && sparkles != null)
        {
            for (int i = 0; i < sparkles.Length; i++)
            {
                UpdateSingleSparkle(ref sparkles[i], curTime);
            }
        }
    }

    private void UpdateSingleSparkle(ref SparkleInstance sp, float curTime)
    {
        if (sp.obj == null || sp.renderer == null) return;

        float cycleTime = (curTime * globalSparkleSpeed + sp.offset) % sp.interval;
        float activeDuration = sp.interval * 0.38f; // flares up during 38% of interval

        if (cycleTime <= activeDuration)
        {
            float progress = cycleTime / activeDuration; // 0..1
            float bell = Mathf.Sin(progress * Mathf.PI);  // smooth 0..1..0 curve

            float scale = Mathf.Lerp(0.05f, sp.maxScale, bell);
            sp.obj.transform.localScale = Vector3.one * scale;
            sp.obj.transform.localRotation = Quaternion.Euler(0, 0, progress * sp.rotationSpeed);

            Color c = sp.tint;
            c.a = bell * 0.98f;
            sp.renderer.color = c;
        }
        else
        {
            sp.obj.transform.localScale = Vector3.zero;
            Color c = sp.tint;
            c.a = 0f;
            sp.renderer.color = c;
        }
    }

    private Sprite CreateSoftGlowSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
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
                    float alpha = factor * factor; // smooth radial falloff
                    tex.SetPixel(x, y, new Color(1f, 0.92f, 0.35f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.75f);
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

                // Crisp 4-pointed diamond star flare
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
