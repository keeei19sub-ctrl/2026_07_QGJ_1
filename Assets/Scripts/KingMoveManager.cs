using System.Collections.Generic;
using UnityEngine;

public class KingMoveManager : MonoBehaviour
{
    private static readonly int[] StoreOffsets = { -1, 1, 2 };

    [SerializeField] private KingController kingController;
    [Header("Store generation")]
    [SerializeField] private GameObject shopPrefab;
    [Tooltip("店舗を生成し始める、王様の初期位置からの距離")]
    [SerializeField, Min(0f)] private float firstStoreOffset = 3f;
    [Tooltip("隣り合う店舗の最小間隔")]
    [SerializeField, Min(0.01f)] private float minStoreInterval = 3f;
    [Tooltip("隣り合う店舗の最大間隔")]
    [SerializeField, Min(0.01f)] private float maxStoreInterval = 5f;
    [Tooltip("このY座標まで店舗を生成する")]
    [SerializeField] private float lastStoreY = 35f;
    [SerializeField] private float leftStoreX = -2f;
    [SerializeField] private float rightStoreX = 4f;
    [Tooltip("スタート地点は0。王様が現在いる店舗番号")]
    [SerializeField, Min(0)] private int currentStoreNumber;

    private readonly int[] candidates = new int[StoreOffsets.Length];
    private readonly List<Vector2> storeDestinations = new List<Vector2>();
    private int nextStoreNumber;

    public int CurrentStoreNumber => currentStoreNumber;

    private void Awake()
    {
        if (kingController == null)
        {
            kingController = GetComponent<KingController>();
        }

        if (kingController != null)
        {
            GenerateStores();
        }
    }

    private void OnEnable()
    {
        if (kingController == null)
        {
            Debug.LogError("KingMoveManager requires a KingController reference.", this);
            return;
        }

        kingController.DestinationRequested += SetRandomNextDestination;
        kingController.DestinationReached += UpdateCurrentStore;
    }

    private void OnDisable()
    {
        if (kingController != null)
        {
            kingController.DestinationRequested -= SetRandomNextDestination;
            kingController.DestinationReached -= UpdateCurrentStore;
        }
    }

    private void SetRandomNextDestination()
    {
        int candidateCount = 0;

        foreach (int offset in StoreOffsets)
        {
            int storeNumber = currentStoreNumber + offset;

            if (storeNumber < 1)
            {
                continue;
            }

            if (storeNumber > storeDestinations.Count)
            {
                continue;
            }

            candidates[candidateCount] = storeNumber;
            candidateCount++;
        }

        if (candidateCount == 0)
        {
            Debug.LogWarning(
                $"No valid store can be selected from store {currentStoreNumber}.",
                this);
            return;
        }

        nextStoreNumber = candidates[Random.Range(0, candidateCount)];
        kingController.SetNextDestination(storeDestinations[nextStoreNumber - 1]);
    }

    private void UpdateCurrentStore()
    {
        currentStoreNumber = nextStoreNumber;
    }

    private void GenerateStores()
    {
        storeDestinations.Clear();

        if (shopPrefab == null)
        {
            Debug.LogError("KingMoveManager requires a shop prefab.", this);
            return;
        }

        float minimumInterval = Mathf.Min(minStoreInterval, maxStoreInterval);
        float maximumInterval = Mathf.Max(minStoreInterval, maxStoreInterval);
        float storeY = kingController.transform.position.y + firstStoreOffset;

        while (storeY <= lastStoreY)
        {
            float storeX = Random.value < 0.5f ? leftStoreX : rightStoreX;
            Vector2 position = new Vector2(storeX, storeY);

            Instantiate(shopPrefab, position, Quaternion.identity);
            storeDestinations.Add(position);

            storeY += Random.Range(minimumInterval, maximumInterval);
        }
    }

    private void OnValidate()
    {
        minStoreInterval = Mathf.Max(0.01f, minStoreInterval);
        maxStoreInterval = Mathf.Max(minStoreInterval, maxStoreInterval);
    }
}
