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

    void Start()
    {
        MoveAction.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
    }

    void FixedUpdate()
        {
            Vector2 position = (Vector2)rigidbody2d.position + move * Speed * Time.deltaTime;
            if(position.x > maxCordinateXPlus)position.x = maxCordinateXPlus;
            else if (position.x < maxCordinateXMinus)position.x = maxCordinateXMinus;
            rigidbody2d.MovePosition(position); 
        }
}