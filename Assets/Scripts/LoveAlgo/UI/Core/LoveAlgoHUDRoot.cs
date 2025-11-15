using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoveAlgo.UI
{
    /// <summary>
    /// Entry point prefab for runtime HUD scenarios. Hosts navigation controls and module prefabs.
    /// </summary>
    public sealed class LoveAlgoHUDRoot : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private UINavigationController navigationController;
        [SerializeField] private Button freeActionNavButton;
        [SerializeField] private Button shopNavButton;
        [SerializeField] private Button messengerNavButton;

        [Header("Module Prefabs")]
        [SerializeField] private LoveAlgoUIModule freeActionModule;
        [SerializeField] private LoveAlgoUIModule shopModule;
        [SerializeField] private LoveAlgoUIModule messengerModule;

        [Header("Lifecycle Events")]
        [SerializeField] private UnityEvent onRootInitialized;

        private void Awake()
        {
            EnsureNavigationReady();
            WireNavigation();
            onRootInitialized?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureNavigationReady();
            if (!Application.isPlaying)
            {
                WireNavigation();
            }
        }

        private void Reset()
        {
            EnsureNavigationReady();
            WireNavigation();
        }
#endif

        private void WireNavigation()
        {
            if (freeActionNavButton != null)
            {
                freeActionNavButton.onClick.RemoveListener(ShowFreeAction);
                freeActionNavButton.onClick.AddListener(ShowFreeAction);
            }

            if (shopNavButton != null)
            {
                shopNavButton.onClick.RemoveListener(ShowShop);
                shopNavButton.onClick.AddListener(ShowShop);
            }

            if (messengerNavButton != null)
            {
                messengerNavButton.onClick.RemoveListener(ShowMessenger);
                messengerNavButton.onClick.AddListener(ShowMessenger);
            }
        }

        private void EnsureNavigationReady()
        {
            if (navigationController == null)
            {
                navigationController = GetComponentInChildren<UINavigationController>();
            }

            navigationController?.Initialize(this);
        }

        public void ShowFreeAction()
        {
            navigationController?.ShowModule(freeActionModule);
        }

        public void ShowShop()
        {
            navigationController?.ShowModule(shopModule);
        }

        public void ShowMessenger()
        {
            navigationController?.ShowModule(messengerModule);
        }

        public void HideActiveModule()
        {
            navigationController?.HideActiveModule();
        }

        public LoveAlgoUIModule ActiveModule => navigationController != null ? navigationController.ActiveModule : null;
    }
}
