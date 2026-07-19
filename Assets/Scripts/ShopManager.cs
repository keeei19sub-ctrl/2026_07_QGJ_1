using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class ShopProductAssignment
{
    [SerializeField, Min(0)] private int shopIndex;
    [SerializeField] private string itemId = string.Empty;

    public int ShopIndex => shopIndex;
    public string ItemId => itemId;
}

public class ShopManager : MonoBehaviour
{
    private const int ExpectedShopCount = 14;
    private const int ExpectedSalesShopCount = 6;

    private static readonly ItemEffectType[] RequiredEffectTypes =
    {
        ItemEffectType.HealKing,
        ItemEffectType.ExpandUmbrella,
        ItemEffectType.StopProjectiles
    };

    [SerializeField] private GameObject shopPrefab;
    [SerializeField] private ItemCatalog itemCatalog;
    [Tooltip("店舗を生成し始める、このオブジェクトの初期位置からの距離")]
    [SerializeField, Min(0f)] private float firstStoreOffset = 3f;
    [Tooltip("店舗を並べるY方向の間隔")]
    [SerializeField, Min(0.01f)] private float storeInterval = 10f;
    [Tooltip("このY座標まで店舗を生成する")]
    [SerializeField] private float lastStoreY = 70f;
    [SerializeField] private float leftStoreX = -3f;
    [SerializeField] private float rightStoreX = 8f;
    [Tooltip("0始まりの店舗番号と商品ID。未登録の店舗は空店舗になる")]
    [SerializeField] private List<ShopProductAssignment> productAssignments = new();

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

        Dictionary<int, ItemDefinition> productsByShop = BuildProductLookup();
        int shopIndex = 0;

        for (float storeY = firstStoreY; storeY <= lastStoreY; storeY += storeInterval)
        {
            productsByShop.TryGetValue(shopIndex, out ItemDefinition leftProduct);
            SpawnShop(new Vector2(leftStoreX - 2f, storeY), leftProduct, shopIndex);
            shopIndex++;

            productsByShop.TryGetValue(shopIndex, out ItemDefinition rightProduct);
            SpawnShop(new Vector2(rightStoreX + 2f, storeY), rightProduct, shopIndex);
            shopIndex++;

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
        StringBuilder errors = new();

        if (shopPrefab == null)
        {
            AppendError(errors, "ShopManager requires a shop prefab.");
        }
        else if (shopPrefab.GetComponent<Shop>() == null)
        {
            AppendError(errors, "The shop prefab requires a Shop component.");
        }

        if (requiredShopCount != ExpectedShopCount)
        {
            AppendError(
                errors,
                $"ShopManager must generate {ExpectedShopCount} shops, but its position settings generate {requiredShopCount}.");
        }

        if (itemCatalog == null)
        {
            AppendError(errors, "ShopManager requires an item catalog.");
        }
        else
        {
            if (!itemCatalog.Validate(out string validationError))
            {
                AppendError(errors, $"Item catalog is invalid:\n{validationError}");
            }

            foreach (ItemEffectType effectType in RequiredEffectTypes)
            {
                if (!itemCatalog.HasEffectType(effectType))
                {
                    AppendError(errors, $"Item catalog is missing required effect type '{effectType}'.");
                }
            }

            ValidateProductAssignments(requiredShopCount, errors);
        }

        if (errors.Length == 0)
        {
            return true;
        }

        Debug.LogError($"Shop setup is invalid:\n{errors}", this);
        return false;
    }

    private void ValidateProductAssignments(int shopCount, StringBuilder errors)
    {
        if (productAssignments == null)
        {
            AppendError(errors, "Shop product assignments are missing.");
            return;
        }

        HashSet<int> assignedShopIndices = new();
        Dictionary<ItemEffectType, int> assignmentCounts = new();
        int validAssignmentCount = 0;

        for (int index = 0; index < productAssignments.Count; index++)
        {
            ShopProductAssignment assignment = productAssignments[index];
            if (assignment == null)
            {
                AppendError(errors, $"Product assignment at index {index} is missing.");
                continue;
            }

            if (assignment.ShopIndex < 0 || assignment.ShopIndex >= shopCount)
            {
                AppendError(
                    errors,
                    $"Product assignment {index} uses shop index {assignment.ShopIndex}, outside 0-{shopCount - 1}.");
                continue;
            }

            if (!assignedShopIndices.Add(assignment.ShopIndex))
            {
                AppendError(errors, $"Shop index {assignment.ShopIndex} has more than one product.");
                continue;
            }

            if (!itemCatalog.TryGetById(assignment.ItemId, out ItemDefinition item))
            {
                AppendError(errors, $"Product assignment {index} has unknown item ID '{assignment.ItemId}'.");
                continue;
            }

            assignmentCounts.TryGetValue(item.EffectType, out int count);
            assignmentCounts[item.EffectType] = count + 1;
            validAssignmentCount++;
        }

        if (validAssignmentCount != ExpectedSalesShopCount)
        {
            AppendError(
                errors,
                $"Exactly {ExpectedSalesShopCount} shops need products, but {validAssignmentCount} valid assignments were found.");
        }

        foreach (ItemEffectType effectType in RequiredEffectTypes)
        {
            assignmentCounts.TryGetValue(effectType, out int count);
            if (count != 2)
            {
                AppendError(errors, $"Effect type '{effectType}' must be sold by exactly 2 shops, but is assigned to {count}.");
            }
        }
    }

    private Dictionary<int, ItemDefinition> BuildProductLookup()
    {
        Dictionary<int, ItemDefinition> productsByShop = new();
        foreach (ShopProductAssignment assignment in productAssignments)
        {
            if (assignment != null
                && itemCatalog.TryGetById(assignment.ItemId, out ItemDefinition item))
            {
                productsByShop[assignment.ShopIndex] = item;
            }
        }

        return productsByShop;
    }

    private void SpawnShop(Vector2 position, ItemDefinition item, int shopIndex)
    {
        GameObject shopObject = Instantiate(shopPrefab, position, Quaternion.identity);
        if(position.x < 0f)
        {
            Vector3 scale = shopObject.transform.localScale;
            scale.x *= -1;
            shopObject.transform.localScale = scale;
        }
        Shop shop = shopObject.GetComponent<Shop>();
        shop.Initialize(item);

        string productName = item != null ? item.Id : "empty";
        shopObject.name = $"shop_{shopIndex + 1:00}_{productName}";
    }

    private static void AppendError(StringBuilder errors, string error)
    {
        if (errors.Length > 0)
        {
            errors.AppendLine();
        }

        errors.Append(error);
    }
}
