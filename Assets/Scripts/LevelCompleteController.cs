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
    public float endX = 1300f;
    public float runDuration = 3.2f;
    public float echoSpacing = 115f;
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
    private List<Image> echoImages = new List<Image>();
    private List<Image> echoAuraImages = new List<Image>();

    // Visual Effect Containers
    private RectTransform fxContainer;
    private List<RectTransform> tetherNodes = new List<RectTransform>();

    private static Sprite cachedSparkleSprite;
    private static Sprite cachedAuraSprite;

    private float sparkTimer = 0f;

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

        // 4. Setup FX container
        SetupFXContainer();

        // 5. Spawn Running Mage + Echoes with Auras & Tethers
        SpawnRunnerParty();

        // 6. Play Transition Audio & Start Sequence
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelTransitionSFX();
        }
        StartCoroutine(RunTransitionSequence());
    }

    private void SetupFXContainer()
    {
        if (runnerContainer == null) return;
        Transform parentCanvas = runnerContainer.parent;

        GameObject fxObj = new GameObject("TransitionFXContainer", typeof(RectTransform));
        fxObj.transform.SetParent(parentCanvas, false);
        fxContainer = fxObj.GetComponent<RectTransform>();
        fxContainer.anchorMin = Vector2.zero;
        fxContainer.anchorMax = Vector2.one;
        fxContainer.sizeDelta = Vector2.zero;
        fxContainer.anchoredPosition = Vector2.zero;

        // Position FX behind characters but above floor track
        fxContainer.SetSiblingIndex(runnerContainer.GetSiblingIndex());
    }

    private void SpawnRunnerParty()
    {
        if (runnerContainer == null) return;

        allCharacters.Clear();
        echoImages.Clear();
        echoAuraImages.Clear();
        tetherNodes.Clear();

        if (cachedAuraSprite == null) cachedAuraSprite = CreateAuraSprite();

        // 1. Lead Player (Purple Mage)
        if (playerRunnerImage != null)
        {
            if (mageSprite != null) playerRunnerImage.sprite = mageSprite;
            playerRunnerImage.color = Color.white;
            RectTransform pRt = playerRunnerImage.rectTransform;
            pRt.anchoredPosition = new Vector2(0f, 0f);
            pRt.sizeDelta = new Vector2(100f, 100f);
            allCharacters.Add(pRt);
        }

        // 2. Spawn Echoes behind the player (matching completedLevel count)
        int echoCount = Mathf.Max(1, completedLevel);
        for (int i = 1; i <= echoCount; i++)
        {
            GameObject echoObj = new GameObject($"EchoRunner_{i}");
            echoObj.transform.SetParent(runnerContainer, false);

            // Child aura image rendered behind echo
            GameObject auraObj = new GameObject("EchoAura", typeof(RectTransform), typeof(Image));
            auraObj.transform.SetParent(echoObj.transform, false);
            RectTransform auraRt = auraObj.GetComponent<RectTransform>();
            auraRt.sizeDelta = new Vector2(140f, 140f);
            auraRt.anchoredPosition = Vector2.zero;
            Image auraImg = auraObj.GetComponent<Image>();
            auraImg.sprite = cachedAuraSprite;
            auraImg.color = new Color(0.20f, 0.90f, 1.00f, 0.35f);
            echoAuraImages.Add(auraImg);

            // Echo Runner Image
            Image echoImg = echoObj.AddComponent<Image>();
            if (mageSprite != null) echoImg.sprite = mageSprite;
            else if (playerRunnerImage != null) echoImg.sprite = playerRunnerImage.sprite;

            echoImg.color = new Color(0.25f, 0.85f, 1.0f, 0.55f);
            echoImages.Add(echoImg);

            RectTransform eRt = echoObj.GetComponent<RectTransform>();
            eRt.anchorMin = new Vector2(0.5f, 0.5f);
            eRt.anchorMax = new Vector2(0.5f, 0.5f);
            eRt.pivot = new Vector2(0.5f, 0.5f);
            eRt.sizeDelta = new Vector2(100f, 100f);
            eRt.anchoredPosition = new Vector2(-i * echoSpacing, 0f);
            allCharacters.Add(eRt);

            // 3. Create Arcane Temporal Tether Nodes between (i - 1) and (i)
            CreateTetherBetween(i - 1, i);
        }
    }

    private void CreateTetherBetween(int leaderIdx, int followerIdx)
    {
        if (cachedSparkleSprite == null) cachedSparkleSprite = CreateSparkleSprite();

        int nodesCount = 4;
        for (int n = 1; n <= nodesCount; n++)
        {
            GameObject nodeObj = new GameObject($"TetherNode_{leaderIdx}_{followerIdx}_{n}", typeof(RectTransform), typeof(Image));
            nodeObj.transform.SetParent(runnerContainer, false);
            nodeObj.transform.SetSiblingIndex(0); // Render behind runners

            Image img = nodeObj.GetComponent<Image>();
            img.sprite = cachedSparkleSprite;
            img.color = new Color(0.30f, 0.90f, 1.00f, 0.70f);

            RectTransform rt = nodeObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(18f, 18f);
            tetherNodes.Add(rt);
        }
    }

    private void Update()
    {
        float time = Time.time;

        // 1. Follow-the-leader Running Bounce with Squash & Stretch
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] != null)
            {
                float phase = i * 0.42f; // Progressive rhythmic wave
                float cycle = Mathf.Sin(time * bobFrequency + phase);
                float bobFactor = Mathf.Abs(cycle);
                float bobY = bobFactor * bobAmplitude;

                Vector2 pos = allCharacters[i].anchoredPosition;
                pos.y = bobY;
                allCharacters[i].anchoredPosition = pos;

                // Running Squash & Stretch
                float sx = Mathf.Lerp(1.06f, 0.94f, bobFactor);
                float sy = Mathf.Lerp(0.94f, 1.06f, bobFactor);
                allCharacters[i].localScale = new Vector3(sx, sy, 1f);
            }
        }

        // 2. Ghostly Phasing Pulse on Echoes
        for (int i = 0; i < echoImages.Count; i++)
        {
            if (echoImages[i] != null)
            {
                float pulse = (Mathf.Sin(time * 4.5f + i * 0.8f) + 1f) * 0.5f;
                float alpha = Mathf.Lerp(0.38f, 0.68f, pulse);
                Color c = new Color(0.25f, 0.85f, 1.0f, alpha);
                echoImages[i].color = c;

                if (i < echoAuraImages.Count && echoAuraImages[i] != null)
                {
                    echoAuraImages[i].color = new Color(0.20f, 0.90f, 1.00f, Mathf.Lerp(0.20f, 0.45f, pulse));
                }
            }
        }

        // 3. Dynamic Temporal Tether Ripple
        UpdateTetherNodes(time);

        // 4. Emit Trailing Stardust & Ground Sparks
        sparkTimer += Time.deltaTime;
        if (sparkTimer >= 0.055f)
        {
            sparkTimer = 0f;
            EmitRunnerSparks();
        }
    }

    private void UpdateTetherNodes(float time)
    {
        int nodeIdx = 0;
        int echoCount = Mathf.Max(1, completedLevel);

        for (int i = 1; i <= echoCount; i++)
        {
            if (i >= allCharacters.Count) break;

            RectTransform leader = allCharacters[i - 1];
            RectTransform follower = allCharacters[i];
            if (leader == null || follower == null) continue;

            int nodesCount = 4;
            for (int n = 1; n <= nodesCount; n++)
            {
                if (nodeIdx >= tetherNodes.Count) break;

                RectTransform nodeRt = tetherNodes[nodeIdx];
                if (nodeRt != null)
                {
                    float frac = (float)n / (nodesCount + 1);
                    Vector2 basePos = Vector2.Lerp(leader.anchoredPosition, follower.anchoredPosition, frac);

                    // Electrical ripple wave
                    float ripple = Mathf.Sin(time * 10f + frac * Mathf.PI * 2f + i * 1.5f) * 7f;
                    nodeRt.anchoredPosition = new Vector2(basePos.x, basePos.y + ripple);

                    // Pulse brightness
                    float pulse = Mathf.Sin(time * 12f - frac * Mathf.PI * 2f);
                    float scale = Mathf.Lerp(0.7f, 1.25f, (pulse + 1f) * 0.5f);
                    nodeRt.localScale = Vector3.one * scale;

                    Image nImg = nodeRt.GetComponent<Image>();
                    if (nImg != null)
                    {
                        // Pulses from Golden Sun to Arcane Cyan
                        nImg.color = Color.Lerp(new Color(1.0f, 0.85f, 0.25f, 0.85f), new Color(0.25f, 0.90f, 1.00f, 0.75f), (pulse + 1f) * 0.5f);
                    }
                }
                nodeIdx++;
            }
        }
    }

    private void EmitRunnerSparks()
    {
        if (fxContainer == null || runnerContainer == null) return;
        if (cachedSparkleSprite == null) cachedSparkleSprite = CreateSparkleSprite();

        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] == null) continue;

            Vector2 runnerWorldScreen = allCharacters[i].position;
            // Spawn spark at runner's foot level
            Vector2 spawnPos = runnerWorldScreen + new Vector2(Random.Range(-15f, 10f), -35f + Random.Range(-5f, 5f));

            GameObject spObj = new GameObject("TransitionSpark", typeof(RectTransform), typeof(Image));
            spObj.transform.SetParent(fxContainer, false);
            spObj.transform.position = spawnPos;

            RectTransform rt = spObj.GetComponent<RectTransform>();
            float initialSize = Random.Range(16f, 24f);
            rt.sizeDelta = new Vector2(initialSize, initialSize);

            Image img = spObj.GetComponent<Image>();
            img.sprite = cachedSparkleSprite;

            // Player kicks up Golden & Cyan sparks; Echoes kick up Arcane Cyan & Lavender sparks
            Color sparkCol;
            if (i == 0)
            {
                sparkCol = (Random.value > 0.5f)
                    ? new Color(1.0f, 0.85f, 0.20f, 0.95f) // Sun Gold
                    : new Color(0.25f, 0.90f, 1.00f, 0.95f); // Arcane Cyan
            }
            else
            {
                sparkCol = (Random.value > 0.5f)
                    ? new Color(0.25f, 0.90f, 1.00f, 0.85f) // Ethereal Cyan
                    : new Color(0.70f, 0.60f, 1.00f, 0.85f); // Mystic Lavender
            }
            img.color = sparkCol;

            StartCoroutine(AnimateUISpark(spObj, rt, img, sparkCol, 0.32f));
        }
    }

    private IEnumerator AnimateUISpark(GameObject obj, RectTransform rt, Image img, Color baseCol, float duration)
    {
        float timer = 0f;
        Vector2 velocity = new Vector2(Random.Range(-35f, -10f), Random.Range(15f, 40f));
        float rotSpeed = Random.Range(-180f, 180f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (obj != null && rt != null && img != null)
            {
                rt.anchoredPosition += velocity * Time.deltaTime;
                rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

                float scale = Mathf.Sin(t * Mathf.PI);
                rt.localScale = Vector3.one * scale;

                Color c = baseCol;
                c.a = Mathf.Lerp(baseCol.a, 0f, t);
                img.color = c;
            }

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private IEnumerator RunTransitionSequence()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        float elapsed = 0f;
        float totalTime = runDuration;

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

    private static Sprite CreateSparkleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.48f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - center.x);
                float dy = Mathf.Abs(y - center.y);

                float nx = dx / radius;
                float ny = dy / radius;
                float val = Mathf.Sqrt(nx) + Mathf.Sqrt(ny);

                if (val <= 1.0f)
                {
                    float alpha = Mathf.Pow(1.0f - val, 0.70f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    private static Sprite CreateAuraSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.48f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float factor = 1f - (dist / radius);
                    float alpha = factor * factor;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }
}
