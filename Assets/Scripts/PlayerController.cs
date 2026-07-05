using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public InputAction MoveAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    public float Speed = 10.0f;
    float maxCordinateXPlus = 2.3f;
    float maxCordinateXMinus = -3.3f;
    int parasolDirection = 0;

    [SerializeField] private Collider2D leftHitbox;
    [SerializeField] private Collider2D rightHitbox;
    void Start()
    {
        MoveAction.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
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
}