using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace LoveAlgo.UI.Gameplay
{
    /// <summary>
    /// 게임 화면 - Conversation을 시작합니다.
    /// </summary>
    public class GameScreen : MonoBehaviour
    {
        private void Start()
        {
            StartInitialConversation();
        }

        private void StartInitialConversation()
        {
            // TitleScreen에서 설정한 시작 Conversation 가져오기
            string startConversation = PlayerPrefs.GetString("StartConversation", "Story_Demo");

            // Conversation 시작
            if (!string.IsNullOrEmpty(startConversation))
            {
                DialogueManager.StartConversation(startConversation);
            }
        }
    }
}
