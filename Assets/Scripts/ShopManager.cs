using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPrefab;
    [Tooltip("店舗を生成し始める、このオブジェクトの初期位置からの距離")]
    [SerializeField, Min(0f)] private float firstStoreOffset = 3f;
    [Tooltip("店舗を並べるY方向の間隔")]
    [SerializeField, Min(0.01f)] private float storeInterval = 4f;
    [Tooltip("このY座標まで店舗を生成する")]
    [SerializeField] private float lastStoreY = 35f;
    [SerializeField] private float leftStoreX = -2f;
    [SerializeField] private float rightStoreX = 4f;

    private float firstStoreY;

    public int StoreRowCount { get; private set; }

    public void GenerateStores()
    {
        if (StoreRowCount > 0)
        {
            return;
        }

        if (shopPrefab == null)
        {
            Debug.LogError("ShopManager requires a shop prefab.", this);
            return;
        }

        firstStoreY = transform.position.y + firstStoreOffset;

        for (float storeY = firstStoreY; storeY <= lastStoreY; storeY += storeInterval)
        {
            Instantiate(shopPrefab, new Vector2(leftStoreX, storeY), Quaternion.identity);
            Instantiate(shopPrefab, new Vector2(rightStoreX, storeY), Quaternion.identity);
            StoreRowCount++;
        }
    }

    public Vector2 GetStoreDestination(int storeRow, bool useLeftSide)
    {
        if (storeRow < 1 || storeRow > StoreRowCount)
        {
            Debug.LogError($"Store row {storeRow} is outside the generated range.", this);
            return transform.position;
        }

        float storeX = useLeftSide ? leftStoreX : rightStoreX;
        float storeY = firstStoreY + storeInterval * (storeRow - 1);
        return new Vector2(storeX, storeY);
    }

    private void OnValidate()
    {
        storeInterval = Mathf.Max(0.01f, storeInterval);
    }
}
