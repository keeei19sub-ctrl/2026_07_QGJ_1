using UnityEngine;

public class KingSun : MonoBehaviour
{
    [Header("判定対象")]
    [Tooltip("このタグを持つオブジェクトと接触している間は処理をスキップする")]
    [SerializeField] private string targetTag = "Shadow";
 
    [Header("動作設定")]
    [Tooltip("接触していない間に呼びたい処理の実行間隔(秒)。0なら毎フレーム")]
    [SerializeField] private float processInterval = 10f;
 
    // 現在、対象タグのコリジョンに接触中かどうか
    private bool isColliding = false;
 
    // 接触中のコライダーを個別に管理する場合のカウント
    // (複数の対象と同時接触してもExit判定がズレないようにするため)
    private int collidingCount = 0;
 
    private float timer = 0f;

    KingHealth kingHealth;
    void Start()
    {
        kingHealth = GetComponent<KingHealth>();  
    }
    private void Update()
    {
        // 当たっていない時だけ処理する
        if (!isColliding)
        {
            if (processInterval <= 0f)
            {
                DoProcessWhenNotColliding();
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= processInterval)
                {
                    timer = 0f;
                    DoProcessWhenNotColliding();
                }
            }
        }
        else
        {
            // 当たっている間はタイマーをリセット(任意)
            timer = 0f;
        }
    }
 
    /// <summary>
    /// 当たっていない時に呼ばれる処理。ここに実際のロジックを書く。
    /// </summary>
    private void DoProcessWhenNotColliding()
    {
        Debug.Log($"[{name}] 「{targetTag}」に当たっていないので処理を実行");
        kingHealth.Damage(10);
    }
 
    // ===== 物理コリジョン(isTrigger = false)を使う場合 =====
 
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollisionEnter(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        HandleCollisionExit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollisionEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        HandleCollisionExit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionEnter(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleCollisionExit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollisionEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HandleCollisionExit(other);
    }

    private void HandleCollisionEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount++;
            isColliding = collidingCount > 0;
        }
    }

    private void HandleCollisionEnter(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount++;
            isColliding = collidingCount > 0;
        }
    }

    private void HandleCollisionExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount = Mathf.Max(0, collidingCount - 1);
            isColliding = collidingCount > 0;
        }
    }

    private void HandleCollisionExit(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount = Mathf.Max(0, collidingCount - 1);
            isColliding = collidingCount > 0;
        }
    }
}
