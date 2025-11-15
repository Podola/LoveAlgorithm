using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LoveAlgo.UI.Shared
{
    /// <summary>
    /// 버튼 호버 효과를 처리하는 독립 컴포넌트
    /// EventTrigger 대신 인터페이스를 사용하여 클릭 충돌 방지
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationDuration = 0.1f;
        
        private Button button;
        private Vector3 originalScale;
        private Coroutine scaleCoroutine;
        
        void Awake()
        {
            button = GetComponent<Button>();
            originalScale = transform.localScale;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 버튼이 비활성화되어 있으면 호버 효과 없음
            if (button == null || !button.interactable) return;
            
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScale));
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (button == null) return;
            
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
        }
        
        private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }
        
        void OnDisable()
        {
            // 비활성화 시 스케일 원상복구
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
            if (transform != null)
            {
                transform.localScale = originalScale;
            }
        }
        
        void OnDestroy()
        {
            // 코루틴 정리
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
        }
    }
}

