using UnityEngine;

namespace LoveAlgo.UI
{
    /// <summary>
    /// Base class for modular UI panels that can be loaded into the LoveAlgo HUD root.
    /// Provides life-cycle hooks so concrete modules can allocate/deallocate lightweight state.
    /// </summary>
    public abstract class LoveAlgoUIModule : MonoBehaviour
    {
        private LoveAlgoHUDRoot owner;

        /// <summary>
        /// Called by the navigation controller right after the module prefab is instantiated.
        /// </summary>
        internal void InitializeInternal(LoveAlgoHUDRoot hudRoot)
        {
            owner = hudRoot;
            OnInitialized();
        }

        /// <summary>
        /// Gives derived modules a chance to perform custom setup after the owner/root is assigned.
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        /// Notifies the module it just became visible.
        /// </summary>
        public virtual void OnShow()
        {
        }

        /// <summary>
        /// Notifies the module it is about to be destroyed/hidden.
        /// </summary>
        public virtual void OnHide()
        {
        }

        protected LoveAlgoHUDRoot Owner => owner;
        protected RectTransform RectTransform => (RectTransform)transform;
    }
}
