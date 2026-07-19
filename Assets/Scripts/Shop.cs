using UnityEngine;

public enum PurchaseResult
{
    Success,
    InsufficientFunds,
    InventoryFull,
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
            Debug.Log("null");
            return PurchaseResult.MissingPlayerState;
        }

        if (item == null
            || string.IsNullOrWhiteSpace(item.Id)
            || item.Price < 0
            || inventory.Catalog == null
            || inventory.Catalog.IndexOf(item.Id) < 0)
        {
            Debug.Log("invalidproduct");
            return PurchaseResult.InvalidProduct;
        }

        if (!inventory.CanAddItem(item))
        {
            Debug.Log("inventoryfull");
            return PurchaseResult.InventoryFull;
        }

        if (!wallet.TrySpend(item.Price))
        {
            Debug.Log("insufficient");
            return PurchaseResult.InsufficientFunds;
        }

        if (!inventory.AddItem(item))
        {
            Debug.Log("invalidProd");
            wallet.AddMoney(item.Price);
            return PurchaseResult.InvalidProduct;
        }
        Debug.Log("success");

        return PurchaseResult.Success;
    }
}
