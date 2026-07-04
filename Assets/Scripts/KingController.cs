using UnityEngine;

public class KingController : MonoBehaviour
{
    public float shopDist = 20;
    public float speed = 5f;
    Vector2 nextShop;
    Rigidbody2D rb;
    [SerializeField]State state;
    bool IsShopping = false;
    float shoppingTimer = 0f;
    public float shoppingMaxTime = 10.0f;
    enum State
    {
        straight,
        goShop,
        shopping,
        exitShop
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        state = State.straight;
        nextShop = getShopCordinate();
    }

    // Update is called once per frame
    void Update()
    {
        //ChangeState();
        if(IsShopping)ShoppingTime();
        move();
    }

    void ChangeState()
    {
        if(state == State.straight && rb.position.y + shopDist > nextShop.y)state = State.goShop;
        else if(state == State.goShop && Mathf.Approximately((rb.position - nextShop).magnitude, 0)){state = State.shopping; IsShopping = true;}
        else if(state == State.shopping && !IsShopping)state = State.exitShop;
        else if(state == State.exitShop && rb.position.x == 0)state = State.straight;
    }

    Vector2 getShopCordinate(){
        Vector2 vec = Vector2.zero;
        return vec;
    }

    void ShoppingTime()
    {
        if(Mathf.Approximately(shoppingTimer, 0))shoppingTimer = shoppingMaxTime;
        shoppingTimer -= Time.deltaTime;
        if(shoppingTimer < 0)
        {
            shoppingTimer = 0f;
            IsShopping = false;
        }
    }

    void move()
    {
        switch (state)
        {
            case State.straight:
                rb.position += Vector2.up * speed * Time.deltaTime;
                Debug.Log("straight");
                break;
            case State.goShop:
                transform.position = Vector3.MoveTowards(transform.position, nextShop, speed * Time.deltaTime);
                break;
            case State.shopping:
                break;
            case State.exitShop:
                Vector2 pos = rb.position;
                pos.x = Mathf.MoveTowards(pos.x, 0f, speed * Time.deltaTime);
                pos.y += speed * Time.deltaTime;
                rb.MovePosition(pos);
                break;
        }
    }
}
