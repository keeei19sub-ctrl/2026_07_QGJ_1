using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class scenechangeButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string toScene;

    private bool isTransitioning;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isTransitioning)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(toScene))
        {
            Debug.LogError(
                $"Scene '{toScene}' is not included in Build Settings.",
                this);
            return;
        }

        isTransitioning = true;
        SceneManager.LoadScene(toScene);
    }
}
