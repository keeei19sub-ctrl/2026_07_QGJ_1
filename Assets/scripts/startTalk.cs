using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class startTalk : MonoBehaviour
{
    [SerializeField] private GameObject[] lines;
    [SerializeField] private GameObject king;
    [SerializeField] private GameObject player;
    [SerializeField] private string nextScene = "main";

    public bool talking;
    private int talk;
    private bool isTransitioning;

    private void Start()
    {
        talking = true;
        talk = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            lines[i].SetActive(false);
        }

        lines[0].SetActive(true);
        king.SetActive(false);
        player.SetActive(true);
    }

    private void Update()
    {
        if (!talking
            || isTransitioning
            || Mouse.current == null
            || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        talk++;
        if (talk == 1 || talk == 4)
        {
            king.SetActive(true);
            player.SetActive(false);
        }

        if (talk == 3 || talk == 5)
        {
            king.SetActive(false);
            player.SetActive(true);
        }

        if (talk < lines.Length)
        {
            lines[talk].SetActive(true);
            lines[talk - 1].SetActive(false);
            return;
        }

        lines[lines.Length - 1].SetActive(false);
        player.SetActive(false);
        talking = false;
        LoadNextScene();
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
