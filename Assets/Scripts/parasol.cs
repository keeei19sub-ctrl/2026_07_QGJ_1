using UnityEngine;

public class parasol : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        KingHealth kinghealth = collision.GetComponent<KingHealth>();
        if(kinghealth != null){
            kinghealth.Damage(-kinghealth.sunDamage);
        }
    }
}
