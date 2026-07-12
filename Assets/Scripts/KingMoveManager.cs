using UnityEngine;

public class KingMoveManager : MonoBehaviour
{
    private static readonly int[] StoreOffsets = { -1, 1, 2 };

    [SerializeField] private KingController kingController;
    [Tooltip("店舗番号が1増えるごとに加算する座標。Y成分を店舗間隔として使用する")]
    [SerializeField] private Vector2 storeInterval = new Vector2(0f, 3f);
    [SerializeField] private float leftStoreX = -2f;
    [SerializeField] private float rightStoreX = 4f;
    [Tooltip("スタート地点は0。王様が現在いる店舗番号")]
    [SerializeField, Min(0)] private int currentStoreNumber;

    private readonly int[] candidates = new int[StoreOffsets.Length];
    private Vector2 startPosition;
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
            startPosition = kingController.transform.position;
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
        float destinationX = Random.value < 0.5f ? leftStoreX : rightStoreX;
        float destinationY = startPosition.y + storeInterval.y * nextStoreNumber;
        Vector2 destination = new Vector2(destinationX, destinationY);
        kingController.SetNextDestination(destination);
    }

    private void UpdateCurrentStore()
    {
        currentStoreNumber = nextStoreNumber;
    }
}
