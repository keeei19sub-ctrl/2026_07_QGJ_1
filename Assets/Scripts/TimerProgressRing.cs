using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TimerProgressRing : MonoBehaviour
{
    private const string CanvasName = "Timer Progress Canvas";
    private const string GaugeName = "Timer Progress Ring";

    [SerializeField] private PlayerItemEffectController itemEffectController;
    [SerializeField] private Sprite gaugeSprite;
    [SerializeField] private Color trackColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color fillColor = Color.white;

    private GameObject gaugeRoot;
    private Image gaugeFill;

    private void Awake()
    {
        CreateGauge();
        SetGaugeVisible(false);
    }

    private void Update()
    {
        ResolveItemEffectController();
        if (gaugeFill == null
            || itemEffectController == null
            || !itemEffectController.TryGetTimedEffectProgress(out float progress))
        {
            SetGaugeVisible(false);
            return;
        }

        gaugeFill.fillAmount = Mathf.Clamp01(progress);
        SetGaugeVisible(true);
    }

    private void ResolveItemEffectController()
    {
        if (itemEffectController == null)
        {
            itemEffectController = FindAnyObjectByType<PlayerItemEffectController>();
        }
    }

    private void CreateGauge()
    {
        if (gaugeSprite == null)
        {
            Debug.LogWarning("TimerProgressRing requires a gauge sprite.", this);
            return;
        }

        GameObject canvasObject = new GameObject(
            CanvasName,
            typeof(RectTransform),
            typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;

        gaugeRoot = new GameObject(
            GaugeName,
            typeof(RectTransform),
            typeof(AspectRatioFitter));
        gaugeRoot.transform.SetParent(canvasObject.transform, false);

        RectTransform gaugeRect = gaugeRoot.GetComponent<RectTransform>();
        gaugeRect.anchorMin = new Vector2(0.46f, 0.04f);
        gaugeRect.anchorMax = new Vector2(0.54f, 0.182f);
        gaugeRect.offsetMin = Vector2.zero;
        gaugeRect.offsetMax = Vector2.zero;

        AspectRatioFitter aspectRatioFitter = gaugeRoot.GetComponent<AspectRatioFitter>();
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectRatioFitter.aspectRatio = 1f;

        CreateGaugeImage("Track", trackColor, false);
        gaugeFill = CreateGaugeImage("Fill", fillColor, true);
    }

    private Image CreateGaugeImage(string objectName, Color color, bool isFill)
    {
        GameObject imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.transform.SetParent(gaugeRoot.transform, false);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = gaugeSprite;
        image.color = color;
        image.raycastTarget = false;
        image.preserveAspect = true;

        if (isFill)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = 0f;
        }

        return image;
    }

    private void SetGaugeVisible(bool visible)
    {
        if (gaugeRoot != null && gaugeRoot.activeSelf != visible)
        {
            gaugeRoot.SetActive(visible);
        }
    }
}
