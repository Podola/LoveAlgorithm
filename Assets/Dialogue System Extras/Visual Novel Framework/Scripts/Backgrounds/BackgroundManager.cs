using UnityEngine;
using System.Collections;
using System;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{

    /// <summary>
    /// Manages the current background image.
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {

        public UnityEngine.UI.Image background;
        public UnityEngine.UI.Image background2;
        public string backgroundVariable = "Background";
        public string backgroundFadeDurationVariable = "BackgroundFadeDuration";

        [Tooltip("Seconds to wait after fading to black before fading in again. If zero, no fade. If non-zero, will use Dialogue Manager's Scene Transition Manager to fade.")]
        public float fadeDuration = 0;

        private static BackgroundManager m_instance = null;

        private void Awake()
        {
            m_instance = this;

            // Try to automatically find background Image components if necessary:
            var images = GetComponentsInChildren<UnityEngine.UI.Image>();
            if (background == null && background2 == null)
            {
                if (images.Length >= 2)
                {
                    background = images[0];
                    background2 = images[1];
                }
            }
            else if (background != null && background2 == null && images.Length >= 2)
            {
                if (images[0] == background) background2 = images[1];
                else if (images[1] == background) background2 = images[0];
            }
            if (background2 != null) background2.enabled = false;

            Lua.RegisterFunction("BackgroundFadeDuration", this, SymbolExtensions.GetMethodInfo(() => BackgroundFadeDuration((double)0)));
            PersistentDataManager.RegisterPersistentData(gameObject);
        }

        private void OnDestroy()
        {
            PersistentDataManager.UnregisterPersistentData(gameObject);
            m_instance = null;
        }

        private void Start()
        {
            UpdateBackgroundFromVariable();
        }

        private void OnApplyPersistentData()
        {
            UpdateBackgroundFromVariable();
        }

        public void BackgroundFadeDuration(double duration)
        {
            fadeDuration = (float)duration;
            DialogueLua.SetVariable(backgroundFadeDurationVariable, duration);
        }

        public static void UpdateBackgroundFromVariable()
        {
            if (m_instance == null) return;
            SetBackgroundImage(DialogueLua.GetVariable(m_instance.backgroundVariable).AsString);
            if (DialogueLua.DoesVariableExist(m_instance.backgroundFadeDurationVariable))
            {
                 m_instance.fadeDuration = DialogueLua.GetVariable(m_instance.backgroundFadeDurationVariable).asFloat;
            }
        }

        private static string m_backgroundName;

        public static void SetBackgroundImage(string backgroundName)
        {
            if (string.IsNullOrEmpty(backgroundName) || string.Equals(backgroundName, "nil")) return;
            m_backgroundName = backgroundName;
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Setting background image to '" + backgroundName + "'.");
            DialogueLua.SetVariable(m_instance.backgroundVariable, backgroundName);
            DialogueManager.LoadAsset(backgroundName, typeof(Sprite), OnAssetLoaded);
        }

        private static void OnAssetLoaded(UnityEngine.Object asset)
        {
            var image = asset as Sprite;
            if (image == null && asset is Texture2D)
            {
                var texture = asset as Texture2D;
                image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
            }
            if (image == null)
            {
#if USE_ADDRESSABLES
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(m_backgroundName).Completed += OnSpriteLoaded;
#else
                Debug.LogWarning("Dialogue System: Can't load background image '" + m_backgroundName + "'. Is the name correct?");
#endif
            }
            else
            {
                m_instance.StartCoroutine(m_instance.SetBackgroundImageCoroutine(image));
            }
        }

#if USE_ADDRESSABLES
        private static void OnSpriteLoaded(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<Sprite> obj)
        {
            var image = obj.Result as Sprite;
            if (image == null)
            {
                Debug.LogWarning("Dialogue System: Can't load background image '" + m_backgroundName + "'. Is the name correct?");
            }
            else
            {
                m_instance.StartCoroutine(m_instance.SetBackgroundImageCoroutine(image));
            }
        }
#endif

        private IEnumerator SetBackgroundImageCoroutine(Sprite image)
        {
            // If there's no fade duration, set background immediately:
            if (Mathf.Approximately(0, fadeDuration))
            {
                background.sprite = image;
                if (background2 != null) background2.enabled = false;
                yield break;
            }

            // If there's a background Image but not background2 Image, use scene transition manager to fade:
            if (background != null && background2 == null)
            {
                var sceneTransitionManager = DialogueManager.instance.GetComponentInChildren<StandardSceneTransitionManager>();
                if (sceneTransitionManager == null)
                {
                    m_instance.background.sprite = image;
                    yield break;
                }
                else
                {
                    sceneTransitionManager.leaveSceneTransition.TriggerAnimation();
                    yield return new WaitForSeconds(sceneTransitionManager.leaveSceneTransition.animationDuration + fadeDuration);
                    m_instance.background.sprite = image;
                    sceneTransitionManager.enterSceneTransition.TriggerAnimation();
                }
            }
            // Otherwise if there are background and background2 Images, cross-fade:
            else if (background != null && background2 != null)
            {
                // Put old image in background 2. Start full opaque and make transparent over time:
                background2.sprite = background.sprite;
                background2.color = new Color(1, 1, 1, 1);
                background2.enabled = true;
                // Put new image in background. Start transparent and make full opaque over time:
                background.sprite = image;
                background.color = new Color(1, 1, 1, 0);
                // Over time:
                float elapsed = 0;
                while (elapsed < fadeDuration)
                {
                    var t = (elapsed / fadeDuration);
                    background2.color = new Color(1, 1, 1, 1 - t);
                    background.color = new Color(1, 1, 1, t);
                    yield return null;
                    elapsed += Time.deltaTime;

                }
                background2.enabled = false;
                background.color = new Color(1, 1, 1, 1);
            }
        }

    }
}