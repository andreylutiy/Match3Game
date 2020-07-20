using UnityEngine;

namespace Match3Game
{
    public class StartGameController : MonoBehaviour
    {
        [SerializeField] private GameObject MainPanel;
        [SerializeField] private GameObject GameLevelPanel;

        public void Play()
        {
            GameLevelPanel.SetActive(true);
            MainPanel.SetActive(false);
        }

        public void SelectGame(int gameLevel)
        {
            GameSettings.GameLevel = gameLevel;
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}