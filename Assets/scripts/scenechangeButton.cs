using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class scenechangeButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private  string toScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void OnPointerClick(PointerEventData eventData){
        SceneManager.LoadScene(toScene);
    }
}
