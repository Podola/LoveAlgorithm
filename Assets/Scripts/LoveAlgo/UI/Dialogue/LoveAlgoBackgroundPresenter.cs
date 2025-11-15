using System.Collections;
using LoveAlgo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace LoveAlgo.UI.Dialogue
{
    public sealed class LoveAlgoBackgroundPresenter : MonoBehaviour
    {
        [SerializeField] private Image primary;
        [SerializeField] private Image secondary;
        [SerializeField] private BackgroundCatalog catalog;
        [SerializeField] [Min(0f)] private float fadeDuration = 0.35f;

        private Coroutine currentRoutine;
        private string currentBackgroundId;

        public void Show(string backgroundId)
        {
            if (string.IsNullOrEmpty(backgroundId) || catalog == null)
            {
                return;
            }

            if (backgroundId == currentBackgroundId)
            {
                return;
            }

            if (!catalog.TryGetSprite(backgroundId, out var sprite))
            {
                Debug.LogWarning($"LoveAlgoBackgroundPresenter: Sprite not found for id '{backgroundId}'.");
                return;
            }

            currentBackgroundId = backgroundId;
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                ApplyImmediate(sprite);
                return;
            }

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = StartCoroutine(FadeRoutine(sprite));
        }

        private void ApplyImmediate(Sprite sprite)
        {
            if (primary != null)
            {
                primary.sprite = sprite;
                primary.color = Color.white;
            }
            if (secondary != null)
            {
                secondary.enabled = false;
            }
        }

        private IEnumerator FadeRoutine(Sprite nextSprite)
        {
            if (primary == null)
            {
                yield break;
            }

            if (secondary == null || Mathf.Approximately(fadeDuration, 0f))
            {
                ApplyImmediate(nextSprite);
                yield break;
            }

            secondary.sprite = nextSprite;
            secondary.color = new Color(1f, 1f, 1f, 0f);
            secondary.enabled = true;

            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / fadeDuration);
                primary.color = new Color(1f, 1f, 1f, 1f - t);
                secondary.color = new Color(1f, 1f, 1f, t);
                yield return null;
            }

            primary.sprite = nextSprite;
            primary.color = Color.white;
            secondary.enabled = false;
            currentRoutine = null;
        }
    }
}
