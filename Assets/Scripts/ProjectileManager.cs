using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject king;
    [SerializeField, Min(1)] private int max = 10;
    [SerializeField, Min(0.01f)] private float spawnInterval = 1f;
    [SerializeField, Min(0f)] private float spawnDistance = 20f;

    private readonly HashSet<Projectile> activeProjectiles = new();
    private float spawnIntervalTimer;

    private void Awake()
    {
        spawnIntervalTimer = spawnInterval;

        if (king == null)
        {
            king = GameObject.Find("King");
        }
    }

    private void Update()
    {
        activeProjectiles.RemoveWhere(projectile => projectile == null);

        if (obj == null || king == null || activeProjectiles.Count >= max)
        {
            return;
        }

        spawnIntervalTimer -= Time.deltaTime;
        if (spawnIntervalTimer > 0f)
        {
            return;
        }

        spawnIntervalTimer = spawnInterval;
        SpawnProjectile();
    }

    private void SpawnProjectile()
    {
        Vector2 targetPosition = king.transform.position;
        float side = Random.value < 0.5f ? -1f : 1f;
        Vector2 spawnPosition = targetPosition + Vector2.right * spawnDistance * side;

        GameObject instance = Instantiate(obj, spawnPosition, Quaternion.identity);
        Projectile projectile = instance.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError("Projectile prefab does not contain a Projectile component.", instance);
            Destroy(instance);
            return;
        }

        activeProjectiles.Add(projectile);
        projectile.Initialize(this, targetPosition);
    }

    public void NotifyDestroyed(Projectile projectile)
    {
        activeProjectiles.Remove(projectile);
    }
}
