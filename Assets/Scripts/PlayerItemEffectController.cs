using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInventory))]
public sealed class PlayerItemEffectController : MonoBehaviour
{
    [Header("Player State")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Umbrella Effect Targets")]
    [SerializeField] private Transform umbrellaVisual;
    [SerializeField] private Transform shadowVisual;
    [SerializeField] private BoxCollider2D leftUmbrellaHitbox;
    [SerializeField] private BoxCollider2D rightUmbrellaHitbox;

    [Header("Runtime Targets (optional)")]
    [SerializeField] private KingHealth kingHealth;
    [SerializeField] private ProjectileManager projectileManager;

    [Header("King Healing Delivery")]
    [SerializeField] private Sprite healingItemSprite;
    [SerializeField] private Vector3 playerHoldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Vector3 kingTargetOffset = new Vector3(0f, 2f, 0f);
    [SerializeField, Min(0f)] private float healingItemHoldDuration = 0.6f;
    [SerializeField, Min(0.01f)] private float healingItemTravelDuration = 0.8f;
    [SerializeField, Min(0f)] private float healingItemArcHeight = 2f;
    [SerializeField, Min(0.01f)] private float healingItemScale = 0.75f;
    [SerializeField] private int healingItemSortingOrder = 20;

    private TransformState umbrellaBaseState;
    private TransformState shadowBaseState;
    private BoxColliderState leftHitboxBaseState;
    private BoxColliderState rightHitboxBaseState;
    private CircleCollider2D shadowCollider;
    private CircleColliderState shadowColliderBaseState;
    private bool umbrellaBaseStateCaptured;
    private bool umbrellaExpanded;
    private Coroutine umbrellaTimer;
    private Coroutine healingDelivery;
    private GameObject healingItemVisual;

    private void Awake()
    {
        ResolvePlayerReferences();
    }

    public bool TryUseSelectedItem()
    {
        if (healingDelivery != null)
        {
            return false;
        }

        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        ItemDefinition item = inventory != null ? inventory.SelectedItem : null;
        if (item == null)
        {
            return false;
        }

        bool effectApplied = TryApplyEffect(item);
        return effectApplied && inventory.TryConsumeSelected();
    }

    public bool TryApplyEffect(ItemDefinition item)
    {
        if (item == null)
        {
            return false;
        }

        switch (item.EffectType)
        {
            case ItemEffectType.HealKing:
                return BeginHealingDelivery(item.EffectAmount);

            case ItemEffectType.ExpandUmbrella:
                return ExpandUmbrella(item.EffectAmount, item.EffectDuration);

            case ItemEffectType.StopProjectiles:
                if (projectileManager == null)
                {
                    projectileManager = FindAnyObjectByType<ProjectileManager>();
                }

                return projectileManager != null
                    && projectileManager.SuppressAttacks(item.EffectDuration);

            default:
                return false;
        }
    }

    private bool BeginHealingDelivery(float maxHealthFraction)
    {
        if (maxHealthFraction <= 0f
            || healingDelivery != null
            || healingItemSprite == null)
        {
            return false;
        }

        if (kingHealth == null)
        {
            kingHealth = FindAnyObjectByType<KingHealth>();
        }

        if (kingHealth == null)
        {
            return false;
        }

        healingDelivery = StartCoroutine(DeliverHealingItem(maxHealthFraction));
        return true;
    }

    private IEnumerator DeliverHealingItem(float maxHealthFraction)
    {
        healingItemVisual = new GameObject("Healing Item Visual");
        GameObject spriteObject = new GameObject("Sprite");
        spriteObject.transform.SetParent(healingItemVisual.transform, false);
        spriteObject.transform.localPosition = -healingItemSprite.bounds.center;

        SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = healingItemSprite;
        spriteRenderer.sortingOrder = healingItemSortingOrder;
        healingItemVisual.transform.localScale = Vector3.one * healingItemScale;

        float holdElapsed = 0f;
        while (holdElapsed < healingItemHoldDuration)
        {
            healingItemVisual.transform.position = transform.position + playerHoldOffset;
            holdElapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 startPosition = transform.position + playerHoldOffset;
        float travelElapsed = 0f;
        float travelDuration = Mathf.Max(0.01f, healingItemTravelDuration);
        while (travelElapsed < travelDuration && kingHealth != null)
        {
            float t = Mathf.Clamp01(travelElapsed / travelDuration);
            Vector3 targetPosition = kingHealth.transform.position + kingTargetOffset;
            Vector3 controlPosition = (startPosition + targetPosition) * 0.5f
                + Vector3.up * healingItemArcHeight;
            healingItemVisual.transform.position = CalculateQuadraticBezier(
                startPosition,
                controlPosition,
                targetPosition,
                t);

            travelElapsed += Time.deltaTime;
            yield return null;
        }

        if (kingHealth != null)
        {
            healingItemVisual.transform.position = kingHealth.transform.position + kingTargetOffset;
            kingHealth.HealByMaxHealthFraction(maxHealthFraction);
        }

        DestroyHealingItemVisual();
        healingDelivery = null;
    }

    private static Vector3 CalculateQuadraticBezier(
        Vector3 start,
        Vector3 control,
        Vector3 end,
        float t)
    {
        float inverseT = 1f - t;
        return inverseT * inverseT * start
            + 2f * inverseT * t * control
            + t * t * end;
    }

    private void DestroyHealingItemVisual()
    {
        if (healingItemVisual == null)
        {
            return;
        }

        Destroy(healingItemVisual);
        healingItemVisual = null;
    }

    private bool ExpandUmbrella(float multiplier, float duration)
    {
        ResolvePlayerReferences();
        if (multiplier <= 1f
            || duration <= 0f)
        {
            return false;
        }

        if (!umbrellaExpanded)
        {
            umbrellaBaseStateCaptured = false;
            if (!CaptureUmbrellaBaseState())
            {
                return false;
            }
        }

        ApplyUmbrellaScale(multiplier);
        umbrellaExpanded = true;

        if (umbrellaTimer != null)
        {
            StopCoroutine(umbrellaTimer);
        }

        umbrellaTimer = StartCoroutine(RestoreUmbrellaAfter(duration));
        return true;
    }

    private IEnumerator RestoreUmbrellaAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        umbrellaTimer = null;
        RestoreUmbrella();
    }

    private void ResolvePlayerReferences()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        Animator umbrellaAnimator = GetComponentInChildren<Animator>(true);
        if (umbrellaVisual == null && umbrellaAnimator != null)
        {
            umbrellaVisual = umbrellaAnimator.transform;
        }

        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (shadowVisual == null && child.CompareTag("Shadow"))
            {
                shadowVisual = child;
            }

            if (leftUmbrellaHitbox == null && child.name == "LeftBox")
            {
                leftUmbrellaHitbox = child.GetComponent<BoxCollider2D>();
            }

            if (rightUmbrellaHitbox == null && child.name == "RightBox")
            {
                rightUmbrellaHitbox = child.GetComponent<BoxCollider2D>();
            }
        }

        if (shadowVisual != null)
        {
            shadowCollider = shadowVisual.GetComponent<CircleCollider2D>();
        }
    }

    private bool CaptureUmbrellaBaseState()
    {
        if (umbrellaBaseStateCaptured)
        {
            return true;
        }

        if (umbrellaVisual == null
            || shadowVisual == null
            || leftUmbrellaHitbox == null
            || rightUmbrellaHitbox == null)
        {
            return false;
        }

        umbrellaBaseState = new TransformState(umbrellaVisual);
        shadowBaseState = new TransformState(shadowVisual);
        leftHitboxBaseState = new BoxColliderState(leftUmbrellaHitbox);
        rightHitboxBaseState = new BoxColliderState(rightUmbrellaHitbox);
        if (shadowCollider != null)
        {
            shadowColliderBaseState = new CircleColliderState(shadowCollider);
        }

        umbrellaBaseStateCaptured = true;
        return true;
    }

    private void ApplyUmbrellaScale(float multiplier)
    {
        umbrellaBaseState.ApplyScale(multiplier);
        shadowBaseState.ApplyScale(multiplier);
        leftHitboxBaseState.ApplyScale(multiplier);
        rightHitboxBaseState.ApplyScale(multiplier);
    }

    private void RestoreUmbrella()
    {
        if (!umbrellaExpanded || !umbrellaBaseStateCaptured)
        {
            return;
        }

        umbrellaBaseState.Restore();
        shadowBaseState.Restore();
        leftHitboxBaseState.Restore();
        rightHitboxBaseState.Restore();
        shadowColliderBaseState.Restore();
        umbrellaExpanded = false;
        umbrellaBaseStateCaptured = false;
    }

    private void OnDisable()
    {
        if (healingDelivery != null)
        {
            StopCoroutine(healingDelivery);
            healingDelivery = null;
        }

        DestroyHealingItemVisual();

        if (umbrellaTimer != null)
        {
            StopCoroutine(umbrellaTimer);
            umbrellaTimer = null;
        }

        RestoreUmbrella();
    }

    private readonly struct TransformState
    {
        private readonly Transform target;
        private readonly Vector3 localScale;

        public TransformState(Transform target)
        {
            this.target = target;
            localScale = target.localScale;
        }

        public void ApplyScale(float multiplier)
        {
            if (target != null)
            {
                target.localScale = localScale * multiplier;
            }
        }

        public void Restore()
        {
            if (target == null)
            {
                return;
            }

            target.localScale = localScale;
        }
    }

    private readonly struct BoxColliderState
    {
        private readonly BoxCollider2D target;
        private readonly Vector2 offset;
        private readonly Vector2 size;

        public BoxColliderState(BoxCollider2D target)
        {
            this.target = target;
            offset = target.offset;
            size = target.size;
        }

        public void ApplyScale(float multiplier)
        {
            if (target == null)
            {
                return;
            }

            target.offset = offset * multiplier;
            target.size = size * multiplier;
        }

        public void Restore()
        {
            if (target == null)
            {
                return;
            }

            target.offset = offset;
            target.size = size;
        }
    }

    private readonly struct CircleColliderState
    {
        private readonly CircleCollider2D target;
        private readonly Vector2 offset;
        private readonly float radius;

        public CircleColliderState(CircleCollider2D target)
        {
            this.target = target;
            offset = target != null ? target.offset : Vector2.zero;
            radius = target != null ? target.radius : 0f;
        }

        public void Restore()
        {
            if (target == null)
            {
                return;
            }

            target.offset = offset;
            target.radius = radius;
        }
    }
}
