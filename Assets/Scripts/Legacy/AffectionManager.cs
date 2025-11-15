using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;

namespace LoveAlgo.Data
{
    /// <summary>
    /// 러브알고리즘 히로인별 호감도 시스템 및 관계도 관리
    /// DialogueSystem Actor와 Variables를 활용한 완전한 캐릭터 관계 시스템
    /// </summary>
    public class AffectionManager : MonoBehaviour
    {
        #region Heroine Data Structure
        [System.Serializable]
        public class HeroineData
        {
            [Header("기본 정보")]
            public string heroineId;         // GameDataManager의 HEROINE_* 상수와 매칭
            public string displayName;       // 한국어 표시명
            public string englishName;       // 영어명 (DialogueSystem Actor명)
            
            [Header("공략 설정 (기획서 기준)")]
            public string preferredStat;     // 선호 스탯 (GameDataManager.STAT_* 상수)
            public int threshold;            // 공략 성공 임계치
            public string difficulty;        // 난이도 표시용
            
            [Header("특수 조건")]
            public bool hasSpecialCondition; // 특수 조건 여부 (로아용)
            public int minFatigueRequired;   // 최소 피로도 요구사항 (로아용)
            
            [Header("UI 정보")]
            public Color themeColor = Color.pink;      // 테마 색상
            public string description;                 // 캐릭터 설명
            
            // 런타임 계산 프로퍼티
            public int CurrentAffection => GameDataManager.Instance?.GetAffection(heroineId) ?? 0;
            public bool IsThresholdMet => CurrentAffection >= threshold;
            public bool IsSpecialConditionMet => !hasSpecialCondition || 
                (GameDataManager.Instance?.GetFatigue() ?? 0) >= minFatigueRequired;
        }
        #endregion

        #region Inspector Settings
        [Header("히로인 데이터 (기획서 기준)")]
        [SerializeField] private HeroineData[] heroines = new HeroineData[]
        {
            new HeroineData
            {
                heroineId = GameDataManager.HEROINE_YEEUN,
                displayName = "하예은",
                englishName = "Yeeun", // DialogueDatabase Actor 이름
                preferredStat = GameDataManager.STAT_HEALTH,
                threshold = 32,
                difficulty = "쉬움",
                themeColor = new Color(1f, 0.7f, 0.7f), // 연한 핑크
                description = "활발하고 건강한 체육과 선배"
            },
            new HeroineData
            {
                heroineId = GameDataManager.HEROINE_DAEUN,
                displayName = "서다은",
                englishName = "Daeun", // DialogueDatabase Actor 이름
                preferredStat = GameDataManager.STAT_INTELLIGENCE,
                threshold = 35,
                difficulty = "보통",
                themeColor = new Color(0.7f, 0.7f, 1f), // 연한 파랑
                description = "똑똑하고 차분한 도서관 도우미"
            },
            new HeroineData
            {
                heroineId = GameDataManager.HEROINE_BOM,
                displayName = "이봄",
                englishName = "Bom", // DialogueDatabase Actor 이름
                preferredStat = GameDataManager.STAT_SOCIAL,
                threshold = 39,
                difficulty = "조금 어려움",
                themeColor = new Color(0.7f, 1f, 0.7f), // 연한 초록
                description = "사교적이고 인기 많은 학생회 임원"
            },
            new HeroineData
            {
                heroineId = GameDataManager.HEROINE_HEEWON,
                displayName = "도희원",
                englishName = "Heewon", // DialogueDatabase Actor 이름
                preferredStat = GameDataManager.STAT_PERSISTENCE,
                threshold = 43,
                difficulty = "최고 난이도",
                themeColor = new Color(1f, 1f, 0.7f), // 연한 노랑
                description = "완벽주의적이고 까다로운 모범생"
            },
            new HeroineData
            {
                heroineId = GameDataManager.HEROINE_ROA,
                displayName = "로아",
                englishName = "Roa", // DialogueDatabase Actor 이름
                preferredStat = GameDataManager.STAT_FATIGUE, // 특수: 피로도
                threshold = 46,
                difficulty = "히든 (특수조건)",
                hasSpecialCondition = true,
                minFatigueRequired = 70,
                themeColor = new Color(0.9f, 0.7f, 1f), // 연한 보라
                description = "신비로운 히든 히로인"
            }
        };

        [Header("호감도 증가 설정")]
        [SerializeField] private int eventChoiceBonus = 8;        // 이벤트 선택 보너스
        [SerializeField] private int dialogueChoiceBonus = 2;     // 대화 선택 보너스  
        [SerializeField] private int statBonusMax = 3;           // 스탯 보너스 최대값
        [SerializeField] private int statBonusTied = 1;          // 스탯 공동 1등 보너스
        [SerializeField] private int recoveryBonus = 2;          // 선택 복구 보너스

        [Header("디버그 설정")]
        [SerializeField] private bool showDetailedLogs = true;
        [SerializeField] private bool enableAffectionEvents = true; // 호감도 기반 랜덤 이벤트
        #endregion

        #region Singleton Pattern
        public static AffectionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Unity Lifecycle
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
            
            while (DialogueManager.masterDatabase == null && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (DialogueManager.masterDatabase != null)
            {
                InitializeHeroineActors();
            }
            else
            {
                DebugLog("❌ DialogueSystem MasterDatabase가 준비되지 않았습니다. 초기화를 건너뜁니다.");
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// 새 게임 시작 시 호감도 초기화
        /// </summary>
        public void ResetForNewGame()
        {
            if (GameDataManager.Instance == null)
            {
                Debug.LogError("[AffectionManager] GameDataManager가 없습니다!");
                return;
            }

            // 모든 히로인 호감도를 0으로 초기화
            foreach (var heroine in heroines)
            {
                GameDataManager.Instance.SetAffection(heroine.heroineId, 0);
            }

            DebugLog("🔄 모든 히로인 호감도 초기화 완료");
        }

        /// <summary>
        /// 히로인 데이터 반환
        /// </summary>
        public HeroineData GetHeroineData(string heroineId)
        {
            return heroines.FirstOrDefault(h => h.heroineId == heroineId);
        }

        /// <summary>
        /// 모든 히로인 데이터 반환
        /// </summary>
        public HeroineData[] GetAllHeroines()
        {
            return heroines;
        }

        /// <summary>
        /// 이벤트 선택으로 인한 호감도 증가
        /// </summary>
        public void AddAffectionFromEvent(string heroineId, string eventType)
        {
            if (GameDataManager.Instance == null) return;

            var heroine = GetHeroineData(heroineId);
            if (heroine == null)
            {
                DebugLog($"❌ 존재하지 않는 히로인: {heroineId}");
                return;
            }

            int bonus = eventChoiceBonus;
            
            // 로아의 경우 모든 이벤트를 로아로 선택해야 함 (기획서 조건)
            if (heroineId == GameDataManager.HEROINE_ROA)
            {
                if (!CheckRoaEventConsistency())
                {
                    bonus = 0;
                    DebugLog("⚠️ 로아: 모든 이벤트를 로아로 선택해야 호감도가 증가합니다!");
                }
            }

            GameDataManager.Instance.AddAffection(heroineId, bonus);
            
            DebugLog($"💕 {heroine.displayName} 이벤트 선택 보너스: +{bonus} " +
                    $"(총 {GameDataManager.Instance.GetAffection(heroineId)}/{heroine.threshold})");

            // 호감도 이벤트 트리거
            if (enableAffectionEvents)
            {
                CheckAffectionMilestone(heroineId);
            }
        }

        /// <summary>
        /// 대화 선택으로 인한 호감도 증가
        /// </summary>
        public void AddAffectionFromDialogue(string heroineId, int customBonus = -1)
        {
            if (GameDataManager.Instance == null) return;

            var heroine = GetHeroineData(heroineId);
            if (heroine == null) return;

            int bonus = customBonus > 0 ? customBonus : dialogueChoiceBonus;
            GameDataManager.Instance.AddAffection(heroineId, bonus);

            DebugLog($"💬 {heroine.displayName} 대화 보너스: +{bonus}");

            if (enableAffectionEvents)
            {
                CheckAffectionMilestone(heroineId);
            }
        }

        /// <summary>
        /// 선물 증정으로 인한 호감도 증가 (기획서 기준 계층별 점수)
        /// </summary>
        public void AddAffectionFromGift(string heroineId, int giftPrice, string eventPhase)
        {
            if (GameDataManager.Instance == null) return;

            var heroine = GetHeroineData(heroineId);
            if (heroine == null) return;

            int bonus = CalculateGiftBonus(giftPrice, eventPhase);
            GameDataManager.Instance.AddAffection(heroineId, bonus);

            DebugLog($"🎁 {heroine.displayName} 선물 보너스 ({giftPrice:N0}원): +{bonus}");
        }

        /// <summary>
        /// 스탯 보너스 계산 및 적용 (고백 시점)
        /// </summary>
        public void ApplyStatBonus(string heroineId)
        {
            if (GameDataManager.Instance == null) return;

            var heroine = GetHeroineData(heroineId);
            if (heroine == null || heroine.heroineId == GameDataManager.HEROINE_ROA) return;

            // 해당 히로인 이벤트 참여 여부 확인
            if (!HasParticipatedInHeroineEvent(heroineId))
            {
                DebugLog($"⚠️ {heroine.displayName}: 이벤트 미참여로 스탯 보너스 없음");
                return;
            }

            int bonus = CalculateStatBonus(heroine.preferredStat);
            if (bonus > 0)
            {
                GameDataManager.Instance.AddAffection(heroineId, bonus);
                DebugLog($"💪 {heroine.displayName} 스탯 보너스: +{bonus}");
            }
        }

        /// <summary>
        /// 로아 피로도 보너스 적용
        /// </summary>
        public void ApplyRoaFatigueBonus()
        {
            if (GameDataManager.Instance == null) return;

            int fatigue = GameDataManager.Instance.GetFatigue();
            int bonus = 0;

            if (fatigue >= 90) bonus = 10;
            else if (fatigue >= 80) bonus = 6;
            else if (fatigue >= 70) bonus = 3;

            if (bonus > 0)
            {
                GameDataManager.Instance.AddAffection(GameDataManager.HEROINE_ROA, bonus);
                DebugLog($"😴 로아 피로도 보너스 (피로도 {fatigue}): +{bonus}");
            }
        }

        /// <summary>
        /// 선택 복구 보너스 적용 (3차 이벤트에서 다른 히로인을 선택했을 때)
        /// </summary>
        public void ApplyRecoveryBonus(string heroineId)
        {
            if (GameDataManager.Instance == null) return;
            if (heroineId == GameDataManager.HEROINE_ROA) return; // 로아는 예외

            var heroine = GetHeroineData(heroineId);
            if (heroine == null) return;

            // 1,2차 이벤트에서 다른 히로인을 선택했는지 확인
            string firstChoice = GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_FIRST_CHOICE);
            string secondChoice = GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_SECOND_CHOICE);

            if ((firstChoice != heroineId && firstChoice != GameDataManager.HEROINE_NONE) ||
                (secondChoice != heroineId && secondChoice != GameDataManager.HEROINE_NONE))
            {
                GameDataManager.Instance.AddAffection(heroineId, recoveryBonus);
                DebugLog($"🔄 {heroine.displayName} 선택 복구 보너스: +{recoveryBonus}");
            }
        }

        /// <summary>
        /// 현재 공략 가능한 히로인들 반환
        /// </summary>
        public List<HeroineData> GetAvailableHeroines()
        {
            var available = new List<HeroineData>();

            foreach (var heroine in heroines)
            {
                if (heroine.IsSpecialConditionMet)
                {
                    available.Add(heroine);
                }
            }

            return available;
        }

        /// <summary>
        /// 최고 호감도 히로인 반환
        /// </summary>
        public HeroineData GetTopAffectionHeroine()
        {
            HeroineData topHeroine = null;
            int maxAffection = -1;

            foreach (var heroine in heroines)
            {
                if (heroine.CurrentAffection > maxAffection && heroine.IsSpecialConditionMet)
                {
                    maxAffection = heroine.CurrentAffection;
                    topHeroine = heroine;
                }
            }

            return topHeroine;
        }

        /// <summary>
        /// 공략 성공 가능한 히로인들 반환
        /// </summary>
        public List<HeroineData> GetConquestableHeroines()
        {
            return heroines.Where(h => h.IsThresholdMet && h.IsSpecialConditionMet).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// DialogueSystem Actor 초기화
        /// </summary>
        void InitializeHeroineActors()
        {
            if (DialogueManager.masterDatabase == null)
            {
                DebugLog("❌ DialogueSystem MasterDatabase가 없습니다!");
                return;
            }

            foreach (var heroine in heroines)
            {
                // DialogueSystem Actor가 있는지 확인
                var actor = DialogueManager.masterDatabase.GetActor(heroine.englishName);
                if (actor == null)
                {
                    // Actor가 없어도 게임 진행에는 문제없음 (나중에 DialogueDatabase에서 생성)
                    // 경고 메시지 제거 (불필요한 로그 방지)
                }
                else
                {
                    // DebugLog($"✅ DialogueSystem Actor 확인: {heroine.englishName}");
                }
            }

            // DebugLog("🎭 히로인 Actor 시스템 초기화 완료");
        }

        /// <summary>
        /// 선물 가격에 따른 보너스 계산 (기획서 기준)
        /// </summary>
        int CalculateGiftBonus(int price, string eventPhase)
        {
            bool isThirdEvent = eventPhase.Contains("Third") || eventPhase.Contains("3차");
            
            if (price <= 10000) // 저가 (1만 이하)
                return isThirdEvent ? 2 : 1;
            else if (price <= 30000) // 중급 (1~3만대)
                return isThirdEvent ? 3 : 2;
            else if (price <= 70000) // 고급 (4~7만대)
                return isThirdEvent ? 4 : 3;
            else // 최고급 (8만 이상)
                return isThirdEvent ? 5 : 3;
        }

        /// <summary>
        /// 스탯 보너스 계산
        /// </summary>
        int CalculateStatBonus(string preferredStat)
        {
            if (GameDataManager.Instance == null) return 0;

            int health = GameDataManager.Instance.GetHealth();
            int intelligence = GameDataManager.Instance.GetIntelligence();
            int social = GameDataManager.Instance.GetSocial();
            int persistence = GameDataManager.Instance.GetPersistence();

            int preferredValue = 0;
            switch (preferredStat)
            {
                case GameDataManager.STAT_HEALTH: preferredValue = health; break;
                case GameDataManager.STAT_INTELLIGENCE: preferredValue = intelligence; break;
                case GameDataManager.STAT_SOCIAL: preferredValue = social; break;
                case GameDataManager.STAT_PERSISTENCE: preferredValue = persistence; break;
            }

            int maxStat = Mathf.Max(health, intelligence, social, persistence);
            
            if (preferredValue == maxStat)
            {
                // 선호 스탯이 1등인지 공동 1등인지 확인
                int maxCount = 0;
                if (health == maxStat) maxCount++;
                if (intelligence == maxStat) maxCount++;
                if (social == maxStat) maxCount++;
                if (persistence == maxStat) maxCount++;

                return maxCount == 1 ? statBonusMax : statBonusTied;
            }

            return 0;
        }

        /// <summary>
        /// 로아 이벤트 일관성 확인
        /// </summary>
        bool CheckRoaEventConsistency()
        {
            if (GameDataManager.Instance == null) return false;

            // 지금까지의 모든 이벤트 선택을 확인
            string[] eventChoices = {
                GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_FIRST_CHOICE),
                GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_FESTIVAL_CHOICE),
                GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_SECOND_CHOICE),
                GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_MT_CHOICE),
                GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_THIRD_CHOICE)
            };

            foreach (string choice in eventChoices)
            {
                if (choice != GameDataManager.HEROINE_NONE && choice != GameDataManager.HEROINE_ROA)
                {
                    return false; // 로아가 아닌 다른 선택이 있음
                }
            }

            return true;
        }

        /// <summary>
        /// 히로인 이벤트 참여 여부 확인
        /// </summary>
        bool HasParticipatedInHeroineEvent(string heroineId)
        {
            if (GameDataManager.Instance == null) return false;

            // 1차, 2차, 3차 개인 이벤트 중 하나라도 참여했는지 확인
            return GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_FIRST_CHOICE) == heroineId ||
                   GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_SECOND_CHOICE) == heroineId ||
                   GameDataManager.Instance.GetEventChoice(GameDataManager.EVENT_THIRD_CHOICE) == heroineId;
        }

        /// <summary>
        /// 호감도 마일스톤 확인 및 이벤트 트리거
        /// </summary>
        void CheckAffectionMilestone(string heroineId)
        {
            var heroine = GetHeroineData(heroineId);
            if (heroine == null) return;

            int affection = heroine.CurrentAffection;
            
            // 특정 호감도 구간에서 특별 메시지 (메신저 시스템 대신 콘솔 로그)
            if (affection == 10)
            {
                Debug.Log($"📱 [메신저] {heroine.displayName}: '오늘 고마웠어! 😊'");
            }
            else if (affection == 20)
            {
                Debug.Log($"📱 [메신저] {heroine.displayName}: '요즘 자주 보게 되네~ 좋아 ☺️'");
            }
            else if (affection >= heroine.threshold && affection - dialogueChoiceBonus < heroine.threshold)
            {
                Debug.Log($"📱 [메신저] {heroine.displayName}: '뭔가... 특별한 감정이 생기는 것 같아... 💕'");
                DebugLog($"🎯 {heroine.displayName} 공략 임계치 달성! ({affection}/{heroine.threshold})");
            }
        }

        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        void DebugLog(string message)
        {
            if (showDetailedLogs)
            {
                Debug.Log($"[AffectionManager] {message}");
            }
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// 모든 히로인의 현재 상태 출력
        /// </summary>
        public void LogAllHeroineStatus()
        {
            string report = "💕 히로인별 호감도 현황:\n";
            
            foreach (var heroine in heroines)
            {
                string status = heroine.IsThresholdMet ? "✅ 공략가능" : "❌ 호감도부족";
                string special = heroine.hasSpecialCondition ? 
                    (heroine.IsSpecialConditionMet ? " [특수조건충족]" : " [특수조건미충족]") : "";
                
                report += $"  • {heroine.displayName}: {heroine.CurrentAffection}/{heroine.threshold} {status}{special}\n";
            }

            DebugLog($"\n{report}");
        }

        /// <summary>
        /// 특정 히로인의 상세 정보 출력
        /// </summary>
        public void LogHeroineDetails(string heroineId)
        {
            var heroine = GetHeroineData(heroineId);
            if (heroine == null) return;

            string report = $"👤 {heroine.displayName} 상세 정보:\n";
            report += $"  📊 호감도: {heroine.CurrentAffection}/{heroine.threshold}\n";
            report += $"  💪 선호스탯: {GetStatDisplayName(heroine.preferredStat)}\n";
            report += $"  🎯 난이도: {heroine.difficulty}\n";
            report += $"  ✅ 공략가능: {(heroine.IsThresholdMet ? "예" : "아니오")}\n";
            
            if (heroine.hasSpecialCondition)
            {
                report += $"  🔮 특수조건: 피로도 {heroine.minFatigueRequired}+ (현재: {GameDataManager.Instance?.GetFatigue()})\n";
            }

            DebugLog(report);
        }

        string GetStatDisplayName(string statKey)
        {
            switch (statKey)
            {
                case GameDataManager.STAT_HEALTH: return "체력";
                case GameDataManager.STAT_INTELLIGENCE: return "지성";
                case GameDataManager.STAT_SOCIAL: return "사교성";
                case GameDataManager.STAT_PERSISTENCE: return "끈기";
                case GameDataManager.STAT_FATIGUE: return "피로도";
                default: return "알수없음";
            }
        }

        #endregion

        #region Editor Utilities

        [ContextMenu("모든 히로인 상태 출력")]
        void EditorLogAllHeroines()
        {
            LogAllHeroineStatus();
        }

        [ContextMenu("하예은 상세 정보")]
        void EditorLogHaYeun()
        {
            LogHeroineDetails(GameDataManager.HEROINE_YEEUN);
        }

        [ContextMenu("테스트 호감도 추가")]
        void EditorTestAffection()
        {
            AddAffectionFromEvent(GameDataManager.HEROINE_YEEUN, "TestEvent");
            AddAffectionFromDialogue(GameDataManager.HEROINE_DAEUN);
        }

        #endregion
    }
}