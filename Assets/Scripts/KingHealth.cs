using UnityEngine;

public class KingHealth : MonoBehaviour
{
    int hp;
    int maxHp = 100;
    void Start()
    {
        hp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Damage(int amount)
    {
        hp = Mathf.Clamp(hp-amount, 0, maxHp);
    }
}
