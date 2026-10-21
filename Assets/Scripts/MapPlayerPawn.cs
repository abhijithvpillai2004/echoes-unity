using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPlayerPawn : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pawnRectTransform;
    public Image mageImage;
    public Image auraRingImage;
    public Image shadowImage;

    [Header("Movement & Animation")]
    public float moveSpeed = 450f; // Pixels per second
    public float bobFrequency = 14f;
    public float bobHeight = 10f;
    public float idleFloatFrequency = 3.5f;
    public float idleFloatHeight = 4f;
    public Vector2 nodeOffset = new Vector2(0f, 36f); // Stands gracefully above the node button

    [Header("Echo Trail")]
    public bool spawnEchoTrail = true;
    public float trailSpawnInterval = 0.08f;
    public Transform trailParent;

    public int currentLevelNumber = 1;
    public bool isMoving { get; private set; } = false;

    private Vector2 baseAnchoredPosition;
    private float moveTimer = 0f;
    private float idleTimer = 0f;
    private int facingDirection = 1; // 1 = right, -1 = left

    private void Awake()
    {
        if (pawnRectTransform == null)
        {
            pawnRectTransform = GetComponent<RectTransform>();
        }
        if (mageImage == null)
        {
            mageImage = GetComponentInChildren<Image>();
        }
    }

    private void Update()
    {
        if (!isMoving)
        {
            // Idle floating animation
            idleTimer += Time.deltaTime;
            float floatY = Mathf.Sin(idleTimer * idleFloatFrequency) * idleFloatHeight;
            if (pawnRectTransform != null)
            {
                pawnRectTransform.anchoredPosition = baseAnchoredPosition + new Vector2(0f, floatY);
            }

            // Subtle magical aura pulse
            if (auraRingImage != null)
            {
                float t = (Mathf.Sin(idleTimer * 4f) + 1f) * 0.5f;
                auraRingImage.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1.15f, t);
                Color c = auraRingImage.color;
                c.a = Mathf.Lerp(0.35f, 0.75f, t);
                auraRingImage.color = c;
            }
        }
    }

    public void SetPositionAtNode(Vector2 nodePos, int levelNum)
    {
        currentLevelNumber = levelNum;
        baseAnchoredPosition = nodePos + nodeOffset;
        if (pawnRectTransform != null)
        {
            pawnRectTransform.anchoredPosition = baseAnchoredPosition;
        }
    }

    public IEnumerator MoveAlongPath(List<Vector2> waypoints, int targetLevelNum, System.Action onComplete)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        isMoving = true;
        float trailTimer = 0f;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector2 targetPos = waypoints[i] + nodeOffset;
            Vector2 startPos = baseAnchoredPosition;
            float distance = Vector2.Distance(startPos, targetPos);
            float duration = Mathf.Max(0.12f, distance / moveSpeed);
            float elapsed = 0f;

            // Determine horizontal facing direction
            float dx = targetPos.x - startPos.x;
            if (Mathf.Abs(dx) > 2f)
            {
                facingDirection = dx > 0 ? 1 : -1;
                if (mageImage != null)
                {
                    Vector3 s = mageImage.rectTransform.localScale;
                    s.x = Mathf.Abs(s.x) * facingDirection;
                    mageImage.rectTransform.localScale = s;
                }
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                moveTimer += Time.deltaTime;
                trailTimer += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                baseAnchoredPosition = Vector2.Lerp(startPos, targetPos, t);

                // Walking hop bounce
                float hopY = Mathf.Abs(Mathf.Sin(moveTimer * bobFrequency)) * bobHeight;
                if (pawnRectTransform != null)
                {
                    pawnRectTransform.anchoredPosition = baseAnchoredPosition + new Vector2(0f, hopY);
                }

                // Spawn ethereal echo footsteps
                if (spawnEchoTrail && trailTimer >= trailSpawnInterval)
                {
                    trailTimer = 0f;
                    SpawnEchoFootstep(pawnRectTransform.anchoredPosition);
                }

                yield return null;
            }

            baseAnchoredPosition = targetPos;
        }

        // Final arrival celebration hop
        yield return StartCoroutine(PlayArrivalHop());

        currentLevelNumber = targetLevelNum;
        isMoving = false;
        idleTimer = 0f;

        onComplete?.Invoke();
    }

    private IEnumerator PlayArrivalHop()
    {
        float hopDuration = 0.25f;
        float elapsed = 0f;
        Vector2 originalPos = baseAnchoredPosition;

        while (elapsed < hopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hopDuration;
            float peak = Mathf.Sin(t * Mathf.PI) * 16f;
            if (pawnRectTransform != null)
            {
                pawnRectTransform.anchoredPosition = originalPos + new Vector2(0f, peak);
            }
            yield return null;
        }

        if (pawnRectTransform != null)
        {
            pawnRectTransform.anchoredPosition = originalPos;
        }
    }

    private void SpawnEchoFootstep(Vector2 pos)
    {
        if (mageImage == null || mageImage.sprite == null) return;

        GameObject echoObj = new GameObject("EchoFootprint");
        Transform parent = trailParent != null ? trailParent : transform.parent;
        echoObj.transform.SetParent(parent, false);
        echoObj.transform.SetSiblingIndex(transform.GetSiblingIndex());

        RectTransform rt = echoObj.AddComponent<RectTransform>();
        rt.anchorMin = pawnRectTransform.anchorMin;
        rt.anchorMax = pawnRectTransform.anchorMax;
        rt.pivot = pawnRectTransform.pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = mageImage.rectTransform.sizeDelta;
        rt.localScale = mageImage.rectTransform.localScale;

        Image img = echoObj.AddComponent<Image>();
        img.sprite = mageImage.sprite;
        img.color = new Color(0.2f, 0.9f, 1f, 0.45f);
        img.raycastTarget = false;

        StartCoroutine(FadeAndDestroyEcho(echoObj, img, 0.4f));
    }

    private IEnumerator FadeAndDestroyEcho(GameObject obj, Image img, float duration)
    {
        float elapsed = 0f;
        Color initialColor = img != null ? img.color : Color.cyan;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (img != null)
            {
                Color c = initialColor;
                c.a = Mathf.Lerp(initialColor.a, 0f, t);
                img.color = c;
            }
            yield return null;
        }

        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
