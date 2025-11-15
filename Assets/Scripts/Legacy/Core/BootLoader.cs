using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

namespace LoveAlgo.Systems
{
    /// <summary>
    /// 부트 로더 - Boot 씬에서 시스템 초기화를 담당하고 Title 씬으로 전환합니다.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string titleSceneName = "Title";
        [SerializeField] private DialogueDatabase defaultDatabase;

        private void Start()
        {
            InitializeSystems();
            SceneManager.LoadScene(titleSceneName);
        }

        private void InitializeSystems()
        {
            // GameCore는 Awake에서 자동 초기화됨
        }
    }
}

