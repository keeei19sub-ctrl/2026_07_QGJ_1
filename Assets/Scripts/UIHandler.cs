using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    private const float KingIndicatorWidthPercent = 5f;
    private const float KingIndicatorEdgePaddingPercent = 1f;
    private const float ProgressIconHeightPercent = 10f;

    [Header("Progress Indicator")]
    [SerializeField] private Transform playerProgressTarget;
    [SerializeField] private Transform kingProgressTarget;
    [SerializeField] private float progressStartY;
    [SerializeField] private float progressGoalY = 41f;

    private VisualElement m_Healthbar;
    private VisualElement m_PlayerProgressIcon;
    private VisualElement m_KingProgressIcon;
    private VisualElement m_KingAboveIndicator;
    private VisualElement m_KingBelowIndicator;
    private VisualElement m_ProjectileWarningLeft;
    private VisualElement m_ProjectileWarningRight;
    private VisualElement m_ShopUI;
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
        m_SelectedItemPicture = root.Q<VisualElement>("ItemPicture");
        m_PauseOverlay = root.Q<VisualElement>("PauseOverlay");
        m_ResumeButton = root.Q<Button>("ResumeButton");
        m_RestartButton = root.Q<Button>("RestartButton");
        m_TitleButton = root.Q<Button>("TitleButton");

        RegisterPauseCallbacks();

        SetHealthValue(1.0f);
        SetPlayerProgressValue(0.0f);
        SetKingProgressValue(0.0f);
        HideKingIndicator();
        HideProjectileWarning();
        HideShop();
        RefreshPlayerUI();
        SetPaused(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetPaused(!m_IsPaused);
        }
    }

    private void LateUpdate()
    {
        UpdateProgressIcons();
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
