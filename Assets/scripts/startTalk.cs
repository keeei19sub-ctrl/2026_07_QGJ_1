using UnityEngine;
using UnityEngine.InputSystem;
public class startTalk : MonoBehaviour
{
    [SerializeField] private GameObject[] lines;
    [SerializeField]private GameObject king;
    [SerializeField] private GameObject player;
    public bool talking;
    private int talk;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        talking=true;
        talk=0;
        for(int i=1; i<lines.Length; i++){
            lines[i].SetActive(false);
        }
        lines[0].SetActive(true);
        king.SetActive(false);
        player.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(talking){
        if(Mouse.current.leftButton.wasPressedThisFrame){
            talk++;
            if(talk==1 || talk==4){
                king.SetActive(true);
                player.SetActive(false);
            }
            if(talk==3 || talk==5){
                king.SetActive(false);
                player.SetActive(true);
            }
            if(talk<lines.Length){
                lines[talk].SetActive(true);
                lines[talk-1].SetActive(false);
            }
            else{
                lines[lines.Length-1].SetActive(false);
                player.SetActive(false);
                talking=false;
            }
        }
        }
    }
}
