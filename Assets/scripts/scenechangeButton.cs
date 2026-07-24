using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class scenechangeButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] private string toScene;

    private bool isTransitioning;

    public void OnPointerClick(PointerEventData eventData)
    {
        LoadTargetScene();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        LoadTargetScene();
    }

    private void LoadTargetScene()
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
