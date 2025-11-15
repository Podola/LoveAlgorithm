using UnityEngine;
using UnityEditor;

namespace LoveAlgo.Editor
{
    /// <summary>
    /// 러브 알고리즘 프로젝트 기본 에디터 메뉴
    /// </summary>
    public static class LoveAlgoEditorMenu
    {
        #region Constants
        public const string DatabasePath = "Assets/Data/LoveAlgo_Database.asset";
        public const string ResourceCatalogPath = "Assets/Data/LoveAlgoResourceCatalog.asset";

        #endregion

       

        #region Info

        [MenuItem("LoveAlgo/About", false, 100)]
        public static void ShowAbout()
        {
            EditorUtility.DisplayDialog(
                "러브 알고리즘",
                "Visual Novel Project\n\n" +
                "• 엔진: Unity 6 + DSU\n" +
                "• 플랫폼: PC (Steam)\n" +
                "• 장르: 연애 시뮬레이션\n\n" +
                "프리팹들은 수동으로 만들어주세요!",
                "확인"
            );
        }

        #endregion
    }
}