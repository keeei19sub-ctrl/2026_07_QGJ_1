using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KingController : MonoBehaviour
{
    public event Action DestinationRequested;
    public event Action DestinationReached;

    [SerializeField, Min(0f)] private float speed = 5f;
    [SerializeField, Min(0f)] private float shoppingMaxTime = 1f;
    [SerializeField] private Vector2 goalPos;
    [SerializeField] private float goalStartY = 35f;
    [SerializeField, Min(0.001f)] private float arrivalDistance = 0.05f;

    private enum State
    {
        GoShop,
        Shopping,
        GoGoal,
        Goal,
        WaitingForDestination
    }

    [SerializeField] private State state = State.GoShop;

    private Rigidbody2D rb;
    private Vector2 nextDestination;
    private Vector2? pendingDestination;
    private float shoppingTimer;
    private float progressStartY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        progressStartY = rb.position.y;
    }

    private void Start()
    {
        AcquireNextDestination();
    }

    private void Update()
    {
        switch (state)
        {
            case State.GoShop:
                if (HasReached(nextDestination))
                {
                    EnterShopping();
                }
                break;

            case State.Shopping:
                shoppingTimer -= Time.deltaTime;
                if (shoppingTimer <= 0f)
                {
                    AcquireNextDestination();
                }
                break;
        }

        if (state != State.GoGoal && state != State.Goal && rb.position.y >= goalStartY)
        {
            ChangeState(State.GoGoal);
        }
        UpdateProgressBar();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.GoShop:
                MoveTowards(nextDestination);
                break;

            case State.GoGoal:
                MoveTowards(goalPos);
                if (HasReached(goalPos))
                {
                    rb.position = goalPos;
                    ChangeState(State.Goal);
                }
                break;
        }
    }

    private void EnterShopping()
    {
        rb.position = nextDestination;
        DestinationReached?.Invoke();
        shoppingTimer = shoppingMaxTime;
        ChangeState(State.Shopping);
    }

    /// <summary>
    /// Sets the destination that will be acquired when the king next chooses where to go.
    /// If the king is already waiting, the destination is acquired immediately.
    /// </summary>
    public void SetNextDestination(Vector2 destination)
    {
        pendingDestination = destination;

        if (state == State.WaitingForDestination)
        {
            AcquireNextDestination();
        }
    }

    private void AcquireNextDestination()
    {
        if (!pendingDestination.HasValue)
        {
            DestinationRequested?.Invoke();
        }

        if (!pendingDestination.HasValue)
        {
            ChangeState(State.WaitingForDestination);
            return;
        }

        nextDestination = pendingDestination.Value;
        pendingDestination = null;
        ChangeState(State.GoShop);
    }

    private void MoveTowards(Vector2 destination)
    {
        Vector2 position = Vector2.MoveTowards(
            rb.position,
            destination,
            speed * Time.fixedDeltaTime);

        rb.MovePosition(position);
    }

    private bool HasReached(Vector2 destination)
    {
        return Vector2.SqrMagnitude(rb.position - destination) <= arrivalDistance * arrivalDistance;
    }

    private void UpdateProgressBar()
    {
        UIHandler uiHandler = UIHandler.instance;
        if (uiHandler == null)
        {
            return;
        }

        float progress = Mathf.Approximately(progressStartY, goalPos.y)
            ? (rb.position.y >= goalPos.y ? 1f : 0f)
            : Mathf.InverseLerp(progressStartY, goalPos.y, rb.position.y);
        uiHandler.SetProgressValue(progress);
    }

    private void ChangeState(State nextState)
    {
        if (state == nextState)
        {
            return;
        }

        state = nextState;
        Debug.Log($"King state changed to {state}.", this);
    }
}
