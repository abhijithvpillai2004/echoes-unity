using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the circular "?" level info button and popup panel showing current level objectives and intel.
/// </summary>
public class LevelInfoButton : MonoBehaviour
{
    public static LevelInfoButton Instance { get; private set; }

    [Header("UI References")]
    public Button infoButton;
    public GameObject popupPanel;
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Button closeButton;

    [Header("Optional Level Info Override")]
    public string customTitle = "";
    [TextArea(3, 8)]
    public string customDetails = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (infoButton != null)
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(ToggleInfo);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseInfo);
        }
    }

    private void Start()
    {
        RefreshInfoUI();
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && popupPanel != null && popupPanel.activeSelf)
        {
            CloseInfo();
        }
    }

    public void ToggleInfo()
    {
        if (popupPanel == null) return;
        bool newState = !popupPanel.activeSelf;
        popupPanel.SetActive(newState);
        if (newState)
        {
            RefreshInfoUI();
            popupPanel.transform.SetAsLastSibling();
            if (HelpButton.Instance != null) HelpButton.Instance.CloseHelp();
        }
    }

    public void OpenInfo()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            RefreshInfoUI();
            popupPanel.transform.SetAsLastSibling();
            if (HelpButton.Instance != null) HelpButton.Instance.CloseHelp();
        }
    }

    public void CloseInfo()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    public void RefreshInfoUI()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string title = !string.IsNullOrEmpty(customTitle) ? customTitle : GetDefaultTitle(sceneName);
        string details = !string.IsNullOrEmpty(customDetails) ? customDetails : GetDefaultDetails(sceneName);

        if (LoopManager.Instance != null && !string.IsNullOrEmpty(LoopManager.Instance.levelTitle) && string.IsNullOrEmpty(customTitle))
        {
            title = LoopManager.Instance.levelTitle.Replace("\"", "");
        }

        if (titleText != null)
        {
            titleText.text = $"<color=#FFD700><b>🛡️ {title.ToUpper()}</b></color>";
        }

        if (contentText != null)
        {
            contentText.text = details;
        }
    }

    private string GetDefaultTitle(string sceneName)
    {
        switch (sceneName.ToLower())
        {
            case "level1": return "Level 1: The First Echo";
            case "level2": return "Level 2: The Blocker";
            case "level3": return "Level 3: The Triple Tier Fortress";
            case "level4": return "Level 4: The Twin Sentry Citadel";
            case "level5": return "Level 5: The Final Loop";
            default: return "Level Intel";
        }
    }

    private string GetDefaultDetails(string sceneName)
    {
        switch (sceneName.ToLower())
        {
            case "level1":
                return "<color=#00FFAA><b>OBJECTIVE:</b></color>\nReach the Golden Chest.\n\n" +
                       "<color=#FFCC00><b>INTEL & CLUES:</b></color>\nThe door won't stay open on its own. Your past self might just be able to lend a hand.";

            case "level2":
                return "<color=#00FFAA><b>OBJECTIVE:</b></color>\nReach the Golden Chest across the corridor.\n\n" +
                       "<color=#FFCC00><b>INTEL & CLUES:</b></color>\nA stubborn sentry blocks the narrow path. Can a diversion lure its attention away from your true goal?";

            case "level3":
                return "<color=#00FFAA><b>OBJECTIVE:</b></color>\nBreach all three tiers of the fortress to claim the chest.\n\n" +
                       "<color=#FFCC00><b>INTEL & CLUES:</b></color>\nEach tier is sealed by its own mechanism. Patience is key—wait before closed gates until your echoes reach their posts.";

            case "level4":
                return "<color=#00FFAA><b>OBJECTIVE:</b></color>\nInfiltrate the Citadel and reach the Inner Sanctum.\n\n" +
                       "<color=#FFCC00><b>INTEL & CLUES:</b></color>\nThe Twin Sentries' enchanted armor deflects physical strikes, and the Citadel Gate yields only to their defeat. Seek the armory, and find a way to disrupt their barrier before engaging in battle.";

            case "level5":
                return "<color=#00FFAA><b>OBJECTIVE:</b></color>\nConquer the Core Chamber.\n\n" +
                       "<color=#FFCC00><b>INTEL & CLUES:</b></color>\nThe ultimate trial demands complete coordination between every echo you leave behind. Overcome the guardian to expose the core.";

            default:
                return "<color=#00FFAA><b>OBJECTIVE:</b></color>\nReach the goal tile using your echoes!";
        }
    }
}
