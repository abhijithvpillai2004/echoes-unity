using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleScreenController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text pressAnyKeyText;
    public TMP_Text titleText;
    public CanvasGroup mainCanvasGroup;

    [Header("Settings")]
    public string firstLevelScene = "LevelSelect";
    public float pulseSpeed = 3.5f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    private bool hasStarted = false;

    private void Start()
    {
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
            StartCoroutine(FadeInTitleScreen());
        }
    }

    private void Update()
    {
        // Animate the "Press Any Key" text with smooth sine pulsing
        if (pressAnyKeyText != null && !hasStarted)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color c = pressAnyKeyText.color;
            c.a = alpha;
            pressAnyKeyText.color = c;
        }

        // Check for any key, space, enter, or mouse click to begin
        if (!hasStarted && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        hasStarted = true;
        StartCoroutine(TransitionToFirstLevel());
    }

    private IEnumerator FadeInTitleScreen()
    {
        float elapsed = 0f;
        float duration = 1.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            }
            yield return null;
        }
        if (mainCanvasGroup != null) mainCanvasGroup.alpha = 1f;
    }

    private IEnumerator TransitionToFirstLevel()
    {
        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.text = "OPENING ADVENTURE MAP...";
            pressAnyKeyText.color = new Color(0f, 1f, 0.7f, 1f);
        }

        float elapsed = 0f;
        float fadeDuration = 0.7f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            }
            yield return null;
        }

        SceneManager.LoadScene(firstLevelScene);
    }
}
