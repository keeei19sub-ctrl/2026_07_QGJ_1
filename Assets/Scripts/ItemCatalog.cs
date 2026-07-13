using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ItemEffectType
{
    None,
    HealKing,
    ExpandUmbrella,
    StopProjectiles
}

[Serializable]
public sealed class ItemDefinition
{
    [SerializeField] private string id = string.Empty;
    [SerializeField] private string displayName = string.Empty;
    [SerializeField, Min(0)] private int price;
    [SerializeField, TextArea] private string description = string.Empty;
    [SerializeField] private ItemEffectType effectType;
    [SerializeField, Min(0f)] private float effectAmount;
    [SerializeField, Min(0f)] private float effectDuration;

    public string Id => id;
    public string DisplayName => displayName;
    public int Price => price;
    public string Description => description;
    public ItemEffectType EffectType => effectType;
    public float EffectAmount => effectAmount;
    public float EffectDuration => effectDuration;

    public ItemDefinition()
    {
    }

    public ItemDefinition(
        string id,
        string displayName,
        int price,
        string description = "",
        ItemEffectType effectType = ItemEffectType.None,
        float effectAmount = 0f,
        float effectDuration = 0f)
    {
        this.id = id;
        this.displayName = displayName;
        this.price = price;
        this.description = description;
        this.effectType = effectType;
        this.effectAmount = effectAmount;
        this.effectDuration = effectDuration;
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

    public bool HasEffectType(ItemEffectType effectType)
    {
        if (items == null)
        {
            return false;
        }

        for (int index = 0; index < items.Count; index++)
        {
            ItemDefinition item = items[index];
            if (item != null && item.EffectType == effectType)
            {
                return true;
            }
        }

        return false;
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

                if (!Enum.IsDefined(typeof(ItemEffectType), item.EffectType)
                    || item.EffectType == ItemEffectType.None)
                {
                    AppendError(errors, $"Item '{item.Id}' has no valid effect type.");
                    continue;
                }

                if (item.EffectAmount < 0f)
                {
                    AppendError(errors, $"Item '{item.Id}' has a negative effect amount.");
                }

                switch (item.EffectType)
                {
                    case ItemEffectType.HealKing:
                        if (item.EffectAmount <= 0f || item.EffectAmount > 1f)
                        {
                            AppendError(
                                errors,
                                $"Healing item '{item.Id}' needs an effect amount between 0 and 1.");
                        }
                        break;

                    case ItemEffectType.ExpandUmbrella:
                        if (item.EffectAmount <= 1f)
                        {
                            AppendError(
                                errors,
                                $"Umbrella item '{item.Id}' needs an effect amount greater than 1.");
                        }

                        if (item.EffectDuration <= 0f)
                        {
                            AppendError(
                                errors,
                                $"Umbrella item '{item.Id}' needs a positive effect duration.");
                        }
                        break;

                    case ItemEffectType.StopProjectiles:
                        if (item.EffectDuration <= 0f)
                        {
                            AppendError(
                                errors,
                                $"Projectile stop item '{item.Id}' needs a positive effect duration.");
                        }
                        break;
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
