using UnityEngine;
using TMPro;
using System.Collections;

public class CalloutManager : MonoBehaviour
{
    public static Color evilHour = new Color(0.7692f, 0.4f, 1f);
    public static Color rushHour = new Color(0.898f, 0.237f, 0.193f);
    public static Color negative = new Color(1f, 0.322f, 0.467f);
    public static Color recipe = new Color(0.631f, 0.545f, 0.945f);
    public static Color eyeball = new Color(1f, 0.678f, 0.549f);
    public static Color inactive = new Color(0.607f, 0.607f, 0.607f);

    
    
    [Header("Callout Sections")]
    [SerializeField] private RectTransform calloutsA;
    [SerializeField] private RectTransform calloutsB;
    [SerializeField] private RectTransform wheelCallouts;
    [SerializeField] private RectTransform timeCallouts;

    [Header("Prefabs")]
    [SerializeField] private TextMeshProUGUI calloutPrefab;

    /// <summary>
    /// Spawns a callout text under a specific section.
    /// type: 0 = calloutsA, 1 = calloutsB, 2 = wheelCallouts, 3 = timeCallouts
    /// </summary>
    public void SpawnCallout(int type, string message, Color? color = null)
    {
        RectTransform parent = null;
        float duration = (type <= 1) ? 3f : 4f;

        switch (type)
        {
            case 0: parent = calloutsA; break;
            case 1: parent = calloutsB; break;
            case 2: parent = wheelCallouts; break;
            case 3: parent = timeCallouts; break;
        }

        if (parent == null) return;

        // Spawn instance
        TextMeshProUGUI tmp = Instantiate(calloutPrefab, parent);
        tmp.text = message;
        tmp.fontSize = (type < 3) ? 48 : 96;
        tmp.color = color ?? Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.lineSpacing = -16f;
        tmp.alpha = 0f;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        GameObject obj = tmp.gameObject;
        obj.SetActive(true);

        if (type <= 1)
            StartCoroutine(AnimateUpwardCallout(tmp, rect, duration));
        else
            StartCoroutine(AnimateScaleCallout(tmp, rect, duration));
    }

    // ==========================================
    // Upward Floating Callouts (A, B)
    // ==========================================
    private IEnumerator AnimateUpwardCallout(TextMeshProUGUI tmp, RectTransform rect, float duration)
    {
        float fadeInTime = 0.25f;
        float fadeOutTime = 1.0f;
        float moveDistance = 120f;
        float elapsed = 0f;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, moveDistance);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Decelerating upward motion
            float moveT = 1f - Mathf.Pow(1f - t, 2f);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, moveT);

            // Fade in
            if (elapsed < fadeInTime)
            {
                tmp.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            }
            // Fade out
            else if (elapsed > duration - fadeOutTime)
            {
                float fadeOutElapsed = elapsed - (duration - fadeOutTime);
                tmp.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeOutTime);
            }
            else
            {
                tmp.alpha = 1f;
            }

            yield return null;
        }

        Destroy(tmp.gameObject);
    }

    // ==========================================
    // Scaling Callouts (Wheel, Time)
    // ==========================================
    private IEnumerator AnimateScaleCallout(TextMeshProUGUI tmp, RectTransform rect, float duration)
    {
        float fadeInTime = 0.75f;
        float fadeOutTime = 1.5f;
        float elapsed = 0f;

        Vector3 startScale = Vector3.one * 0.8f;
        Vector3 endScale = Vector3.one * 1.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.localScale = Vector3.Lerp(startScale, endScale, t);

            // Fade in
            if (elapsed < fadeInTime)
            {
                tmp.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            }
            // Fade out
            else if (elapsed > duration - fadeOutTime)
            {
                float fadeOutElapsed = elapsed - (duration - fadeOutTime);
                tmp.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeOutTime);
            }
            else
            {
                tmp.alpha = 1f;
            }

            yield return null;
        }

        Destroy(tmp.gameObject);
    }
}
