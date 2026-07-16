using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(KingSun))]
public sealed class KingCoinRewardEmitter : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private CoinPickup coinPrefab;
    [SerializeField, Min(0.01f)] private float protectionDuration = 5f;
    [SerializeField, Min(0)] private int rewardAmount = 10;

    [Header("Throw Animation")]
    [SerializeField] private Vector2 horizontalDistanceRange = new(1f, 1.8f);
    [SerializeField, Min(0f)] private float downwardDistance = 0.5f;
    [SerializeField, Min(0f)] private float flightDuration = 0.6f;
    [SerializeField, Min(0f)] private float arcHeight = 1.2f;

    private KingSun kingSun;
    private CoinPickup activeCoin;
    private float protectionTimer;

    private void Awake()
    {
        kingSun = GetComponent<KingSun>();
    }

    private void Update()
    {
        if (kingSun == null || !kingSun.IsProtectedFromSun)
        {
            protectionTimer = 0f;
            return;
        }

        if (activeCoin != null || coinPrefab == null)
        {
            return;
        }

        protectionTimer += Time.deltaTime;
        if (protectionTimer < protectionDuration)
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
        float direction = Random.value < 0.5f ? -1f : 1f;
        Vector2 landingPosition = spawnPosition + new Vector2(
            direction * Random.Range(minDistance, maxDistance),
            -downwardDistance);

        activeCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
        activeCoin.Removed += OnCoinRemoved;
        activeCoin.Initialize(rewardAmount, landingPosition, flightDuration, arcHeight);
    }

    private void OnCoinRemoved(CoinPickup coin)
    {
        coin.Removed -= OnCoinRemoved;
        if (activeCoin == coin)
        {
            activeCoin = null;
        }
    }

    private void OnDisable()
    {
        protectionTimer = 0f;
    }

    private void OnDestroy()
    {
        if (activeCoin != null)
        {
            activeCoin.Removed -= OnCoinRemoved;
        }
    }

    private void OnValidate()
    {
        protectionDuration = Mathf.Max(0.01f, protectionDuration);
        rewardAmount = Mathf.Max(0, rewardAmount);
        horizontalDistanceRange.x = Mathf.Max(0f, horizontalDistanceRange.x);
        horizontalDistanceRange.y = Mathf.Max(0f, horizontalDistanceRange.y);
        downwardDistance = Mathf.Max(0f, downwardDistance);
        flightDuration = Mathf.Max(0f, flightDuration);
        arcHeight = Mathf.Max(0f, arcHeight);
    }
}
