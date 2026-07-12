using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    public InputAction BuyItem;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    Vector2 rayDirection = Vector2.right;
    public float Speed = 10.0f;
    float maxCordinateXPlus = 4.6f;
    float maxCordinateXMinus = -2.7f;
    int parasolDirection = 0;

    [SerializeField] private Collider2D leftHitbox;
    [SerializeField] private Collider2D rightHitbox;
    void Start()
    {
        MoveAction.Enable();
        BuyItem.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        buyItem();
        parasolDirectionChange();
        parasolChange();
    }

    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + move * Speed * Time.deltaTime;
        if(position.x > maxCordinateXPlus)position.x = maxCordinateXPlus;
        else if (position.x < maxCordinateXMinus)position.x = maxCordinateXMinus;
        rigidbody2d.MovePosition(position); 
    }
    void parasolDirectionChange()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            parasolDirection--;
        }else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            parasolDirection++;
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            parasolDirection++;
        }else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            parasolDirection--;
        }
    }
    void parasolChange()
    {
        // 一旦両方消す
        leftHitbox.enabled = false;
        rightHitbox.enabled = false;

        switch (parasolDirection)
        {
            case -1:
                leftHitbox.enabled = true;
                break;
            case 1:
                rightHitbox.enabled = true;
                break;
            case 0:
                // どちらも無効のまま
                break;
        }
    }

    void buyItem()
    {
        const float rayDistance = 5f;
        if (move.sqrMagnitude > 0.001f)
        {
            rayDirection = move.normalized;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            rigidbody2d.position,
            rayDirection,
            rayDistance,
            LayerMask.GetMask("Shop"));

        Color rayColor = hit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(rigidbody2d.position, rayDirection * rayDistance, rayColor);

        if(hit.collider != null)
        {
            FindShop(hit);
        }
    }
    void FindShop(RaycastHit2D hit)
    {
        if (BuyItem.WasPressedThisFrame())
        {
            Debug.Log("raycast hit" + hit.collider.gameObject);
        }
    }
}
