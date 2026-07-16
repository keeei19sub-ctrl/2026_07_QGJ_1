using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(KingSun))]
public sealed class KingCoinRewardEmitter : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private CoinPickup coinPrefab;
    [SerializeField, Min(0.01f)] private float spawnInterval = 2f;
    [SerializeField, Min(0)] private int rewardAmount = 10;
    [SerializeField, Min(1)] private int maxConcurrentCoins = 5;

    [Header("Throw Animation")]
    [Tooltip("Road center in world-space X. Coins are always thrown toward this position.")]
    [SerializeField] private float roadCenterX = 0.5f;
    [SerializeField] private Vector2 horizontalDistanceRange = new(1f, 1.8f);
    [SerializeField, Min(0f)] private float downwardDistance = 0.5f;
    [SerializeField, Min(0f)] private float flightDuration = 0.6f;
    [SerializeField, Min(0f)] private float arcHeight = 1.2f;

    private KingSun kingSun;
    private readonly HashSet<CoinPickup> activeCoins = new();
    private float protectionTimer;

    private void Awake()
    {
        kingSun = GetComponent<KingSun>();
    }

    private void Update()
    {
        activeCoins.RemoveWhere(coin => coin == null);

        if (kingSun == null || !kingSun.IsProtectedFromSun)
        {
            return;
        }

        if (activeCoins.Count >= maxConcurrentCoins || coinPrefab == null)
        {
            return;
        }

        protectionTimer += Time.deltaTime;
        if (protectionTimer < spawnInterval)
        {
            return;
        }

        protectionTimer = 0f;
        SpawnCoin();
    }

    private void SpawnCoin()
    {
        Vector2 spawnPosition = transform.position;
        float minDistance = Mathf.Min(horizontalDistanceRange.x, horizontalDistanceRange.y);
        float maxDistance = Mathf.Max(horizontalDistanceRange.x, horizontalDistanceRange.y);
        float direction = Mathf.Sign(roadCenterX - spawnPosition.x);
        if (Mathf.Approximately(direction, 0f))
        {
            direction = Random.value < 0.5f ? -1f : 1f;
        }

        Vector2 landingPosition = spawnPosition + new Vector2(
            direction * Random.Range(minDistance, maxDistance),
            -downwardDistance);

        CoinPickup coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
        activeCoins.Add(coin);
        coin.Removed += OnCoinRemoved;
        coin.Initialize(rewardAmount, landingPosition, flightDuration, arcHeight);
    }

    private void OnCoinRemoved(CoinPickup coin)
    {
        coin.Removed -= OnCoinRemoved;
        activeCoins.Remove(coin);
    }

    private void OnDestroy()
    {
        foreach (CoinPickup coin in activeCoins)
        {
            if (coin != null)
            {
                coin.Removed -= OnCoinRemoved;
            }
        }

        activeCoins.Clear();
    }

    private void OnValidate()
    {
        spawnInterval = Mathf.Max(0.01f, spawnInterval);
        rewardAmount = Mathf.Max(0, rewardAmount);
        maxConcurrentCoins = Mathf.Max(1, maxConcurrentCoins);
        horizontalDistanceRange.x = Mathf.Max(0f, horizontalDistanceRange.x);
        horizontalDistanceRange.y = Mathf.Max(0f, horizontalDistanceRange.y);
        downwardDistance = Mathf.Max(0f, downwardDistance);
        flightDuration = Mathf.Max(0f, flightDuration);
        arcHeight = Mathf.Max(0f, arcHeight);
    }
}
