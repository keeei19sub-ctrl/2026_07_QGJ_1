using System.Reflection.Metadata.Ecma335;
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
    int nowShop = 0;
    float cordXmax = 2.35f;
    float cordXmin = -3.3f;
    enum State
    {
        goShop,
        shopping,
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nowShop = 1;
        nextShop = getShopCordinate();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeState();
        if(IsShopping)ShoppingTime();
        move();
    }

    void ChangeState()
    {
        Debug.Log("changeState");
        if(state == State.goShop && Mathf.Approximately((rb.position - nextShop).magnitude, 0)){
            state = State.shopping;
            IsShopping = true;
            Debug.Log("state2ing");
        }
        else if(state == State.shopping && !IsShopping){
            state = State.goShop;
            choiceShop();
            nextShop = getShopCordinate();
            Debug.Log("state2go");
        }
    }

    Vector2 getShopCordinate(){
        Vector2 vec = Vector2.zero;
        if(Random.Range(1, 3) == 1)vec += Vector2.left * cordXmax;
        else vec += Vector2.left * cordXmin;
        vec += Vector2.up * nowShop * 10;
        return vec;
    }

    void choiceShop()
    {
        int diff = 0;
        int rnd = Random.Range(1, 11);
        if(rnd < 3)diff = -1;
        else if(rnd < 7)diff = 1;
        else if(rnd < 10)diff = 2;
        else diff = 3;
        nowShop += diff;
        if(nowShop < 0)nowShop = 1;
        Debug.Log("choice" + nowShop);
    }
    void ShoppingTime()
    {
        if(Mathf.Approximately(shoppingTimer, 0))shoppingTimer = shoppingMaxTime;
        shoppingTimer -= Time.deltaTime;
        if(shoppingTimer < 0)
        {
            shoppingTimer = 0f;
            IsShopping = false;
            Debug.Log("finishShopping");
        }
    }

    void move()
    {
        switch (state)
        {
            case State.goShop:
                transform.position = Vector3.MoveTowards(transform.position, nextShop, speed * Time.deltaTime);
                break;
            case State.shopping:
                break;
        }
    }
}
