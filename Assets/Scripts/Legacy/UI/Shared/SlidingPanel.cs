using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LoveAlgo.UI.Shared
{
    /// <summary>
    /// 우측에서 슬라이드 인/아웃되는 패널 컨트롤러.
    /// PanelManager가 Settings/SaveLoad/Extra 패널을 표시할 때 사용한다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SlidingPanel : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float slideDuration = 0.35f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float hiddenPadding = 64f;
        [SerializeField] private bool startHidden = true;

        [Header("Cached Positions (auto calculated)")]
        [SerializeField] private Vector2 shownPosition;
        [SerializeField] private Vector2 hiddenPosition;

        private RectTransform rectTransform = default!;
        private Coroutine slideCoroutine;
        private bool initializedPositions;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            EnsurePositions();
            if (startHidden)
            {
                rectTransform.anchoredPosition = hiddenPosition;
            }
        }

        /// <summary>
        /// 슬라이드 인 (표시)
        /// </summary>
        public void SlideIn()
        {
            EnsurePositions();
            gameObject.SetActive(true);
            StartSlide(hiddenPosition, shownPosition, deactivateOnComplete: false);
        }

        /// <summary>
        /// 슬라이드 아웃 (숨김)
        /// </summary>
        public void SlideOut(Action onComplete = null)
        {
            EnsurePositions();
            StartSlide(rectTransform.anchoredPosition, hiddenPosition, deactivateOnComplete: true, onComplete);
        }

        private void EnsurePositions()
        {
            if (initializedPositions) return;

            // 현재 레이아웃 정보를 최신 상태로 업데이트
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            shownPosition = rectTransform.anchoredPosition;
            float width = Mathf.Abs(rectTransform.rect.width);
            if (width <= Mathf.Epsilon)
            {
                width = Mathf.Abs(rectTransform.sizeDelta.x);
            }

            hiddenPosition = shownPosition + new Vector2(width + hiddenPadding, 0f);
            initializedPositions = true;
        }

        private void StartSlide(Vector2 from, Vector2 to, bool deactivateOnComplete, Action onComplete = null)
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }
            slideCoroutine = StartCoroutine(SlideRoutine(from, to, deactivateOnComplete, onComplete));
        }

        private IEnumerator SlideRoutine(Vector2 from, Vector2 to, bool deactivateOnComplete, Action onComplete)
        {
            if (rectTransform == null) yield break;
            
            float elapsed = 0f;
            rectTransform.anchoredPosition = from;

            if (Mathf.Approximately(slideDuration, 0f))
            {
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = to;
                }
            }
            else
            {
                while (elapsed < slideDuration)
                {
                    // 객체가 파괴되었는지 체크
                    if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
                    {
                        slideCoroutine = null;
                        yield break;
                    }
                    
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / slideDuration);
                    float easedT = easing.Evaluate(t);
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, easedT);
                    yield return null;
                }
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = to;
                }
            }

            if (deactivateOnComplete && rectTransform != null)
            {
                gameObject.SetActive(false);
            }

            slideCoroutine = null;
            onComplete?.Invoke();
        }
        
        private void OnDestroy()
        {
            // 실행 중인 코루틴 정리
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }
        }
    }
}
