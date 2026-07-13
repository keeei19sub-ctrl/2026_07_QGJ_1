using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerWallet : MonoBehaviour
{
    [SerializeField, Min(0)] private int initialMoney = 100;

    private int money;

    public int Money => money;

    public event Action<int> MoneyChanged;

    private void Awake()
    {
        money = Mathf.Max(0, initialMoney);
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && money >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount))
        {
            return false;
        }

        if (amount == 0)
        {
            return true;
        }

        money -= amount;
        MoneyChanged?.Invoke(money);
        return true;
    }

    public bool AddMoney(int amount)
    {
        if (amount < 0 || money > int.MaxValue - amount)
        {
            return false;
        }

        if (amount == 0)
        {
            return true;
        }

        money += amount;
        MoneyChanged?.Invoke(money);
        return true;
    }

    private void OnValidate()
    {
        initialMoney = Mathf.Max(0, initialMoney);
    }
}
