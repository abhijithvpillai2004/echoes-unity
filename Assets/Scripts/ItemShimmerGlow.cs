using UnityEngine;

/// <summary>
/// Adds an animated star-like sparkle twinkle to weapons.
/// Strictly renders only the 4-pointed diamond star flare on the weapon edge.
/// No aura blur and no point light.
/// </summary>
[ExecuteAlways]
public class ItemShimmerGlow : MonoBehaviour
{
    [Header("Sparkle Timing")]
    public float sparkleInterval = 2.0f;

    private SpriteRenderer weaponRenderer;
    private GameObject sparkleObj;
    private SpriteRenderer sparkleRenderer;

    private static Sprite cachedSparkleSprite;

    private void Awake()
    {
        SetupSparkle();
    }

    private void OnEnable()
    {
        SetupSparkle();
    }

    public void SetupSparkle()
    {
        // Remove any old aura or light if present
        Transform oldAura = transform.Find("ItemAura");
        if (oldAura != null)
        {
            if (Application.isPlaying) Destroy(oldAura.gameObject);
            else DestroyImmediate(oldAura.gameObject);
        }

        Light oldLight = GetComponent<Light>();
        if (oldLight != null)
        {
            if (Application.isPlaying) Destroy(oldLight);
            else DestroyImmediate(oldLight);
        }

        weaponRenderer = GetComponent<SpriteRenderer>();

        Transform existing = transform.Find("ItemSparkle");
        if (existing != null) sparkleObj = existing.gameObject;
        else
        {
            sparkleObj = new GameObject("ItemSparkle");
            sparkleObj.transform.SetParent(transform, false);
        }

        sparkleRenderer = sparkleObj.GetComponent<SpriteRenderer>();
        if (sparkleRenderer == null) sparkleRenderer = sparkleObj.AddComponent<SpriteRenderer>();

        if (cachedSparkleSprite == null) cachedSparkleSprite = CreateSparkleSprite();
        sparkleRenderer.sprite = cachedSparkleSprite;

        int baseOrder = (weaponRenderer != null) ? weaponRenderer.sortingOrder : 10;
        sparkleRenderer.sortingOrder = baseOrder + 2; // Renders right on top of the weapon

        // Position sparkle at the blade / axe edge
        sparkleObj.transform.localPosition = new Vector3(0.18f, 0.22f, 0f);
    }

    private void Update()
    {
        if (sparkleObj == null || sparkleRenderer == null)
        {
            SetupSparkle();
            return;
        }

        float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;

        // Twinkling Glint / Star Sparkle Animation
        float phase = (t % sparkleInterval) / sparkleInterval; // 0 to 1 cycle
        // Sparkle flares up during 0.0 -> 0.36 of cycle, rests during remaining time
        float progress = phase / 0.36f;
        if (progress <= 1.0f)
        {
            // Smooth bell curve flare
            float bell = Mathf.Sin(progress * Mathf.PI);
            float scale = Mathf.Lerp(0.1f, 0.75f, bell);
            sparkleObj.transform.localScale = Vector3.one * scale;
            sparkleObj.transform.localRotation = Quaternion.Euler(0, 0, progress * 90f);

            Color c = Color.white;
            c.a = bell * 0.95f;
            sparkleRenderer.color = c;
        }
        else
        {
            sparkleObj.transform.localScale = Vector3.zero;
            Color c = Color.white;
            c.a = 0f;
            sparkleRenderer.color = c;
        }
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
