using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private KingHealth target;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField, Min(1)] private int maxConcurrentAttacks = 3;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;
    [SerializeField, Min(0f)] private float warningDuration = 0.8f;
    [SerializeField, Range(0f, 0.25f)] private float spawnViewportMargin = 0.05f;

    private readonly HashSet<Projectile> activeProjectiles = new();
    private float attackIntervalTimer;
    private Coroutine pendingAttack;

    private void Awake()
    {
        attackIntervalTimer = attackInterval;

        if (target == null)
        {
            target = FindAnyObjectByType<KingHealth>();
        }

        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }

    private void Update()
    {
        activeProjectiles.RemoveWhere(projectile => projectile == null);

        if (projectilePrefab == null || target == null || pendingAttack != null)
        {
            return;
        }

        if (activeProjectiles.Count >= maxConcurrentAttacks)
        {
            return;
        }

        attackIntervalTimer -= Time.deltaTime;
        if (attackIntervalTimer > 0f)
        {
            return;
        }

        attackIntervalTimer = attackInterval;
        ProjectileWarningSide side = Random.value < 0.5f
            ? ProjectileWarningSide.Left
            : ProjectileWarningSide.Right;
        pendingAttack = StartCoroutine(AttackSequence(side));
    }

    private IEnumerator AttackSequence(ProjectileWarningSide side)
    {
        UIHandler.instance?.ShowProjectileWarning(side);
        yield return new WaitForSeconds(warningDuration);
        UIHandler.instance?.HideProjectileWarning();

        if (projectilePrefab == null || target == null)
        {
            pendingAttack = null;
            yield break;
        }

        Vector2 targetPosition = target.transform.position;
        Vector2 spawnPosition = CalculateSpawnPosition(targetPosition, side);
        Projectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        activeProjectiles.Add(projectile);
        projectile.Destroyed += OnProjectileDestroyed;
        projectile.Initialize(targetPosition);
        pendingAttack = null;
    }

    private Vector2 CalculateSpawnPosition(Vector2 targetPosition, ProjectileWarningSide side)
    {
        if (gameplayCamera == null)
        {
            float direction = side == ProjectileWarningSide.Left ? -1f : 1f;
            return targetPosition + Vector2.right * 20f * direction;
        }

        Vector3 targetViewport = gameplayCamera.WorldToViewportPoint(targetPosition);
        float viewportX = side == ProjectileWarningSide.Left
            ? -spawnViewportMargin
            : 1f + spawnViewportMargin;
        float distanceFromCamera = Mathf.Abs(gameplayCamera.transform.position.z);
        Vector3 spawnPosition = gameplayCamera.ViewportToWorldPoint(
            new Vector3(viewportX, targetViewport.y, distanceFromCamera));
        spawnPosition.z = 0f;
        return spawnPosition;
    }

    private void OnProjectileDestroyed(Projectile projectile)
    {
        projectile.Destroyed -= OnProjectileDestroyed;
        activeProjectiles.Remove(projectile);
    }

    public void NotifyDestroyed(Projectile projectile)
    {
        OnProjectileDestroyed(projectile);
    }

    private void OnDisable()
    {
        if (pendingAttack != null)
        {
            StopCoroutine(pendingAttack);
            pendingAttack = null;
        }

        UIHandler.instance?.HideProjectileWarning();
    }
}
