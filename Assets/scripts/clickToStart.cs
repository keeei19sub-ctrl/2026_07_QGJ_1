using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class clickToStart : MonoBehaviour
{
    [SerializeField] private string nextScene = "main";

    private bool isTransitioning;

    private void Update()
    {
        if (!isTransitioning
            && Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError(
                $"Scene '{nextScene}' is not included in Build Settings.",
                this);
            return;
        }

        isTransitioning = true;
        SceneManager.LoadScene(nextScene);
    }
}
