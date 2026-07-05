using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    static int cnt = 0;
    int max = 10;
    [SerializeField] GameObject obj;
    [SerializeField] GameObject king;
    float spawnInterval = 1f;
    float spawnIntervalTimer;

    void Start()
    {
        spawnIntervalTimer = spawnInterval;
        king = GameObject.Find("King");
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
        int rnd = Random.Range(1, 3);
        int direction = rnd == 1 ? 1 : -1;
        Vector2 spawnPos = (Vector2)king.transform.position;
        spawnPos += Vector2.right * 20 * direction;
        return spawnPos;
    }
    public static void destroyed()
    {
        cnt--;
    }
}
