#if UNITY_EDITOR
using LoveAlgo.Editor.PrefabFactories;
using UnityEditor;

namespace LoveAlgo.Editor
{
    internal static class LoveAlgoModuleBuilderMenus
    {
        [MenuItem("LoveAlgo/Build Modules/FreeAction Module", priority = 20)]
        private static void BuildFreeActionModule()
        {
            PrefabFactoryManager.BuildFreeActionModule();
        }

        [MenuItem("LoveAlgo/Build Modules/Shop Module", priority = 21)]
        private static void BuildShopModule()
        {
            PrefabFactoryManager.BuildShopModule();
        }

        [MenuItem("LoveAlgo/Build Modules/Dialogue Panel", priority = 22)]
        private static void BuildDialoguePanel()
        {
            PrefabFactoryManager.BuildDialoguePanel();
        }
    }
}
#endif
