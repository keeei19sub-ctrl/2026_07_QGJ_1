using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float timerFly = 3f;
    [SerializeField] int damage = 10;
    void Start()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Isfly())
        {
            GetComponent<Collider2D>().enabled = true;
            ProjectileManager.destroyed();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("flying");
        }
    }
    bool Isfly()
    {
        timerFly -= Time.deltaTime;
        if(timerFly < 0)return false;
        else return true;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        KingHealth kinghealth = collision.GetComponent<KingHealth>();
        if(kinghealth != null){
            kinghealth.Damage(damage);
        }
    }
}
