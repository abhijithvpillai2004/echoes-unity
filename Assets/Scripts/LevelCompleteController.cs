using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompleteController : MonoBehaviour
{
    [Header("UI Text References")]
    public TMP_Text levelCompleteText;
    public TMP_Text encouragementText;
    public CanvasGroup canvasGroup;

    [Header("Runner Settings")]
    public RectTransform runnerContainer;
    public Image playerRunnerImage;
    public Sprite mageSprite;
    public float startX = -1100f;
    public float endX = 1100f;
    public float runDuration = 3.2f;
    public float echoSpacing = 110f;
    public float bobFrequency = 14f;
    public float bobAmplitude = 12f;

    [Header("Messages")]
    public string[] encouragementMessages = new string[]
    {
        "Keep going!",
        "Your echoes are proud of you!",
        "The dungeon grows deeper...",
        "You're getting the hang of it!",
        "One step ahead of time!"
    };

    private int completedLevel = 1;
    private string nextSceneName = "Level1";
    private List<RectTransform> allCharacters = new List<RectTransform>();

    private void Start()
    {
        // 1. Read parameters from PlayerPrefs
        completedLevel = PlayerPrefs.GetInt("CompletedLevelNumber", 1);
        nextSceneName = PlayerPrefs.GetString("NextLevelSceneName", "Level1");

        // 2. Set Header Text
        if (levelCompleteText != null)
        {
            if (completedLevel >= 5)
            {
                levelCompleteText.text = "<color=#FFD700><size=72><b>Game Complete!</b></size></color>";
            }
            else
            {
                levelCompleteText.text = $"<color=#FFD700><size=72><b>Level {completedLevel} Complete!</b></size></color>";
            }
        }

        // 3. Set Random Encouragement Message
        if (encouragementText != null)
        {
            if (completedLevel >= 5)
            {
                encouragementText.text = "<i><color=#5EEAD4>\"You conquered all time loops! The realm is saved!\"</color></i>";
            }
            else if (encouragementMessages.Length > 0)
            {
                string randomMsg = encouragementMessages[Random.Range(0, encouragementMessages.Length)];
                encouragementText.text = $"<i><color=#5EEAD4>\"{randomMsg}\"</color></i>";
            }
        }

        // 4. Spawn Running Mage + Echoes
        SpawnRunnerParty();

        // 5. Start Sequence
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelTransitionSFX();
        }
        StartCoroutine(RunTransitionSequence());
    }

    private void SpawnRunnerParty()
    {
        if (runnerContainer == null) return;

        allCharacters.Clear();

        // Lead Player (Purple Mage)
        if (playerRunnerImage != null)
        {
            if (mageSprite != null) playerRunnerImage.sprite = mageSprite;
            playerRunnerImage.color = Color.white;
            RectTransform pRt = playerRunnerImage.rectTransform;
            pRt.anchoredPosition = new Vector2(0f, 0f);
            pRt.sizeDelta = new Vector2(100f, 100f);
            allCharacters.Add(pRt);
        }

        // Spawn Echoes behind the player (matching completedLevel count)
        int echoCount = Mathf.Max(1, completedLevel);
        for (int i = 1; i <= echoCount; i++)
        {
            GameObject echoObj = new GameObject($"EchoRunner_{i}");
            echoObj.transform.SetParent(runnerContainer, false);
            
            Image echoImg = echoObj.AddComponent<Image>();
            if (mageSprite != null) echoImg.sprite = mageSprite;
            else if (playerRunnerImage != null) echoImg.sprite = playerRunnerImage.sprite;

            // Semi-transparent ethereal cyan echo look (alpha 0.5)
            echoImg.color = new Color(0.2f, 0.9f, 1f, 0.5f);

            RectTransform eRt = echoObj.GetComponent<RectTransform>();
            eRt.anchorMin = new Vector2(0.5f, 0.5f);
            eRt.anchorMax = new Vector2(0.5f, 0.5f);
            eRt.pivot = new Vector2(0.5f, 0.5f);
            eRt.sizeDelta = new Vector2(100f, 100f);
            
            // Position behind player
            eRt.anchoredPosition = new Vector2(-i * echoSpacing, 0f);
            allCharacters.Add(eRt);
        }
    }

    private void Update()
    {
        // Add running bobbing bounce animation to all runners
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] != null)
            {
                float phase = i * 0.45f;
                float bobY = Mathf.Abs(Mathf.Sin(Time.time * bobFrequency + phase)) * bobAmplitude;
                Vector2 pos = allCharacters[i].anchoredPosition;
                pos.y = bobY;
                allCharacters[i].anchoredPosition = pos;
            }
        }
    }

    private IEnumerator RunTransitionSequence()
    {
        // Fade In
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        float elapsed = 0f;
        float totalTime = runDuration;

        // Animate Container across the screen from startX to endX
        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalTime;

            // Smooth horizontal movement across the screen
            if (runnerContainer != null)
            {
                float currentX = Mathf.Lerp(startX, endX, t);
                Vector2 cPos = runnerContainer.anchoredPosition;
                cPos.x = currentX;
                runnerContainer.anchoredPosition = cPos;
            }

            // Smooth Fade In (first 0.4s) and Fade Out (last 0.4s)
            if (canvasGroup != null)
            {
                if (elapsed < 0.4f)
                {
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / 0.4f);
                }
                else if (elapsed > totalTime - 0.4f)
                {
                    canvasGroup.alpha = Mathf.Clamp01((totalTime - elapsed) / 0.4f);
                }
                else
                {
                    canvasGroup.alpha = 1f;
                }
            }

            yield return null;
        }

        // Load next level
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }
}
