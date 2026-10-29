using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Beautiful, animated End Game Victory Screen displayed when completing the final level (Level 5).
/// Features a dark vignette overlay, golden ornate victory card, celebratory Mage + Echoes party bounce,
/// floating celebratory stars, and interactive action buttons.
/// </summary>
public class FinalVictoryScreen : MonoBehaviour
{
    public static FinalVictoryScreen Instance { get; private set; }

    [Header("Sprite References")]
    public Sprite mageSprite;
    public Sprite chestSprite;
    public Sprite starSprite;
    public Sprite pillSprite;

    private CanvasGroup canvasGroup;
    private RectTransform cardTransform;
    private RectTransform chestTransform;
    private List<RectTransform> partyCharacters = new List<RectTransform>();
    private List<RectTransform> floatingStars = new List<RectTransform>();
    private List<float> starSpeeds = new List<float>();
    private List<float> starRotSpeeds = new List<float>();

    private bool isShowing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        FindSprites();
        BuildUI();
        gameObject.SetActive(false);
    }

    private void FindSprites()
    {
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (var s in allSprites)
        {
            if (s == null) continue;
            if (mageSprite == null && s.name == "tilemap_packed_84") mageSprite = s;
            if (chestSprite == null && s.name == "chest_grand") chestSprite = s;
            if (starSprite == null && s.name == "ui_icon_star") starSprite = s;
            if (pillSprite == null && s.name == "ui_nameplate_pill") pillSprite = s;
        }
    }

    private void BuildUI()
    {
        // 1. Root Setup
        RectTransform rootRt = GetComponent<RectTransform>();
        if (rootRt == null) rootRt = gameObject.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        rootRt.anchoredPosition = Vector2.zero;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 2. Full-screen dark cinematic backdrop
        GameObject backdrop = new GameObject("DarkBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backdrop.transform.SetParent(transform, false);
        RectTransform bdRt = backdrop.GetComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdrop.GetComponent<Image>();
        bdImg.color = new Color(0.04f, 0.05f, 0.10f, 0.94f); // Rich midnight dark translucent

        // 3. Floating Celebration Stars Container
        GameObject starParent = new GameObject("CelebrationStars", typeof(RectTransform));
        starParent.transform.SetParent(transform, false);
        RectTransform spRt = starParent.GetComponent<RectTransform>();
        spRt.anchorMin = Vector2.zero;
        spRt.anchorMax = Vector2.one;
        spRt.sizeDelta = Vector2.zero;

        for (int i = 0; i < 16; i++)
        {
            GameObject starObj = new GameObject($"Star_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            starObj.transform.SetParent(starParent.transform, false);
            RectTransform sRt = starObj.GetComponent<RectTransform>();
            sRt.sizeDelta = new Vector2(Random.Range(20f, 36f), Random.Range(20f, 36f));
            sRt.anchoredPosition = new Vector2(Random.Range(-550f, 550f), Random.Range(-350f, 350f));

            Image sImg = starObj.GetComponent<Image>();
            if (starSprite != null) sImg.sprite = starSprite;
            sImg.color = new Color(1f, 0.88f, 0.35f, Random.Range(0.4f, 0.85f));

            floatingStars.Add(sRt);
            starSpeeds.Add(Random.Range(30f, 75f));
            starRotSpeeds.Add(Random.Range(-60f, 60f));
        }

        // 4. Central Golden Victory Card Outer Frame
        GameObject cardOuter = new GameObject("VictoryCardBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardOuter.transform.SetParent(transform, false);
        cardTransform = cardOuter.GetComponent<RectTransform>();
        cardTransform.sizeDelta = new Vector2(760f, 540f);
        cardTransform.anchoredPosition = Vector2.zero;

        Image borderImg = cardOuter.GetComponent<Image>();
        if (pillSprite != null)
        {
            borderImg.sprite = pillSprite;
            borderImg.type = Image.Type.Sliced;
        }
        borderImg.color = new Color(1f, 0.82f, 0.25f, 1f); // Radiant gold border

        // Card Inner Body
        GameObject cardInner = new GameObject("VictoryCardInner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardInner.transform.SetParent(cardOuter.transform, false);
        RectTransform innerRt = cardInner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.sizeDelta = new Vector2(-8f, -8f); // 4px border inset
        innerRt.anchoredPosition = Vector2.zero;

        Image innerImg = cardInner.GetComponent<Image>();
        if (pillSprite != null)
        {
            innerImg.sprite = pillSprite;
            innerImg.type = Image.Type.Sliced;
        }
        innerImg.color = new Color(0.09f, 0.11f, 0.18f, 0.98f); // Deep navy panel

        // 5. Grand Chest / Trophy Header Icon
        GameObject chestObj = new GameObject("ChestIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        chestObj.transform.SetParent(cardInner.transform, false);
        chestTransform = chestObj.GetComponent<RectTransform>();
        chestTransform.sizeDelta = new Vector2(72f, 72f);
        chestTransform.anchoredPosition = new Vector2(0f, 195f);

        Image chestImg = chestObj.GetComponent<Image>();
        if (chestSprite != null) chestImg.sprite = chestSprite;
        chestImg.preserveAspect = true;

        // 6. Header Title Text
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(cardInner.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.sizeDelta = new Vector2(700f, 60f);
        titleRt.anchoredPosition = new Vector2(0f, 130f);

        TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
        titleTmp.text = "<color=#FFD700><size=44><b>CONGRATULATIONS!</b></size></color>";
        titleTmp.alignment = TextAlignmentOptions.Center;

        // 7. Subtitle Text
        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(cardInner.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.sizeDelta = new Vector2(700f, 40f);
        subRt.anchoredPosition = new Vector2(0f, 78f);

        TextMeshProUGUI subTmp = subObj.GetComponent<TextMeshProUGUI>();
        subTmp.text = "<color=#2DD4BF><size=22><b>YOU CONQUERED ALL TIME LOOPS!</b></size></color>";
        subTmp.alignment = TextAlignmentOptions.Center;

        // 8. Lore Text
        GameObject descObj = new GameObject("DescText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        descObj.transform.SetParent(cardInner.transform, false);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.sizeDelta = new Vector2(680f, 50f);
        descRt.anchoredPosition = new Vector2(0f, 32f);

        TextMeshProUGUI descTmp = descObj.GetComponent<TextMeshProUGUI>();
        descTmp.text = "<color=#CBD5E1><size=17>The timeline is restored and the dungeon has fallen.\nYou have mastered the echoes of time!</size></color>";
        descTmp.alignment = TextAlignmentOptions.Center;

        // 9. Animated Character Party Row (Center Hero Mage + 4 Flanking Echoes)
        GameObject partyObj = new GameObject("PartyContainer", typeof(RectTransform));
        partyObj.transform.SetParent(cardInner.transform, false);
        RectTransform partyRt = partyObj.GetComponent<RectTransform>();
        partyRt.sizeDelta = new Vector2(500f, 80f);
        partyRt.anchoredPosition = new Vector2(0f, -48f);

        float[] xOffsets = new float[] { -170f, -85f, 0f, 85f, 170f };
        for (int i = 0; i < 5; i++)
        {
            bool isLeader = (i == 2);
            GameObject charObj = new GameObject(isLeader ? "LeadMage" : $"Echo_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            charObj.transform.SetParent(partyRt, false);

            RectTransform cRt = charObj.GetComponent<RectTransform>();
            cRt.sizeDelta = isLeader ? new Vector2(76f, 76f) : new Vector2(58f, 58f);
            cRt.anchoredPosition = new Vector2(xOffsets[i], 0f);

            Image cImg = charObj.GetComponent<Image>();
            if (mageSprite != null) cImg.sprite = mageSprite;
            cImg.preserveAspect = true;

            if (isLeader)
            {
                cImg.color = Color.white; // True purple mage
            }
            else
            {
                // Semi-transparent cyan ethereal echo
                cImg.color = new Color(0.35f, 0.95f, 1f, 0.65f);
            }

            partyCharacters.Add(cRt);
        }

        // 10. Action Buttons Row (Play Again, Level Select, Title Screen)
        GameObject btnRow = new GameObject("ButtonRow", typeof(RectTransform));
        btnRow.transform.SetParent(cardInner.transform, false);
        RectTransform btnRowRt = btnRow.GetComponent<RectTransform>();
        btnRowRt.sizeDelta = new Vector2(680f, 60f);
        btnRowRt.anchoredPosition = new Vector2(0f, -170f);

        CreateActionButton(btnRow.transform, "PlayAgainBtn", "Play Again (R)", new Vector2(-220f, 0f), new Color(0.18f, 0.55f, 0.35f, 1f), () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        CreateActionButton(btnRow.transform, "LevelSelectBtn", "Level Select (L)", new Vector2(0f, 0f), new Color(0.20f, 0.45f, 0.75f, 1f), () =>
        {
            SceneManager.LoadScene("LevelSelect");
        });

        CreateActionButton(btnRow.transform, "TitleBtn", "Main Menu (ESC)", new Vector2(220f, 0f), new Color(0.55f, 0.22f, 0.28f, 1f), () =>
        {
            SceneManager.LoadScene("TitleScreen");
        });
    }

    private void CreateActionButton(Transform parent, string name, string label, Vector2 pos, Color baseColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform bRt = btnObj.GetComponent<RectTransform>();
        bRt.sizeDelta = new Vector2(195f, 48f);
        bRt.anchoredPosition = pos;

        Image bImg = btnObj.GetComponent<Image>();
        if (pillSprite != null)
        {
            bImg.sprite = pillSprite;
            bImg.type = Image.Type.Sliced;
        }
        bImg.color = baseColor;

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        ColorBlock cb = btn.colors;
        cb.normalColor = baseColor;
        cb.highlightedColor = baseColor * 1.25f;
        cb.pressedColor = baseColor * 0.85f;
        btn.colors = cb;

        // Label
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform tRt = textObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = $"<b>{label}</b>";
        tmp.fontSize = 17;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        isShowing = true;
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (cardTransform != null) cardTransform.localScale = Vector3.one * 0.6f;

        float elapsed = 0f;
        float duration = 0.55f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(elapsed / 0.35f);
            }

            if (cardTransform != null)
            {
                // Elastic pop bounce: t + sin overshoot
                float bounce = Mathf.Sin(t * Mathf.PI * 0.5f) + 0.15f * Mathf.Sin(t * Mathf.PI);
                float scale = Mathf.Lerp(0.6f, 1.0f, bounce);
                cardTransform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (cardTransform != null) cardTransform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (!isShowing) return;

        // 1. Keyboard shortcuts for immediate, frictionless navigation
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SceneManager.LoadScene("LevelSelect");
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("TitleScreen");
            return;
        }

        // 2. Animated Mage and Echoes celebration bounce
        for (int i = 0; i < partyCharacters.Count; i++)
        {
            if (partyCharacters[i] != null)
            {
                float phase = i * 0.65f;
                float bounceY = Mathf.Abs(Mathf.Sin(Time.time * 6.5f + phase)) * 14f;
                Vector2 pos = partyCharacters[i].anchoredPosition;
                pos.y = bounceY;
                partyCharacters[i].anchoredPosition = pos;
            }
        }

        // 3. Chest gentle breathing pulse
        if (chestTransform != null)
        {
            float chestScale = 1.0f + 0.08f * Mathf.Sin(Time.time * 3.5f);
            chestTransform.localScale = Vector3.one * chestScale;
        }

        // 4. Floating celebratory stars upward drift
        for (int i = 0; i < floatingStars.Count; i++)
        {
            if (floatingStars[i] != null)
            {
                Vector2 sPos = floatingStars[i].anchoredPosition;
                sPos.y += starSpeeds[i] * Time.unscaledDeltaTime;
                if (sPos.y > 380f)
                {
                    sPos.y = -380f;
                    sPos.x = Random.Range(-550f, 550f);
                }
                floatingStars[i].anchoredPosition = sPos;
                floatingStars[i].Rotate(0f, 0f, starRotSpeeds[i] * Time.unscaledDeltaTime);
            }
        }
    }
}
