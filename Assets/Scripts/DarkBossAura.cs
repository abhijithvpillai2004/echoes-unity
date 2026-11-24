using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atmospheric visual effects for the Level 5 Final Boss (The Dark Echo / Corrupted Shadow Archmage):
/// 1. Sprite Setup: Ensures the Dark Mage sprite (tilemap_packed_111) is applied.
/// 2. Full-Body Purple Flame Aura: The game's animated purple fire enveloping his entire body
///    (flames surging along his left, right, and head/cowl silhouette).
/// 3. Piercing Eye Glint: Menacing ruby pulse enhancing his glowing red eyes.
/// 4. Combat Health Bar: Ultra-tiny 3-pip bar that stays hidden normally and only appears briefly
///    when the boss attacks or takes damage.
/// 5. Defeat Cleanup: Extinguishes fire and effects upon defeat.
/// Runs in Edit Mode ([ExecuteAlways]) for live preview in Scene & Game views.
/// </summary>
[ExecuteAlways]
public class DarkBossAura : MonoBehaviour
{
    [Header("Aura Flame Config")]
    public float flameAnimationFps = 8f; // Balanced, fluid, imposing boss aura cadence (1.25s loop)
    public Color innerFlameColor = new Color(0.92f, 0.75f, 1.0f, 0.95f); // Light glowing lilac inner core
    public Color outerFlameColor = new Color(0.38f, 0.08f, 0.60f, 1.0f); // Deep dark purple outer rim & tips

    [Header("Piercing Eye Glow")]
    public bool enableEyeGlow = true;
    public Color eyeGlowColor = new Color(1.0f, 0.15f, 0.25f, 0.95f);

    [Header("Combat Health Bar Config")]
    public float healthBarDisplayDuration = 2.0f;

    private CombatEnemy combatEnemy;
    private SpriteRenderer bossRenderer;

    private GameObject flameRootObj;
    private SpriteRenderer srOutLeft;
    private SpriteRenderer srOutRight;
    private SpriteRenderer srOutCenter;
    private SpriteRenderer srInLeft;
    private SpriteRenderer srInRight;
    private SpriteRenderer srInCenter;

    private GameObject eyeGlowObj;
    private SpriteRenderer eyeGlowRenderer;

    private GameObject healthBarObj;
    private SpriteRenderer healthBarRenderer;

    private float flameTimer = 0f;
    private int currentFlameFrame = 0;
    private float healthBarVisibleTimer = 0f;

    private static Sprite[] cachedPurpleFlameSprites;
    private static Sprite[] cachedHealthBarSprites;
    private static Sprite cachedEyeGlintSprite;
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

        if (combatEnemy != null)
        {
            combatEnemy.maxEnemyHP = 3;
            if (combatEnemy.currentEnemyHP <= 0 || combatEnemy.currentEnemyHP > 3)
            {
                combatEnemy.currentEnemyHP = 3;
            }
        }

        CleanupLegacyObjects();
        EnsureDarkMageSprite();

        SetupFullBodyFlame();
        SetupEyeGlow();
        SetupHealthBar();
    }

    private void CleanupLegacyObjects()
    {
        string[] legacyNames = new string[]
        {
            "DarkBossVoidAura",
            "DarkBossShadowPool",
            "DarkBossFlameAura",
            "DarkBossEnergyFlame",
            "DarkBossEnergyFlameFront",
            "EmberContainer",
            "GlowAura",
            "TorchHalo",
            "TorchCore"
        };

        foreach (string n in legacyNames)
        {
            Transform t = transform.Find(n);
            if (t != null)
            {
                if (Application.isPlaying) Destroy(t.gameObject);
                else DestroyImmediate(t.gameObject);
            }
        }

        EntityGlow eg = GetComponent<EntityGlow>();
        if (eg != null)
        {
            if (Application.isPlaying) Destroy(eg);
            else DestroyImmediate(eg);
        }
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
                bossRenderer.sortingOrder = 10;
            }
        }
    }

    private void SetupFullBodyFlame()
    {
        EnsurePurpleFlameSprites();

        Transform existing = transform.Find("BossFullBodyFlame");
        if (existing != null) flameRootObj = existing.gameObject;
        else
        {
            flameRootObj = new GameObject("BossFullBodyFlame");
            flameRootObj.transform.SetParent(transform, false);
            flameRootObj.transform.localPosition = Vector3.zero;
        }

        // Clean any old single-layer objects
        string[] oldNames = new string[] { "FlameLeft", "FlameRight", "FlameCenter" };
        foreach (var oldN in oldNames)
        {
            Transform ot = flameRootObj.transform.Find(oldN);
            if (ot != null)
            {
                if (Application.isPlaying) Destroy(ot.gameObject);
                else DestroyImmediate(ot.gameObject);
            }
        }

        // 1. OUTER FLAME LAYER (Sorting order 8: Deep dark purple outer shell & tips)
        Transform tOutLeft = flameRootObj.transform.Find("OutLeft");
        GameObject goOutLeft = (tOutLeft != null) ? tOutLeft.gameObject : new GameObject("OutLeft");
        goOutLeft.transform.SetParent(flameRootObj.transform, false);
        goOutLeft.transform.localPosition = new Vector3(-0.32f, -0.45f, 0f);
        goOutLeft.transform.localScale = new Vector3(1.48f, 1.25f, 1f);
        srOutLeft = goOutLeft.GetComponent<SpriteRenderer>();
        if (srOutLeft == null) srOutLeft = goOutLeft.AddComponent<SpriteRenderer>();
        srOutLeft.sortingOrder = 8;
        srOutLeft.color = outerFlameColor;

        Transform tOutRight = flameRootObj.transform.Find("OutRight");
        GameObject goOutRight = (tOutRight != null) ? tOutRight.gameObject : new GameObject("OutRight");
        goOutRight.transform.SetParent(flameRootObj.transform, false);
        goOutRight.transform.localPosition = new Vector3(0.32f, -0.45f, 0f);
        goOutRight.transform.localScale = new Vector3(1.48f, 1.25f, 1f);
        srOutRight = goOutRight.GetComponent<SpriteRenderer>();
        if (srOutRight == null) srOutRight = goOutRight.AddComponent<SpriteRenderer>();
        srOutRight.sortingOrder = 8;
        srOutRight.color = outerFlameColor;

        Transform tOutCenter = flameRootObj.transform.Find("OutCenter");
        GameObject goOutCenter = (tOutCenter != null) ? tOutCenter.gameObject : new GameObject("OutCenter");
        goOutCenter.transform.SetParent(flameRootObj.transform, false);
        goOutCenter.transform.localPosition = new Vector3(0.02f, -0.01f, 0f);
        goOutCenter.transform.localScale = new Vector3(1.38f, 0.98f, 1f);
        srOutCenter = goOutCenter.GetComponent<SpriteRenderer>();
        if (srOutCenter == null) srOutCenter = goOutCenter.AddComponent<SpriteRenderer>();
        srOutCenter.sortingOrder = 8;
        srOutCenter.color = outerFlameColor;

        // 2. INNER FLAME LAYER (Sorting order 9: Light glowing lilac/lavender inner core)
        Transform tInLeft = flameRootObj.transform.Find("InLeft");
        GameObject goInLeft = (tInLeft != null) ? tInLeft.gameObject : new GameObject("InLeft");
        goInLeft.transform.SetParent(flameRootObj.transform, false);
        goInLeft.transform.localPosition = new Vector3(-0.28f, -0.44f, 0f);
        goInLeft.transform.localScale = new Vector3(1.22f, 1.05f, 1f);
        srInLeft = goInLeft.GetComponent<SpriteRenderer>();
        if (srInLeft == null) srInLeft = goInLeft.AddComponent<SpriteRenderer>();
        srInLeft.sortingOrder = 9;
        srInLeft.color = innerFlameColor;

        Transform tInRight = flameRootObj.transform.Find("InRight");
        GameObject goInRight = (tInRight != null) ? tInRight.gameObject : new GameObject("InRight");
        goInRight.transform.SetParent(flameRootObj.transform, false);
        goInRight.transform.localPosition = new Vector3(0.28f, -0.44f, 0f);
        goInRight.transform.localScale = new Vector3(1.22f, 1.05f, 1f);
        srInRight = goInRight.GetComponent<SpriteRenderer>();
        if (srInRight == null) srInRight = goInRight.AddComponent<SpriteRenderer>();
        srInRight.sortingOrder = 9;
        srInRight.color = innerFlameColor;

        Transform tInCenter = flameRootObj.transform.Find("InCenter");
        GameObject goInCenter = (tInCenter != null) ? tInCenter.gameObject : new GameObject("InCenter");
        goInCenter.transform.SetParent(flameRootObj.transform, false);
        goInCenter.transform.localPosition = new Vector3(0.02f, -0.03f, 0f);
        goInCenter.transform.localScale = new Vector3(1.15f, 0.82f, 1f);
        srInCenter = goInCenter.GetComponent<SpriteRenderer>();
        if (srInCenter == null) srInCenter = goInCenter.AddComponent<SpriteRenderer>();
        srInCenter.sortingOrder = 9;
        srInCenter.color = innerFlameColor;

        UpdateFlameFrames();
    }

    private void EnsurePurpleFlameSprites()
    {
        if (cachedPurpleFlameSprites != null && cachedPurpleFlameSprites.Length == 10) return;

        cachedPurpleFlameSprites = new Sprite[10];
#if UNITY_EDITOR
        var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/TorchFlames/torch_flame_purple.png");
        for (int i = 0; i < 10; i++)
        {
            foreach (var a in all)
            {
                if (a is Sprite && a.name == "torch_flame_purple_" + i)
                {
                    cachedPurpleFlameSprites[i] = (Sprite)a;
                    break;
                }
            }
        }
#endif
        if (cachedPurpleFlameSprites[0] == null)
        {
            var resSprites = Resources.LoadAll<Sprite>("TorchFlames/torch_flame_purple");
            for (int i = 0; i < 10; i++)
            {
                foreach (var s in resSprites)
                {
                    if (s.name == "torch_flame_purple_" + i)
                    {
                        cachedPurpleFlameSprites[i] = s;
                        break;
                    }
                }
            }
        }
    }

    private void UpdateFlameFrames()
    {
        if (cachedPurpleFlameSprites == null || cachedPurpleFlameSprites[0] == null) return;

        int count = cachedPurpleFlameSprites.Length;
        int fLeft = currentFlameFrame % count;
        int fRight = (currentFlameFrame + 4) % count;
        int fCenter = (currentFlameFrame + 7) % count;

        Sprite spLeft = cachedPurpleFlameSprites[fLeft];
        Sprite spRight = cachedPurpleFlameSprites[fRight];
        Sprite spCenter = cachedPurpleFlameSprites[fCenter];

        if (srOutLeft != null) srOutLeft.sprite = spLeft;
        if (srInLeft != null) srInLeft.sprite = spLeft;

        if (srOutRight != null) srOutRight.sprite = spRight;
        if (srInRight != null) srInRight.sprite = spRight;

        if (srOutCenter != null) srOutCenter.sprite = spCenter;
        if (srInCenter != null) srInCenter.sprite = spCenter;
    }

    private void SetupEyeGlow()
    {
        if (!enableEyeGlow) return;

        Transform existing = transform.Find("DarkBossEyeGlow");
        if (existing != null) eyeGlowObj = existing.gameObject;
        else
        {
            eyeGlowObj = new GameObject("DarkBossEyeGlow");
            eyeGlowObj.transform.SetParent(transform, false);
            eyeGlowObj.transform.localPosition = new Vector3(0f, 0.035f, 0f);
        }

        eyeGlowRenderer = eyeGlowObj.GetComponent<SpriteRenderer>();
        if (eyeGlowRenderer == null) eyeGlowRenderer = eyeGlowObj.AddComponent<SpriteRenderer>();

        if (cachedEyeGlintSprite == null) cachedEyeGlintSprite = CreateEyeGlintSprite();
        eyeGlowRenderer.sprite = cachedEyeGlintSprite;
        eyeGlowRenderer.sortingOrder = 12; // Placed right over his eyes
        eyeGlowRenderer.color = eyeGlowColor;
        eyeGlowObj.transform.localScale = new Vector3(0.55f, 0.30f, 1f);
    }

    private void SetupHealthBar()
    {
        Transform existing = transform.Find("BossHealthBar");
        if (existing != null) healthBarObj = existing.gameObject;
        else
        {
            healthBarObj = new GameObject("BossHealthBar");
            healthBarObj.transform.SetParent(transform, false);
        }
        healthBarObj.transform.localPosition = new Vector3(0f, 0.54f, 0f);
        healthBarObj.transform.localScale = Vector3.one;

        healthBarRenderer = healthBarObj.GetComponent<SpriteRenderer>();
        if (healthBarRenderer == null) healthBarRenderer = healthBarObj.AddComponent<SpriteRenderer>();
        healthBarRenderer.sortingOrder = 15;

        // Hidden by default
        healthBarObj.SetActive(false);
    }

    /// <summary>
    /// Triggers the mini health bar to appear during combat (when attacking or taking damage).
    /// </summary>
    public void ShowHealthBarBriefly()
    {
        healthBarVisibleTimer = healthBarDisplayDuration;
        if (healthBarObj != null) healthBarObj.SetActive(true);
        UpdateHealthBarVisual();
    }

    private void Update()
    {
        EnsureDarkMageSprite();

        bool isDead = (combatEnemy != null && combatEnemy.IsDead);
        if (isDead)
        {
            SetAuraActive(false);
            return;
        }

        SetAuraActive(true);

        // 1. Advance Full-Body Purple Flame Animation (10 FPS)
        float dt = Application.isPlaying ? Time.deltaTime : 0.033f;
        flameTimer += dt;
        float frameInterval = 1f / Mathf.Max(1f, flameAnimationFps);

        if (flameTimer >= frameInterval)
        {
            flameTimer -= frameInterval;
            currentFlameFrame = (currentFlameFrame + 1) % 10;
            UpdateFlameFrames();
        }

        // 2. Animate Piercing Eye Glow
        float curTime = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
        if (eyeGlowObj != null && eyeGlowRenderer != null)
        {
            float eyePulse = (Mathf.Sin(curTime * 4.5f) + 1f) * 0.5f;
            float eyeScaleX = Mathf.Lerp(0.50f, 0.70f, eyePulse);
            float eyeScaleY = Mathf.Lerp(0.28f, 0.40f, eyePulse);
            eyeGlowObj.transform.localScale = new Vector3(eyeScaleX, eyeScaleY, 1f);

            Color c = eyeGlowColor;
            c.a = Mathf.Lerp(0.65f, 1.0f, eyePulse);
            eyeGlowRenderer.color = c;
        }

        // 3. Health Bar Visibility Countdown
        if (healthBarVisibleTimer > 0f)
        {
            healthBarVisibleTimer -= dt;
            if (healthBarObj != null && !healthBarObj.activeSelf)
            {
                healthBarObj.SetActive(true);
            }
            UpdateHealthBarVisual();

            if (healthBarVisibleTimer <= 0f)
            {
                if (healthBarObj != null) healthBarObj.SetActive(false);
            }
        }
        else
        {
            if (healthBarObj != null && healthBarObj.activeSelf)
            {
                healthBarObj.SetActive(false);
            }
        }
    }

    private void UpdateHealthBarVisual()
    {
        if (combatEnemy != null && healthBarRenderer != null)
        {
            int hp = Mathf.Clamp(combatEnemy.currentEnemyHP, 0, combatEnemy.maxEnemyHP);
            healthBarRenderer.sprite = GetHealthBarSprite(hp, combatEnemy.maxEnemyHP);
        }
    }

    private void SetAuraActive(bool active)
    {
        if (flameRootObj != null && flameRootObj.activeSelf != active) flameRootObj.SetActive(active);
        if (eyeGlowObj != null && eyeGlowObj.activeSelf != active) eyeGlowObj.SetActive(active);
        if (!active && healthBarObj != null && healthBarObj.activeSelf) healthBarObj.SetActive(false);
    }

    private static Sprite GetHealthBarSprite(int hp, int maxHp)
    {
        if (cachedHealthBarSprites == null || cachedHealthBarSprites.Length != (maxHp + 1))
        {
            cachedHealthBarSprites = new Sprite[maxHp + 1];
        }

        hp = Mathf.Clamp(hp, 0, maxHp);
        if (cachedHealthBarSprites[hp] == null)
        {
            cachedHealthBarSprites[hp] = CreateHealthBarSprite(hp, maxHp);
        }
        return cachedHealthBarSprites[hp];
    }

    private static Sprite CreateHealthBarSprite(int currentHP, int maxHP)
    {
        // Super tiny 13x3 pixel health bar
        int w = 13;
        int h = 3;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color border = new Color(0.10f, 0.05f, 0.12f, 0.95f);
        Color emptyPip = new Color(0.25f, 0.10f, 0.18f, 0.85f);
        Color fillPip = new Color(0.95f, 0.15f, 0.30f, 1.0f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    tex.SetPixel(x, y, border);
                }
                else if (x == 4 || x == 8)
                {
                    tex.SetPixel(x, y, border);
                }
                else
                {
                    int seg = (x < 4) ? 0 : (x < 8 ? 1 : 2);
                    tex.SetPixel(x, y, (seg < currentHP) ? fillPip : emptyPip);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f);
    }

    private static Sprite CreateEyeGlintSprite()
    {
        int width = 48;
        int height = 24;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float nx = (x - center.x) / (width * 0.48f);
                float ny = (y - center.y) / (height * 0.48f);
                float d = (nx * nx) + (ny * ny);

                if (d <= 1f)
                {
                    float factor = 1f - Mathf.Sqrt(d);
                    float alpha = Mathf.Pow(factor, 1.2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 48f);
    }
}
