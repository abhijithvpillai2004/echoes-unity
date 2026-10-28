using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the circular "i" help button and the semi-transparent controls popup.
/// Easy to extend with new controls per level or dynamically at runtime.
/// </summary>
public class HelpButton : MonoBehaviour
{
    [System.Serializable]
    public class ControlEntry
    {
        public string key;
        public string description;

        public ControlEntry(string key, string description)
        {
            this.key = key;
            this.description = description;
        }
    }

    public static HelpButton Instance { get; private set; }

    [Header("UI References")]
    public Button helpButton;
    public GameObject popupPanel;
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Button closeButton;

    [Header("Controls Configuration")]
    public List<ControlEntry> defaultControls = new List<ControlEntry>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        InitializeDefaultControls();

        if (helpButton != null)
        {
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(ToggleHelp);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseHelp);
        }
    }

    private void Start()
    {
        InitializeDefaultControls();
        RefreshHelpUI();
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && popupPanel != null && popupPanel.activeSelf)
        {
            CloseHelp();
        }
    }

    public void InitializeDefaultControls()
    {
        defaultControls = new List<ControlEntry>()
        {
            new ControlEntry("W / A / S / D", "Move up / left / down / right"),
            new ControlEntry("Space", "Wait (skip 1 turn)"),
            new ControlEntry("Enter / Tab / Q", "End Loop Early (fast-forwards turns)"),
            new ControlEntry("E", "Collect item (weapon or potion)"),
            new ControlEntry("X / F", "Attack enemy (when weapon equipped)"),
            new ControlEntry("R", "Restart level (from loop 1)"),
            new ControlEntry("Esc", "Close help popup")
        };
    }

    public void ToggleHelp()
    {
        if (popupPanel == null) return;
        bool newState = !popupPanel.activeSelf;
        popupPanel.SetActive(newState);
        if (newState)
        {
            InitializeDefaultControls();
            RefreshHelpUI();
            popupPanel.transform.SetAsLastSibling();
            if (LevelInfoButton.Instance != null) LevelInfoButton.Instance.CloseInfo();
        }
    }

    public void OpenHelp()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            InitializeDefaultControls();
            RefreshHelpUI();
            popupPanel.transform.SetAsLastSibling();
            if (LevelInfoButton.Instance != null) LevelInfoButton.Instance.CloseInfo();
        }
    }

    public void CloseHelp()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    public void AddControl(string key, string description)
    {
        defaultControls.Add(new ControlEntry(key, description));
        RefreshHelpUI();
    }

    public void SetControls(List<ControlEntry> newControls)
    {
        defaultControls = new List<ControlEntry>(newControls);
        RefreshHelpUI();
    }

    public void RefreshHelpUI()
    {
        if (titleText != null)
        {
            titleText.text = "<color=#FFD700><b>📖 CONTROLS GUIDE</b></color>";
        }

        if (contentText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var entry in defaultControls)
            {
                sb.AppendLine($"<color=#00FFAA><b>{entry.key}</b></color> <color=#CCCCCC>—</color> {entry.description}");
                sb.AppendLine();
            }
            contentText.text = sb.ToString().TrimEnd();
        }
    }
}
