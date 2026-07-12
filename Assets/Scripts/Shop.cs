using UnityEngine;

public enum PurchaseResult
{
    Success,
    InsufficientFunds,
    InvalidProduct,
    MissingPlayerState
}

[DisallowMultipleComponent]
public sealed class Shop : MonoBehaviour
{
    [SerializeField] private ItemDefinition item;

    public ItemDefinition Item => item;

    public void Initialize(ItemDefinition product)
    {
        item = product;
    }

    public PurchaseResult TryPurchase(PlayerWallet wallet, PlayerInventory inventory)
    {
        if (wallet == null || inventory == null)
        {
            return PurchaseResult.MissingPlayerState;
        }

        if (item == null
            || string.IsNullOrWhiteSpace(item.Id)
            || item.Price < 0
            || inventory.Catalog == null
            || inventory.Catalog.IndexOf(item.Id) < 0)
        {
            return PurchaseResult.InvalidProduct;
        }

        if (!wallet.TrySpend(item.Price))
        {
            return PurchaseResult.InsufficientFunds;
        }

        if (!inventory.AddItem(item))
        {
            wallet.AddMoney(item.Price);
            return PurchaseResult.InvalidProduct;
        }

        return PurchaseResult.Success;
    }
}
