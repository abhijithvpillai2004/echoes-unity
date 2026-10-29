using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls the Title Screen with lively, beautiful animations:
/// - Hero Mage idle hover, squash & stretch breathing, and periodic playful hops
/// - Echo Ghost ethereal floating, spirit alpha pulse, and phantom afterimage trails
/// - Floating temporal sparkle stars drifting through the dungeon background
/// - Title and Card gentle majestic floating
/// - Celebratory jump launch sequence when transitioning to the Adventure Map
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text pressAnyKeyText;
    public TMP_Text titleText;
    public CanvasGroup mainCanvasGroup;
    public RectTransform cardFrame;
    public RectTransform mageIcon;
    public RectTransform echoIcon;
    public Sprite starSprite;

    [Header("Settings")]
    public string firstLevelScene = "LevelSelect";
    public float pulseSpeed = 3.5f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    // Animation internal state
    private bool hasStarted = false;
    private Image echoImage;
    private Color echoBaseColor = new Color(0.2f, 0.9f, 1f, 0.65f);

    private Vector2 cardBasePos;
    private Vector2 titleBasePos;
    private Vector2 mageBasePos;
    private Vector2 echoBasePos;

    private float hopTimer = 0f;
    private float nextHopInterval = 4.2f;
    private bool isHopping = false;

    // Ghost trail afterimages
    private List<RectTransform> ghostTrails = new List<RectTransform>();
    private List<Image> ghostTrailImages = new List<Image>();
    private Queue<Vector2> echoPosHistory = new Queue<Vector2>();

    // Floating background time particles
    private List<RectTransform> floatingMotes = new List<RectTransform>();
    private List<float> moteSpeeds = new List<float>();
    private List<float> moteSwayFreq = new List<float>();
    private List<float> moteSwayAmp = new List<float>();
    private List<float> moteRotSpeeds = new List<float>();
    private List<float> moteBaseX = new List<float>();

    private void Awake()
    {
        AutoFindReferences();
    }

    private void Start()
    {
        RecordBasePositions();
        SetupGhostTrails();
        SpawnBackgroundMotes();

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
            StartCoroutine(FadeInTitleScreen());
        }
    }

    private void AutoFindReferences()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            if (mainCanvasGroup == null) mainCanvasGroup = canvas.GetComponent<CanvasGroup>() ?? canvas.gameObject.AddComponent<CanvasGroup>();
            if (cardFrame == null) cardFrame = canvas.transform.Find("CardFrame") as RectTransform;
            if (pressAnyKeyText == null)
            {
                Transform p = canvas.transform.Find("PressAnyKeyText");
                if (p != null) pressAnyKeyText = p.GetComponent<TMP_Text>();
            }

            if (cardFrame != null)
            {
                if (titleText == null)
                {
                    Transform t = cardFrame.Find("TitleText");
                    if (t != null) titleText = t.GetComponent<TMP_Text>();
                }
                if (mageIcon == null) mageIcon = cardFrame.Find("MageIcon") as RectTransform;
                if (echoIcon == null) echoIcon = cardFrame.Find("EchoIcon") as RectTransform;
            }
        }

        if (echoIcon != null)
        {
            echoImage = echoIcon.GetComponent<Image>();
            if (echoImage != null) echoBaseColor = echoImage.color;
        }

        if (starSprite == null)
        {
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in allSprites)
            {
                if (s != null && s.name == "ui_icon_star")
                {
                    starSprite = s;
                    break;
                }
            }
        }
    }

    private void RecordBasePositions()
    {
        if (cardFrame != null) cardBasePos = cardFrame.anchoredPosition;
        if (titleText != null) titleBasePos = titleText.rectTransform.anchoredPosition;
        if (mageIcon != null) mageBasePos = mageIcon.anchoredPosition;
        if (echoIcon != null) echoBasePos = echoIcon.anchoredPosition;
    }

    private void SetupGhostTrails()
    {
        if (echoIcon == null || echoIcon.parent == null) return;

        // Create 2 ethereal trailing ghosts behind the main Echo
        for (int i = 0; i < 2; i++)
        {
            GameObject trailObj = new GameObject($"GhostTrail_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            trailObj.transform.SetParent(echoIcon.parent, false);
            trailObj.transform.SetSiblingIndex(echoIcon.GetSiblingIndex()); // Behind echo

            RectTransform tRt = trailObj.GetComponent<RectTransform>();
            tRt.sizeDelta = echoIcon.sizeDelta;
            tRt.anchoredPosition = echoIcon.anchoredPosition;

            Image tImg = trailObj.GetComponent<Image>();
            if (echoImage != null) tImg.sprite = echoImage.sprite;
            tImg.preserveAspect = true;

            float alpha = (i == 0) ? 0.35f : 0.18f;
            tImg.color = new Color(echoBaseColor.r, echoBaseColor.g, echoBaseColor.b, alpha);

            ghostTrails.Add(tRt);
            ghostTrailImages.Add(tImg);
        }
    }

    private void SpawnBackgroundMotes()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject motesRoot = new GameObject("TimeParticles", typeof(RectTransform));
        motesRoot.transform.SetParent(canvas.transform, false);
        motesRoot.transform.SetSiblingIndex(1); // Above background panel, behind card

        RectTransform rootRt = motesRoot.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        int moteCount = 20;
        for (int i = 0; i < moteCount; i++)
        {
            GameObject mObj = new GameObject($"Mote_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            mObj.transform.SetParent(motesRoot.transform, false);

            RectTransform mRt = mObj.GetComponent<RectTransform>();
            float size = Random.Range(14f, 28f);
            mRt.sizeDelta = new Vector2(size, size);

            float startX = Random.Range(-750f, 750f);
            float startY = Random.Range(-450f, 450f);
            mRt.anchoredPosition = new Vector2(startX, startY);

            Image mImg = mObj.GetComponent<Image>();
            if (starSprite != null) mImg.sprite = starSprite;

            // Palette: golden stars and cyan temporal dust
            Color c = (Random.value > 0.45f) ? new Color(1f, 0.85f, 0.3f, Random.Range(0.3f, 0.75f))
                                             : new Color(0.3f, 0.95f, 1f, Random.Range(0.25f, 0.7f));
            mImg.color = c;

            floatingMotes.Add(mRt);
            moteBaseX.Add(startX);
            moteSpeeds.Add(Random.Range(28f, 65f));
            moteSwayFreq.Add(Random.Range(1.2f, 2.5f));
            moteSwayAmp.Add(Random.Range(10f, 25f));
            moteRotSpeeds.Add(Random.Range(-45f, 45f));
        }
    }

    private void Update()
    {
        float time = Time.time;

        // 1. Hero Mage Animation (Left Character)
        AnimateMage(time);

        // 2. Echo Ghost Animation (Right Character)
        AnimateEchoGhost(time);

        // 3. Title & Card Float
        AnimateTitleAndCard(time);

        // 4. Background Temporal Sparkles Drift
        AnimateFloatingMotes(time);

        // 5. "Press Any Key" Text Pulse
        AnimatePressKeyText(time);

        // 6. Input Check to Start
        if (!hasStarted && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StartGame();
        }
    }

    private void AnimateMage(float time)
    {
        if (mageIcon == null) return;

        if (!hasStarted && !isHopping)
        {
            // Gentle hovering bob
            float hoverY = Mathf.Sin(time * 3.4f) * 9f;
            mageIcon.anchoredPosition = mageBasePos + new Vector2(0f, hoverY);

            // Subtle breathing squash & stretch
            float breathe = Mathf.Sin(time * 3.4f) * 0.035f;
            mageIcon.localScale = new Vector3(1.0f + breathe, 1.0f - breathe, 1.0f);

            // Hop timer
            hopTimer += Time.deltaTime;
            if (hopTimer >= nextHopInterval)
            {
                hopTimer = 0f;
                nextHopInterval = Random.Range(3.5f, 6.0f);
                StartCoroutine(MagePlayfulHop());
            }
        }
    }

    private IEnumerator MagePlayfulHop()
    {
        isHopping = true;
        float duration = 0.42f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Parabolic jump arc: sin(t * PI)
            float jumpY = Mathf.Sin(t * Mathf.PI) * 26f;
            mageIcon.anchoredPosition = mageBasePos + new Vector2(0f, jumpY);

            // Stretch up while ascending, squash on landing
            float stretch = Mathf.Sin(t * Mathf.PI) * 0.12f;
            mageIcon.localScale = new Vector3(1f - stretch * 0.5f, 1f + stretch, 1f);

            yield return null;
        }

        mageIcon.anchoredPosition = mageBasePos;
        mageIcon.localScale = Vector3.one;
        isHopping = false;
    }

    private void AnimateEchoGhost(float time)
    {
        if (echoIcon == null) return;

        if (!hasStarted)
        {
            // Ethereal floating: vertical sine wave + horizontal subtle weave
            float floatY = Mathf.Sin(time * 2.7f) * 12f;
            float floatX = Mathf.Cos(time * 1.9f) * 7f;
            Vector2 currentEchoPos = echoBasePos + new Vector2(floatX, floatY);
            echoIcon.anchoredPosition = currentEchoPos;

            // Ghostly alpha breathing pulse
            if (echoImage != null)
            {
                float alphaPulse = Mathf.Lerp(0.40f, 0.85f, 0.5f + 0.5f * Mathf.Sin(time * 3.0f));
                echoImage.color = new Color(echoBaseColor.r, echoBaseColor.g, echoBaseColor.b, alphaPulse);
            }

            // Ghost scale pulse
            float scalePulse = 1.0f + 0.045f * Mathf.Sin(time * 2.7f);
            echoIcon.localScale = Vector3.one * scalePulse;

            // Update Ghost Trails History (Delay queue)
            echoPosHistory.Enqueue(currentEchoPos);
            while (echoPosHistory.Count > 16) echoPosHistory.Dequeue();

            Vector2[] historyArr = echoPosHistory.ToArray();
            if (ghostTrails.Count > 0 && historyArr.Length >= 8)
            {
                ghostTrails[0].anchoredPosition = historyArr[historyArr.Length - 8];
            }
            if (ghostTrails.Count > 1 && historyArr.Length >= 15)
            {
                ghostTrails[1].anchoredPosition = historyArr[0];
            }
        }
    }

    private void AnimateTitleAndCard(float time)
    {
        // Gentle majestic title hover
        if (titleText != null && !hasStarted)
        {
            float titleHoverY = Mathf.Sin(time * 2.2f) * 5f;
            titleText.rectTransform.anchoredPosition = titleBasePos + new Vector2(0f, titleHoverY);

            float titleScale = 1.0f + 0.02f * Mathf.Sin(time * 2.2f);
            titleText.rectTransform.localScale = Vector3.one * titleScale;
        }

        // Subtle Card Frame float
        if (cardFrame != null && !hasStarted)
        {
            float cardFloatY = Mathf.Sin(time * 1.6f) * 3.5f;
            cardFrame.anchoredPosition = cardBasePos + new Vector2(0f, cardFloatY);
        }
    }

    private void AnimateFloatingMotes(float time)
    {
        for (int i = 0; i < floatingMotes.Count; i++)
        {
            if (floatingMotes[i] == null) continue;

            Vector2 pos = floatingMotes[i].anchoredPosition;
            pos.y += moteSpeeds[i] * Time.deltaTime;

            // Gentle horizontal sway
            pos.x = moteBaseX[i] + Mathf.Sin(time * moteSwayFreq[i] + i) * moteSwayAmp[i];

            // Wrap around when reaching top
            if (pos.y > 480f)
            {
                pos.y = -480f;
                moteBaseX[i] = Random.Range(-750f, 750f);
            }

            floatingMotes[i].anchoredPosition = pos;
            floatingMotes[i].Rotate(0f, 0f, moteRotSpeeds[i] * Time.deltaTime);
        }
    }

    private void AnimatePressKeyText(float time)
    {
        if (pressAnyKeyText != null && !hasStarted)
        {
            float t = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color c = pressAnyKeyText.color;
            c.a = alpha;
            pressAnyKeyText.color = c;

            // Subtle scale pulse on text
            float scale = Mathf.Lerp(0.96f, 1.04f, t);
            pressAnyKeyText.rectTransform.localScale = Vector3.one * scale;
        }
    }

    private void StartGame()
    {
        hasStarted = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        StartCoroutine(LaunchGameSequence());
    }

    private IEnumerator LaunchGameSequence()
    {
        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.text = "OPENING ADVENTURE MAP...";
            pressAnyKeyText.color = new Color(0.1f, 1f, 0.75f, 1f);
            pressAnyKeyText.rectTransform.localScale = Vector3.one * 1.15f;
        }

        // Celebratory synchronized hop for both Mage and Echo!
        StartCoroutine(CelebratoryLaunchHop(mageIcon, mageBasePos));
        StartCoroutine(CelebratoryLaunchHop(echoIcon, echoBasePos));

        // Title pop pulse
        if (titleText != null)
        {
            titleText.rectTransform.localScale = Vector3.one * 1.10f;
        }

        // Smooth cinematic fade out
        float elapsed = 0f;
        float fadeDuration = 0.75f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = Mathf.Clamp01(1f - t);
            }

            if (cardFrame != null)
            {
                // Subtle zoom inward as it transitions
                cardFrame.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.06f, t);
            }

            yield return null;
        }

        SceneManager.LoadScene(firstLevelScene);
    }

    private IEnumerator CelebratoryLaunchHop(RectTransform target, Vector2 basePos)
    {
        if (target == null) yield break;

        float duration = 0.55f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float jumpY = Mathf.Sin(t * Mathf.PI) * 35f;
            target.anchoredPosition = basePos + new Vector2(0f, jumpY);
            yield return null;
        }
    }

    private IEnumerator FadeInTitleScreen()
    {
        float elapsed = 0f;
        float duration = 0.85f;
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
}
