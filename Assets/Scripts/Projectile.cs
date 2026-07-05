using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] float speed = 5f;
    int direction;
    void Start()
    {
        direction = transform.position.x > 0 ? -1 : 1;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
        if(transform.position.magnitude > 100.0f){
            ProjectileManager.destroyed();
            Debug.Log("far");
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        KingHealth kinghealth = collision.GetComponent<KingHealth>();
        if(kinghealth != null){
            kinghealth.Damage(damage);
            Debug.Log("damage?");
        }
        ProjectileManager.destroyed();
        Debug.Log("trigger");
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision){
        ProjectileManager.destroyed();
        Debug.Log("coll");
        Destroy(gameObject);
    }
}