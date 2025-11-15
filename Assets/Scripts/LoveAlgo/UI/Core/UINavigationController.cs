using UnityEngine;

namespace LoveAlgo.UI
{
    /// <summary>
    /// Simple controller that instantiates modular UI prefabs into a target container and keeps only one active at a time.
    /// </summary>
    public sealed class UINavigationController : MonoBehaviour
    {
        [SerializeField] private RectTransform moduleHost;

        private LoveAlgoUIModule activeModule;
        private LoveAlgoHUDRoot hudRoot;

        public LoveAlgoUIModule ActiveModule => activeModule;

        public void Initialize(LoveAlgoHUDRoot owner)
        {
            hudRoot = owner;
        }

        /// <summary>
        /// Instantiates the requested module prefab and destroys any previously active module.
        /// </summary>
        public LoveAlgoUIModule ShowModule(LoveAlgoUIModule prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[UINavigationController] Prefab reference is missing.", this);
                return null;
            }

            if (hudRoot == null)
            {
                Debug.LogWarning("[UINavigationController] Navigation controller has not been initialized.", this);
                return null;
            }

            if (activeModule != null)
            {
                if (activeModule.GetType() == prefab.GetType())
                {
                    return activeModule;
                }

                HideActiveModule();
            }

            var instance = Instantiate(prefab, moduleHost != null ? moduleHost : (RectTransform)transform);
            instance.InitializeInternal(hudRoot);
            instance.OnShow();
            activeModule = instance;
            return activeModule;
        }

        public void HideActiveModule()
        {
            if (activeModule == null)
            {
                return;
            }

            activeModule.OnHide();
            Destroy(activeModule.gameObject);
            activeModule = null;
        }
    }
}
