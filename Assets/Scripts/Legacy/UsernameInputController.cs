/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using PixelCrushers.DialogueSystem;
using LoveAlgo.UI.Gameplay;
using LoveAlgo.Data;
using LoveAlgo.Systems;
using LoveAlgo.UI.Shared;
using LoveAlgo.VN;

namespace LoveAlgo.UI.Gameplay
{
    /// <summary>
    /// Gameplay 씬 진입 시 유저네임 입력 UI를 관리하고, 입력 완료 후 Story_Demo 대화를 시작합니다.
    /// </summary>
    public class UsernameInputController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject usernameInputPanel;
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Gameplay References")]
        [SerializeField] private GameplayPanel gameplayPanel;
        [SerializeField] private GameplaySceneManager gameplaySceneManager;

    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;

        private void Start()
        {
            InitializeUI();
            
            // DialogueSystem이 준비될 때까지 대기 후 처리
            StartCoroutine(WaitForDialogueSystemAndCheckUsername());
        }

        /// <summary>
        /// DialogueSystem이 준비될 때까지 대기하고 유저네임 입력 여부 확인
        /// </summary>
        private IEnumerator WaitForDialogueSystemAndCheckUsername()
        {
            // DialogueSystem이 준비될 때까지 최대 5초 대기
            float timeout = 5f;
            float elapsed = 0f;
            
            while ((DialogueManager.instance == null || DialogueManager.masterDatabase == null) && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (DialogueManager.instance == null || DialogueManager.masterDatabase == null)
            {
                Debug.LogError("[UsernameInputController] DialogueSystem이 준비되지 않았습니다!");
                yield break;
            }

            EnsureGameplayReferences();

            // GameplayPanel 비활성화 (대화 중에는 숨김)
            if (gameplayPanel != null)
            {
                gameplayPanel.gameObject.SetActive(false);
                Debug.Log("[UsernameInputController] GameplayPanel 비활성화 (대화 대기 중)");
            }

            // 새 게임인 경우에만 유저네임 입력 표시
            if (showOnStart && ShouldShowUsernameInput())
            {
                ShowUsernameInput();
                Debug.Log("[UsernameInputController] 유저네임 입력 UI 표시됨 - 사용자 입력 대기 중");
            }
            else
            {
                // 이미 유저네임이 설정되어 있으면 바로 Story_Demo 시작
                Debug.Log("[UsernameInputController] 유저네임이 이미 설정되어 있음 - 바로 Story_Demo 시작");
                OnUsernameInputCompleted();
            }
        }

        /// <summary>
        /// 유저네임 입력 UI를 표시해야 하는지 확인
        /// </summary>
        private bool ShouldShowUsernameInput()
        {
            // DialogueSystem이 준비되지 않았으면 표시하지 않음
            if (DialogueManager.instance == null || DialogueManager.masterDatabase == null)
            {
                return false;
            }

            // PlayerName이 이미 설정되어 있으면 표시하지 않음
            string currentPlayerName = DialogueLua.GetVariable("PlayerName").asString;
            if (!string.IsNullOrEmpty(currentPlayerName))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // UI 요소가 없으면 생성
            if (usernameInputPanel == null)
            {
                CreateUsernameInputUI();
            }

            // 버튼 이벤트 연결
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelButtonClicked);

            // InputField 검증 설정
            if (usernameInputField != null)
            {
                usernameInputField.onValidateInput += ValidateUsernameInput;
                usernameInputField.onSubmit.AddListener(OnUsernameSubmitted);
                
                // 플레이스홀더 설정
                var placeholder = usernameInputField.placeholder as TextMeshProUGUI;
                if (placeholder != null)
                {
                    placeholder.text = "이름을 입력하세요 (2-10자)";
                }
            }

            // 초기에는 숨김 (WaitForDialogueSystemAndCheckUsername에서 표시 여부 결정)
            if (usernameInputPanel != null)
                usernameInputPanel.SetActive(false);

            // 에러 텍스트 숨김
            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }

        /// <summary>
        /// GameplayPanel 참조 가져오기 (비활성화용)
        /// </summary>
        private GameplayPanel GetGameplayPanel()
        {
            EnsureGameplayReferences();
            return gameplayPanel;
        }

        private void EnsureGameplayReferences()
        {
            if (gameplayPanel == null)
            {
                gameplayPanel = PixelCrushers.DialogueSystem.VNFrameworkFindUtility.FindFirstInstance<GameplayPanel>(includeInactive: true);
                if (gameplayPanel == null)
                {
                    Debug.LogWarning("[UsernameInputController] GameplayPanel을 찾을 수 없습니다. GameplayRoot 구성을 확인하세요.");
                }
            }

            if (gameplaySceneManager == null)
            {
                gameplaySceneManager = PixelCrushers.DialogueSystem.VNFrameworkFindUtility.FindFirstInstance<GameplaySceneManager>(includeInactive: true);
                if (gameplaySceneManager == null)
                {
                    Debug.LogWarning("[UsernameInputController] GameplaySceneManager 참조가 비어 있습니다. GameplayRoot 구성을 확인하세요.");
                }
            }
        }

        /// <summary>
        /// 유저네임 입력 UI 표시
        /// </summary>
        public void ShowUsernameInput()
        {
            if (usernameInputPanel == null)
            {
                Debug.LogError("[UsernameInputController] UsernameInputPanel이 설정되지 않았습니다!");
                return;
            }

            // GameplayPanel 비활성화 (유저네임 입력 중)
            EnsureGameplayReferences();

            if (gameplayPanel != null)
            {
                gameplayPanel.gameObject.SetActive(false);
            }

            // DialogueCanvas는 GameplaySceneManager가 관리하므로 여기서는 건드리지 않음

            // 유저네임 입력 UI 표시
            usernameInputPanel.SetActive(true);

            // InputField 포커스
            if (usernameInputField != null)
            {
                usernameInputField.text = "";
                usernameInputField.Select();
                usernameInputField.ActivateInputField();
            }

            // 에러 텍스트 숨김
            if (errorText != null)
                errorText.gameObject.SetActive(false);

            Debug.Log("[UsernameInputController] 유저네임 입력 UI 표시");
        }

        /// <summary>
        /// 유저네임 입력 UI 숨김
        /// </summary>
        public void HideUsernameInput()
        {
            if (usernameInputPanel != null)
                usernameInputPanel.SetActive(false);
        }

        /// <summary>
        /// 입력 문자 검증 (문자 단위)
        /// </summary>
        private char ValidateUsernameInput(string text, int charIndex, char addedChar)
        {
            char filteredChar;
            UsernameValidationError error;
            
            filteredChar = LoveAlgoUsernameRules.FilterCharacter(text, charIndex, addedChar, out error);
            
            if (error != UsernameValidationError.None)
            {
                ShowError(GetErrorMessage(error));
                return '\0';
            }

            // 에러 숨김
            if (errorText != null)
                errorText.gameObject.SetActive(false);

            return filteredChar;
        }

        /// <summary>
        /// 유저네임 제출 (Enter 키 또는 확인 버튼)
        /// </summary>
        private void OnUsernameSubmitted(string username)
        {
            OnConfirmButtonClicked();
        }

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            if (usernameInputField == null)
            {
                Debug.LogError("[UsernameInputController] UsernameInputField가 설정되지 않았습니다!");
                return;
            }

            string username = usernameInputField.text.Trim();

            // 전체 검증
            var validationResult = LoveAlgoUsernameRules.Validate(username);
            
            if (!validationResult.IsValid)
            {
                ShowError(GetErrorMessage(validationResult.Error));
                return;
            }

            // 유저네임 저장
            SetPlayerName(username);

            // UI 숨김
            HideUsernameInput();

            Debug.Log("[UsernameInputController] 유저네임 입력 완료 - Story_Demo 시작 요청 전송");

            // 유저네임 입력 완료 이벤트 알림
            // GameplaySceneManager가 Story_Demo를 시작하도록 함
            OnUsernameInputCompleted();

            Debug.Log("[UsernameInputController] 유저네임 입력 완료, Story_Demo 시작 대기 중");
        }

        /// <summary>
        /// 취소 버튼 클릭 (게임 종료 또는 타이틀로 복귀)
        /// </summary>
        private void OnCancelButtonClicked()
        {
            // 취소 시 타이틀 씬으로 복귀
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        }

        /// <summary>
        /// 플레이어 이름 설정
        /// </summary>
        private void SetPlayerName(string name)
        {
            if (DialogueManager.instance == null || DialogueManager.masterDatabase == null)
            {
                Debug.LogError("[UsernameInputController] DialogueSystem이 준비되지 않았습니다!");
                return;
            }

            // DialogueSystem Variables에 저장
            DialogueLua.SetVariable("PlayerName", name);
            
            // GameDataManager에도 저장 (있는 경우)
            if (GameDataManager.Instance != null)
            {
                // GameDataManager에 SetPlayerName 메서드가 있다면 호출
                var method = typeof(GameDataManager).GetMethod("SetPlayerName");
                if (method != null)
                {
                    method.Invoke(GameDataManager.Instance, new object[] { name });
                }
            }

            Debug.Log($"[UsernameInputController] 플레이어 이름 설정: {name}");
        }

        /// <summary>
        /// 유저네임 입력 완료 이벤트
        /// </summary>
        private void OnUsernameInputCompleted()
        {
            // GameplaySceneManager에 유저네임 입력 완료 알림
            EnsureGameplayReferences();

            if (gameplaySceneManager != null)
            {
                if (!gameplaySceneManager.isActiveAndEnabled)
                {
                    Debug.LogWarning("[UsernameInputController] GameplaySceneManager가 비활성 상태입니다. 활성화 후 Story_Demo를 시작합니다.");
                }

                // Story_Demo 시작 요청
                gameplaySceneManager.OnUsernameInputCompleted();
            }
            else
            {
                Debug.LogError("[UsernameInputController] GameplaySceneManager를 찾을 수 없습니다! LoveAlgoGameplayRoot 프리팹 구성을 확인하세요.");
            }
        }

        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[UsernameInputController] {message}");
            }
        }

        /// <summary>
        /// 에러 메시지 가져오기
        /// </summary>
        private string GetErrorMessage(UsernameValidationError error)
        {
            switch (error)
            {
                case UsernameValidationError.Empty:
                    return "이름을 입력해주세요.";
                case UsernameValidationError.LengthTooShort:
                    return $"이름은 최소 {LoveAlgoUsernameRules.MinLength}자 이상이어야 합니다.";
                case UsernameValidationError.LengthTooLong:
                    return $"이름은 최대 {LoveAlgoUsernameRules.MaxLength}자까지 입력할 수 있습니다.";
                case UsernameValidationError.MixedScript:
                    return "한글과 영어를 혼용할 수 없습니다.";
                case UsernameValidationError.InvalidCharacter:
                    return "사용할 수 없는 문자가 포함되어 있습니다.";
                default:
                    return "입력값이 올바르지 않습니다.";
            }
        }

        /// <summary>
        /// 유저네임 입력 UI 생성 (에디터에서 자동 생성되지 않은 경우)
        /// </summary>
        private void CreateUsernameInputUI()
        {
            Debug.LogWarning("[UsernameInputController] UsernameInputPanel이 없습니다. 수동으로 생성해야 합니다.");
            // TODO: 필요 시 런타임에 UI 생성하는 로직 추가
        }
        
        /// <summary>
        /// 이벤트 해제 (메모리 누수 방지)
        /// </summary>
        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
            if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
            
            if (usernameInputField != null)
            {
                usernameInputField.onValidateInput -= ValidateUsernameInput;
                usernameInputField.onSubmit.RemoveAllListeners();
            }
        }
    }
}
*/