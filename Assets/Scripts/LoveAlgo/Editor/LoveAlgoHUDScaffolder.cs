#if UNITY_EDITOR
using LoveAlgo.Editor.PrefabFactories;

namespace LoveAlgo.Editor
{
    /// <summary>
    /// Legacy scaffolder kept for reference. Prefer the LoveAlgo/Build Modules menu commands.
    /// </summary>
    [System.Obsolete("Use LoveAlgo/Build Modules menu commands instead.")]
    public static class LoveAlgoHUDScaffolder
    {
        public static void BuildPrefabs()
        {
            PrefabFactoryManager.BuildHudPrefab();
        }
    }
}
#endif
