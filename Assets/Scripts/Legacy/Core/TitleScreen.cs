using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LoveAlgo.UI.Title
{
    /// <summary>
    /// 타이틀 화면 - 게임 시작을 처리합니다.
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private string gameplaySceneName = "Gameplay";
        [SerializeField] private string startConversationName = "Story_Demo";

        private void Start()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(StartNewGame);
            }
        }

        private void StartNewGame()
        {
            // 시작 Conversation 설정
            PlayerPrefs.SetString("StartConversation", startConversationName);
            PlayerPrefs.Save();

            // Gameplay 씬으로 전환
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}

