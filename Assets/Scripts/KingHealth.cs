using UnityEngine;
using UnityEngine.UIElements;

public class KingHealth : MonoBehaviour
{
    int hp;
    int maxHp = 10000;
    public int sunDamage = 10;
    float sunTimer;
    float sunInterval = 10;
    public static bool shadow = false;
  	public int health { get { return hp; }}
    void Start()
    {
        hp = maxHp;
        sunTimer = sunInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if(hp <= 0)
        {
            angry();
        }
    }
    public void Damage(int amount)
    {
        hp = Mathf.Clamp(hp-amount, 0, maxHp);
     		UIHandler.instance.SetHealthValue(hp / (float)maxHp);
    }
    void angry()
    {
        Debug.Log("angry");
    }
}
