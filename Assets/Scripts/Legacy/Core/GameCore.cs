using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace LoveAlgo.Systems
{
    /// <summary>
    /// 게임 코어 - DSU Variables를 통한 게임 상태 관리
    /// </summary>
    public class GameCore : MonoBehaviour
    {
        public static GameCore Instance { get; private set; }

        // DSU Variables를 통한 게임 상태 접근
        public int CurrentDay => DialogueLua.GetVariable("Day").asInt;
        public int Money => DialogueLua.GetVariable("Money").asInt;
        public string TimeOfDay => DialogueLua.GetVariable("TimeOfDay").asString;
        public bool IsEventDay => DialogueLua.GetVariable("IsEventDay").asBool;

        // 캐릭터 호감도
        public int Affection_Jina => DialogueLua.GetVariable("Affection_Jina").asInt;
        public int Affection_Sora => DialogueLua.GetVariable("Affection_Sora").asInt;
        public int Affection_Yuna => DialogueLua.GetVariable("Affection_Yuna").asInt;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
