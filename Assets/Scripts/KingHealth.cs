using UnityEngine;

public class KingHealth : MonoBehaviour
{
    int hp;
    int maxHp = 10;
    void Start()
    {
        hp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        if(hp < 0)
        {
            angry();
        }
    }
    public void Damage(int amount)
    {
        hp = Mathf.Clamp(hp-amount, 0, maxHp);
        Debug.Log("damaged");
    }
    void angry()
    {
        Debug.Log("angry");
    }
}
