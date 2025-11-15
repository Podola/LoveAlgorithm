using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace LoveAlgo.UI.Shared
{
    /// <summary>
    /// Popup 관리자 (DDOL)
    /// Popup Canvas (Sort: 100) 관리: Confirm, Info, Choice
    /// 작은 확인창 전용 (Panel보다 작음)
    /// SystemsManager가 DDOL 처리
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        [Header("Popup Canvas (Sort: 100) - DDOL")]
        [SerializeField] private Canvas popupCanvas;
        
        [Header("팝업 프리팹들 (PopupCanvas 하위에 미리 배치)")]
        [SerializeField] private GameObject confirmPopup;  // 확인/취소 2버튼
        [SerializeField] private GameObject infoPopup;     // 확인 1버튼
        [SerializeField] private GameObject choicePopup;   // 선택지 여러 버튼
        
        [Header("Popup 전용 Dim (선택적)")]
        [SerializeField] private GameObject dimBackground;
        [SerializeField] private float dimAlpha = 0.5f;
        
        [Header("애니메이션")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        
        // 현재 표시 중인 팝업
        private GameObject currentPopup = null;
        
        // 싱글톤
        private static PopupManager instance;
        public static PopupManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PopupManager>();
                    if (instance == null && Application.isPlaying)
                    {
                        Debug.LogWarning("[PopupManager] 인스턴스를 찾을 수 없습니다. 씬에 PopupManager가 있는지 확인하세요.");
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        public bool HasActivePopup => currentPopup != null;
        
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
            SetupPopupCanvas();
            HideAllPopups();
            HideDim();

            // UIManager에 등록
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterPopupManager(this);
            }

            Debug.Log("[PopupManager] Initialized (Confirm/Info/Choice)");
        }
        
        #region Public Methods
        
        /// <summary>
        /// 정보 팝업 (확인 버튼 1개)
        /// </summary>
        public void ShowInfo(string title, string message, Action onClose = null)
        {
            if (infoPopup == null)
            {
                Debug.LogError("[PopupManager] InfoPopup not assigned!");
                return;
            }
            
            HideCurrentPopup();
            
            SetPopupText(infoPopup, "Title", title);
            SetPopupText(infoPopup, "Message", message);
            
            SetButtonAction(infoPopup, "OkButton", () => {
                onClose?.Invoke();
                HideCurrentPopup();
            });
            
            ShowPopup(infoPopup, useDim: false);
            Debug.Log($"[PopupManager] Info popup: {title}");
        }
        
        /// <summary>
        /// 확인 팝업 (확인/취소 버튼 2개)
        /// </summary>
        public void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null, bool useDim = false)
        {
            if (confirmPopup == null)
            {
                Debug.LogError("[PopupManager] ConfirmPopup not assigned!");
                return;
            }
            
            HideCurrentPopup();
            
            SetPopupText(confirmPopup, "Title", title);
            SetPopupText(confirmPopup, "Message", message);
            
            SetButtonAction(confirmPopup, "ConfirmButton", () => {
                onConfirm?.Invoke();
                HideCurrentPopup();
            });
            
            SetButtonAction(confirmPopup, "CancelButton", () => {
                onCancel?.Invoke();
                HideCurrentPopup();
            });
            
            ShowPopup(confirmPopup, useDim);
            Debug.Log($"[PopupManager] Confirm popup: {title}");
        }
        
        /// <summary>
        /// 선택지 팝업 (여러 버튼)
        /// </summary>
        public void ShowChoice(string title, string message, params (string text, Action action)[] choices)
        {
            if (choicePopup == null)
            {
                Debug.LogError("[PopupManager] ChoicePopup not assigned!");
                return;
            }
            
            if (choices.Length == 0 || choices.Length > 4)
            {
                Debug.LogError($"[PopupManager] Choice count must be 1-4, got {choices.Length}");
                return;
            }
            
            HideCurrentPopup();
            
            SetPopupText(choicePopup, "Title", title);
            SetPopupText(choicePopup, "Message", message);
            
            // 선택지 버튼 설정 (최대 4개)
            for (int i = 0; i < 4; i++)
            {
                string buttonName = $"Choice{i + 1}Button";
                GameObject buttonObj = FindChild(choicePopup.transform, buttonName);
                
                if (buttonObj != null)
                {
                    if (i < choices.Length)
                    {
                        // 버튼 활성화 및 설정
                        buttonObj.SetActive(true);
                        
                        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = choices[i].text;
                        }
                        
                        Button button = buttonObj.GetComponent<Button>();
                        if (button != null)
                        {
                            int index = i; // 클로저 캡처
                            button.onClick.RemoveAllListeners();
                            button.onClick.AddListener(() => {
                                choices[index].action?.Invoke();
                                HideCurrentPopup();
                            });
                        }
                    }
                    else
                    {
                        // 사용하지 않는 버튼 비활성화
                        buttonObj.SetActive(false);
                    }
                }
            }
            
            ShowPopup(choicePopup, useDim: false);
            Debug.Log($"[PopupManager] Choice popup: {title} ({choices.Length} options)");
        }
        
        /// <summary>
        /// 현재 팝업 닫기
        /// </summary>
        public void CloseCurrentPopup()
        {
            HideCurrentPopup();
        }
        
        #endregion
        
        #region Internal Methods
        
        void SetupPopupCanvas()
        {
            if (popupCanvas != null)
            {
                popupCanvas.sortingOrder = 100;
                popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log("[PopupManager] Popup Canvas setup (Sort: 100)");
            }
            else
            {
                Debug.LogError("[PopupManager] Popup Canvas not assigned!");
            }
        }
        
        void ShowPopup(GameObject popup, bool useDim)
        {
            if (popup == null) return;
            
            if (useDim)
            {
                ShowDim();
            }
            
            popup.SetActive(true);
            currentPopup = popup;
            
            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn(canvasGroup));
            }
        }
        
        void HideCurrentPopup()
        {
            if (currentPopup != null)
            {
                CanvasGroup canvasGroup = currentPopup.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    StartCoroutine(FadeOut(canvasGroup, currentPopup));
                }
                else
                {
                    currentPopup.SetActive(false);
                    currentPopup = null;
                }
                
                HideDim();
            }
        }
        
        void HideAllPopups()
        {
            if (confirmPopup != null) confirmPopup.SetActive(false);
            if (infoPopup != null) infoPopup.SetActive(false);
            if (choicePopup != null) choicePopup.SetActive(false);
            currentPopup = null;
        }
        
        void SetPopupText(GameObject popup, string childName, string text)
        {
            GameObject textObj = FindChild(popup.transform, childName);
            if (textObj != null)
            {
                TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = text;
                }
            }
        }
        
        void SetButtonAction(GameObject popup, string buttonName, Action action)
        {
            GameObject buttonObj = FindChild(popup.transform, buttonName);
            if (buttonObj != null)
            {
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => action?.Invoke());
                }
            }
        }
        
        GameObject FindChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
                
                GameObject result = FindChild(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        
        #endregion
        
        #region Dim
        
        void ShowDim()
        {
            if (dimBackground != null)
            {
                dimBackground.SetActive(true);
                
                Image dimImage = dimBackground.GetComponent<Image>();
                if (dimImage != null)
                {
                    Color color = dimImage.color;
                    color.a = dimAlpha;
                    dimImage.color = color;
                }
            }
        }
        
        void HideDim()
        {
            if (dimBackground != null)
            {
                dimBackground.SetActive(false);
            }
        }
        
        #endregion
        
        #region Animation
        
        System.Collections.IEnumerator FadeIn(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) yield break;
            
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < fadeInDuration)
            {
                // 객체가 파괴되었는지 체크
                if (canvasGroup == null || !canvasGroup.gameObject.activeInHierarchy)
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        System.Collections.IEnumerator FadeOut(CanvasGroup canvasGroup, GameObject popup)
        {
            if (canvasGroup == null) yield break;
            
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < fadeOutDuration)
            {
                // 객체가 파괴되었는지 체크
                if (canvasGroup == null || !canvasGroup.gameObject.activeInHierarchy)
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            if (popup != null)
            {
                popup.SetActive(false);
            }
            
            if (currentPopup == popup)
            {
                currentPopup = null;
            }
        }
        
        #endregion
        
        #region ESC Key
        
        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (currentPopup != null)
                {
                    HideCurrentPopup();
                }
            }
        }
        
        #endregion
    }
}
