using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    static int cnt = 0;
    int max = 10;
    [SerializeField] GameObject obj;
    [SerializeField] GameObject king;
    float radius = 1f;
    float spawnInterval = 1f;
    float spawnIntervalTimer;

    void Start()
    {
        spawnIntervalTimer = spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if(cnt< max)CreateTimer();
    }
    void create()
    {
        Instantiate(obj, choiceCordinate(), Quaternion.identity);
        cnt++;
    }
    void CreateTimer()
    {
        spawnIntervalTimer -= Time.deltaTime;
        if(spawnIntervalTimer < 0)
        {
            spawnIntervalTimer = spawnInterval;
            create();
        }
    }
    Vector2 choiceCordinate()
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        Vector2 spawnPos = (Vector2)king.transform.position + randomPoint;
        return spawnPos;
    }
    public static void destroyed()
    {
        cnt--;
    }
}
