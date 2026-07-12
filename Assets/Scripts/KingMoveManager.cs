using UnityEngine;

public class KingMoveManager : MonoBehaviour
{
    private static readonly int[] StoreOffsets = { -1, 1, 2 };

    [SerializeField] private KingController kingController;
    [Tooltip("1店舗目から順番に、王様が移動する座標のTransformを設定する")]
    [SerializeField] private Transform[] storeDestinations = new Transform[0];
    [Tooltip("スタート地点は0。王様が現在いる店舗番号")]
    [SerializeField, Min(0)] private int currentStoreNumber;

    private readonly int[] candidates = new int[StoreOffsets.Length];
    private int nextStoreNumber;

    public int CurrentStoreNumber => currentStoreNumber;

    private void Awake()
    {
        if (kingController == null)
        {
            kingController = GetComponent<KingController>();
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
            int destinationIndex = storeNumber - 1;

            if (destinationIndex < 0 || destinationIndex >= storeDestinations.Length)
            {
                continue;
            }

            if (storeDestinations[destinationIndex] == null)
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
        Transform destination = storeDestinations[nextStoreNumber - 1];
        kingController.SetNextDestination(destination.position);
    }

    private void UpdateCurrentStore()
    {
        currentStoreNumber = nextStoreNumber;
    }
}
