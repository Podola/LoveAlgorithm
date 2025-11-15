using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;

namespace LoveAlgo.Data
{
    /// <summary>
    /// 러브알고리즘 게임의 모든 핵심 데이터를 DialogueSystem Variables로 관리하는 중앙 관리자
    /// 게임 기획서에 따른 완전한 데이터 구조를 제공
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static GameDataManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DialogueSystemController 초기화를 기다리기 위해 Start로 지연
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // DialogueSystemController가 준비될 때까지 대기
            StartCoroutine(WaitForDialogueSystemAndInitialize());
        }

        private System.Collections.IEnumerator WaitForDialogueSystemAndInitialize()
        {
            // DialogueSystemController가 준비될 때까지 최대 1초 대기
            float timeout = 1f;
            float elapsed = 0f;
            
            while (!IsDialogueSystemReady() && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (IsDialogueSystemReady())
            {
                InitializeGameData();
            }
            else
            {
                Debug.LogError("[GameDataManager] DialogueSystem이 준비되지 않았습니다. 초기화를 건너뜁니다.");
            }
        }
        #endregion

        #region Game Data Structure Constants
        
        // === 플레이어 스탯 ===
        public const string STAT_HEALTH = "Health";          // 체력 (0-100)
        public const string STAT_INTELLIGENCE = "Intelligence"; // 지성 (0-100)
        public const string STAT_SOCIAL = "Social";          // 사교성 (0-100)
        public const string STAT_PERSISTENCE = "Persistence"; // 끈기 (0-100)
        public const string STAT_FATIGUE = "Fatigue";        // 피로도 (0-100, 낮을수록 좋음)
        public const string STAT_MONEY = "Money";            // 돈
        
        // === 게임 진행 상태 ===
        public const string GAME_CURRENT_DAY = "CurrentDay";       // 현재 일차 (1부터 시작)
        public const string GAME_CURRENT_PHASE = "CurrentPhase";   // 현재 게임 단계
        public const string GAME_TIME_OF_DAY = "TimeOfDay";        // 현재 시간대 (Morning/Afternoon/Evening)
        public const string GAME_FREE_ACTIONS_LEFT = "FreeActionsLeft"; // 남은 자유행동 횟수 (0-2)
        
        // === 게임 단계 (Phase) ===
        public const string PHASE_OPENING = "Opening";                    // 개강
        public const string PHASE_DAILY_BEFORE_EVENT1 = "DailyBeforeEvent1";  // 1차 이벤트 전 자유행동 기간
        public const string PHASE_FIRST_EVENT = "FirstEvent";             // 1차 개인 이벤트
        public const string PHASE_DAILY_BEFORE_FESTIVAL = "DailyBeforeFestival"; // 축제 전 자유행동 기간
        public const string PHASE_FESTIVAL = "Festival";                  // 축제
        public const string PHASE_DAILY_BEFORE_EVENT2 = "DailyBeforeEvent2";    // 2차 이벤트 전 자유행동 기간
        public const string PHASE_SECOND_EVENT = "SecondEvent";           // 2차 개인 이벤트
        public const string PHASE_DAILY_BEFORE_MT = "DailyBeforeMT";      // MT 전 자유행동 기간
        public const string PHASE_MT = "MT";                              // MT
        public const string PHASE_DAILY_BEFORE_EVENT3 = "DailyBeforeEvent3";    // 3차 이벤트 전 자유행동 기간
        public const string PHASE_THIRD_EVENT = "ThirdEvent";             // 3차 개인 이벤트
        public const string PHASE_DAILY_BEFORE_CONFESSION = "DailyBeforeConfession"; // 고백 전 자유행동 기간
        public const string PHASE_CONFESSION = "Confession";              // 고백
        public const string PHASE_ENDING = "Ending";                      // 엔딩
        
        // === 히로인별 호감도 포인트 (DialogueDatabase Variables와 일치) ===
        public const string AFFECTION_YEEUN = "Yeeun_Points";    // 하예은 호감도
        public const string AFFECTION_DAEUN = "Daeun_Points";    // 서다은 호감도  
        public const string AFFECTION_BOM = "Bom_Points";        // 이봄 호감도
        public const string AFFECTION_HEEWON = "Heewon_Points";  // 도희원 호감도
        public const string AFFECTION_ROA = "Roa_Points";        // 로아 호감도 (히든)
        
        // === 레거시 호감도 변수명 (하위 호환성) ===
        [System.Obsolete("Use AFFECTION_YEEUN instead")]
        public const string AFFECTION_HAYEEUN = "Yeeun_Points";
        [System.Obsolete("Use AFFECTION_DAEUN instead")]
        public const string AFFECTION_SEODAEUN = "Daeun_Points";
        [System.Obsolete("Use AFFECTION_BOM instead")]
        public const string AFFECTION_LEEBOM = "Bom_Points";
        [System.Obsolete("Use AFFECTION_HEEWON instead")]
        public const string AFFECTION_DOHEEWON = "Heewon_Points";
        
        // === 이벤트 선택 추적 (각 이벤트에서 어떤 히로인을 선택했는지) ===
        public const string EVENT_FIRST_CHOICE = "Event1_Choice";   // 1차 이벤트 선택
        public const string EVENT_FESTIVAL_CHOICE = "Festival_Choice"; // 축제 선택
        public const string EVENT_SECOND_CHOICE = "Event2_Choice"; // 2차 이벤트 선택
        public const string EVENT_MT_CHOICE = "MT_Choice";         // MT 선택
        public const string EVENT_THIRD_CHOICE = "Event3_Choice";   // 3차 이벤트 선택
        public const string EVENT_CONFESSION_CHOICE = "Confession_Choice"; // 고백 선택
        
        // === 히로인 이름 상수 (DialogueDatabase Actor 이름과 일치) ===
        public const string HEROINE_YEEUN = "Yeeun";
        public const string HEROINE_DAEUN = "Daeun";
        public const string HEROINE_BOM = "Bom";
        public const string HEROINE_HEEWON = "Heewon";
        public const string HEROINE_ROA = "Roa";
        public const string HEROINE_NONE = "None";
        
        // === 레거시 히로인 이름 (하위 호환성) ===
        [System.Obsolete("Use HEROINE_YEEUN instead")]
        public const string HEROINE_HAYEEUN = "Yeeun";
        [System.Obsolete("Use HEROINE_DAEUN instead")]
        public const string HEROINE_SEODAEUN = "Daeun";
        [System.Obsolete("Use HEROINE_BOM instead")]
        public const string HEROINE_LEEBOM = "Bom";
        [System.Obsolete("Use HEROINE_HEEWON instead")]
        public const string HEROINE_DOHEEWON = "Heewon";
        
        // === 아이템 사용 추적 ===
        public const string ITEM_USED_TODAY = "LoveAlgo_Item_UsedToday";         // 오늘 사용한 아이템 목록
        public const string ITEM_DUPLICATE_PENALTY = "LoveAlgo_Item_DuplicatePenalty"; // 중복 사용 페널티 플래그
        
        // === 기타 게임 상태 ===
        public const string MISC_TUTORIAL_COMPLETED = "LoveAlgo_Tutorial_Completed"; // 튜토리얼 완료 여부
        public const string MISC_FIRST_PLAY = "LoveAlgo_First_Play";                 // 첫 플레이 여부
        
        #endregion

        #region Data Initialization

        private bool isDataInitialized = false;
        
        /// <summary>
        /// 게임 데이터 초기화 (DialogueSystem Variables 설정)
        /// </summary>
        public void InitializeGameData()
        {
            if (isDataInitialized)
            {
                Debug.Log("[GameDataManager] ⚠️ 게임 데이터가 이미 초기화되어 있습니다. 건너뛰기...");
                return;
            }
            
            if (!IsDialogueSystemReady())
            {
                Debug.LogError("[GameDataManager] DialogueSystem이 준비되지 않았습니다!");
                return;
            }

            try
            {
                // PlayerName 초기화 (새 게임 시 빈 값으로 설정)
                DialogueLua.SetVariable("PlayerName", "");

                // 플레이어 스탯 초기화 (기획서 기준)
                DialogueLua.SetVariable(STAT_HEALTH, 50);
                DialogueLua.SetVariable(STAT_INTELLIGENCE, 50);
                DialogueLua.SetVariable(STAT_SOCIAL, 50);
                DialogueLua.SetVariable(STAT_PERSISTENCE, 50);
                DialogueLua.SetVariable(STAT_FATIGUE, 0);
                DialogueLua.SetVariable(STAT_MONEY, 10000); // 시작 1만원

                // 게임 진행 상태 초기화 (이벤트 데이로 시작)
                DialogueLua.SetVariable(GAME_CURRENT_DAY, 0); // 0일차 = 이벤트 데이
                DialogueLua.SetVariable(GAME_CURRENT_PHASE, PHASE_OPENING);
                DialogueLua.SetVariable(GAME_TIME_OF_DAY, "Morning");
                DialogueLua.SetVariable(GAME_FREE_ACTIONS_LEFT, 0); // 이벤트 데이이므로 자유행동 없음

                // 히로인별 호감도 초기화 (DialogueDatabase Variables와 일치)
                DialogueLua.SetVariable(AFFECTION_YEEUN, 0);
                DialogueLua.SetVariable(AFFECTION_DAEUN, 0);
                DialogueLua.SetVariable(AFFECTION_BOM, 0);
                DialogueLua.SetVariable(AFFECTION_HEEWON, 0);
                DialogueLua.SetVariable(AFFECTION_ROA, 0);

                // 이벤트 선택 초기화
                DialogueLua.SetVariable(EVENT_FIRST_CHOICE, HEROINE_NONE);
                DialogueLua.SetVariable(EVENT_FESTIVAL_CHOICE, HEROINE_NONE);
                DialogueLua.SetVariable(EVENT_SECOND_CHOICE, HEROINE_NONE);
                DialogueLua.SetVariable(EVENT_MT_CHOICE, HEROINE_NONE);
                DialogueLua.SetVariable(EVENT_THIRD_CHOICE, HEROINE_NONE);
                DialogueLua.SetVariable(EVENT_CONFESSION_CHOICE, HEROINE_NONE);

                // 기타 상태 초기화
                DialogueLua.SetVariable(ITEM_USED_TODAY, "");
                DialogueLua.SetVariable(ITEM_DUPLICATE_PENALTY, false);
                DialogueLua.SetVariable(MISC_TUTORIAL_COMPLETED, false);
                DialogueLua.SetVariable(MISC_FIRST_PLAY, true);

                isDataInitialized = true; // 초기화 완료 플래그 설정
                Debug.Log("[GameDataManager] ✅ 게임 데이터 초기화 완료!");
                LogCurrentGameState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameDataManager] ❌ 게임 데이터 초기화 실패: {e.Message}");
            }
        }

        /// <summary>
        /// DialogueSystem이 준비되었는지 확인
        /// </summary>
        bool IsDialogueSystemReady()
        {
            return DialogueManager.instance != null && 
                   DialogueManager.instance.isActiveAndEnabled &&
                   DialogueManager.masterDatabase != null;
        }

        /// <summary>
        /// 새 게임 시작 시 초기화 플래그 리셋
        /// </summary>
        public void ResetForNewGame()
        {
            isDataInitialized = false;
            Debug.Log("[GameDataManager] 🔄 새 게임을 위한 초기화 플래그 리셋");
        }

        #endregion

        #region Data Access Methods

        // === 플레이어 스탯 관련 (안전성 개선) ===
        public int GetHealth() => GetVariableSafe(STAT_HEALTH, 50);
        public int GetIntelligence() => GetVariableSafe(STAT_INTELLIGENCE, 50);
        public int GetSocial() => GetVariableSafe(STAT_SOCIAL, 50);
        public int GetPersistence() => GetVariableSafe(STAT_PERSISTENCE, 50);
        public int GetFatigue() => GetVariableSafe(STAT_FATIGUE, 0);
        public int GetMoney() => GetVariableSafe(STAT_MONEY, 10000);

        public void SetHealth(int value) => SetVariableSafe(STAT_HEALTH, Mathf.Clamp(value, 0, 100));
        public void SetIntelligence(int value) => SetVariableSafe(STAT_INTELLIGENCE, Mathf.Clamp(value, 0, 100));
        public void SetSocial(int value) => SetVariableSafe(STAT_SOCIAL, Mathf.Clamp(value, 0, 100));
        public void SetPersistence(int value) => SetVariableSafe(STAT_PERSISTENCE, Mathf.Clamp(value, 0, 100));
        public void SetFatigue(int value) => SetVariableSafe(STAT_FATIGUE, Mathf.Clamp(value, 0, 100));
        public void SetMoney(int value) => SetVariableSafe(STAT_MONEY, Mathf.Max(value, 0));

        // === 게임 진행 상태 관련 (안전성 개선) ===
        public int GetCurrentDay() => GetVariableSafe(GAME_CURRENT_DAY, 1);
        public string GetCurrentPhase() => GetVariableStringSafe(GAME_CURRENT_PHASE, PHASE_OPENING);
        public string GetTimeOfDay() => GetVariableStringSafe(GAME_TIME_OF_DAY, "Morning");
        public int GetFreeActionsLeft() => GetVariableSafe(GAME_FREE_ACTIONS_LEFT, 2);

        public void SetCurrentDay(int day) => SetVariableSafe(GAME_CURRENT_DAY, day);
        public void SetCurrentPhase(string phase) => SetVariableSafe(GAME_CURRENT_PHASE, phase);
        public void SetTimeOfDay(string timeOfDay) => SetVariableSafe(GAME_TIME_OF_DAY, timeOfDay);
        public void SetFreeActionsLeft(int actions) => SetVariableSafe(GAME_FREE_ACTIONS_LEFT, Mathf.Clamp(actions, 0, 2));

        // === 히로인 호감도 관련 ===
        public int GetAffection(string heroine)
        {
            // 레거시 이름 호환 처리 (문자열 리터럴로 비교하여 경고 방지)
            if (heroine == "HaYeEun" || heroine == HEROINE_YEEUN) heroine = HEROINE_YEEUN;
            if (heroine == "SeoDaEun" || heroine == HEROINE_DAEUN) heroine = HEROINE_DAEUN;
            if (heroine == "LeeBom" || heroine == HEROINE_BOM) heroine = HEROINE_BOM;
            if (heroine == "DoHeeWon" || heroine == HEROINE_HEEWON) heroine = HEROINE_HEEWON;
            
            switch (heroine)
            {
                case HEROINE_YEEUN:
                    return GetVariableSafe(AFFECTION_YEEUN, 0);
                case HEROINE_DAEUN:
                    return GetVariableSafe(AFFECTION_DAEUN, 0);
                case HEROINE_BOM:
                    return GetVariableSafe(AFFECTION_BOM, 0);
                case HEROINE_HEEWON:
                    return GetVariableSafe(AFFECTION_HEEWON, 0);
                case HEROINE_ROA:
                    return GetVariableSafe(AFFECTION_ROA, 0);
                default: return 0;
            }
        }

        public void AddAffection(string heroine, int points)
        {
            int current = GetAffection(heroine);
            SetAffection(heroine, current + points);
            
            Debug.Log($"[GameDataManager] {heroine} 호감도 +{points} → {GetAffection(heroine)}");
        }

        public void SetAffection(string heroine, int value)
        {
            // 레거시 이름 호환 처리 (문자열 리터럴로 비교하여 경고 방지)
            if (heroine == "HaYeEun" || heroine == HEROINE_YEEUN) heroine = HEROINE_YEEUN;
            if (heroine == "SeoDaEun" || heroine == HEROINE_DAEUN) heroine = HEROINE_DAEUN;
            if (heroine == "LeeBom" || heroine == HEROINE_BOM) heroine = HEROINE_BOM;
            if (heroine == "DoHeeWon" || heroine == HEROINE_HEEWON) heroine = HEROINE_HEEWON;
            
            switch (heroine)
            {
                case HEROINE_YEEUN:
                    DialogueLua.SetVariable(AFFECTION_YEEUN, value);
                    break;
                case HEROINE_DAEUN:
                    DialogueLua.SetVariable(AFFECTION_DAEUN, value);
                    break;
                case HEROINE_BOM:
                    DialogueLua.SetVariable(AFFECTION_BOM, value);
                    break;
                case HEROINE_HEEWON:
                    DialogueLua.SetVariable(AFFECTION_HEEWON, value);
                    break;
                case HEROINE_ROA:
                    DialogueLua.SetVariable(AFFECTION_ROA, value);
                    break;
            }
        }

        // === 이벤트 선택 관련 ===
        public void SetEventChoice(string eventType, string heroine)
        {
            DialogueLua.SetVariable(eventType, heroine);
            Debug.Log($"[GameDataManager] 이벤트 선택 기록: {eventType} → {heroine}");
        }

        public string GetEventChoice(string eventType)
        {
            return DialogueLua.GetVariable(eventType).asString;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 현재 게임 상태를 콘솔에 출력 (디버깅용)
        /// </summary>
        public void LogCurrentGameState()
        {
            string report = "📊 현재 게임 상태:\n";
            report += $"  📅 일차: {GetCurrentDay()}일, 단계: {GetCurrentPhase()}, 시간: {GetTimeOfDay()}\n";
            report += $"  🎮 남은 자유행동: {GetFreeActionsLeft()}회\n";
            report += $"  💪 스탯 - 체력:{GetHealth()} 지성:{GetIntelligence()} 사교성:{GetSocial()} 끈기:{GetPersistence()} 피로:{GetFatigue()}\n";
            report += $"  💰 돈: {GetMoney():N0}원\n";
            report += $"  💕 호감도 - 하예은:{GetAffection(HEROINE_YEEUN)} 서다은:{GetAffection(HEROINE_DAEUN)} 이봄:{GetAffection(HEROINE_BOM)} 도희원:{GetAffection(HEROINE_HEEWON)} 로아:{GetAffection(HEROINE_ROA)}\n";
            
            Debug.Log($"[GameDataManager] {report}");
        }

        /// <summary>
        /// 히로인별 공략 임계치 반환 (기획서 기준)
        /// </summary>
        public int GetHeroineThreshold(string heroine)
        {
            // 레거시 이름 호환 처리 (문자열 리터럴로 비교하여 경고 방지)
            if (heroine == "HaYeEun" || heroine == HEROINE_YEEUN) heroine = HEROINE_YEEUN;
            if (heroine == "SeoDaEun" || heroine == HEROINE_DAEUN) heroine = HEROINE_DAEUN;
            if (heroine == "LeeBom" || heroine == HEROINE_BOM) heroine = HEROINE_BOM;
            if (heroine == "DoHeeWon" || heroine == HEROINE_HEEWON) heroine = HEROINE_HEEWON;
            
            switch (heroine)
            {
                case HEROINE_YEEUN: return 32;   // 쉬움
                case HEROINE_DAEUN: return 35; // 보통
                case HEROINE_BOM: return 39;    // 조금 어려움
                case HEROINE_HEEWON: return 43; // 최고 난이도
                case HEROINE_ROA: return 46;      // 히든 (특수 조건)
                default: return 999;
            }
        }

        /// <summary>
        /// 히로인별 선호 스탯 반환 (기획서 기준)
        /// </summary>
        public string GetHeroinePreferredStat(string heroine)
        {
            // 레거시 이름 호환 처리 (문자열 리터럴로 비교하여 경고 방지)
            if (heroine == "HaYeEun" || heroine == HEROINE_YEEUN) heroine = HEROINE_YEEUN;
            if (heroine == "SeoDaEun" || heroine == HEROINE_DAEUN) heroine = HEROINE_DAEUN;
            if (heroine == "LeeBom" || heroine == HEROINE_BOM) heroine = HEROINE_BOM;
            if (heroine == "DoHeeWon" || heroine == HEROINE_HEEWON) heroine = HEROINE_HEEWON;
            
            switch (heroine)
            {
                case HEROINE_YEEUN: return STAT_HEALTH;
                case HEROINE_DAEUN: return STAT_INTELLIGENCE;
                case HEROINE_BOM: return STAT_SOCIAL;
                case HEROINE_HEEWON: return STAT_PERSISTENCE;
                case HEROINE_ROA: return STAT_FATIGUE; // 특수: 피로도 높을수록 좋음
                default: return "";
            }
        }

        #endregion

        #region Editor Utilities

        [ContextMenu("게임 데이터 초기화")]
        void EditorInitializeGameData()
        {
            InitializeGameData();
        }

        [ContextMenu("현재 상태 출력")]
        void EditorLogGameState()
        {
            LogCurrentGameState();
        }

        [ContextMenu("테스트 데이터 설정")]
        void EditorSetTestData()
        {
            SetCurrentDay(5);
            SetCurrentPhase(PHASE_FIRST_EVENT);
            SetMoney(50000);
            AddAffection(HEROINE_YEEUN, 10);
            AddAffection(HEROINE_DAEUN, 8);
            LogCurrentGameState();
        }

        #endregion

        #region Safe DialogueLua Helpers

        /// <summary>
        /// 안전한 DialogueLua Variable 읽기 (정수)
        /// </summary>
        int GetVariableSafe(string variableName, int defaultValue = 0)
        {
            try
            {
                if (!IsDialogueSystemReady())
                {
                    Debug.LogWarning($"[GameDataManager] DialogueSystem이 준비되지 않음: {variableName}, 기본값 {defaultValue} 반환");
                    return defaultValue;
                }
                
                return DialogueLua.GetVariable(variableName).asInt;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameDataManager] Variable 읽기 오류 {variableName}: {e.Message}, 기본값 {defaultValue} 반환");
                return defaultValue;
            }
        }

        /// <summary>
        /// 안전한 DialogueLua Variable 읽기 (문자열)
        /// </summary>
        string GetVariableStringSafe(string variableName, string defaultValue = "")
        {
            try
            {
                if (!IsDialogueSystemReady())
                {
                    Debug.LogWarning($"[GameDataManager] DialogueSystem이 준비되지 않음: {variableName}, 기본값 '{defaultValue}' 반환");
                    return defaultValue;
                }
                
                return DialogueLua.GetVariable(variableName).asString;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameDataManager] Variable 읽기 오류 {variableName}: {e.Message}, 기본값 '{defaultValue}' 반환");
                return defaultValue;
            }
        }

        /// <summary>
        /// 안전한 DialogueLua Variable 쓰기
        /// </summary>
        void SetVariableSafe(string variableName, object value)
        {
            try
            {
                if (!IsDialogueSystemReady())
                {
                    Debug.LogWarning($"[GameDataManager] DialogueSystem이 준비되지 않음: {variableName} 설정 실패");
                    return;
                }
                
                DialogueLua.SetVariable(variableName, value);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameDataManager] Variable 설정 오류 {variableName}: {e.Message}");
            }
        }

        #endregion
    }
}