using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class clickToStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame){
            SceneManager.LoadScene("main");
        }
    }
}

