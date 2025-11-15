#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static LoveAlgo.Editor.PrefabFactories.PrefabBuilderUtils;

namespace LoveAlgo.Editor.PrefabFactories
{
    public static class PrefabFactoryManager
    {
        public static bool BuildFreeActionModule()
        {
            return BuildSingleFactory(new FreeActionPanelFactory());
        }

        public static bool BuildShopModule()
        {
            return BuildSingleFactory(new ShopPanelFactory());
        }

        public static bool BuildHudPrefab()
        {
            return BuildSingleFactory(new HudPrefabFactory());
        }

        public static bool BuildDialoguePanel()
        {
            return BuildSingleFactory(new DialoguePanelFactory());
        }

        private static bool BuildSingleFactory(IPrefabFactory factory)
        {
            if (factory == null)
            {
                Debug.LogError("PrefabFactoryManager: No factory provided.");
                return false;
            }

            EnsureDefaultFonts();

            var instance = factory.BuildPrefab();
            if (instance == null)
            {
                Debug.LogError($"PrefabFactoryManager: {factory.DisplayName} returned null prefab. Build aborted.");
                return false;
            }

            EnsureOutputFolder(factory.OutputPath);
            SavePrefab(instance, factory.OutputPath);
            UnityEngine.Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkAllScenesDirty();

            Debug.Log($"PrefabFactoryManager: Built {factory.DisplayName} ({factory.OutputPath}).");
            return true;
        }

        private static void EnsureOutputFolder(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            var directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(directory))
            {
                EnsureFolder(directory);
            }
        }
    }
}
#endif
