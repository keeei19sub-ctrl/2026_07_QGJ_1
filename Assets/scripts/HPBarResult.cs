using UnityEngine;

public class HPBarResult : MonoBehaviour
{
    [SerializeField]private float maxSize;
     Vector3 pos;
     Vector3 size;
     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos=transform.position;
        size=transform.localScale;
        size.x=(float)KingHealth.hp/KingHealth.maxHp;
        pos.x+=-maxSize*(KingHealth.maxHp-KingHealth.hp)/((float)KingHealth.maxHp*2);
        transform.position=pos;
        transform.localScale=size;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

