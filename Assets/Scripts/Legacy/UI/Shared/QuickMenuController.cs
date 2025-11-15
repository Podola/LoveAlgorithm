using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using LoveAlgo.Systems;
using PixelCrushers.DialogueSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

namespace LoveAlgo.UI.Shared
{
    /// <summary>
    /// 빠른 메뉴 토글 및 뒤로가기 입력을 관리한다.
    /// PanelManager / PopupManager 연동을 통해 팝업/패널/메신저를 순차로 닫는다.
    /// </summary>
    public class QuickMenuController : MonoBehaviour
    {
        [Header("Toggle & Panels")]
        [SerializeField] private Button toggleButton = default!;
        [SerializeField] private GameObject quickMenuPanel = default!;
        [SerializeField] private CanvasGroup quickMenuCanvasGroup = default!;

        [Header("Messenger")]
        [SerializeField] private GameObject messengerCanvasObject = default!;
        [SerializeField] private CanvasGroup messengerCanvasGroup = default!;
        [SerializeField] private GameObject messengerRoot = default!;

        [Header("Quick Menu Buttons")]
        [SerializeField] private Button phoneButton = default!;
        [SerializeField] private Button messengerButton = default!;
        [SerializeField] private Button saveButton = default!;
        [SerializeField] private Button loadButton = default!;
        [SerializeField] private Button settingsButton = default!;
        [SerializeField] private Button titleButton = default!;
        [SerializeField] private Button exitButton = default!;

        [Header("Optional")]
        [SerializeField] private Button backButton = default!;

        private PanelManager? panelManager;
        private PopupManager? popupManager;

        private bool quickMenuOpen;
        private bool messengerOpen;
        private bool quickMenuAvailable;
        private bool phoneButtonAvailable = true;
        private bool freeActionActive;

        public bool IsQuickMenuOpen => quickMenuOpen;
        public bool IsMessengerOpen => messengerOpen;

        private void Awake()
        {
            panelManager = PanelManager.Instance;
            popupManager = PopupManager.Instance;

            HideQuickMenuImmediate();
            HideMessengerImmediate();

            if (toggleButton != null)
            {
                toggleButton.gameObject.SetActive(quickMenuAvailable);
            }

            UpdatePhoneButtonState();
        }

        private void OnEnable()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleButtonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackAction);
            }

            if (messengerButton != null)
            {
                messengerButton.onClick.AddListener(OnMessengerButtonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.AddListener(() =>
                {
                    ShowPanel(() => panelManager?.ShowSaveLoad(), "저장/불러오기 패널 열림");
                });
            }

            if (loadButton != null)
            {
                loadButton.onClick.AddListener(() =>
                {
                    ShowPanel(() => panelManager?.ShowSaveLoad(), "불러오기 패널 열림");
                });
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(() =>
                {
                    ShowPanel(() => panelManager?.ShowSettings(), "환경설정 패널 열림");
                });
            }

            if (titleButton != null)
            {
                titleButton.onClick.AddListener(ConfirmReturnToTitle);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(ConfirmExitGame);
            }
        }

        private void OnDisable()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleButtonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackAction);
            }

            if (messengerButton != null)
            {
                messengerButton.onClick.RemoveListener(OnMessengerButtonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveAllListeners();
            }

            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners();
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
            }

            if (titleButton != null)
            {
                titleButton.onClick.RemoveAllListeners();
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
            }
        }

        private void Update()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                HandleBackAction();
            }
        }

        private void OnToggleButtonClicked()
        {
            if (messengerOpen)
            {
                HideMessenger();
                return;
            }

            if (quickMenuOpen)
            {
                HideQuickMenu();
            }
            else
            {
                ShowQuickMenu();
            }
        }

        private void OnMessengerButtonClicked()
        {
            ShowMessenger();
        }

        private void ShowQuickMenu()
        {
            if (quickMenuPanel == null || quickMenuCanvasGroup == null) return;
            if (!quickMenuAvailable) return;

            quickMenuPanel.SetActive(true);
            quickMenuCanvasGroup.alpha = 1f;
            quickMenuCanvasGroup.interactable = true;
            quickMenuCanvasGroup.blocksRaycasts = true;
            quickMenuOpen = true;
            UpdatePhoneButtonState();
        }

        private void HideQuickMenu()
        {
            if (quickMenuPanel == null || quickMenuCanvasGroup == null) return;

            quickMenuCanvasGroup.alpha = 0f;
            quickMenuCanvasGroup.interactable = false;
            quickMenuCanvasGroup.blocksRaycasts = false;
            quickMenuPanel.SetActive(false);
            quickMenuOpen = false;
            UpdatePhoneButtonState();
        }

        private void HideQuickMenuImmediate()
        {
            if (quickMenuPanel == null || quickMenuCanvasGroup == null) return;

            quickMenuCanvasGroup.alpha = 0f;
            quickMenuCanvasGroup.interactable = false;
            quickMenuCanvasGroup.blocksRaycasts = false;
            quickMenuPanel.SetActive(false);
            quickMenuOpen = false;
            UpdatePhoneButtonState();
        }

        private void ShowMessenger()
        {
            if (messengerCanvasObject != null)
            {
                messengerCanvasObject.SetActive(true);
            }

            if (messengerCanvasGroup != null)
            {
                messengerCanvasGroup.alpha = 1f;
                messengerCanvasGroup.interactable = true;
                messengerCanvasGroup.blocksRaycasts = true;
            }

            if (messengerRoot != null)
            {
                messengerRoot.SetActive(true);
            }

            messengerOpen = true;
            HideQuickMenu();
            UpdatePhoneButtonState();
        }

        private void HideMessenger()
        {
            if (!messengerOpen) return;

            HideMessengerImmediate();
        }

        private void HideMessengerImmediate()
        {
            if (messengerCanvasGroup != null)
            {
                messengerCanvasGroup.alpha = 0f;
                messengerCanvasGroup.interactable = false;
                messengerCanvasGroup.blocksRaycasts = false;
            }

            if (messengerRoot != null)
            {
                messengerRoot.SetActive(false);
            }

            if (messengerCanvasObject != null)
            {
                messengerCanvasObject.SetActive(false);
            }

            messengerOpen = false;
            UpdatePhoneButtonState();
        }

        private void ShowPanel(System.Action? panelAction, string logMessage)
        {
            HideQuickMenu();
            HideMessenger();

            panelAction?.Invoke();
            Debug.Log($"[QuickMenuController] {logMessage}");
            UpdatePhoneButtonState();
        }

        private void ConfirmReturnToTitle()
        {
            HideQuickMenu();
            HideMessenger();

            popupManager?.ShowConfirm(
                "메인 타이틀",
                "저장되지 않은 내용이 있을 수 있습니다.\n메인 타이틀로 돌아가시겠습니까?",
                () =>
                {
                    Debug.Log("[QuickMenuController] 메인 타이틀로 이동");
                    SceneManager.LoadScene("Title");
                },
                onCancel: null,
                useDim: true);
        }

        private void ConfirmExitGame()
        {
            HideQuickMenu();
            HideMessenger();

            popupManager?.ShowConfirm(
                "게임 종료",
                "게임을 종료하시겠습니까?",
                () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                },
                onCancel: null,
                useDim: true);
            UpdatePhoneButtonState();
        }

        public void HandleBackAction()
        {
            if (popupManager != null && popupManager.HasActivePopup)
            {
                popupManager.CloseCurrentPopup();
                UpdatePhoneButtonState();
                return;
            }

            if (messengerOpen)
            {
                HideMessenger();
                UpdatePhoneButtonState();
                return;
            }

            if (panelManager != null && panelManager.HasActivePanel())
            {
                panelManager.HideAllPanels();
                UpdatePhoneButtonState();
                return;
            }

            if (quickMenuOpen)
            {
                HideQuickMenu();
                UpdatePhoneButtonState();
            }
        }

        public void SetQuickMenuAvailable(bool available)
        {
            quickMenuAvailable = available;
            freeActionActive = available;

            if (toggleButton != null)
            {
                toggleButton.gameObject.SetActive(available);
            }

            if (!available)
            {
                HideQuickMenu();
            }

            UpdatePhoneButtonState();
        }

        public void SetPhoneButtonAvailable(bool available)
        {
            phoneButtonAvailable = available;
            UpdatePhoneButtonState();
        }

        private void UpdatePhoneButtonState()
        {
            if (phoneButton == null) return;

            bool hasPopup = popupManager != null && popupManager.HasActivePopup;
            bool hasPanel = panelManager != null && panelManager.HasActivePanel();
            bool shouldShow = phoneButtonAvailable && !freeActionActive && !messengerOpen && !quickMenuOpen && !hasPopup && !hasPanel;

            if (phoneButton.gameObject.activeSelf != shouldShow)
            {
                phoneButton.gameObject.SetActive(shouldShow);
                Debug.Log($"[QuickMenuController] PhoneButton 상태 변경: {shouldShow} (available={phoneButtonAvailable}, freeAction={freeActionActive}, messenger={messengerOpen}, quickMenu={quickMenuOpen}, popup={hasPopup}, panel={hasPanel})");
            }
        }

        public static QuickMenuController? FindFirstInstance(bool includeInactive)
        {
            return PixelCrushers.DialogueSystem.VNFrameworkFindUtility.FindFirstInstance<QuickMenuController>(includeInactive);
        }
    }
}

