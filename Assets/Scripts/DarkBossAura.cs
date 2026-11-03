using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atmospheric visual effects for the Level 5 Final Boss (The Dark Echo / Corrupted Shadow Archmage):
/// 1. Sprite Setup: Ensures the Dark Mage sprite (tilemap_packed_111) is applied.
/// 2. Void Halo Aura: Deep pulsing violet/crimson aura radiating behind the dark wizard.
/// 3. Eerie Point Light: Atmospheric crimson illumination bathing the north boss chamber.
/// 4. Shadow Ember Motes: Floating dark particles rising from the corrupted robes.
/// 5. Defeat Dissolve: Disperses shadow particles upon defeat.
/// Runs in Edit Mode ([ExecuteAlways]) for live preview.
/// </summary>
[ExecuteAlways]
public class DarkBossAura : MonoBehaviour
{
    [Header("Aura Config")]
    public Color voidAuraColor = new Color(0.55f, 0.08f, 0.75f, 0.55f); // Deep Void Violet
    public Color emberColor = new Color(0.95f, 0.15f, 0.30f, 0.85f);    // Crimson Ember
    public float pulseSpeed = 2.8f;
    public float minScale = 1.35f;
    public float maxScale = 1.65f;

    private CombatEnemy combatEnemy;
    private SpriteRenderer bossRenderer;
    private GameObject auraObj;
    private SpriteRenderer auraRenderer;
    private Light bossLight;

    private float emberTimer = 0f;
    private static Sprite cachedVoidAuraSprite;
    private static Sprite cachedEmberSprite;
    private static Sprite cachedDarkMageSprite;

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

    public void InitializeComponents()
    {
        combatEnemy = GetComponent<CombatEnemy>();
        bossRenderer = GetComponent<SpriteRenderer>();

        EnsureDarkMageSprite();
        SetupVoidAura();
        SetupBossLight();
    }

    private void EnsureDarkMageSprite()
    {
        if (bossRenderer == null) bossRenderer = GetComponent<SpriteRenderer>();
        if (bossRenderer != null)
        {
            if (cachedDarkMageSprite == null)
            {
                var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var s in sprites)
                {
                    if (s.name == "tilemap_packed_111")
                    {
                        cachedDarkMageSprite = s;
                        break;
                    }
                }
#if UNITY_EDITOR
                if (cachedDarkMageSprite == null)
                {
                    var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Tilesets/tilemap_packed.png");
                    foreach (var a in all)
                    {
                        if (a is Sprite && a.name == "tilemap_packed_111")
                        {
                            cachedDarkMageSprite = (Sprite)a;
                            break;
                        }
                    }
                }
#endif
            }

            if (cachedDarkMageSprite != null)
            {
                bossRenderer.sprite = cachedDarkMageSprite;
                bossRenderer.color = Color.white;
                bossRenderer.sortingOrder = 10; // Sits above floor and chest
            }
        }
    }

    private void SetupVoidAura()
    {
        Transform existing = transform.Find("DarkBossVoidAura");
        if (existing != null) auraObj = existing.gameObject;
        else
        {
            auraObj = new GameObject("DarkBossVoidAura");
            auraObj.transform.SetParent(transform, false);
            auraObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        }

        auraRenderer = auraObj.GetComponent<SpriteRenderer>();
        if (auraRenderer == null) auraRenderer = auraObj.AddComponent<SpriteRenderer>();

        if (cachedVoidAuraSprite == null) cachedVoidAuraSprite = CreateVoidAuraSprite();
        auraRenderer.sprite = cachedVoidAuraSprite;
        auraRenderer.sortingOrder = (bossRenderer != null ? bossRenderer.sortingOrder - 1 : 9);
        auraRenderer.color = voidAuraColor;
    }

    private void SetupBossLight()
    {
        bossLight = GetComponent<Light>();
        if (bossLight == null) bossLight = gameObject.AddComponent<Light>();

        bossLight.type = LightType.Point;
        bossLight.range = 3.2f;
        bossLight.intensity = 1.0f;
        bossLight.color = new Color(0.95f, 0.15f, 0.35f); // Eerie Crimson
        bossLight.renderMode = LightRenderMode.ForcePixel;
    }

    private void Update()
    {
        EnsureDarkMageSprite();

        bool isDead = (combatEnemy != null && combatEnemy.IsDead);

        if (isDead)
        {
            if (auraObj != null && auraObj.activeSelf) auraObj.SetActive(false);
            if (bossLight != null && bossLight.enabled) bossLight.enabled = false;
            return;
        }

        if (auraObj != null && !auraObj.activeSelf) auraObj.SetActive(true);
        if (bossLight != null && !bossLight.enabled) bossLight.enabled = true;

        float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
        float pulse = (Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f;

        // Animate void halo aura breathing
        if (auraObj != null && auraRenderer != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, pulse);
            auraObj.transform.localScale = Vector3.one * scale;

            Color c = voidAuraColor;
            c.a = Mathf.Lerp(0.40f, 0.75f, pulse);
            auraRenderer.color = c;
        }

        // Animate dynamic point light
        if (bossLight != null)
        {
            bossLight.intensity = Mathf.Lerp(0.85f, 1.45f, pulse);
        }

        // Spawn shadow ember particles in play mode
        if (Application.isPlaying)
        {
            emberTimer += Time.deltaTime;
            if (emberTimer >= 0.18f)
            {
                emberTimer = 0f;
                SpawnShadowEmber();
            }
        }
    }

    private void SpawnShadowEmber()
    {
        if (cachedEmberSprite == null) cachedEmberSprite = CreateEmberSprite();

        GameObject emberObj = new GameObject("BossShadowEmber");
        emberObj.transform.position = transform.position + new Vector3(
            Random.Range(-0.25f, 0.25f),
            Random.Range(-0.25f, 0.05f),
            0f
        );
        emberObj.transform.localScale = Vector3.zero;

        SpriteRenderer sr = emberObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedEmberSprite;
        sr.sortingOrder = (bossRenderer != null ? bossRenderer.sortingOrder + 1 : 11);
        sr.color = emberColor;

        Vector3 velocity = new Vector3(
            Random.Range(-0.15f, 0.15f),
            Random.Range(0.35f, 0.75f),
            0f
        );

        StartCoroutine(AnimateEmber(emberObj, sr, velocity, 0.40f));
    }

    private IEnumerator AnimateEmber(GameObject obj, SpriteRenderer sr, Vector3 velocity, float duration)
    {
        float timer = 0f;
        float maxScale = Random.Range(0.25f, 0.40f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (obj != null && sr != null)
            {
                obj.transform.position += velocity * Time.deltaTime;

                float scale = Mathf.Sin(t * Mathf.PI) * maxScale;
                obj.transform.localScale = Vector3.one * scale;

                Color c = emberColor;
                c.a = Mathf.Lerp(emberColor.a, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private static Sprite CreateVoidAuraSprite()
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
                    float alpha = Mathf.Pow(factor, 1.4f);
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

    private static Sprite CreateEmberSprite()
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
                    float alpha = Mathf.Pow(1.0f - val, 0.70f);
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
