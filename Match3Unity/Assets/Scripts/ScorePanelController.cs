using UnityEngine;

public class ScorePanelController : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text scoreText;

    void Start()
    {
        if (scoreText != null)
            scoreText.text = GameSettings.lastScore.ToString();
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void GoToStartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}