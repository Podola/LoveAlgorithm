using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
    internal static class VNFrameworkFindUtility
    {
        public static T FindFirstInstance<T>(bool includeInactive = false) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            var options = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            return Object.FindFirstObjectByType<T>(options);
#else
            return Object.FindObjectOfType<T>(includeInactive);
#endif
        }
    }
}

