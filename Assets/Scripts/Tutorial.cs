using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private InputAction escape;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        escape.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (escape.WasPressedThisFrame())
        {
            SceneManager.LoadScene("title");
        }
    }
}
