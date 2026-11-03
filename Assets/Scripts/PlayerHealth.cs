using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages player health with a granular 18-unit system (3 hearts x 6 units each),
/// turn-based poison status effects from spike traps, smooth heart drain animations,
/// and antidote potion curing.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    private static PlayerHealth _instance;
    public static PlayerHealth Instance
    {
        get
        {
            if (_instance == null) _instance = Object.FindFirstObjectByType<PlayerHealth>();
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Granular Health (6 units per heart, 3 hearts = 18 units)")]
    public int unitsPerHeart = 6;
    public int maxHearts = 3;
    public int maxUnits = 18;
    public int currentUnits = 18;

    [Header("Poison Settings")]
    public bool isPoisoned = false;
    public int poisonDamagePerTurn = 2; // -2 units per move = 9 moves to find antidote
    private bool poisonedThisTurn = false;

    [Header("UI References")]
    public GameObject healthUIContainer;
    public Image[] heartImages; // Array of 3 base heart Image components
    public Sprite fullHeartSprite;
    public Sprite brokenHeartSprite;

    // Sub-unit filled layer for each heart
    private Image[] heartFillImages;
    private Coroutine[] heartDrainCoroutines;
    private Coroutine cureFlashCoroutine;
    private GameObject playerPoisonFX;

    // Backwards compatibility property
    public int currentHP
    {
        get => Mathf.CeilToInt((float)currentUnits / unitsPerHeart);
        set => currentUnits = Mathf.Clamp(value * unitsPerHeart, 0, maxUnits);
    }
    public int maxHP
    {
        get => maxHearts;
        set { maxHearts = value; maxUnits = maxHearts * unitsPerHeart; }
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(this);

        SetupHeartFillLayers();
    }

    private void Start()
    {
        CheckHealthUIVisibility();
        SetupHeartFillLayers();
        ResetHealth();
    }

    public void SetupHeartFillLayers()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        heartFillImages = new Image[heartImages.Length];
        heartDrainCoroutines = new Coroutine[heartImages.Length];

        for (int i = 0; i < heartImages.Length; i++)
        {
            var baseImg = heartImages[i];
            if (baseImg == null) continue;

            // Base image displays dark/broken heart background
            if (brokenHeartSprite != null) baseImg.sprite = brokenHeartSprite;
            baseImg.color = new Color(0.35f, 0.35f, 0.35f, 0.75f);

            // Child "Fill" image displays the full heart with vertical fill
            Transform existingFill = baseImg.transform.Find("Fill");
            GameObject fillObj;
            if (existingFill != null)
            {
                fillObj = existingFill.gameObject;
            }
            else
            {
                fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(baseImg.transform, false);
            }

            var rt = fillObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var fillImg = fillObj.GetComponent<Image>();
            if (fullHeartSprite != null) fillImg.sprite = fullHeartSprite;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;
            fillImg.fillOrigin = (int)Image.OriginVertical.Bottom;
            fillImg.color = Color.white;

            heartFillImages[i] = fillImg;
        }
    }

    public void CheckHealthUIVisibility()
    {
        CombatEnemy[] combatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        TrapTile[] traps = Object.FindObjectsByType<TrapTile>(FindObjectsSortMode.None);
        bool hasCombatOrTraps = (combatEnemies != null && combatEnemies.Length > 0) || (traps != null && traps.Length > 0);

        if (healthUIContainer == null)
        {
            GameObject hc = GameObject.Find("HeartsContainer") ?? GameObject.Find("HealthContainer") ?? GameObject.Find("HealthUI");
            if (hc != null) healthUIContainer = hc;
            else if (heartImages != null && heartImages.Length > 0 && heartImages[0] != null)
            {
                healthUIContainer = heartImages[0].transform.parent?.gameObject;
            }
        }

        if (healthUIContainer != null)
        {
            healthUIContainer.SetActive(hasCombatOrTraps);
        }
    }

    public void ResetHealth()
    {
        isPoisoned = false;
        poisonedThisTurn = false;
        maxUnits = maxHearts * unitsPerHeart;
        currentUnits = maxUnits;

        CleanupPoisonFX();
        SetupHeartFillLayers();
        UpdateHeartsUI(instant: true);
    }

    /// <summary>
    /// Inflicts the toxic spike poison effect on the player.
    /// </summary>
    public void InflictPoison()
    {
        if (currentUnits <= 0) return;

        bool wasPoisoned = isPoisoned;
        isPoisoned = true;
        poisonedThisTurn = true;

        EnsurePoisonFX();

        // Prick damage on initial contact (-2 units)
        TakeDamageUnits(poisonDamagePerTurn);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPoisonDamage();
        }

        if (!wasPoisoned)
        {
            Debug.Log("<color=#00FF66><b>POISONED!</b> Stepped on toxic spikes! Health draining 2 units per step. Reach the antidote potion!</color>");
        }
    }

    /// <summary>
    /// Called when the player drinks the potion bottle to neutralize poison and restore full health.
    /// </summary>
    public void CurePoison()
    {
        isPoisoned = false;
        poisonedThisTurn = false;
        currentUnits = maxUnits;

        CleanupPoisonFX();
        UpdateHeartsUI(instant: false);

        if (cureFlashCoroutine != null) StopCoroutine(cureFlashCoroutine);
        cureFlashCoroutine = StartCoroutine(CureCleanseFlashRoutine());

        Debug.Log("<color=green><b>ANTIDOTE CONSUMED!</b> Poison cleansed and full health restored!</color>");
    }

    /// <summary>
    /// Ticks poison damage on each player movement / turn.
    /// </summary>
    public void OnPlayerStepTurn()
    {
        if (!isPoisoned || currentUnits <= 0) return;

        // If player stepped on spikes during this exact turn, poison already dealt contact damage
        if (poisonedThisTurn)
        {
            poisonedThisTurn = false;
            return;
        }

        Debug.Log($"<color=#00FF66>Poison ticks -{poisonDamagePerTurn} units! ({currentUnits - poisonDamagePerTurn}/{maxUnits})</color>");
        TakeDamageUnits(poisonDamagePerTurn);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPoisonDamage();
        }

        StartCoroutine(FlashPlayerPoisonDamage());
    }

    /// <summary>
    /// Deals discrete heart damage (from enemies, e.g. 1 damage = 1 full heart = 6 units).
    /// </summary>
    public void TakeDamage(int hearts)
    {
        TakeDamageUnits(hearts * unitsPerHeart);
    }

    /// <summary>
    /// Granular damage subtraction with smooth animated heart drain.
    /// </summary>
    public void TakeDamageUnits(int units)
    {
        if (currentUnits <= 0) return;

        int prevUnits = currentUnits;
        currentUnits = Mathf.Max(0, currentUnits - units);

        Debug.Log($"<color=red>Player took {units} HP units damage! HP: {currentUnits}/{maxUnits} ({currentHP} Hearts)</color>");
        UpdateHeartsUI(instant: false);

        if (currentUnits <= 0)
        {
            Die();
        }
    }

    public void Heal(int hearts)
    {
        HealUnits(hearts * unitsPerHeart);
    }

    public void HealUnits(int units)
    {
        if (currentUnits <= 0) return;

        currentUnits = Mathf.Min(maxUnits, currentUnits + units);
        Debug.Log($"<color=green>Player restored +{units} HP units! HP: {currentUnits}/{maxUnits}</color>");
        UpdateHeartsUI(instant: false);
    }

    public void UpdateHeartsUI(bool instant = false)
    {
        if (heartFillImages == null || heartFillImages.Length == 0)
        {
            SetupHeartFillLayers();
            if (heartFillImages == null || heartFillImages.Length == 0) return;
        }

        for (int i = 0; i < heartFillImages.Length; i++)
        {
            var fillImg = heartFillImages[i];
            if (fillImg == null) continue;

            // Calculate fill for this heart (0.0 to 1.0)
            int minUnits = i * unitsPerHeart;
            int unitsInHeart = Mathf.Clamp(currentUnits - minUnits, 0, unitsPerHeart);
            float targetFill = (float)unitsInHeart / unitsPerHeart;

            if (instant)
            {
                fillImg.fillAmount = targetFill;
                fillImg.color = isPoisoned ? new Color(0.25f, 0.95f, 0.40f) : Color.white;
            }
            else
            {
                if (heartDrainCoroutines[i] != null) StopCoroutine(heartDrainCoroutines[i]);
                heartDrainCoroutines[i] = StartCoroutine(AnimateHeartFillRoutine(fillImg, targetFill));
            }
        }
    }

    private IEnumerator AnimateHeartFillRoutine(Image fillImg, float targetFill)
    {
        float startFill = fillImg.fillAmount;
        float elapsed = 0f;
        float duration = 0.28f;

        // Scale punch on the heart when it takes damage
        Transform heartTransform = fillImg.transform.parent;
        if (startFill > targetFill && heartTransform != null)
        {
            heartTransform.localScale = Vector3.one * 1.25f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth ease out
            fillImg.fillAmount = Mathf.Lerp(startFill, targetFill, Mathf.SmoothStep(0f, 1f, t));

            if (heartTransform != null)
            {
                heartTransform.localScale = Vector3.Lerp(Vector3.one * 1.25f, Vector3.one, t);
            }
            yield return null;
        }

        fillImg.fillAmount = targetFill;
        if (heartTransform != null) heartTransform.localScale = Vector3.one;
    }

    private IEnumerator CureCleanseFlashRoutine()
    {
        if (heartFillImages == null) yield break;

        // Bright sparkling cyan flash on cure
        for (int i = 0; i < heartFillImages.Length; i++)
        {
            if (heartFillImages[i] != null) heartFillImages[i].color = new Color(0.30f, 1.0f, 1.0f);
        }
        yield return new WaitForSeconds(0.20f);

        // Transition back to pure red/white
        float elapsed = 0f;
        while (elapsed < 0.30f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.30f;
            for (int i = 0; i < heartFillImages.Length; i++)
            {
                if (heartFillImages[i] != null)
                {
                    heartFillImages[i].color = Color.Lerp(new Color(0.30f, 1.0f, 1.0f), Color.white, t);
                }
            }
            yield return null;
        }

        for (int i = 0; i < heartFillImages.Length; i++)
        {
            if (heartFillImages[i] != null) heartFillImages[i].color = Color.white;
        }
    }

    private void Update()
    {
        // Animate pulsating toxic green on hearts while poisoned
        if (isPoisoned && heartFillImages != null)
        {
            float pulse = (Mathf.Sin(Time.time * 6.5f) + 1f) * 0.5f;
            Color toxicColor = Color.Lerp(new Color(0.20f, 0.95f, 0.35f), new Color(0.60f, 1.0f, 0.45f), pulse);

            for (int i = 0; i < heartFillImages.Length; i++)
            {
                if (heartFillImages[i] != null && heartFillImages[i].fillAmount > 0f)
                {
                    heartFillImages[i].color = toxicColor;
                }
            }
        }
    }

    private void EnsurePoisonFX()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        if (playerPoisonFX == null)
        {
            Transform existing = player.transform.Find("PoisonStatusFX");
            if (existing != null) playerPoisonFX = existing.gameObject;
        }

        if (playerPoisonFX == null)
        {
            playerPoisonFX = new GameObject("PoisonStatusFX");
            playerPoisonFX.transform.SetParent(player.transform, false);
            playerPoisonFX.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            var sr = playerPoisonFX.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 14;

            // Generate small toxic bubble aura sprite
            sr.sprite = CreatePoisonMoteSprite();
            sr.color = new Color(0.25f, 0.95f, 0.40f, 0.75f);
        }

        playerPoisonFX.SetActive(true);
    }

    private void CleanupPoisonFX()
    {
        if (playerPoisonFX != null)
        {
            playerPoisonFX.SetActive(false);
            if (Application.isPlaying) Destroy(playerPoisonFX);
            else DestroyImmediate(playerPoisonFX);
            playerPoisonFX = null;
        }
    }

    private IEnumerator FlashPlayerPoisonDamage()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;
        sr.color = new Color(0.35f, 1.0f, 0.45f, 1.0f);
        yield return new WaitForSeconds(0.12f);
        if (sr != null) sr.color = original;
    }

    private static Sprite CreatePoisonMoteSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.45f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float factor = 1f - (dist / radius);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(factor, 1.5f)));
                }
                else tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    private void Die()
    {
        LoopManager lm = LoopManager.Instance ?? Object.FindFirstObjectByType<LoopManager>();
        if (lm != null)
        {
            if (lm.currentLoop < lm.maxLoops)
            {
                Debug.Log($"<color=yellow>Player was sacrificed in loop {lm.currentLoop}! Advancing to next loop with Echo distraction...</color>");
                lm.EndCurrentLoopEarly();
            }
            else
            {
                Debug.Log("<color=red>Player has run out of hearts in the final loop! GAME OVER.</color>");
                lm.isGameOver = true;
                if (lm.gameOverPanel != null) lm.gameOverPanel.SetActive(true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
            }
        }
        else
        {
            Debug.Log("<color=red>Player has run out of hearts! GAME OVER.</color>");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        }
    }
}
