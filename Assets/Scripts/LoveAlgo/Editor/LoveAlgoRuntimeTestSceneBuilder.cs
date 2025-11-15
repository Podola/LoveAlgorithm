#if UNITY_EDITOR
using System.IO;
using LoveAlgo.Core;
using LoveAlgo.Data;
using LoveAlgo.Testing;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LoveAlgo.Editor
{
    public static class LoveAlgoRuntimeTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/LoveAlgo/RuntimeSmokeTestScene.unity";
        private const string ConfigPath = "Assets/Data/LoveAlgo/Config/LoveAlgoConfiguration.asset";

        [MenuItem("LoveAlgo/Create Runtime Smoke Test Scene", priority = 20)]
        public static void CreateRuntimeSmokeTestScene()
        {
            if (!TryLoadConfiguration(out var configuration))
            {
                EditorUtility.DisplayDialog("LoveAlgo", "LoveAlgoConfiguration.asset을 찾을 수 없습니다.\n경로: " + ConfigPath, "확인");
                return;
            }

            EnsureDirectoryExists(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LoveAlgoRuntimeSmokeTestScene";

            var bootstrapper = new GameObject("LoveAlgoBootstrapper").AddComponent<LoveAlgoBootstrapper>();
            SetSerializedReference(bootstrapper, "configuration", configuration);

            var driver = new GameObject("GameLoopDriver").AddComponent<GameLoopSample>();
            var tester = new GameObject("RuntimeSmokeTester").AddComponent<LoveAlgoRuntimeSmokeTester>();
            SetSerializedReference(tester, "loopDriver", driver);

            var canvas = CreateCanvas("RuntimeSmokeCanvas", new Color(0.08f, 0.12f, 0.18f), "Runtime Smoke Test");
            var panel = canvas.transform.Find("Panel");
            var controlsRoot = CreateControlsRoot(panel ?? canvas.transform);

            var buttonY = 120f;
            var storyButton = CreateButton(controlsRoot, "Enter Story", new Vector2(0f, buttonY));
            UnityEventTools.AddPersistentListener(storyButton.onClick, driver.EnterStory);

            buttonY -= 70f;
            var freeButton = CreateButton(controlsRoot, "Enter Free Action", new Vector2(0f, buttonY));
            UnityEventTools.AddPersistentListener(freeButton.onClick, driver.EnterFreeAction);

            buttonY -= 70f;
            var autoFreeButton = CreateButton(controlsRoot, "Auto Free Action", new Vector2(0f, buttonY));
            UnityEventTools.AddPersistentListener(autoFreeButton.onClick, tester.AutoExecuteFirstFreeAction);

            buttonY -= 70f;
            var pushStatsButton = CreateButton(controlsRoot, "Push Stats", new Vector2(0f, buttonY));
            UnityEventTools.AddPersistentListener(pushStatsButton.onClick, tester.RequestStatsSync);

            buttonY -= 70f;
            var completeEventButton = CreateButton(controlsRoot, "Complete Event", new Vector2(0f, buttonY));
            UnityEventTools.AddPersistentListener(completeEventButton.onClick, tester.CompleteEvent);

            buttonY -= 70f;
            var dumpTimelineButton = CreateButton(controlsRoot, "Dump Timeline", new Vector2(0f, buttonY));
            UnityEventTools.AddPersistentListener(dumpTimelineButton.onClick, tester.DumpEpisodeTimeline);

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            Debug.Log("LoveAlgoRuntimeSmokeTestScene 생성 완료", AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        }

        private static Canvas CreateCanvas(string name, Color backgroundColor, string label)
        {
            var canvasGO = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var image = panel.GetComponent<Image>();
            image.color = backgroundColor;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.2f);
            rect.anchorMax = new Vector2(0.7f, 0.8f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var title = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            title.transform.SetParent(panel.transform, false);
            var text = title.GetComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.alignment = TextAnchor.UpperCenter;
            text.color = Color.white;
            var textRect = title.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.7f);
            textRect.anchorMax = new Vector2(0.9f, 0.95f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return canvas;
        }

        private static Transform CreateControlsRoot(Transform parent)
        {
            var root = new GameObject("Controls", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.7f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return root.transform;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition)
        {
            var buttonGO = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(parent, false);
            var rect = buttonGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 60f);
            rect.anchoredPosition = anchoredPosition;

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGO.transform.SetParent(buttonGO.transform, false);
            var text = textGO.GetComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonGO.GetComponent<Button>();
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static bool TryLoadConfiguration(out LoveAlgoConfiguration configuration)
        {
            configuration = AssetDatabase.LoadAssetAtPath<LoveAlgoConfiguration>(ConfigPath);
            return configuration != null;
        }

        private static void SetSerializedReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"LoveAlgoRuntimeTestSceneBuilder: '{propertyName}' 필드를 {target.name}에서 찾을 수 없습니다.", target);
                return;
            }

            so.Update();
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
#endif
