using UnityEngine;

public class KingMoveManager : MonoBehaviour
{
    private static readonly int[] StoreOffsets = { -1, 1, 2 };

    [SerializeField] private KingController kingController;
    [SerializeField] private ShopManager shopManager;
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

        if (shopManager == null)
        {
            shopManager = GetComponent<ShopManager>();
        }

        if (shopManager != null)
        {
            shopManager.GenerateStores();
        }
    }

    private void OnEnable()
    {
        if (kingController == null || shopManager == null)
        {
            Debug.LogError("KingMoveManager requires KingController and ShopManager references.", this);
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

            if (storeNumber > shopManager.StoreRowCount)
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
        bool useLeftSide = Random.value < 0.5f;
        kingController.SetNextDestination(
            shopManager.GetStoreDestination(nextStoreNumber, useLeftSide));
    }

    private void UpdateCurrentStore()
    {
        currentStoreNumber = nextStoreNumber;
    }

}
