using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the minimal training HUD at runtime and keeps it in sync.</summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Text statsText;
    private Text ammoText;
    private Text reloadText;
    private Text hitFeedbackText;
    private bool initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Canvas canvas = GetComponent<Canvas>();
        if (!canvas)
        {
            Debug.LogError("UIManager requires a Canvas on the same GameObject.", this);
            enabled = false;
            return;
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Initialize(canvas);
    }

    public void Initialize(Canvas canvas)
    {
        if (!canvas || initialized) return;

        Transform oldGeneratedUi = canvas.transform.Find("GeneratedUI");
        if (oldGeneratedUi)
        {
            oldGeneratedUi.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(oldGeneratedUi.gameObject);
            else DestroyImmediate(oldGeneratedUi.gameObject);
        }

        GameObject container = new GameObject("GeneratedUI", typeof(RectTransform));
        container.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        GameObject hud = new GameObject("TrainingHUD", typeof(RectTransform));
        hud.transform.SetParent(container.transform, false);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        statsText = Label(hud.transform, "Stats", 22, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(330f, 126f), new Vector2(28f, -24f));
        statsText.alignment = TextAnchor.UpperLeft;
        ammoText = Label(hud.transform, "Ammo", 22, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(290f, 54f), new Vector2(-28f, -24f));
        ammoText.alignment = TextAnchor.UpperRight;
        Label(hud.transform, "+", 26, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28f, 28f));
        hitFeedbackText = Label(hud.transform, "", 28, new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.59f), new Vector2(320f, 48f));
        hitFeedbackText.gameObject.SetActive(false);
        reloadText = Label(hud.transform, "RELOADING", 22, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(260f, 44f));
        reloadText.gameObject.SetActive(false);

        initialized = true;
        Refresh();
    }

    public void Refresh()
    {
        if (statsText)
        {
            ScoreManager score = ScoreManager.Instance;
            int scoreValue = score ? score.Score : 0;
            int hits = score ? score.Hits : 0;
            int shots = score ? score.Shots : 0;
            float accuracy = score ? score.Accuracy : 0f;
            statsText.text = $"Targets Hit: {hits}\nShots Fired: {shots}\nAccuracy: {accuracy:0.0}%\nScore: {scoreValue}";
        }

        GunController gun = FindAnyObjectByType<GunController>();
        if (ammoText) ammoText.text = gun ? $"Ammo: {gun.Magazine} / {gun.Reserve}" : "Ammo: --";
        if (reloadText) reloadText.gameObject.SetActive(gun && gun.Reloading);
    }

    public void ShowHitFeedback(int points)
    {
        if (!hitFeedbackText) return;
        hitFeedbackText.text = $"HIT  +{points}";
        hitFeedbackText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideHitFeedback));
        Invoke(nameof(HideHitFeedback), 0.55f);
    }

    private void HideHitFeedback()
    {
        if (hitFeedbackText) hitFeedbackText.gameObject.SetActive(false);
    }

    private static Text Label(Transform parent, string name, int size, Vector2 anchorMin, Vector2 anchorMax, Vector2 dimensions, Vector2? position = null)
    {
        GameObject label = new GameObject(name, typeof(RectTransform), typeof(Text));
        label.transform.SetParent(parent, false);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position ?? Vector2.zero;

        Text text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        Outline outline = label.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(1f, -1f);
        return text;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
