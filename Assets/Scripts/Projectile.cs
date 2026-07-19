using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public event Action<Projectile> Destroyed;

    [SerializeField] private int damage = 100;
    [SerializeField, Min(0f)] private float speed = 5f;
    [SerializeField, Min(0f)] private float maxTravelDistance = 100f;
    [SerializeField] private AudioSource seDeffence;

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector2 spawnPosition;
    private bool isDestroyed;
    private bool isInitialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = rb.position;
    }

    public void Initialize(Vector2 targetPosition)
    {
        spawnPosition = rb.position;
        direction = (targetPosition - spawnPosition).normalized;

        if (direction == Vector2.zero)
        {
            direction = Vector2.left;
        }

        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (Vector2.Distance(spawnPosition, rb.position) >= maxTravelDistance)
        {
            DestroyProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed)
        {
            return;
        }

        PlayerController playerController = collision.GetComponentInParent<PlayerController>();
        if (playerController != null && playerController.IsUmbrellaHitbox(collision))
        {
            Debug.Log("Defence");
            PlayDefenceSound();
            DestroyProjectile();
            return;
        }

        KingHealth kingHealth = collision.GetComponent<KingHealth>();
        if (kingHealth == null)
        {
            return;
        }

        Debug.Log("Damage");
        kingHealth.Damage(damage);
        DestroyProjectile();
    }

    private void PlayDefenceSound()
    {
        if (seDeffence == null || seDeffence.clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            seDeffence.clip,
            transform.position,
            seDeffence.volume);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyProjectile();
    }

    public void Despawn()
    {
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;
        Destroyed?.Invoke(this);
        Destroy(gameObject);
    }
}
