using UnityEngine;

public class GoldenChestGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = new Color(1.0f, 0.84f, 0.1f, 0.7f); // Rich golden aura
    public float pulseSpeed = 2.8f;
    public float minScale = 1.1f;
    public float maxScale = 1.5f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 0.85f;

    private SpriteRenderer chestRenderer;
    private GameObject glowAuraObj;
    private SpriteRenderer glowRenderer;
    private float timer = 0f;

    private void Awake()
    {
        chestRenderer = GetComponent<SpriteRenderer>();
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
            glowAuraObj.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        }

        glowRenderer = glowAuraObj.GetComponent<SpriteRenderer>();
        if (glowRenderer == null) glowRenderer = glowAuraObj.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = CreateSoftGlowSprite();
        glowRenderer.sortingOrder = (chestRenderer != null ? chestRenderer.sortingOrder - 1 : 5);
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

    private void Update()
    {
        timer += Time.deltaTime * pulseSpeed;
        float wave = (Mathf.Sin(timer) + 1f) * 0.5f; // 0..1

        if (glowAuraObj != null && glowRenderer != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, wave);
            glowAuraObj.transform.localScale = new Vector3(scale * 1.3f, scale * 1.15f, 1f);

            Color c = glowColor;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
            glowRenderer.color = c;
        }

        if (chestRenderer != null)
        {
            float shimmer = Mathf.Lerp(0.92f, 1.18f, wave);
            chestRenderer.color = new Color(shimmer, shimmer * 0.98f, shimmer * 0.85f, 1f);
        }
    }
}
