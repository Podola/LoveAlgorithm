using UnityEngine;
using LoveAlgo.UI.Gameplay;

namespace LoveAlgo.UI.Shared
{
    /// <summary>
    /// Panel 관리자 (DDOL)
    /// Panel Canvas (Sort: 50) 관리: Settings, SaveLoad, Extra, GameplayPanel
    /// 모든 Panel은 필요 시 활성화되는 큰 UI (Popup보다 큼)
    /// SystemsManager가 DDOL 처리
    /// </summary>
    public class PanelManager : MonoBehaviour
    {
        [Header("Panel Canvas (Sort: 50) - DDOL")]
        [SerializeField] private Canvas panelCanvas;
        
        [Header("공유 Panel들 (타이틀 & 인게임)")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject saveLoadPanel;
        [SerializeField] private GameObject extraPanel;
        
        [Header("인게임 Panel들")]
        [SerializeField] private GameObject logPanel;
        
        [Header("Panel 전용 Dim")]
        [SerializeField] private GameObject dimBackground;
        
        private static PanelManager instance;
        public static PanelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PanelManager>();
                    if (instance == null && Application.isPlaying)
                    {
                        Debug.LogWarning("[PanelManager] 인스턴스를 찾을 수 없습니다. 씬에 PanelManager가 있는지 확인하세요.");
                    }
                }
                return instance;
            }
            private set => instance = value;
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                // DontDestroyOnLoad는 SystemsManager가 처리
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Panel Canvas 설정
            SetupPanelCanvas();

            // 초기 상태: 모든 Panel 비활성화
            HideAllPanels();

            // UIManager에 등록
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterPanelManager(this);
            }

            Debug.Log("[PanelManager] Initialized (Settings/SaveLoad/Extra/Log/GameplayPanel)");
        }
        
        #region Public Methods
        
        /// <summary>
        /// 설정 패널 표시
        /// </summary>
        public void ShowSettings()
        {
            ShowPanel(settingsPanel, "설정 패널");
        }
        
        /// <summary>
        /// 세이브/로드 패널 표시
        /// </summary>
        public void ShowSaveLoad()
        {
            ShowPanel(saveLoadPanel, "세이브/로드 패널");
        }
        
        /// <summary>
        /// Extra 패널 표시 (타이틀 & 인게임)
        /// </summary>
        public void ShowExtra()
        {
            ShowPanel(extraPanel, "Extra 패널");
        }
        
        /// <summary>
        /// 로그 패널 표시 (인게임 전용)
        /// </summary>
        public void ShowLog()
        {
            ShowPanel(logPanel, "로그 패널");
        }
        
      

        /// <summary>
        /// 활성화된 Panel이 존재하는지 여부
        /// </summary>
        public bool HasActivePanel()
        {
            if (settingsPanel != null && settingsPanel.activeSelf) return true;
            if (saveLoadPanel != null && saveLoadPanel.activeSelf) return true;
            if (extraPanel != null && extraPanel.activeSelf) return true;
            if (logPanel != null && logPanel.activeSelf) return true;
            return false;
        }
        
        
        
        /// <summary>
        /// 모든 패널 숨기기
        /// </summary>
        public void HideAllPanels()
        {
            HideAllPanels(null, true);
        }
        
        #endregion
        
        #region Canvas Setup
        
        /// <summary>
        /// Panel Canvas 설정 (Sorting Order: 50)
        /// </summary>
        void SetupPanelCanvas()
        {
            if (panelCanvas != null)
            {
                panelCanvas.sortingOrder = 50;
                panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log("[PanelManager] Panel Canvas setup (Sort: 50)");
            }
            else
            {
                Debug.LogError("[PanelManager] Panel Canvas not assigned!");
            }
        }
        
        #endregion
        
        #region Dim Background
        
        void ShowDimBackground()
        {
            if (dimBackground != null)
            {
                dimBackground.SetActive(true);
            }
        }
        
        void HideDimBackground()
        {
            if (dimBackground != null)
            {
                dimBackground.SetActive(false);
            }
        }
        
        #endregion

        private void ShowPanel(GameObject panel, string label)
        {
            if (panel == null)
            {
                Debug.LogWarning($"[PanelManager] {label} 오브젝트가 할당되지 않았습니다.");
                return;
            }

            HideAllPanels(panel, false);

            panel.SetActive(true);
            var sliding = panel.GetComponent<SlidingPanel>();
            if (sliding != null)
            {
                sliding.SlideIn();
            }

            ShowDimBackground();
            Debug.Log($"[PanelManager] {label} 열림");
        }

        private void HideAllPanels(GameObject exceptPanel, bool hideDim)
        {
            HidePanel(settingsPanel, exceptPanel);
            HidePanel(saveLoadPanel, exceptPanel);
            HidePanel(extraPanel, exceptPanel);
            HidePanel(logPanel, exceptPanel);

            if (hideDim)
            {
                HideDimBackground();
            }
        }

        private void HidePanel(GameObject panel, GameObject except)
        {
            if (panel == null || panel == except) return;

            if (!panel.activeSelf) return;

            var sliding = panel.GetComponent<SlidingPanel>();
            if (sliding != null)
            {
                sliding.SlideOut(() => panel.SetActive(false));
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }
}

