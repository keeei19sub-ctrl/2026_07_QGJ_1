using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class ItemDefinition
{
    [SerializeField] private string id = string.Empty;
    [SerializeField] private string displayName = string.Empty;
    [SerializeField, Min(0)] private int price;
    [SerializeField, TextArea] private string description = string.Empty;

    public string Id => id;
    public string DisplayName => displayName;
    public int Price => price;
    public string Description => description;

    public ItemDefinition()
    {
    }

    public ItemDefinition(string id, string displayName, int price, string description = "")
    {
        this.id = id;
        this.displayName = displayName;
        this.price = price;
        this.description = description;
    }
}

[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Game/Items/Item Catalog")]
public sealed class ItemCatalog : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

    public IReadOnlyList<ItemDefinition> Items => items;
    public int Count => items?.Count ?? 0;

    public bool TryGetById(string itemId, out ItemDefinition item)
    {
        int index = IndexOf(itemId);
        if (index >= 0)
        {
            item = items[index];
            return true;
        }

        item = null;
        return false;
    }

    public int IndexOf(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || items == null)
        {
            return -1;
        }

        for (int index = 0; index < items.Count; index++)
        {
            ItemDefinition item = items[index];
            if (item != null && string.Equals(item.Id, itemId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    public int IndexOf(ItemDefinition item)
    {
        return item == null ? -1 : IndexOf(item.Id);
    }

    public bool Validate(out string validationError)
    {
        StringBuilder errors = new StringBuilder();
        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);

        if (items != null)
        {
            for (int index = 0; index < items.Count; index++)
            {
                ItemDefinition item = items[index];
                if (item == null)
                {
                    AppendError(errors, $"Item at index {index} is missing.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    AppendError(errors, $"Item at index {index} has an empty ID.");
                }
                else if (!ids.Add(item.Id))
                {
                    AppendError(errors, $"Item ID '{item.Id}' is duplicated.");
                }

                if (item.Price < 0)
                {
                    AppendError(errors, $"Item '{item.Id}' has a negative price.");
                }
            }
        }

        validationError = errors.ToString();
        return errors.Length == 0;
    }

    private static void AppendError(StringBuilder errors, string error)
    {
        if (errors.Length > 0)
        {
            errors.AppendLine();
        }

        errors.Append(error);
    }

    private void OnValidate()
    {
        if (!Validate(out string validationError))
        {
            Debug.LogError($"Item catalog validation failed:\n{validationError}", this);
        }
    }
}
