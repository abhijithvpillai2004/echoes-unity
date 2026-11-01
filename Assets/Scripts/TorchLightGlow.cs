using UnityEngine;

/// <summary>
/// Attaches to wall torches to render stylized, animated flame tongues
/// (matching the user's reference torch flame animation with leaping fire tendrils and sparks),
/// with a matching flame-shaped aura (NO circular halos).
/// Runs in Edit Mode ([ExecuteAlways]) so the animation is live in Scene & Game views.
/// </summary>
[ExecuteAlways]
public class TorchLightGlow : MonoBehaviour
{
    public enum FlameColorStyle
    {
        ClassicFire,    // Matches the golden/orange fire from the reference image
        MysticPurple,   // Matches the purple/magenta flame crystal (#D176D0)
        MysticBlue,     // Arcane sapphire/azure blue flame
        CrimsonRed      // Intense crimson scarlet/ruby red flame
    }

    [Header("Flame Style & Animation")]
    public FlameColorStyle flameStyle = FlameColorStyle.MysticBlue;
    public float animationFps = 12f;
    public float flameScale = 0.68f;
    public Vector3 flameOffset = new Vector3(0f, 0.18f, 0f);

    [Header("Flame-Shaped Aura (No Circles)")]
    public bool enableFlameAura = true;
    public float auraScale = 1.32f;
    public Color auraFireColor = new Color(1.0f, 0.30f, 0.05f, 0.40f);
    public Color auraPurpleColor = new Color(0.85f, 0.25f, 0.90f, 0.40f);
    public Color auraBlueColor = new Color(0.12f, 0.55f, 1.00f, 0.40f);
    public Color auraRedColor = new Color(1.00f, 0.15f, 0.10f, 0.42f);

    [Header("Organic Flame Flicker")]
    public bool enableFlicker = true;
    public float flickerSpeed = 5.5f;
    public float flickerAmount = 0.08f;

    private GameObject flameAnimObj;
    private SpriteRenderer flameRenderer;
    private GameObject auraObj;
    private SpriteRenderer auraRenderer;
    private Light pointLight;

    private Sprite[] fireSprites;
    private Sprite[] purpleSprites;
    private Sprite[] blueSprites;
    private Sprite[] redSprites;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private float noiseSeed;

    private void Awake()
    {
        noiseSeed = Random.Range(0f, 100f);
        LoadSprites();
        SetupLighting();
    }

    private void OnEnable()
    {
        if (noiseSeed == 0f) noiseSeed = Random.Range(0f, 100f);
        LoadSprites();
        SetupLighting();
    }

    private void Start()
    {
        LoadSprites();
        SetupLighting();
    }

    private void LoadSprites()
    {
        if (fireSprites == null || fireSprites.Length == 0)
        {
            var assets = Resources.LoadAll<Sprite>("TorchFlames/torch_flame_fire");
            if (assets == null || assets.Length == 0)
            {
#if UNITY_EDITOR
                var editorAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/TorchFlames/torch_flame_fire.png");
                var list = new System.Collections.Generic.List<Sprite>();
                foreach (var a in editorAssets) if (a is Sprite s) list.Add(s);
                fireSprites = list.ToArray();
#endif
            }
            else fireSprites = assets;
        }

        if (purpleSprites == null || purpleSprites.Length == 0)
        {
            var assets = Resources.LoadAll<Sprite>("TorchFlames/torch_flame_purple");
            if (assets == null || assets.Length == 0)
            {
#if UNITY_EDITOR
                var editorAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/TorchFlames/torch_flame_purple.png");
                var list = new System.Collections.Generic.List<Sprite>();
                foreach (var a in editorAssets) if (a is Sprite s) list.Add(s);
                purpleSprites = list.ToArray();
#endif
            }
            else purpleSprites = assets;
        }

        if (blueSprites == null || blueSprites.Length == 0)
        {
            var assets = Resources.LoadAll<Sprite>("TorchFlames/torch_flame_blue");
            if (assets == null || assets.Length == 0)
            {
#if UNITY_EDITOR
                var editorAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/TorchFlames/torch_flame_blue.png");
                var list = new System.Collections.Generic.List<Sprite>();
                foreach (var a in editorAssets) if (a is Sprite s) list.Add(s);
                blueSprites = list.ToArray();
#endif
            }
            else blueSprites = assets;
        }

        if (redSprites == null || redSprites.Length == 0)
        {
            var assets = Resources.LoadAll<Sprite>("TorchFlames/torch_flame_red");
            if (assets == null || assets.Length == 0)
            {
#if UNITY_EDITOR
                var editorAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/TorchFlames/torch_flame_red.png");
                var list = new System.Collections.Generic.List<Sprite>();
                foreach (var a in editorAssets) if (a is Sprite s) list.Add(s);
                redSprites = list.ToArray();
#endif
            }
            else redSprites = assets;
        }
    }

    public void SetupLighting()
    {
        // 1. Destroy any legacy circular halo if present
        Transform oldHalo = transform.Find("TorchHalo");
        if (oldHalo != null)
        {
            if (Application.isPlaying) Destroy(oldHalo.gameObject);
            else DestroyImmediate(oldHalo.gameObject);
        }

        Transform oldCore = transform.Find("TorchCore");
        if (oldCore != null)
        {
            if (Application.isPlaying) Destroy(oldCore.gameObject);
            else DestroyImmediate(oldCore.gameObject);
        }

        // 2. Flame-Shaped Aura Object (outlines the flame with soft ambient fire light, strictly flame-shaped)
        Transform aTrans = transform.Find("FlameAura");
        if (aTrans != null) auraObj = aTrans.gameObject;
        else
        {
            auraObj = new GameObject("FlameAura");
            auraObj.transform.SetParent(transform, false);
        }
        auraObj.transform.localPosition = flameOffset + new Vector3(0f, 0.03f, 0f);
        auraObj.transform.localScale = Vector3.one * (flameScale * auraScale);

        auraRenderer = auraObj.GetComponent<SpriteRenderer>();
        if (auraRenderer == null) auraRenderer = auraObj.AddComponent<SpriteRenderer>();
        auraRenderer.sortingOrder = 9; // In front of wall bricks, behind torch bracket & flame
        auraObj.SetActive(enableFlameAura);

        // 3. Foreground Animated Flame Object
        Transform fTrans = transform.Find("FlameAnimation");
        if (fTrans != null) flameAnimObj = fTrans.gameObject;
        else
        {
            flameAnimObj = new GameObject("FlameAnimation");
            flameAnimObj.transform.SetParent(transform, false);
        }
        flameAnimObj.transform.localPosition = flameOffset;
        flameAnimObj.transform.localScale = Vector3.one * flameScale;

        flameRenderer = flameAnimObj.GetComponent<SpriteRenderer>();
        if (flameRenderer == null) flameRenderer = flameAnimObj.AddComponent<SpriteRenderer>();
        flameRenderer.sortingOrder = 12; // In front of torch bracket and wall

        // 4. Subtle point light for engine lighting
        pointLight = GetComponent<Light>();
        if (pointLight == null) pointLight = gameObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.range = 3.0f;
        pointLight.intensity = 1.8f;
        pointLight.renderMode = LightRenderMode.ForcePixel;

        UpdateFlameAppearance();
    }

    private void Update()
    {
        if (flameAnimObj == null || auraObj == null)
        {
            SetupLighting();
            return;
        }

        // Advance flame animation frames
        float dt = Application.isPlaying ? Time.deltaTime : 0.033f;
        animTimer += dt;
        float frameInterval = 1f / Mathf.Max(1f, animationFps);

        if (animTimer >= frameInterval)
        {
            animTimer -= frameInterval;
            currentFrame = (currentFrame + 1) % 10;
            UpdateFlameAppearance();
        }

        // Organic fire flicker
        if (enableFlicker)
        {
            float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
            float n1 = Mathf.Sin(t * flickerSpeed + noiseSeed) * 0.5f;
            float n2 = Mathf.PerlinNoise(t * flickerSpeed * 1.5f, noiseSeed) - 0.5f;
            float flicker = (n1 + n2 * 2f) * flickerAmount;

            if (auraObj != null && enableFlameAura)
            {
                auraObj.transform.localScale = Vector3.one * (flameScale * auraScale * (1f + flicker * 0.5f));
                Color baseAura = GetCurrentAuraColor();
                Color ac = baseAura;
                ac.a = Mathf.Clamp01(baseAura.a + flicker * 0.4f);
                if (auraRenderer != null) auraRenderer.color = ac;
            }

            if (pointLight != null)
            {
                pointLight.intensity = Mathf.Max(0.5f, 1.8f + flicker * 1.0f);
            }
        }
    }

    public Color GetCurrentAuraColor()
    {
        switch (flameStyle)
        {
            case FlameColorStyle.MysticPurple: return auraPurpleColor;
            case FlameColorStyle.MysticBlue: return auraBlueColor;
            case FlameColorStyle.CrimsonRed: return auraRedColor;
            default: return auraFireColor;
        }
    }

    public Sprite[] GetActiveSprites()
    {
        switch (flameStyle)
        {
            case FlameColorStyle.MysticPurple: return purpleSprites;
            case FlameColorStyle.MysticBlue: return blueSprites;
            case FlameColorStyle.CrimsonRed: return redSprites;
            default: return fireSprites;
        }
    }

    public Color GetLightColor()
    {
        switch (flameStyle)
        {
            case FlameColorStyle.MysticPurple: return new Color(0.92f, 0.48f, 0.95f);
            case FlameColorStyle.MysticBlue: return new Color(0.20f, 0.65f, 1.00f);
            case FlameColorStyle.CrimsonRed: return new Color(1.00f, 0.18f, 0.12f);
            default: return new Color(1.00f, 0.65f, 0.20f);
        }
    }

    public void UpdateFlameAppearance()
    {
        Sprite[] activeSprites = GetActiveSprites();
        if (activeSprites != null && activeSprites.Length > 0)
        {
            int idx = Mathf.Clamp(currentFrame, 0, activeSprites.Length - 1);
            Sprite s = activeSprites[idx];

            if (flameRenderer != null)
            {
                flameRenderer.sprite = s;
            }

            if (auraRenderer != null)
            {
                auraRenderer.sprite = s;
                auraRenderer.color = GetCurrentAuraColor();
            }
        }

        if (pointLight != null)
        {
            pointLight.color = GetLightColor();
        }
    }
}
