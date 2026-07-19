using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public sealed class CoinPickup : MonoBehaviour
{
    [SerializeField] private CircleCollider2D pickupCollider;

    private Coroutine flightCoroutine;
    private int rewardAmount;
    private bool collected;
    private bool removalNotified;

    public event Action<CoinPickup> Removed;

    private void Awake()
    {
        pickupCollider = pickupCollider != null
            ? pickupCollider
            : GetComponent<CircleCollider2D>();
        pickupCollider.enabled = false;
    }

    public void Initialize(
        int amount,
        Vector2 landingPosition,
        float flightDuration,
        float arcHeight)
    {
        rewardAmount = Mathf.Max(0, amount);

        if (flightCoroutine != null)
        {
            StopCoroutine(flightCoroutine);
        }

        pickupCollider.enabled = false;
        flightCoroutine = StartCoroutine(FlyTo(
            landingPosition,
            Mathf.Max(0f, flightDuration),
            Mathf.Max(0f, arcHeight)));
    }

    private IEnumerator FlyTo(
        Vector2 landingPosition,
        float flightDuration,
        float arcHeight)
    {
        Vector2 startPosition = transform.position;

        if (flightDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / flightDuration);
                Vector2 position = Vector2.Lerp(startPosition, landingPosition, progress);
                position += Vector2.up * (4f * arcHeight * progress * (1f - progress));
                transform.position = position;
                yield return null;
            }
        }

        transform.position = landingPosition;
        flightCoroutine = null;
        pickupCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected || !pickupCollider.enabled || other == null)
        {
            return;
        }

        // The wallet is attached to the player's body object. Requiring it on the
        // exact collider object prevents umbrella and shadow child colliders from
        // collecting the coin.
        if (!other.TryGetComponent(out PlayerWallet wallet)
            || !wallet.AddMoney(rewardAmount))
        {
            return;
        }

        collected = true;
        pickupCollider.enabled = false;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (removalNotified)
        {
            return;
        }

        removalNotified = true;
        Removed?.Invoke(this);
    }
}
