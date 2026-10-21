using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }

    [Header("Player Character Pawn")]
    public MapPlayerPawn playerPawn;

    [Header("Node Setup")]
    public LevelNode[] levelNodes;
    public RectTransform pathContainer;
    public GameObject pathDotPrefab;

    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public TMP_Text hoverInfoText;
    public Button backButton;
    public Button resetProgressButton;
    public CanvasGroup fadeCanvasGroup;

    [Header("Sound (Optional)")]
    public AudioSource ambientAudio;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.RemoveAllListeners();
            resetProgressButton.onClick.AddListener(ResetProgress);
        }
    }

    private void Start()
    {
        SetupAmbientAudio();
        if (levelNodes == null || levelNodes.Length == 0)
        {
            levelNodes = FindObjectsByType<LevelNode>(FindObjectsSortMode.None);
            System.Array.Sort(levelNodes, (a, b) => a.levelNumber.CompareTo(b.levelNumber));
        }

        RefreshAllNodes();
        GenerateDottedPath();
        SetupPlayerPawnInitialPosition();
        StartCoroutine(FadeIn());
    }

    private void SetupPlayerPawnInitialPosition()
    {
        if (playerPawn == null)
        {
            playerPawn = FindAnyObjectByType<MapPlayerPawn>();
        }

        if (playerPawn != null && levelNodes != null && levelNodes.Length > 0)
        {
            int highestUnlocked = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);
            int startLevelNum = Mathf.Clamp(highestUnlocked, 1, levelNodes.Length);
            
            // Find corresponding node
            LevelNode targetNode = GetNodeByNumber(startLevelNum) ?? levelNodes[0];
            RectTransform rt = targetNode.GetComponent<RectTransform>();
            if (rt != null)
            {
                playerPawn.SetPositionAtNode(rt.anchoredPosition, targetNode.levelNumber);
            }
        }
    }

    public LevelNode GetNodeByNumber(int levelNum)
    {
        if (levelNodes == null) return null;
        for (int i = 0; i < levelNodes.Length; i++)
        {
            if (levelNodes[i] != null && levelNodes[i].levelNumber == levelNum)
                return levelNodes[i];
        }
        return null;
    }

    public void RefreshAllNodes()
    {
        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);

        for (int i = 0; i < levelNodes.Length; i++)
        {
            var node = levelNodes[i];
            if (node == null) continue;

            int lvl = node.levelNumber;
            bool unlocked = (lvl <= highestUnlocked);
            bool completed = (PlayerPrefs.GetInt($"LevelCompleted_{lvl}", 0) == 1);
            bool current = (lvl == highestUnlocked);

            node.Setup(unlocked, completed, current);
        }

        if (subtitleText != null)
        {
            int completedCount = 0;
            for (int i = 1; i <= levelNodes.Length; i++)
            {
                if (PlayerPrefs.GetInt($"LevelCompleted_{i}", 0) == 1) completedCount++;
            }
            subtitleText.text = $"Progress: {completedCount} / {levelNodes.Length} Areas Conquered  •  Unlocked: {Mathf.Min(highestUnlocked, levelNodes.Length)} / {levelNodes.Length}";
        }
    }

    public void ShowHoverInfo(LevelNode node)
    {
        if (hoverInfoText == null || node == null) return;

        string statusTag;
        if (!node.isUnlocked)
        {
            statusTag = "<color=#EF4444>[ LOCKED - CONQUER PREVIOUS LEVEL ]</color>";
        }
        else if (node.isCompleted)
        {
            statusTag = "<color=#00FFAA>[ COMPLETED ]</color>";
        }
        else
        {
            statusTag = "<color=#FFD700>[ READY TO ENTER ]</color>";
        }

        string actionPrompt = "";
        if (node.isUnlocked)
        {
            if (playerPawn != null && playerPawn.currentLevelNumber == node.levelNumber)
            {
                actionPrompt = "\n<size=14><color=#67E8F9>★ Mage is here — Click to Enter</color></size>";
            }
            else
            {
                actionPrompt = "\n<size=14><color=#FBBF24>✦ Click to Travel & Enter</color></size>";
            }
        }

        hoverInfoText.text = $"<size=16><b>LEVEL {node.levelNumber}: {node.levelTitle.ToUpper()}</b></size>\n" +
                             $"<size=13><color=#D1D5DB>{node.levelDescription}</color></size>  •  {statusTag}{actionPrompt}";
    }

    public void HideHoverInfo()
    {
        if (hoverInfoText != null)
        {
            hoverInfoText.text = "<color=#AAAAAA>Select a destination on the dungeon map to begin</color>";
        }
    }

    public void SelectLevel(LevelNode targetNode)
    {
        if (isTransitioning || targetNode == null || !targetNode.isUnlocked) return;
        if (playerPawn != null && playerPawn.isMoving) return;

        isTransitioning = true;

        if (playerPawn != null)
        {
            int fromLevel = playerPawn.currentLevelNumber;
            int toLevel = targetNode.levelNumber;

            List<Vector2> waypoints = new List<Vector2>();

            if (fromLevel < toLevel)
            {
                for (int lvl = fromLevel + 1; lvl <= toLevel; lvl++)
                {
                    LevelNode n = GetNodeByNumber(lvl);
                    if (n != null)
                    {
                        waypoints.Add(n.GetComponent<RectTransform>().anchoredPosition);
                    }
                }
            }
            else if (fromLevel > toLevel)
            {
                for (int lvl = fromLevel - 1; lvl >= toLevel; lvl--)
                {
                    LevelNode n = GetNodeByNumber(lvl);
                    if (n != null)
                    {
                        waypoints.Add(n.GetComponent<RectTransform>().anchoredPosition);
                    }
                }
            }
            else
            {
                // Already at node
                waypoints.Add(targetNode.GetComponent<RectTransform>().anchoredPosition);
            }

            if (hoverInfoText != null)
            {
                hoverInfoText.text = $"<color=#67E8F9>TRAVELLING TO {targetNode.levelTitle.ToUpper()}...</color>";
            }

            StartCoroutine(playerPawn.MoveAlongPath(waypoints, targetNode.levelNumber, () =>
            {
                StartCoroutine(TransitionToLevel(targetNode));
            }));
        }
        else
        {
            StartCoroutine(TransitionToLevel(targetNode));
        }
    }

    private IEnumerator TransitionToLevel(LevelNode node)
    {
        StartCoroutine(node.PlaySelectAnimation());

        if (hoverInfoText != null)
        {
            hoverInfoText.text = $"<color=#00FFAA>ENTERING {node.levelTitle.ToUpper()}...</color>";
        }

        float elapsed = 0f;
        float duration = 0.65f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            }
            yield return null;
        }

        PlayerPrefs.SetInt("SelectedLevelNumber", node.levelNumber);
        PlayerPrefs.Save();
        SceneManager.LoadScene(node.sceneName);
    }

    public void OnBackClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(TransitionToScene("TitleScreen"));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        float elapsed = 0f;
        float duration = 0.4f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            }
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }
    }

    public void ResetProgress()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        PlayerPrefs.SetInt("HighestUnlockedLevel", 1);
        for (int i = 1; i <= 10; i++)
        {
            PlayerPrefs.DeleteKey($"LevelCompleted_{i}");
        }
        PlayerPrefs.Save();
        RefreshAllNodes();
        Debug.Log("<color=yellow>Progress reset to Level 1.</color>");
    }

    public Sprite pathDotSprite;

    public void GenerateDottedPath()
    {
        if (pathContainer == null || levelNodes == null || levelNodes.Length < 2) return;

        // Clear old dots safely
        for (int i = pathContainer.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(pathContainer.GetChild(i).gameObject);
            else
                DestroyImmediate(pathContainer.GetChild(i).gameObject);
        }

        Sprite dot = pathDotSprite;
        if (dot == null)
        {
            dot = Resources.Load<Sprite>("ui_path_dot");
        }

        for (int i = 0; i < levelNodes.Length - 1; i++)
        {
            LevelNode a = levelNodes[i];
            LevelNode b = levelNodes[i + 1];
            if (a == null || b == null) continue;

            RectTransform rtA = a.GetComponent<RectTransform>();
            RectTransform rtB = b.GetComponent<RectTransform>();

            Vector2 posA = rtA.anchoredPosition;
            Vector2 posB = rtB.anchoredPosition;

            float dist = Vector2.Distance(posA, posB);
            int dotsCount = Mathf.Max(3, Mathf.RoundToInt(dist / 32f));

            bool segmentUnlocked = (b.isUnlocked);

            for (int d = 1; d < dotsCount; d++)
            {
                float t = (float)d / dotsCount;
                Vector2 dotPos = Vector2.Lerp(posA, posB, t);

                GameObject dotObj = new GameObject($"PathDot_{i}_{d}");
                dotObj.transform.SetParent(pathContainer, false);
                RectTransform dotRt = dotObj.AddComponent<RectTransform>();
                dotRt.anchorMin = new Vector2(0.5f, 0.5f);
                dotRt.anchorMax = new Vector2(0.5f, 0.5f);
                dotRt.pivot = new Vector2(0.5f, 0.5f);
                dotRt.anchoredPosition = dotPos;
                dotRt.sizeDelta = new Vector2(10f, 10f);

                Image img = dotObj.AddComponent<Image>();
                if (dot != null) img.sprite = dot;
                img.color = segmentUnlocked ? new Color(1f, 0.85f, 0.3f, 0.85f) : new Color(0.4f, 0.4f, 0.45f, 0.4f);
            }
        }
    }
    private void SetupAmbientAudio()
    {
        // Procedural synthetic buzzing audio removed as requested.
        // ambientAudio field remains available in the Inspector if an ambient track is assigned.
        if (ambientAudio != null && ambientAudio.clip != null)
        {
            ambientAudio.loop = true;
            ambientAudio.Play();
        }
    }

}
