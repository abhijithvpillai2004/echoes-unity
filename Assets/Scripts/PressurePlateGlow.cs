using UnityEngine;

/// <summary>
/// Adds dynamic magical rune illumination to Pressure Plates.
/// When stepped on (by player or echo), the plate surges with bright, vibrant light,
/// providing clear feedback that the switch is active and holding linked doors open.
/// Runs in Edit Mode ([ExecuteAlways]) for live preview.
/// </summary>
[ExecuteAlways]
public class PressurePlateGlow : MonoBehaviour
{
    [Header("Color Config")]
    public Color activeColor = new Color(0.20f, 0.95f, 1.00f, 0.90f); // Bright Arcane Cyan
    public Color dormantColor = new Color(0.20f, 0.95f, 1.00f, 0.22f); // Subtle resting glow

    [Header("Scale & Brightness")]
    public float activeScale = 1.30f;
    public float dormantScale = 0.95f;
    public float pulseSpeed = 3.5f;

    private PressurePlate pressurePlate;
    private SpriteRenderer plateRenderer;
    private GameObject runeAuraObj;
    private SpriteRenderer runeRenderer;
    private Light plateLight;

    private static Sprite cachedPlateAuraSprite;

    private void Awake()
    {
        pressurePlate = GetComponent<PressurePlate>();
        plateRenderer = GetComponent<SpriteRenderer>();
        AutoConfigureColors();
        SetupAura();
    }

    private void OnEnable()
    {
        if (pressurePlate == null) pressurePlate = GetComponent<PressurePlate>();
        if (plateRenderer == null) plateRenderer = GetComponent<SpriteRenderer>();
        AutoConfigureColors();
        SetupAura();
    }

    private void Start()
    {
        SetupAura();
    }

    private void AutoConfigureColors()
    {
        if (plateRenderer != null && plateRenderer.sprite != null)
        {
            string sName = plateRenderer.sprite.name;
            if (sName.Contains("62")) // Blue jewel switch (Level 3)
            {
                activeColor = new Color(0.15f, 0.85f, 1.00f, 0.92f); // Arcane Cyan
                dormantColor = new Color(0.15f, 0.85f, 1.00f, 0.25f);
            }
            else if (sName.Contains("61")) // Rune switches (Level 4, 5)
            {
                activeColor = new Color(0.25f, 1.00f, 0.85f, 0.92f); // Radiant Emerald-Cyan
                dormantColor = new Color(0.25f, 1.00f, 0.85f, 0.22f);
            }
            else // Stone plate (Level 1, 2)
            {
                activeColor = new Color(1.00f, 0.85f, 0.20f, 0.90f); // Radiant Golden Energy
                dormantColor = new Color(1.00f, 0.85f, 0.20f, 0.20f);
            }
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

        // Subtle point light for atmospheric illumination
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
                    // Beautiful rounded rune glow with intense core
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
}
