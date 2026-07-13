using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerWallet), typeof(PlayerInventory))]
public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    public InputAction BuyItem = new InputAction(
        "Buy Item",
        InputActionType.Button,
        "<Keyboard>/space");
    public InputAction ConsumeItem = new InputAction(
        "Consume Item",
        InputActionType.Button,
        "<Keyboard>/e");

    Rigidbody2D rigidbody2d;
    Vector2 move;
    Vector2 rayDirection = Vector2.right;
    public float Speed = 10.0f;
    float maxCordinateXPlus = 4.6f;
    float maxCordinateXMinus = -2.7f;
    int parasolDirection = 0;

    public bool IsUmbrellaSwinging => parasolDirection != 0;

    [SerializeField] private Collider2D leftHitbox;
    [SerializeField] private Collider2D rightHitbox;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private PlayerInventory inventory;

    Animator m_umbrella;

    private Shop currentShop;

    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        wallet = wallet != null ? wallet : GetComponent<PlayerWallet>();
        inventory = inventory != null ? inventory : GetComponent<PlayerInventory>();

        EnsureButtonAction(ref BuyItem, "Buy Item", "<Keyboard>/space");
        EnsureButtonAction(ref ConsumeItem, "Consume Item", "<Keyboard>/e");
    }

    private void OnEnable()
    {
        MoveAction?.Enable();
        BuyItem?.Enable();
        ConsumeItem?.Enable();
    }

    private void Start()
    {
        UIHandler.instance?.BindPlayer(wallet, inventory);
        this.m_umbrella = GetComponentInChildren<Animator>();
    }

    private void OnDisable()
    {
        MoveAction?.Disable();
        BuyItem?.Disable();
        ConsumeItem?.Disable();

        currentShop = null;
        UIHandler.instance?.HideShop();
    }

    void Update()
    {
        move = MoveAction != null ? MoveAction.ReadValue<Vector2>() : Vector2.zero;
        UpdateShopInteraction();
        UpdateInventorySelection();
        parasolDirectionChange();
        parasolChange();
    }

    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + move * Speed * Time.deltaTime;
        if (position.x > maxCordinateXPlus) position.x = maxCordinateXPlus;
        else if (position.x < maxCordinateXMinus) position.x = maxCordinateXMinus;
        rigidbody2d.MovePosition(position);
    }
    void parasolDirectionChange()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            parasolDirection--;
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            parasolDirection++;
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            parasolDirection++;
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            parasolDirection--;
        }
    }
    void parasolChange()
    {
        bool isSwingingLeft = parasolDirection < 0;
        bool isSwingingRight = parasolDirection > 0;

        leftHitbox.enabled = isSwingingLeft;
        rightHitbox.enabled = isSwingingRight;

        if (m_umbrella != null)
        {
            m_umbrella.SetBool("Left", isSwingingLeft);
            m_umbrella.SetBool("Right", isSwingingRight);
        }
    }

    private void UpdateShopInteraction()
    {
        const float rayDistance = 1f;
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

        Shop detectedShop = hit.collider != null
            ? hit.collider.GetComponentInParent<Shop>()
            : null;

        if (detectedShop != currentShop)
        {
            currentShop = detectedShop;

            if (currentShop != null)
            {
                UIHandler.instance?.ShowShop(currentShop);
            }
            else
            {
                UIHandler.instance?.HideShop();
            }
        }

        if (currentShop != null && BuyItem.WasPressedThisFrame())
        {
            PurchaseResult result = currentShop.TryPurchase(wallet, inventory);
            UIHandler.instance?.ShowPurchaseResult(result);
        }
    }

    private void UpdateInventorySelection()
    {
        if (inventory == null)
        {
            return;
        }
        if (ConsumeItem.WasPressedThisFrame())
        {
            inventory.TryConsumeSelected();
        }
    }

    private static void EnsureButtonAction(
        ref InputAction action,
        string actionName,
        string defaultBinding)
    {
        if (action == null)
        {
            action = new InputAction(actionName, InputActionType.Button, defaultBinding);
            return;
        }

        if (action.bindings.Count == 0)
        {
            action.AddBinding(defaultBinding);
        }
    }
}
