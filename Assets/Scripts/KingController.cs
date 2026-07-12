using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KingController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 5f;
    [SerializeField, Min(0f)] private float shoppingMaxTime = 4f;
    [SerializeField] private Vector2 goalPos;
    [SerializeField] private float goalStartY = 35f;
    [SerializeField] private float leftShopX = -2.7f;
    [SerializeField] private float rightShopX = 3.3f;
    [SerializeField, Min(0.1f)] private float shopFloorInterval = 10f;
    [SerializeField, Min(0.001f)] private float arrivalDistance = 0.05f;

    private enum State
    {
        GoShop,
        Shopping,
        GoGoal,
        Goal
    }

    [SerializeField] private State state = State.GoShop;

    private Rigidbody2D rb;
    private Vector2 nextShop;
    private float shoppingTimer;
    private int currentShopFloor = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetNextShop();
    }

    private void Update()
    {
        switch (state)
        {
            case State.GoShop:
                if (HasReached(nextShop))
                {
                    EnterShopping();
                }
                break;

            case State.Shopping:
                shoppingTimer -= Time.deltaTime;
                if (shoppingTimer <= 0f)
                {
                    SelectNextShopFloor();
                    SetNextShop();
                    ChangeState(State.GoShop);
                }
                break;
        }

        if (state != State.GoGoal && state != State.Goal && rb.position.y >= goalStartY)
        {
            ChangeState(State.GoGoal);
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.GoShop:
                MoveTowards(nextShop);
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
        rb.position = nextShop;
        shoppingTimer = shoppingMaxTime;
        ChangeState(State.Shopping);
    }

    private void SetNextShop()
    {
        float x = Random.value < 0.5f ? leftShopX : rightShopX;
        nextShop = new Vector2(x, currentShopFloor * shopFloorInterval);
    }

    private void SelectNextShopFloor()
    {
        int roll = Random.Range(1, 11);
        int floorDifference = roll < 3 ? -1 : roll < 7 ? 1 : roll < 10 ? 2 : 3;
        currentShopFloor = Mathf.Max(1, currentShopFloor + floorDifference);
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
