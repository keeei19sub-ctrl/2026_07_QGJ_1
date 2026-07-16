using System;
using UnityEngine;

public class KingSun : MonoBehaviour
{
    private static readonly int UmbrellaOpenParameter = Animator.StringToHash("Open");

    [Header("判定対象")]
    [Tooltip("このタグを持つオブジェクトと接触している間は処理をスキップする")]
    [SerializeField] private string targetTag = "Shadow";
 
    [Header("動作設定")]
    [Tooltip("接触していない間に呼びたい処理の実行間隔(秒)。0なら毎フレーム")]
    [SerializeField] private float processInterval = 10f;

    [Header("傘のアニメーション")]
    [Tooltip("王様の影判定を反映する Umbrella Prefab の Animator")]
    [SerializeField] private Animator umbrellaAnimator;
 
    // 現在、影によって太陽ダメージを防げているかどうか
    private bool isProtectedFromSun = false;

    public bool IsProtectedFromSun => isProtectedFromSun;

    public event Action<bool> ProtectionChanged;
 
    // 接触中のコライダーを個別に管理する場合のカウント
    // (複数の対象と同時接触してもExit判定がズレないようにするため)
    private int collidingCount = 0;
 
    private float timer = 0f;

    KingHealth kingHealth;
    PlayerController playerController;

    void Start()
    {
        kingHealth = GetComponent<KingHealth>();
        playerController = FindAnyObjectByType<PlayerController>();

        if (umbrellaAnimator == null)
        {
            if (playerController != null)
            {
                umbrellaAnimator = playerController.GetComponentInChildren<Animator>();
            }
        }

        UpdateShadowState();
    }
    private void Update()
    {
        // 傘を左右に振っている間も状態が変わるため毎フレーム更新する
        UpdateShadowState();

        // 太陽から守られていない時だけ処理する
        if (!isProtectedFromSun && Time.deltaTime != 0f)
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
            UpdateShadowState();
        }
    }

    private void HandleCollisionEnter(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount++;
            UpdateShadowState();
        }
    }

    private void HandleCollisionExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount = Mathf.Max(0, collidingCount - 1);
            UpdateShadowState();
        }
    }

    private void HandleCollisionExit(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            collidingCount = Mathf.Max(0, collidingCount - 1);
            UpdateShadowState();
        }
    }

    private void UpdateShadowState()
    {
        bool isInsideShadow = collidingCount > 0;
        bool isUmbrellaSwinging = playerController != null
            && playerController.IsUmbrellaSwinging;

        bool nextProtectionState = isInsideShadow && !isUmbrellaSwinging;
        bool protectionChanged = isProtectedFromSun != nextProtectionState;
        isProtectedFromSun = nextProtectionState;
        KingHealth.shadow = isProtectedFromSun;

        if (umbrellaAnimator != null)
        {
            // 左右振り中も影の内外自体は変えず、解除後の遷移先を安定させる
            umbrellaAnimator.SetBool(UmbrellaOpenParameter, isInsideShadow);
        }

        if (protectionChanged)
        {
            ProtectionChanged?.Invoke(isProtectedFromSun);
        }
    }

    private void OnDisable()
    {
        if (!isProtectedFromSun)
        {
            return;
        }

        isProtectedFromSun = false;
        KingHealth.shadow = false;
        ProtectionChanged?.Invoke(false);
    }
}
