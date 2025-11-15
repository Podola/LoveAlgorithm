#if UNITY_EDITOR
using UnityEngine;

namespace LoveAlgo.Editor.PrefabFactories
{
    [CreateAssetMenu(menuName = "LoveAlgo/Prefab Build Configuration", fileName = "PrefabBuildConfiguration")]
    public class PrefabBuildConfiguration : ScriptableObject
    {
        [Header("Factory Toggles")]
        public bool buildFreeAction = true;
        public bool buildShop = true;
        public bool buildHud = true;

        [Header("Build Options")]
        public bool forceRebuild = false;
        public bool validateAfterBuild = true;
    }
}
#endif
