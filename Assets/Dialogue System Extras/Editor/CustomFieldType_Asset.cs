using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This adds a custom field type named Asset. You can assign any asset type
    /// to it. Internally, the field stores the asset path, such as:
    ///    Assets/Art/Resources/Smiley.png
    /// If you want to access the asset at runtime, put it in a folder named
    /// Resources. Then trim off the path up to and including "Resources/" as
    /// well as the file extension, and use Resources.Load() to load it.
    /// </summary>
    [CustomFieldTypeService.Name("Asset")]
    public class CustomFieldType_Asset : CustomFieldType
    {

        private UnityEngine.Object FindAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        }

        public override string Draw(string currentValue, DialogueDatabase database)
        {
            var asset = FindAsset(currentValue);
            EditorGUI.BeginChangeCheck();
            asset = EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                return AssetDatabase.GetAssetPath(asset);
            }
            else
            {
                return currentValue;
            }
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            var asset = FindAsset(currentValue);
            EditorGUI.BeginChangeCheck();
            asset = EditorGUI.ObjectField(rect, asset, typeof(UnityEngine.Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                return AssetDatabase.GetAssetPath(asset);
            }
            else
            {
                return currentValue;
            }
        }

    }
}
