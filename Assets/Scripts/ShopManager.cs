using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPrefab;
    [SerializeField] private ItemCatalog itemCatalog;
    [Tooltip("店舗を生成し始める、このオブジェクトの初期位置からの距離")]
    [SerializeField, Min(0f)] private float firstStoreOffset = 3f;
    [Tooltip("店舗を並べるY方向の間隔")]
    [SerializeField, Min(0.01f)] private float storeInterval = 10f;
    [Tooltip("このY座標まで店舗を生成する")]
    [SerializeField] private float lastStoreY = 70f;
    [SerializeField] private float leftStoreX = -2f;
    [SerializeField] private float rightStoreX = 4f;

    private float firstStoreY;

    public int StoreRowCount { get; private set; }
    public ItemCatalog ItemCatalog => itemCatalog;

    public void GenerateStores()
    {
        if (StoreRowCount > 0)
        {
            return;
        }

        firstStoreY = transform.position.y + firstStoreOffset;
        int requiredShopCount = CountRequiredShops();

        if (!ValidateShopSetup(requiredShopCount))
        {
            return;
        }

        int itemIndex = 0;

        for (float storeY = firstStoreY; storeY <= lastStoreY; storeY += storeInterval)
        {
            SpawnShop(new Vector2(leftStoreX - 2f, storeY), itemCatalog.Items[itemIndex++]);
            SpawnShop(new Vector2(rightStoreX + 2f, storeY), itemCatalog.Items[itemIndex++]);
            StoreRowCount++;
        }
    }

    public Vector2 GetStoreDestination(int storeRow, bool useLeftSide)
    {
        if (storeRow < 1 || storeRow > StoreRowCount)
        {
            Debug.LogError($"Store row {storeRow} is outside the generated range.", this);
            return transform.position;
        }

        float storeX = useLeftSide ? leftStoreX : rightStoreX;
        float storeY = firstStoreY + storeInterval * (storeRow - 1);
        return new Vector2(storeX, storeY);
    }

    private void OnValidate()
    {
        storeInterval = Mathf.Max(0.01f, storeInterval);
    }

    private int CountRequiredShops()
    {
        int rowCount = 0;
        for (float storeY = firstStoreY; storeY <= lastStoreY; storeY += storeInterval)
        {
            rowCount++;
        }

        return rowCount * 2;
    }

    private bool ValidateShopSetup(int requiredShopCount)
    {
        if (shopPrefab == null)
        {
            Debug.LogError("ShopManager requires a shop prefab.", this);
            return false;
        }

        if (shopPrefab.GetComponent<Shop>() == null)
        {
            Debug.LogError("The shop prefab requires a Shop component.", shopPrefab);
            return false;
        }

        if (itemCatalog == null)
        {
            Debug.LogError("ShopManager requires an item catalog.", this);
            return false;
        }

        if (!itemCatalog.Validate(out string validationError))
        {
            Debug.LogError($"Item catalog is invalid:\n{validationError}", itemCatalog);
            return false;
        }

        if (itemCatalog.Count < requiredShopCount)
        {
            Debug.LogError(
                $"Item catalog needs at least {requiredShopCount} unique items, but contains {itemCatalog.Count}.",
                itemCatalog);
            return false;
        }

        return true;
    }

    private void SpawnShop(Vector2 position, ItemDefinition item)
    {
        GameObject shopObject = Instantiate(shopPrefab, position, Quaternion.identity);
        Shop shop = shopObject.GetComponent<Shop>();
        shop.Initialize(item);
        shopObject.name = $"shop_{item.Id}";
    }
}
