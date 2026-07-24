using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    private const float KingIndicatorWidthPercent = 5f;
    private const float KingIndicatorEdgePaddingPercent = 1f;
    private const float ProgressIconHeightPercent = 10f;
    private const float SpeechBubbleWidthPercent = 26f;
    private const float SpeechBubbleHeightPercent = 19f;
    private const float SpeechBubbleEdgePaddingPercent = 1f;
    private const float SpeechBubbleGapPercent = 1f;
    private const float MinimumSpeechDuration = 0.1f;
    private const float DefaultSpeechDuration = 3f;

    [Header("Progress Indicator")]
    [SerializeField] private Transform playerProgressTarget;
    [SerializeField] private Transform kingProgressTarget;
    [SerializeField] private float progressStartY;
    [SerializeField] private float progressGoalY = 41f;

    [Header("Speech Bubbles")]
    [SerializeField] private Camera speechCamera;
    [SerializeField] private Transform playerSpeechTarget;
    [SerializeField] private SpriteRenderer playerSpeechRenderer;
    [SerializeField] private Transform kingSpeechTarget;
    [SerializeField] private SpriteRenderer kingSpeechRenderer;

    [Header("Inventory Images")]
    [SerializeField] private Sprite inventoryFallbackSprite;
    [SerializeField] private Sprite kingMedicineSprite;
    [SerializeField] private Sprite largeUmbrellaPotionSprite;
    [SerializeField] private Sprite stoneStopCharmSprite;

    private VisualElement m_Healthbar;
    private VisualElement m_PlayerProgressIcon;
    private VisualElement m_KingProgressIcon;
    private VisualElement m_KingAboveIndicator;
    private VisualElement m_KingBelowIndicator;
    private VisualElement m_ProjectileWarningLeft;
    private VisualElement m_ProjectileWarningRight;
    private VisualElement m_ShopUI;
    private VisualElement m_InventoryImage;
    private Label m_MoneyLabel;
    private Label m_SelectedItemLabel;
    private Label m_SelectedItemCountLabel;
    private Label m_SelectedItemDescriptionLabel;
    private VisualElement m_SelectedItemPicture;
    private Label m_ShopItemNameLabel;
    private Label m_ShopPriceLabel;
    private Label m_ShopMessageLabel;
    private VisualElement m_PauseOverlay;
    private Button m_ResumeButton;
    private Button m_RestartButton;
    private Button m_TitleButton;
    private SpeechBubbleState m_PlayerSpeech;
    private SpeechBubbleState m_KingSpeech;
    private Label m_PlayerMoney;

    private PlayerWallet m_Wallet;
    private PlayerInventory m_Inventory;
    private Shop m_VisibleShop;
    private bool m_PlayerEventsSubscribed;
    private bool m_IsPaused;

    public static UIHandler instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        SubscribeToPlayerEvents();
    }

    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIHandler requires a UIDocument component.", this);
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        m_Healthbar = root.Q<VisualElement>("HealthBar");
        m_PlayerProgressIcon = root.Q<VisualElement>("PlayerProgressIcon");
        m_KingProgressIcon = root.Q<VisualElement>("KingProgressIcon");
        m_KingAboveIndicator = root.Q<VisualElement>("KingAboveIndicator");
        m_KingBelowIndicator = root.Q<VisualElement>("KingBelowIndicator");
        m_ProjectileWarningLeft = root.Q<VisualElement>("ProjectileWarningLeft");
        m_ProjectileWarningRight = root.Q<VisualElement>("ProjectileWarningRight");

        m_ShopUI = root.Q<VisualElement>("ShopUI");
        m_MoneyLabel = root.Q<Label>("MoneyLabel");
        m_ShopMessageLabel = root.Q<Label>("ShopMessageLabel");
        m_ShopPriceLabel = root.Q<Label>("ShopItemPrice");
        m_ShopItemNameLabel = root.Q<Label>("ShopItemName");
        m_SelectedItemLabel = root.Q<Label>("ItemName");
        m_SelectedItemCountLabel = root.Q<Label>("ItemCount");
        m_SelectedItemDescriptionLabel = root.Q<Label>("ItemDescription");
        m_PlayerMoney = root.Q<Label>("PlayerWalletCount");
        m_SelectedItemPicture = root.Q<VisualElement>("ItemPicture");
        m_PauseOverlay = root.Q<VisualElement>("PauseOverlay");
        m_ResumeButton = root.Q<Button>("ResumeButton");
        m_RestartButton = root.Q<Button>("RestartButton");
        m_TitleButton = root.Q<Button>("TitleButton");
        m_PlayerSpeech = new SpeechBubbleState(
            root.Q<VisualElement>("PlayerSpeechBubble"),
            root.Q<Label>("PlayerSpeechLabel"),
            playerSpeechTarget,
            playerSpeechRenderer);
        m_KingSpeech = new SpeechBubbleState(
            root.Q<VisualElement>("KingSpeechBubble"),
            root.Q<Label>("KingSpeechLabel"),
            kingSpeechTarget,
            kingSpeechRenderer);
        m_InventoryImage = root.Q<VisualElement>("ItemPicture");

        if (speechCamera == null)
        {
            speechCamera = Camera.main;
        }

        RegisterPauseCallbacks();

        SetHealthValue(1.0f);
        SetPlayerProgressValue(0.0f);
        SetKingProgressValue(0.0f);
        HideKingIndicator();
        HideProjectileWarning();
        HideSpeech(SpeechSpeaker.Player);
        HideSpeech(SpeechSpeaker.King);
        HideShop();
        RefreshPlayerUI();
        SetPaused(false);

        ShowSpeech(SpeechSpeaker.King, "出発じゃ！");
        ShowSpeech(SpeechSpeaker.Player, "王様についていこう！");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetPaused(!m_IsPaused);
        }

        UpdateSpeechTimers();
    }

    private void LateUpdate()
    {
        UpdateProgressIcons();
        UpdateSpeechBubblePositions();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayerEvents();
    }

    private void OnDestroy()
    {
        UnregisterPauseCallbacks();
        Time.timeScale = 1f;

        UnsubscribeFromPlayerEvents();
        if (instance == this)
        {
            instance = null;
        }
    }

    public void SetPaused(bool paused)
    {
        m_IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (m_PauseOverlay != null)
        {
            m_PauseOverlay.style.display = paused ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (paused)
        {
            m_ResumeButton?.Focus();
        }
    }

    private void RegisterPauseCallbacks()
    {
        if (m_ResumeButton != null)
        {
            m_ResumeButton.clicked += ResumeGame;
        }

        if (m_RestartButton != null)
        {
            m_RestartButton.clicked += RestartScene;
        }

        if (m_TitleButton != null)
        {
            m_TitleButton.clicked += ReturnToTitle;
        }
    }

    private void UnregisterPauseCallbacks()
    {
        if (m_ResumeButton != null)
        {
            m_ResumeButton.clicked -= ResumeGame;
        }

        if (m_RestartButton != null)
        {
            m_RestartButton.clicked -= RestartScene;
        }

        if (m_TitleButton != null)
        {
            m_TitleButton.clicked -= ReturnToTitle;
        }
    }

    private void ResumeGame()
    {
        SetPaused(false);
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToTitle()
    {
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded("title"))
        {
            SceneManager.LoadScene("title");
            return;
        }

        Debug.LogWarning("The title scene is not included in Build Settings.", this);
        SetPaused(false);
    }

    public void BindPlayer(PlayerWallet wallet, PlayerInventory inventory)
    {
        if (m_Wallet == wallet && m_Inventory == inventory)
        {
            RefreshPlayerUI();
            return;
        }

        UnsubscribeFromPlayerEvents();
        m_Wallet = wallet;
        m_Inventory = inventory;

        if (isActiveAndEnabled)
        {
            SubscribeToPlayerEvents();
        }

        RefreshPlayerUI();
    }

    private void ChangeItemImage(string itemId)
    {
        if (m_InventoryImage == null)
        {
            return;
        }

        Sprite sprite = InventoryImg.ItemImg(itemId) switch
        {
            "item_cake" => kingMedicineSprite,
            "item_umbrella" => largeUmbrellaPotionSprite,
            "item_pan" => stoneStopCharmSprite,
            _ => inventoryFallbackSprite
        };

        if (sprite != null)
        {
            m_InventoryImage.style.backgroundImage = new StyleBackground(sprite);
        }
    }
    public void ShowShop(Shop shop)
    {
        m_VisibleShop = shop;

        if (m_ShopUI != null)
        {
            m_ShopUI.style.display = DisplayStyle.Flex;
        }

        ItemDefinition item = shop != null ? shop.Item : null;
        if (item == null)
        {
            SetLabelText(m_ShopItemNameLabel, "商品未設定");
            SetLabelText(m_ShopPriceLabel, "--");
            SetLabelText(m_ShopMessageLabel, "この店では購入できません");
            return;
        }

        SetLabelText(m_ShopItemNameLabel, string.IsNullOrWhiteSpace(item.DisplayName) ? "名称未設定" : item.DisplayName);
        SetLabelText(m_ShopPriceLabel, $"{item.Price} G");
        SetLabelText(m_ShopMessageLabel, string.Empty);
    }

    public void HideShop()
    {
        m_VisibleShop = null;

        if (m_ShopUI != null)
        {
            m_ShopUI.style.display = DisplayStyle.None;
        }

        SetLabelText(m_ShopMessageLabel, string.Empty);
    }

    public void ShowPurchaseResult(PurchaseResult result)
    {
        if (m_VisibleShop == null)
        {
            return;
        }

        string message;
        switch (result)
        {
            case PurchaseResult.Success:
                message = "購入しました";
                break;
            case PurchaseResult.InsufficientFunds:
                message = "お金が足りません";
                break;
            case PurchaseResult.InventoryFull:
                message = "アイテムは1個しか持てません";
                break;
            case PurchaseResult.InvalidProduct:
                message = "商品が設定されていません";
                break;
            case PurchaseResult.MissingPlayerState:
                message = "購入できません";
                break;
            default:
                message = "購入に失敗しました";
                break;
        }

        SetLabelText(m_ShopMessageLabel, message);
    }

    public void SetHealthValue(float percentage)
    {
        if (m_Healthbar != null)
        {
            m_Healthbar.style.width = Length.Percent(100 * Mathf.Clamp01(percentage));
        }
    }

    public void SetPlayerProgressValue(float percentage)
    {
        SetProgressIconPosition(m_PlayerProgressIcon, percentage);
    }

    public void SetKingProgressValue(float percentage)
    {
        SetProgressIconPosition(m_KingProgressIcon, percentage);
    }

    public void ShowKingIndicator(KingIndicatorDirection direction, float viewportX)
    {
        float leftPercent = Mathf.Clamp(
            Mathf.Clamp01(viewportX) * 100f - KingIndicatorWidthPercent * 0.5f,
            KingIndicatorEdgePaddingPercent,
            100f - KingIndicatorWidthPercent - KingIndicatorEdgePaddingPercent);

        bool showAbove = direction == KingIndicatorDirection.Above;
        SetIndicatorVisible(m_KingAboveIndicator, showAbove, leftPercent);
        SetIndicatorVisible(m_KingBelowIndicator, !showAbove, leftPercent);
    }

    public void HideKingIndicator()
    {
        SetIndicatorVisible(m_KingAboveIndicator, false, 0f);
        SetIndicatorVisible(m_KingBelowIndicator, false, 0f);
    }

    public void ShowProjectileWarning(ProjectileWarningSide side)
    {
        SetElementVisible(
            m_ProjectileWarningLeft,
            side == ProjectileWarningSide.Left);
        SetElementVisible(
            m_ProjectileWarningRight,
            side == ProjectileWarningSide.Right);
    }

    public void HideProjectileWarning()
    {
        SetElementVisible(m_ProjectileWarningLeft, false);
        SetElementVisible(m_ProjectileWarningRight, false);
    }

    public void ShowSpeech(
        SpeechSpeaker speaker,
        string text,
        float duration = DefaultSpeechDuration)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            HideSpeech(speaker);
            return;
        }

        SpeechBubbleState state = GetSpeechState(speaker);
        if (!CanShowSpeech(speaker, state))
        {
            return;
        }

        state.Label.text = text;
        state.RemainingTime = Mathf.Max(MinimumSpeechDuration, duration);
        state.IsActive = true;
        UpdateSpeechBubblePosition(state);
    }

    public void HideSpeech(SpeechSpeaker speaker)
    {
        SpeechBubbleState state = GetSpeechState(speaker);
        if (state == null)
        {
            return;
        }

        state.IsActive = false;
        state.RemainingTime = 0f;
        SetElementVisible(state.Bubble, false);
    }

    private void UpdateSpeechTimers()
    {
        UpdateSpeechTimer(SpeechSpeaker.Player, m_PlayerSpeech);
        UpdateSpeechTimer(SpeechSpeaker.King, m_KingSpeech);
    }

    private void UpdateSpeechTimer(SpeechSpeaker speaker, SpeechBubbleState state)
    {
        if (state == null || !state.IsActive)
        {
            return;
        }

        state.RemainingTime -= Time.deltaTime;
        if (state.RemainingTime <= 0f)
        {
            HideSpeech(speaker);
        }
    }

    private void UpdateSpeechBubblePositions()
    {
        UpdateSpeechBubblePosition(m_PlayerSpeech);
        UpdateSpeechBubblePosition(m_KingSpeech);
    }

    private void UpdateSpeechBubblePosition(SpeechBubbleState state)
    {
        if (state == null || !state.IsActive || state.Bubble == null)
        {
            return;
        }

        if (speechCamera == null || state.Target == null || state.Renderer == null)
        {
            SetElementVisible(state.Bubble, false);
            return;
        }

        Bounds bounds = state.Renderer.bounds;
        Vector3 viewportMin = speechCamera.WorldToViewportPoint(bounds.min);
        Vector3 viewportMax = speechCamera.WorldToViewportPoint(bounds.max);
        bool isInFrontOfCamera = viewportMin.z > 0f && viewportMax.z > 0f;
        bool overlapsViewport = viewportMax.x >= 0f && viewportMin.x <= 1f
            && viewportMax.y >= 0f && viewportMin.y <= 1f;

        if (!isInFrontOfCamera || !overlapsViewport)
        {
            SetElementVisible(state.Bubble, false);
            return;
        }

        Vector3 anchorWorldPosition = new Vector3(
            state.Target.position.x,
            bounds.max.y,
            state.Target.position.z);
        Vector3 anchorViewportPosition = speechCamera.WorldToViewportPoint(anchorWorldPosition);
        float leftPercent = Mathf.Clamp(
            anchorViewportPosition.x * 100f - SpeechBubbleWidthPercent * 0.5f,
            SpeechBubbleEdgePaddingPercent,
            100f - SpeechBubbleWidthPercent - SpeechBubbleEdgePaddingPercent);
        float topPercent = Mathf.Clamp(
            (1f - anchorViewportPosition.y) * 100f
                - SpeechBubbleHeightPercent
                - SpeechBubbleGapPercent,
            SpeechBubbleEdgePaddingPercent,
            100f - SpeechBubbleHeightPercent - SpeechBubbleEdgePaddingPercent);

        state.Bubble.style.left = Length.Percent(leftPercent);
        state.Bubble.style.top = Length.Percent(topPercent);
        SetElementVisible(state.Bubble, true);
    }

    private SpeechBubbleState GetSpeechState(SpeechSpeaker speaker)
    {
        switch (speaker)
        {
            case SpeechSpeaker.Player:
                return m_PlayerSpeech;
            case SpeechSpeaker.King:
                return m_KingSpeech;
            default:
                Debug.LogWarning($"Unsupported speech speaker: {speaker}.", this);
                return null;
        }
    }

    private bool CanShowSpeech(SpeechSpeaker speaker, SpeechBubbleState state)
    {
        if (state != null
            && state.Bubble != null
            && state.Label != null
            && state.Target != null
            && state.Renderer != null
            && speechCamera != null)
        {
            return true;
        }

        if (state == null || !state.SetupWarningLogged)
        {
            Debug.LogWarning(
                $"Speech bubble setup is incomplete for {speaker}; the line was ignored.",
                this);

            if (state != null)
            {
                state.SetupWarningLogged = true;
            }
        }

        return false;
    }

    private void UpdateProgressIcons()
    {
        if (Mathf.Approximately(progressStartY, progressGoalY))
        {
            SetPlayerProgressValue(0f);
            SetKingProgressValue(0f);
            return;
        }

        if (playerProgressTarget != null)
        {
            SetPlayerProgressValue(Mathf.InverseLerp(
                progressStartY,
                progressGoalY,
                playerProgressTarget.position.y));
        }

        if (kingProgressTarget != null)
        {
            SetKingProgressValue(Mathf.InverseLerp(
                progressStartY,
                progressGoalY,
                kingProgressTarget.position.y));
        }
    }

    private void SubscribeToPlayerEvents()
    {
        if (m_PlayerEventsSubscribed)
        {
            return;
        }

        if (m_Wallet != null)
        {
            m_Wallet.MoneyChanged += OnMoneyChanged;
        }

        if (m_Inventory != null)
        {
            m_Inventory.InventoryChanged += OnInventoryChanged;
            m_Inventory.SelectionChanged += OnSelectionChanged;
        }

        m_PlayerEventsSubscribed = m_Wallet != null || m_Inventory != null;
    }

    private void UnsubscribeFromPlayerEvents()
    {
        if (!m_PlayerEventsSubscribed)
        {
            return;
        }

        if (m_Wallet != null)
        {
            m_Wallet.MoneyChanged -= OnMoneyChanged;
        }

        if (m_Inventory != null)
        {
            m_Inventory.InventoryChanged -= OnInventoryChanged;
            m_Inventory.SelectionChanged -= OnSelectionChanged;
        }

        m_PlayerEventsSubscribed = false;
    }

    private void OnMoneyChanged(int money)
    {
        SetLabelText(m_MoneyLabel, $"所持金: {money} G");
    }

    private void OnInventoryChanged()
    {
        RefreshInventoryUI();
    }

    private void OnSelectionChanged()
    {
        RefreshInventoryUI();
    }

    private void RefreshPlayerUI()
    {
        int money = m_Wallet != null ? m_Wallet.Money : 0;
        OnMoneyChanged(money);
        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {
        ItemDefinition selectedItem = m_Inventory != null ? m_Inventory.SelectedItem : null;
        int selectedCount = m_Inventory != null ? m_Inventory.SelectedCount : 0;

        if (selectedItem == null || selectedCount <= 0)
        {
            SetLabelText(m_SelectedItemLabel, "アイテムなし");
            SetLabelText(m_SelectedItemCountLabel, "×0");
            SetLabelText(m_SelectedItemDescriptionLabel, "アイテムを入手すると、ここに説明が表示されます");
            ChangeItemImage("none");
            SetItemPictureVisible(false);
            return;
        }

        string displayName = string.IsNullOrWhiteSpace(selectedItem.DisplayName)
            ? "名称未設定"
            : selectedItem.DisplayName;
        SetLabelText(m_SelectedItemLabel, displayName);
        SetLabelText(m_SelectedItemCountLabel, $"×{selectedCount}");
        SetLabelText(
            m_SelectedItemDescriptionLabel,
            string.IsNullOrWhiteSpace(selectedItem.Description) ? "説明はありません" : selectedItem.Description);
        ChangeItemImage(selectedItem.Id);
        SetItemPictureVisible(true);
    }

    private void SetItemPictureVisible(bool visible)
    {
        if (m_SelectedItemPicture != null)
        {
            m_SelectedItemPicture.style.opacity = visible ? 1.0f : 0.25f;
        }
    }

    private static void SetLabelText(Label label, string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }

    private static void SetIndicatorVisible(VisualElement indicator, bool visible, float leftPercent)
    {
        if (indicator == null)
        {
            return;
        }

        if (visible)
        {
            indicator.style.left = Length.Percent(leftPercent);
        }

        indicator.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void SetElementVisible(VisualElement element, bool visible)
    {
        if (element != null)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private static void SetProgressIconPosition(VisualElement icon, float percentage)
    {
        if (icon == null)
        {
            return;
        }

        float clampedPercentage = Mathf.Clamp01(percentage);
        float topPercent = (1f - clampedPercentage) * (100f - ProgressIconHeightPercent);
        icon.style.top = Length.Percent(topPercent);
    }

    private sealed class SpeechBubbleState
    {
        public SpeechBubbleState(
            VisualElement bubble,
            Label label,
            Transform target,
            SpriteRenderer renderer)
        {
            Bubble = bubble;
            Label = label;
            Target = target;
            Renderer = renderer;
        }

        public VisualElement Bubble { get; }
        public Label Label { get; }
        public Transform Target { get; }
        public SpriteRenderer Renderer { get; }
        public float RemainingTime { get; set; }
        public bool IsActive { get; set; }
        public bool SetupWarningLogged { get; set; }
    }
}

public enum SpeechSpeaker
{
    Player,
    King
}

public enum KingIndicatorDirection
{
    Above,
    Below
}

public enum ProjectileWarningSide
{
    Left,
    Right
}
