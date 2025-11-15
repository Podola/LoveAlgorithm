using UnityEngine;

namespace LoveAlgo.UI.Shared
{
    /// <summary>
    /// UI 통합 관리자 (씬별 독립)
    /// PanelManager와 PopupManager를 통합 관리하며 대화 진행 차단 여부를 결정한다.
    /// 각 씬마다 독립적으로 존재하며 씬 전환 시 자동 해제된다.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("등록된 매니저들")]
        [SerializeField] private PanelManager panelManager;
        [SerializeField] private PopupManager popupManager;

        // 싱글톤 (씬별 독립)
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UIManager>();
                    if (instance == null && Application.isPlaying)
                    {
                        Debug.LogWarning("[UIManager] 인스턴스를 찾을 수 없습니다. 씬에 UIManager가 있는지 확인하세요.");
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
                // 씬별 독립이므로 DontDestroyOnLoad 하지 않음
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// PanelManager 등록
        /// </summary>
        public void RegisterPanelManager(PanelManager manager)
        {
            if (manager != null)
            {
                panelManager = manager;
                Debug.Log("[UIManager] PanelManager 등록 완료");
            }
            else
            {
                Debug.LogWarning("[UIManager] 등록하려는 PanelManager가 null입니다.");
            }
        }

        /// <summary>
        /// PopupManager 등록
        /// </summary>
        public void RegisterPopupManager(PopupManager manager)
        {
            if (manager != null)
            {
                popupManager = manager;
                Debug.Log("[UIManager] PopupManager 등록 완료");
            }
            else
            {
                Debug.LogWarning("[UIManager] 등록하려는 PopupManager가 null입니다.");
            }
        }

        /// <summary>
        /// 대화 진행을 차단할지 여부를 반환
        /// Panel이나 Popup이 활성화되어 있으면 true
        /// </summary>
        public bool ShouldBlockDialogueProgress()
        {
            // PanelManager가 활성화된 Panel이 있는지 확인
            if (panelManager != null && panelManager.HasActivePanel())
            {
                return true;
            }

            // PopupManager가 활성화된 Popup이 있는지 확인
            if (popupManager != null && popupManager.HasActivePopup)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 현재 활성화된 UI가 있는지 확인
        /// </summary>
        public bool HasActiveUI()
        {
            return ShouldBlockDialogueProgress();
        }

        /// <summary>
        /// 모든 UI 숨기기 (긴급 상황용)
        /// </summary>
        public void HideAllUI()
        {
            if (panelManager != null)
            {
                panelManager.HideAllPanels();
            }

            if (popupManager != null)
            {
                popupManager.CloseCurrentPopup();
            }
        }
    }
}
