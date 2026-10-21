using UnityEngine;

public class EntityGlow : MonoBehaviour
{
    [Header("Glow Color & Brightness")]
    public Color glowColor = new Color(1.0f, 0.82f, 0.10f, 0.85f);
    public float pulseSpeed = 2.5f;
    public float minScale = 1.30f;
    public float maxScale = 1.55f;
    public float minAlpha = 0.55f;
    public float maxAlpha = 0.90f;
    public Vector3 offset = new Vector3(0f, 0.05f, 0f);
    public bool isGolden = true;

    private SpriteRenderer targetRenderer;
    private GameObject glowAuraObj;
    private SpriteRenderer glowRenderer;
    private float timer = 0f;

    private void Awake()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
        SetupGlowAura();
    }

    private void Start()
    {
        if (glowAuraObj == null) SetupGlowAura();
    }

    public void SetupGlowAura()
    {
        Transform existing = transform.Find("GlowAura");
        if (existing != null) glowAuraObj = existing.gameObject;
        else
        {
            glowAuraObj = new GameObject("GlowAura");
            glowAuraObj.transform.SetParent(transform, false);
            glowAuraObj.transform.localPosition = offset;
        }

        glowRenderer = glowAuraObj.GetComponent<SpriteRenderer>();
        if (glowRenderer == null) glowRenderer = glowAuraObj.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = CreateSoftGlowSprite();
        glowRenderer.sortingOrder = (targetRenderer != null ? targetRenderer.sortingOrder - 1 : 6);
        glowRenderer.color = glowColor;
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
                    // Beautiful radial dropoff: bright inner core, smooth radiant edge
                    float alpha = Mathf.Pow(factor, 1.3f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.8f);
    }

    private void Update()
    {
        timer += Time.deltaTime * pulseSpeed;
        float wave = (Mathf.Sin(timer) + 1f) * 0.5f;

        if (glowAuraObj != null && glowRenderer != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, wave);
            glowAuraObj.transform.localScale = new Vector3(scale * 1.25f, scale * 1.15f, 1f);

            Color c = glowColor;
            float targetA = (targetRenderer != null ? targetRenderer.color.a : 1f);
            c.a = Mathf.Lerp(minAlpha, maxAlpha, wave) * targetA;
            glowRenderer.color = c;
        }

        if (targetRenderer != null && targetRenderer.color.a > 0.05f)
        {
            float shimmer = Mathf.Lerp(1.0f, isGolden ? 1.15f : 1.08f, wave);
            if (isGolden)
                targetRenderer.color = new Color(shimmer, shimmer * 0.96f, shimmer * 0.82f, targetRenderer.color.a);
            else
                targetRenderer.color = new Color(shimmer * 0.96f, shimmer, shimmer * 1.04f, targetRenderer.color.a);
        }
    }
}
