using UnityEngine;

public class KingHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHp = 30000;

    private int hp;
    private bool initialized;

    public int sunDamage = 1;
    public static bool shadow = false;

    public int health => hp;
    public int CurrentHealth => hp;
    public int MaxHealth => maxHp;

    [SerializeField] private AudioSource seHeal;
    private void Awake()
    {
        InitializeHealth();
    }

    private void Start()
    {
        RefreshHealthBar();
    }

    private void Update()
    {
        if (hp <= 0)
        {
            Angry();
        }
    }

    public void Damage(int amount)
    {
        InitializeHealth();
        hp = Mathf.Clamp(hp - amount, 0, maxHp);
        RefreshHealthBar();
    }

    public bool Heal(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        InitializeHealth();
        hp = Mathf.Clamp(hp + amount, 0, maxHp);
        seHeal.Play();
        RefreshHealthBar();
        return true;
    }

    public bool HealByMaxHealthFraction(float fraction)
    {
        if (fraction <= 0f)
        {
            return false;
        }

        int amount = Mathf.Max(1, Mathf.RoundToInt(maxHp * fraction));
        return Heal(amount);
    }

    private void InitializeHealth()
    {
        if (initialized)
        {
            return;
        }

        maxHp = Mathf.Max(1, maxHp);
        hp = maxHp;
        initialized = true;
    }

    private void RefreshHealthBar()
    {
        UIHandler.instance?.SetHealthValue(hp / (float)maxHp);
    }

    private void Angry()
    {
        Debug.Log("angry");
    }
}
