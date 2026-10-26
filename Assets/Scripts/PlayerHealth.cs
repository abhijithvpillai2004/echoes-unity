using UnityEngine;
using UnityEngine.UI;

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

    [Header("HP Settings")]
    public int maxHP = 3;
    public int currentHP = 3;

    [Header("UI References")]
    public GameObject healthUIContainer;
    public Image[] heartImages; // Array of 3 heart Image components
    public Sprite fullHeartSprite;
    public Sprite brokenHeartSprite;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(this);
    }

    private void Start()
    {
        CheckHealthUIVisibility();
        ResetHealth();
    }

    public void CheckHealthUIVisibility()
    {
        // Only show Health Hearts in levels where combat / enemies exist (e.g. Level 4 & Level 5)
        CombatEnemy[] combatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        bool hasCombat = (combatEnemies != null && combatEnemies.Length > 0);

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
            healthUIContainer.SetActive(hasCombat);
        }
    }

    public void ResetHealth()
    {
        currentHP = maxHP;
        UpdateHeartsUI();
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP = Mathf.Max(0, currentHP - damage);
        Debug.Log($"<color=red>Player took {damage} damage! Current HP: {currentHP}/{maxHP}</color>");
        UpdateHeartsUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHP <= 0) return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"<color=green>Player healed +{amount}! Current HP: {currentHP}/{maxHP}</color>");
        UpdateHeartsUI();
    }

    public void UpdateHeartsUI()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                if (i < currentHP)
                {
                    if (fullHeartSprite != null) heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].color = Color.white;
                }
                else
                {
                    if (brokenHeartSprite != null) heartImages[i].sprite = brokenHeartSprite;
                    heartImages[i].color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                }
            }
        }
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
