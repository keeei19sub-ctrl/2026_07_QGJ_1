using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemCatalog itemCatalog;

    private int[] itemCounts = Array.Empty<int>();
    private int selectedIndex = -1;

    public ItemCatalog Catalog => itemCatalog;

    public ItemDefinition SelectedItem
    {
        get
        {
            EnsureStorage();
            return IsOwnedIndex(selectedIndex) ? itemCatalog.Items[selectedIndex] : null;
        }
    }

    public int SelectedCount
    {
        get
        {
            EnsureStorage();
            return IsOwnedIndex(selectedIndex) ? itemCounts[selectedIndex] : 0;
        }
    }

    public event Action InventoryChanged;
    public event Action SelectionChanged;

    public bool CanAddItem(ItemDefinition item, int amount = 1)
    {
        return item != null && CanAddItem(item.Id, amount);
    }

    public bool CanAddItem(string itemId, int amount = 1)
    {
        EnsureStorage();
        int index = itemCatalog == null ? -1 : itemCatalog.IndexOf(itemId);
        return index >= 0 && amount == 1 && !HasAnyItem();
    }

    private void Awake()
    {
        EnsureStorage();
    }

    public void Initialize(ItemCatalog catalog)
    {
        if (ReferenceEquals(itemCatalog, catalog) && itemCounts.Length == (catalog == null ? 0 : catalog.Count))
        {
            return;
        }

        bool selectionChanged = selectedIndex != -1;
        itemCatalog = catalog;
        itemCounts = catalog == null ? Array.Empty<int>() : new int[catalog.Count];
        selectedIndex = -1;

        InventoryChanged?.Invoke();
        if (selectionChanged)
        {
            SelectionChanged?.Invoke();
        }
    }

    public bool AddItem(ItemDefinition item, int amount = 1)
    {
        if (item == null)
        {
            return false;
        }

        return AddItem(item.Id, amount);
    }

    public bool AddItem(string itemId, int amount = 1)
    {
        if (!CanAddItem(itemId, amount))
        {
            return false;
        }

        int index = itemCatalog.IndexOf(itemId);
        int previousSelection = selectedIndex;
        itemCounts[index] = 1;
        if (!IsOwnedIndex(selectedIndex))
        {
            selectedIndex = FindOwnedIndex(-1, 1);
        }

        InventoryChanged?.Invoke();
        if (selectedIndex != previousSelection)
        {
            SelectionChanged?.Invoke();
        }

        return true;
    }

    private bool HasAnyItem()
    {
        for (int index = 0; index < itemCounts.Length; index++)
        {
            if (itemCounts[index] > 0)
            {
                return true;
            }
        }

        return false;
    }

    public int GetCount(ItemDefinition item)
    {
        return item == null ? 0 : GetCount(item.Id);
    }

    public int GetCount(string itemId)
    {
        EnsureStorage();
        int index = itemCatalog == null ? -1 : itemCatalog.IndexOf(itemId);
        return index >= 0 ? itemCounts[index] : 0;
    }

    public bool SelectPrevious()
    {
        return SelectInDirection(-1);
    }

    public bool SelectNext()
    {
        return SelectInDirection(1);
    }

    public bool TryConsumeSelected()
    {
        EnsureStorage();
        if (!IsOwnedIndex(selectedIndex))
        {
            return false;
        }

        int previousSelection = selectedIndex;
        itemCounts[selectedIndex]--;
        if (itemCounts[selectedIndex] == 0)
        {
            selectedIndex = FindOwnedIndex(selectedIndex, 1);
        }

        InventoryChanged?.Invoke();
        if (selectedIndex != previousSelection)
        {
            SelectionChanged?.Invoke();
        }

        return true;
    }

    private bool SelectInDirection(int direction)
    {
        EnsureStorage();
        int nextIndex = FindOwnedIndex(selectedIndex, direction);
        if (nextIndex < 0)
        {
            return false;
        }

        if (nextIndex != selectedIndex)
        {
            selectedIndex = nextIndex;
            SelectionChanged?.Invoke();
        }

        return true;
    }

    private int FindOwnedIndex(int startIndex, int direction)
    {
        if (itemCounts.Length == 0)
        {
            return -1;
        }

        int origin = startIndex;
        if (origin < 0 || origin >= itemCounts.Length)
        {
            origin = direction > 0 ? -1 : 0;
        }

        for (int offset = 1; offset <= itemCounts.Length; offset++)
        {
            int index = PositiveModulo(origin + direction * offset, itemCounts.Length);
            if (itemCounts[index] > 0)
            {
                return index;
            }
        }

        return -1;
    }

    private bool IsOwnedIndex(int index)
    {
        return itemCatalog != null
            && index >= 0
            && index < itemCounts.Length
            && itemCounts[index] > 0;
    }

    private void EnsureStorage()
    {
        int requiredLength = itemCatalog == null ? 0 : itemCatalog.Count;
        if (itemCounts.Length == requiredLength)
        {
            return;
        }

        int[] resizedCounts = new int[requiredLength];
        Array.Copy(itemCounts, resizedCounts, Math.Min(itemCounts.Length, resizedCounts.Length));
        itemCounts = resizedCounts;

        if (!IsOwnedIndex(selectedIndex))
        {
            selectedIndex = FindOwnedIndex(-1, 1);
        }
    }

    private static int PositiveModulo(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}
