#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LoveAlgo.Data;
using LoveAlgo.UI.Dialogue;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static LoveAlgo.Editor.PrefabFactories.PrefabBuilderUtils;

namespace LoveAlgo.Editor.PrefabFactories
{
    /// <summary>
    /// Builds a Dialogue UI prefab that mirrors the VN-style layout approved in Step3 planning.
    /// Includes background/standing presenters, CG slot, DSU-compatible labels, and response menu wiring.
    /// </summary>
    public sealed class DialoguePanelFactory : IPrefabFactory
    {
        private const string PrefabPath = "Assets/Prefabs/Simple/Modules/DialoguePanel.prefab";
        private const string DefaultBackgroundCatalogPath = "Assets/Data/LoveAlgo/Backgrounds/TestBackgroundCatalog.asset";
        private const string DefaultStandingCatalogPath = "Assets/Data/LoveAlgo/Standing/TestStandingPoseCatalog.asset";

        public string DisplayName => "Dialogue Panel";
        public string OutputPath => PrefabPath;
        public IEnumerable<FactoryDependency> Dependencies => Array.Empty<FactoryDependency>();

        public GameObject BuildPrefab()
        {
            var canvasRoot = CreateUIRoot("LoveAlgoDialoguePanel");
            ConfigureCanvas(canvasRoot);

            var dialogueView = canvasRoot.AddComponent<LoveAlgoDialogueView>();
            var backgroundPresenter = BuildBackgroundLayer(canvasRoot.transform);
            var standingPresenter = BuildStandingLayer(canvasRoot.transform);
            BuildCgLayer(canvasRoot.transform);
            BuildDialogueOverlay(canvasRoot.transform, out var speakerLabel, out var bodyLabel, out var choicePanel, out var skipButton);

            WireDialogueView(dialogueView, speakerLabel, bodyLabel, backgroundPresenter, standingPresenter, choicePanel);
            ConfigureSkipButton(skipButton, dialogueView);

            return canvasRoot;
        }

        private static void ConfigureCanvas(GameObject canvasRoot)
        {
            var canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
            scaler.matchWidthOrHeight = 1f;

            var raycaster = canvasRoot.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.ignoreReversedGraphics = true;
            }
        }

        private static LoveAlgoBackgroundPresenter BuildBackgroundLayer(Transform parent)
        {
            var layer = CreateUIObject("BackgroundLayer", parent);
            var rect = layer.GetComponent<RectTransform>();
            Stretch(rect);

            var primary = CreateFullscreenImage("PrimaryBackground", layer.transform);
            var secondary = CreateFullscreenImage("SecondaryBackground", layer.transform);
            secondary.color = new Color(1f, 1f, 1f, 0f);
            secondary.enabled = false;

            var presenter = layer.AddComponent<LoveAlgoBackgroundPresenter>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("primary").objectReferenceValue = primary;
            serialized.FindProperty("secondary").objectReferenceValue = secondary;
            serialized.FindProperty("catalog").objectReferenceValue = LoadBackgroundCatalog();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return presenter;
        }

        private static LoveAlgoStandingPresenter BuildStandingLayer(Transform parent)
        {
            var layer = CreateUIObject("StandingLayer", parent);
            var rect = layer.GetComponent<RectTransform>();
            Stretch(rect);

            var left = CreateStandingImage(layer.transform, "LeftPose", new Vector2(-520f, 0f));
            var center = CreateStandingImage(layer.transform, "CenterPose", Vector2.zero);
            var right = CreateStandingImage(layer.transform, "RightPose", new Vector2(520f, 0f));

            var presenter = layer.AddComponent<LoveAlgoStandingPresenter>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("catalog").objectReferenceValue = LoadStandingCatalog();
            var slotsProperty = serialized.FindProperty("slots");
            slotsProperty.arraySize = 0;
            AddStandingSlot(slotsProperty, StandingSlot.Left, left);
            AddStandingSlot(slotsProperty, StandingSlot.Center, center);
            AddStandingSlot(slotsProperty, StandingSlot.Right, right);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return presenter;
        }

        private static void BuildCgLayer(Transform parent)
        {
            var cgLayer = CreateUIObject("CgLayer", parent);
            var rect = cgLayer.GetComponent<RectTransform>();
            Stretch(rect);
            var cgImage = cgLayer.AddComponent<Image>();
            cgImage.color = Color.white;
            cgImage.raycastTarget = false;
            cgImage.preserveAspect = true;
            cgImage.enabled = false;
        }

        private static void BuildDialogueOverlay(Transform parent, out TMP_Text speakerLabel, out TMP_Text bodyLabel, out LoveAlgoDialogueChoicePanel choicePanel, out Button skipButton)
        {
            var overlay = CreateUIObject("DialogueOverlay", parent);
            var overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = new Vector2(0f, 0f);
            overlayRect.anchorMax = new Vector2(1f, 0f);
            overlayRect.pivot = new Vector2(0.5f, 0f);
            overlayRect.sizeDelta = new Vector2(0f, 360f);
            overlayRect.anchoredPosition = Vector2.zero;
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.02f, 0.02f, 0.05f, 0.82f);

            var content = CreateUIObject("Content", overlay.transform);
            var contentRect = content.GetComponent<RectTransform>();
            Stretch(contentRect);
            contentRect.offsetMin = new Vector2(40f, 32f);
            contentRect.offsetMax = new Vector2(-40f, -24f);
            var horizontalLayout = content.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 32f;
            horizontalLayout.childForceExpandWidth = true;
            horizontalLayout.childForceExpandHeight = true;
            horizontalLayout.childControlWidth = true;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childAlignment = TextAnchor.UpperLeft;

            BuildDialogueColumn(content.transform, out speakerLabel, out bodyLabel, out skipButton);
            choicePanel = BuildChoiceColumn(content.transform);
        }

        private static void BuildDialogueColumn(Transform parent, out TMP_Text speakerLabel, out TMP_Text bodyLabel, out Button skipButton)
        {
            var column = CreateUIObject("DialogueColumn", parent);
            var columnRect = column.GetComponent<RectTransform>();
            Stretch(columnRect);
            var layout = column.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var columnElement = column.AddComponent<LayoutElement>();
            columnElement.flexibleWidth = 1f;

            var nameplate = CreateStyledPanel("Nameplate", column.transform, PanelStyle.Surface);
            var nameplateRect = nameplate.GetComponent<RectTransform>();
            Stretch(nameplateRect);
            var nameLayout = nameplate.AddComponent<LayoutElement>();
            nameLayout.minHeight = 70f;
            nameLayout.preferredHeight = 78f;
            speakerLabel = CreateStyledText("SpeakerLabel", nameplate.transform, TextStyle.SectionTitle, false);
            var speakerRect = speakerLabel.rectTransform;
            Stretch(speakerRect);
            speakerRect.offsetMin = new Vector2(20f, 12f);
            speakerRect.offsetMax = new Vector2(-20f, -12f);
            speakerLabel.alignment = TextAlignmentOptions.MidlineLeft;

            var bodyPanel = CreateStyledPanel("BodyPanel", column.transform, PanelStyle.ScrollSurface);
            var bodyPanelRect = bodyPanel.GetComponent<RectTransform>();
            Stretch(bodyPanelRect);
            var bodyElement = bodyPanel.AddComponent<LayoutElement>();
            bodyElement.flexibleHeight = 1f;
            bodyElement.minHeight = 140f;
            bodyLabel = CreateStyledText("BodyLabel", bodyPanel.transform, TextStyle.Body, false);
            var bodyRect = bodyLabel.rectTransform;
            Stretch(bodyRect);
            bodyRect.offsetMin = new Vector2(24f, 24f);
            bodyRect.offsetMax = new Vector2(-24f, -24f);
            bodyLabel.alignment = TextAlignmentOptions.TopLeft;
            bodyLabel.enableWordWrapping = true;
            bodyLabel.textWrappingMode = TextWrappingModes.Normal;

            var controls = CreateUIObject("ControlsRow", column.transform);
            var controlsRect = controls.GetComponent<RectTransform>();
            Stretch(controlsRect);
            var controlsLayout = controls.AddComponent<HorizontalLayoutGroup>();
            controlsLayout.spacing = 16f;
            controlsLayout.childAlignment = TextAnchor.UpperLeft;
            controlsLayout.childControlWidth = true;
            controlsLayout.childForceExpandWidth = false;
            controlsLayout.childControlHeight = true;
            controlsLayout.childForceExpandHeight = false;
            var controlsElement = controls.AddComponent<LayoutElement>();
            controlsElement.minHeight = 72f;
            controlsElement.preferredHeight = 72f;

            skipButton = CreateStyledButton("SkipButton", controls.transform, "타자기 건너뛰기", ButtonStyle.Primary);
            var skipLayout = skipButton.GetComponent<LayoutElement>();
            if (skipLayout != null)
            {
                skipLayout.preferredWidth = 260f;
                skipLayout.minWidth = 220f;
            }

            var logButton = CreateStyledButton("HistoryButton", controls.transform, "로그", ButtonStyle.Ghost);
            var logLayout = logButton.GetComponent<LayoutElement>();
            if (logLayout != null)
            {
                logLayout.preferredWidth = 160f;
                logLayout.minWidth = 140f;
            }
        }

        private static LoveAlgoDialogueChoicePanel BuildChoiceColumn(Transform parent)
        {
            var choicesPanel = CreateUIObject("ChoicesPanel", parent);
            var panelRect = choicesPanel.GetComponent<RectTransform>();
            Stretch(panelRect);
            var layoutElement = choicesPanel.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 440f;
            layoutElement.minWidth = 360f;

            var scrollRect = CreateScrollView(choicesPanel.transform, out var contentRect, "ChoicesScroll", PanelStyle.ScrollSurface);
            var scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            Stretch(scrollRectTransform);
            scrollRectTransform.offsetMin = new Vector2(6f, 6f);
            scrollRectTransform.offsetMax = new Vector2(-6f, -6f);
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            var contentLayout = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 14f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = false;

            var template = CreateChoiceButtonTemplate(contentRect);
            template.gameObject.SetActive(false);

            var panel = choicesPanel.AddComponent<LoveAlgoDialogueChoicePanel>();
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("contentRoot").objectReferenceValue = contentRect;
            serialized.FindProperty("choiceButtonTemplate").objectReferenceValue = template;
            serialized.FindProperty("autoSelectSingleResponse").boolValue = true;
            serialized.FindProperty("hideWhenEmpty").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        private static Button CreateChoiceButtonTemplate(Transform parent)
        {
            var button = CreateStyledButton("ChoiceButtonTemplate", parent, "선택지", ButtonStyle.Secondary);
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.fontSize = 26f;
            }

            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredHeight = 72f;
                layout.minHeight = 60f;
            }

            return button;
        }

        private static void WireDialogueView(LoveAlgoDialogueView view, TMP_Text speaker, TMP_Text body, LoveAlgoBackgroundPresenter backgroundPresenter, LoveAlgoStandingPresenter standingPresenter, LoveAlgoDialogueChoicePanel choicePanel)
        {
            var serialized = new SerializedObject(view);
            serialized.FindProperty("speakerLabel").objectReferenceValue = speaker;
            serialized.FindProperty("bodyLabel").objectReferenceValue = body;
            serialized.FindProperty("backgroundPresenter").objectReferenceValue = backgroundPresenter;
            serialized.FindProperty("standingPresenter").objectReferenceValue = standingPresenter;
            serialized.FindProperty("choicePanel").objectReferenceValue = choicePanel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureSkipButton(Button skipButton, LoveAlgoDialogueView view)
        {
            if (skipButton == null || view == null)
            {
                return;
            }

            while (skipButton.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(skipButton.onClick, 0);
            }
            UnityEventTools.AddPersistentListener(skipButton.onClick, view.CompleteTypewriter);
        }

        private static Image CreateFullscreenImage(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreateStandingImage(Transform parent, string name, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(720f, 1080f);
            rect.anchoredPosition = anchoredPosition;
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            image.enabled = false;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static void AddStandingSlot(SerializedProperty slotsProperty, StandingSlot slot, Image image)
        {
            var index = slotsProperty.arraySize;
            slotsProperty.InsertArrayElementAtIndex(index);
            var element = slotsProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("slot").enumValueIndex = (int)slot;
            element.FindPropertyRelative("image").objectReferenceValue = image;
        }

        private static BackgroundCatalog LoadBackgroundCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<BackgroundCatalog>(DefaultBackgroundCatalogPath);
            if (catalog == null)
            {
                Debug.LogWarning($"DialoguePanelFactory: BackgroundCatalog asset not found at {DefaultBackgroundCatalogPath}. Assign one manually after build.");
            }

            return catalog;
        }

        private static StandingPoseCatalog LoadStandingCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<StandingPoseCatalog>(DefaultStandingCatalogPath);
            if (catalog == null)
            {
                Debug.LogWarning($"DialoguePanelFactory: StandingPoseCatalog asset not found at {DefaultStandingCatalogPath}. Assign one manually after build.");
            }

            return catalog;
        }
    }
}
#endif
