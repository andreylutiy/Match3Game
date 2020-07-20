using UnityEngine;

namespace Match3Game
{
    public class ScorePanelController : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Text ScoreText;

        void Start()
        {
            if (ScoreText == null)
                return;

            ScoreText.text = GameSettings.LastScore.ToString();
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
}