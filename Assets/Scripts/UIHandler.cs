using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    private VisualElement m_Healthbar;
    private VisualElement m_Progressbar;
    private VisualElement m_ShopUI;
    private Label m_MoneyLabel;
    private Label m_SelectedItemLabel;
    private Label m_SelectedItemCountLabel;
    private Label m_SelectedItemDescriptionLabel;
    private VisualElement m_SelectedItemPicture;
    private Label m_ShopItemNameLabel;
    private Label m_ShopPriceLabel;
    private Label m_ShopMessageLabel;

    private PlayerWallet m_Wallet;
    private PlayerInventory m_Inventory;
    private Shop m_VisibleShop;
    private bool m_PlayerEventsSubscribed;

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
        m_Progressbar = root.Q<VisualElement>("ProgressBar");

        m_ShopUI = root.Q<VisualElement>("ShopUI");
        m_MoneyLabel = root.Q<Label>("MoneyLabel");
        m_ShopMessageLabel = root.Q<Label>("ShopMessageLabel");
        m_ShopPriceLabel = root.Q<Label>("ShopItemPrice");
        m_ShopItemNameLabel = root.Q<Label>("ShopItemName");
        m_SelectedItemLabel = root.Q<Label>("ItemName");
        m_SelectedItemCountLabel = root.Q<Label>("ItemCount");
        m_SelectedItemDescriptionLabel = root.Q<Label>("ItemDescription");
        m_SelectedItemPicture = root.Q<VisualElement>("ItemPicture");

        SetHealthValue(1.0f);
        SetProgressValue(0.0f);
        HideShop();
        RefreshPlayerUI();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayerEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayerEvents();
        if (instance == this)
        {
            instance = null;
        }
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

    public void SetProgressValue(float percentage)
    {
        if (m_Progressbar != null)
        {
            m_Progressbar.style.height = Length.Percent(100 * Mathf.Clamp01(percentage));
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
}
