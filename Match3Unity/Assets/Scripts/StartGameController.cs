using UnityEngine;

public class StartGameController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject gameLevelPanel;
    public void Play()
    {
        gameLevelPanel.SetActive(true);
        mainPanel.SetActive(false);
    }
    public void SelectGame(int gameLevel)
    {
        GameSettings.gameLevel = gameLevel;
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void Exit()
    {
        Application.Quit();
    }
}