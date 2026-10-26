using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Level Config")]
    public int levelNumber = 1;
    public string levelTitle = "Level 1";
    public string levelDescription = "Explore the temporal dungeon";
    public string sceneName = "Level1";

    [Header("UI References")]
    public Button button;
    public Image nodeBgImage;
    public TMP_Text levelNumText;
    public GameObject lockIcon;
    public GameObject badgeIcon; // Star / Checkmark
    public GameObject pulseRing; // Ethereal current level ring
    public TMP_Text nameplateText;
    public Image nameplateBg;

    [Header("Sprites")]
    public Sprite unlockedSprite;
    public Sprite lockedSprite;
    public Sprite completedSprite;

    [Header("State")]
    public bool isUnlocked = false;
    public bool isCompleted = false;
    public bool isCurrent = false;

    private Vector3 originalScale;
    private bool isHovered = false;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnNodeClicked);
        }

        // Clean up any legacy duplicate text objects
        Transform legacyLabel = transform.Find("NodeLabel");
        if (legacyLabel != null && legacyLabel.gameObject != null)
        {
            Destroy(legacyLabel.gameObject);
        }
    }

    public void Setup(bool unlocked, bool completed, bool current)
    {
        isUnlocked = unlocked;
        isCompleted = completed;
        isCurrent = current;

        if (button != null)
        {
            button.interactable = isUnlocked;
        }

        if (nodeBgImage != null)
        {
            if (!isUnlocked && lockedSprite != null)
            {
                nodeBgImage.sprite = lockedSprite;
                nodeBgImage.color = Color.white;
            }
            else if (isCompleted && completedSprite != null)
            {
                nodeBgImage.sprite = completedSprite;
                nodeBgImage.color = Color.white;
            }
            else if (unlockedSprite != null)
            {
                nodeBgImage.sprite = unlockedSprite;
                nodeBgImage.color = Color.white;
            }
        }

        if (levelNumText != null)
        {
            levelNumText.text = levelNumber.ToString();
            levelNumText.color = isUnlocked ? (isCompleted ? new Color(0.2f, 1f, 0.7f) : new Color(1f, 0.9f, 0.3f)) : new Color(0.4f, 0.45f, 0.55f);
            levelNumText.gameObject.SetActive(isUnlocked);
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        if (badgeIcon != null)
        {
            badgeIcon.SetActive(isCompleted);
        }

        if (nameplateText != null)
        {
            if (isUnlocked)
            {
                nameplateText.text = isCompleted 
                    ? $"<size=11><color=#34D399><b>{levelTitle.ToUpper()}</b></color></size>" 
                    : $"<size=11><color=#FCD34D><b>{levelTitle.ToUpper()}</b></color></size>";
            }
            else
            {
                nameplateText.text = $"<size=11><color=#64748B>{levelTitle.ToUpper()}</color></size>";
            }
        }

        if (nameplateBg != null)
        {
            nameplateBg.color = isUnlocked ? new Color(0.08f, 0.11f, 0.18f, 0.95f) : new Color(0.04f, 0.05f, 0.08f, 0.8f);
        }

        if (pulseRing != null)
        {
            pulseRing.SetActive(isCurrent && isUnlocked);
            if (Application.isPlaying && isCurrent && isUnlocked && pulseRoutine == null)
            {
                pulseRoutine = StartCoroutine(AnimateCurrentPulse());
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUnlocked) return;
        isHovered = true;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale * 1.15f, 0.15f));
        if (LevelSelectManager.Instance != null)
        {
            LevelSelectManager.Instance.ShowHoverInfo(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isUnlocked) return;
        isHovered = false;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale, 0.15f));
        if (isCurrent && pulseRing != null && pulseRing.activeSelf)
        {
            pulseRoutine = StartCoroutine(AnimateCurrentPulse());
        }
        if (LevelSelectManager.Instance != null)
        {
            LevelSelectManager.Instance.HideHoverInfo();
        }
    }

    private void OnNodeClicked()
    {
        if (!isUnlocked) return;
        if (LevelSelectManager.Instance != null)
        {
            LevelSelectManager.Instance.SelectLevel(this);
        }
    }

    public IEnumerator PlaySelectAnimation()
    {
        float elapsed = 0f;
        float duration = 0.35f;
        Vector3 startScale = transform.localScale;
        Vector3 peakScale = originalScale * 1.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(peakScale, Vector3.zero, t * t);
            yield return null;
        }
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        transform.localScale = target;
    }

    private IEnumerator AnimateCurrentPulse()
    {
        while (isCurrent && isUnlocked && !isHovered)
        {
            float t = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            if (pulseRing != null)
            {
                pulseRing.transform.localScale = Vector3.one * Mathf.Lerp(1.1f, 1.45f, t);
                Image img = pulseRing.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = Mathf.Lerp(0.8f, 0.15f, t);
                    img.color = c;
                }
            }
            yield return null;
        }
    }
}
