using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float timerFly = .1f;
    [SerializeField] int damage = 10;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] float impactRadius = 0.3f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.magnitude > 100.0f){
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        KingHealth kinghealth = collision.GetComponent<KingHealth>();
        if(kinghealth != null){
            kinghealth.Damage(damage);
        }
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision){
        Destroy(gameObject);
    }
}