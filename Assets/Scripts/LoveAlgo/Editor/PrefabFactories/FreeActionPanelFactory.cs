#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LoveAlgo.UI;
using LoveAlgo.UI.Modules;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static LoveAlgo.Editor.PrefabFactories.PrefabBuilderUtils;

namespace LoveAlgo.Editor.PrefabFactories
{
    public sealed class FreeActionPanelFactory : IPrefabFactory
    {
        private const string PrefabPath = "Assets/Prefabs/Simple/Modules/FreeActionPanel.prefab";
        public static string DefaultOutputPath => PrefabPath;

        public string DisplayName => "FreeAction Panel";
        public string OutputPath => PrefabPath;
        public IEnumerable<FactoryDependency> Dependencies => Array.Empty<FactoryDependency>();

        public GameObject BuildPrefab()
        {
            var panelRoot = CreateUIObject("FreeActionPanel", null);
            var rect = panelRoot.GetComponent<RectTransform>();
            Stretch(rect);
            var background = panelRoot.AddComponent<Image>();
            background.color = new Color(0.07f, 0.07f, 0.09f, 0.92f);

            var controller = panelRoot.AddComponent<FreeActionPanelController>();

            var actionsColumn = CreateUIObject("ActionsColumn", panelRoot.transform);
            var actionsRect = actionsColumn.GetComponent<RectTransform>();
            actionsRect.anchorMin = new Vector2(0f, 0f);
            actionsRect.anchorMax = new Vector2(0.3f, 1f);
            actionsRect.offsetMin = new Vector2(20f, 20f);
            actionsRect.offsetMax = new Vector2(-20f, -20f);
            var actionsBackground = actionsColumn.AddComponent<Image>();
            actionsBackground.color = new Color(0.1f, 0.13f, 0.19f, 0.9f);
            var actionsLayout = actionsColumn.AddComponent<VerticalLayoutGroup>();
            actionsLayout.spacing = 10f;
            actionsLayout.childForceExpandWidth = true;
            actionsLayout.childForceExpandHeight = false;

            var detailPanel = CreateUIObject("DetailPanel", panelRoot.transform);
            var detailRect = detailPanel.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0.32f, 0f);
            detailRect.anchorMax = new Vector2(1f, 1f);
            detailRect.offsetMin = new Vector2(20f, 20f);
            detailRect.offsetMax = new Vector2(-20f, -20f);
            var detailBackground = detailPanel.AddComponent<Image>();
            detailBackground.color = new Color(0.12f, 0.12f, 0.18f, 0.9f);

            var detailContent = CreateUIObject("Content", detailPanel.transform);
            var detailContentRect = detailContent.GetComponent<RectTransform>();
            Stretch(detailContentRect);
            detailContentRect.offsetMin = new Vector2(10f, 10f);
            detailContentRect.offsetMax = new Vector2(-10f, -10f);
            var detailLayout = detailContent.AddComponent<VerticalLayoutGroup>();
            detailLayout.spacing = 15f;
            detailLayout.padding = new RectOffset(30, 30, 30, 30);
            detailLayout.childControlHeight = true;
            detailLayout.childForceExpandHeight = false;
            detailLayout.childForceExpandWidth = true;
            detailLayout.childAlignment = TextAnchor.UpperLeft;

            var titleLabel = CreateStyledText("TitleLabel", detailContent.transform, TextStyle.Display);
            var summaryLabel = CreateStyledText("SummaryLabel", detailContent.transform, TextStyle.Body);
            summaryLabel.textWrappingMode = TextWrappingModes.Normal;
            summaryLabel.overflowMode = TextOverflowModes.Overflow;
            var summaryLayout = summaryLabel.GetComponent<LayoutElement>();
            if (summaryLayout != null)
            {
                summaryLayout.preferredHeight = 180f;
                summaryLayout.minHeight = 120f;
            }

            var expectedLabel = CreateStyledText("ExpectedResultLabel", detailContent.transform, TextStyle.Body);
            expectedLabel.fontStyle = FontStyles.Italic;

            var popupRoot = CreateUIObject("ActionPopup", panelRoot.transform);
            var popupRect = popupRoot.GetComponent<RectTransform>();
            Stretch(popupRect);
            var popupImage = popupRoot.AddComponent<Image>();
            popupImage.color = new Color(0f, 0f, 0f, 0.65f);

            var popupWindow = CreateUIObject("PopupWindow", popupRoot.transform);
            var windowRect = popupWindow.GetComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(520f, 320f);
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = Vector2.zero;
            var windowImage = popupWindow.AddComponent<Image>();
            windowImage.color = new Color(0.18f, 0.2f, 0.3f, 0.95f);

            var popupContent = CreateUIObject("PopupContent", popupWindow.transform);
            var popupContentRect = popupContent.GetComponent<RectTransform>();
            popupContentRect.anchorMin = new Vector2(0f, 0f);
            popupContentRect.anchorMax = new Vector2(1f, 1f);
            popupContentRect.offsetMin = new Vector2(30f, 110f);
            popupContentRect.offsetMax = new Vector2(-30f, -140f);
            var popupLayout = popupContent.AddComponent<VerticalLayoutGroup>();
            popupLayout.spacing = 10f;
            popupLayout.childControlHeight = true;
            popupLayout.childForceExpandHeight = false;
            popupLayout.childForceExpandWidth = true;
            popupLayout.childAlignment = TextAnchor.UpperLeft;

            var popupTitle = CreateStyledText("PopupTitle", popupContent.transform, TextStyle.SectionTitle);
            var popupBody = CreateStyledText("PopupBody", popupContent.transform, TextStyle.Body);
            popupBody.textWrappingMode = TextWrappingModes.Normal;
            popupBody.overflowMode = TextOverflowModes.Overflow;
            var popupBodyLayout = popupBody.GetComponent<LayoutElement>();
            if (popupBodyLayout != null)
            {
                popupBodyLayout.preferredHeight = 160f;
                popupBodyLayout.minHeight = 120f;
            }

            var popupButtons = CreateUIObject("PopupButtons", popupWindow.transform);
            var popupButtonsRect = popupButtons.GetComponent<RectTransform>();
            popupButtonsRect.anchorMin = new Vector2(0f, 0f);
            popupButtonsRect.anchorMax = new Vector2(1f, 0f);
            popupButtonsRect.pivot = new Vector2(0.5f, 0f);
            popupButtonsRect.sizeDelta = new Vector2(0f, 80f);
            popupButtonsRect.anchoredPosition = new Vector2(0f, 20f);
            var popupButtonLayout = popupButtons.AddComponent<HorizontalLayoutGroup>();
            popupButtonLayout.spacing = 20f;
            popupButtonLayout.padding = new RectOffset(50, 50, 10, 10);

            var cancelButton = CreateNavButton("CancelButton", popupButtons.transform, "취소");
            var confirmButton = CreateNavButton("ConfirmButton", popupButtons.transform, "확인");
            ConfigurePopupButton(cancelButton);
            ConfigurePopupButton(confirmButton);

            var buttonMap = new Dictionary<FreeActionOption, Button>
            {
                { FreeActionOption.Exercise, CreateActionButton(actionsColumn.transform, "운동") },
                { FreeActionOption.Study, CreateActionButton(actionsColumn.transform, "공부") },
                { FreeActionOption.ConvenienceStore, CreateActionButton(actionsColumn.transform, "편의점 알바") },
                { FreeActionOption.WarehouseJob, CreateActionButton(actionsColumn.transform, "상하차 알바") },
                { FreeActionOption.Investment, CreateActionButton(actionsColumn.transform, "투자") },
                { FreeActionOption.OpenShop, CreateActionButton(actionsColumn.transform, "아이템 구매") }
            };

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("exerciseButton").objectReferenceValue = buttonMap[FreeActionOption.Exercise];
            serialized.FindProperty("studyButton").objectReferenceValue = buttonMap[FreeActionOption.Study];
            serialized.FindProperty("convenienceButton").objectReferenceValue = buttonMap[FreeActionOption.ConvenienceStore];
            serialized.FindProperty("warehouseButton").objectReferenceValue = buttonMap[FreeActionOption.WarehouseJob];
            serialized.FindProperty("investmentButton").objectReferenceValue = buttonMap[FreeActionOption.Investment];
            serialized.FindProperty("openShopButton").objectReferenceValue = buttonMap[FreeActionOption.OpenShop];
            serialized.FindProperty("titleLabel").objectReferenceValue = titleLabel;
            serialized.FindProperty("summaryLabel").objectReferenceValue = summaryLabel;
            serialized.FindProperty("expectedResultLabel").objectReferenceValue = expectedLabel;
            serialized.FindProperty("popupRoot").objectReferenceValue = popupRoot;
            serialized.FindProperty("popupTitleLabel").objectReferenceValue = popupTitle;
            serialized.FindProperty("popupBodyLabel").objectReferenceValue = popupBody;
            serialized.FindProperty("popupConfirmButton").objectReferenceValue = confirmButton;
            serialized.FindProperty("popupCancelButton").objectReferenceValue = cancelButton;

            var optionList = serialized.FindProperty("optionDefinitions");
            optionList.arraySize = 6;
            SetDefinition(optionList.GetArrayElementAtIndex(0), FreeActionOption.Exercise, "운동", "체력 스탯이 증가합니다.", "체력 +3 / 피로 +0", false);
            SetDefinition(optionList.GetArrayElementAtIndex(1), FreeActionOption.Study, "공부", "지성 스탯이 증가합니다.", "지성 +3 / 피로 +5", false);
            SetDefinition(optionList.GetArrayElementAtIndex(2), FreeActionOption.ConvenienceStore, "편의점 아르바이트", "끈기 +1, 20,000원 획득.", "끈기 +1 / 피로 +5", false);
            SetDefinition(optionList.GetArrayElementAtIndex(3), FreeActionOption.WarehouseJob, "상하차 아르바이트", "끈기 +2, 50,000원 획득 (하루 1회).", "끈기 +2 / 피로 +15", false);
            SetDefinition(optionList.GetArrayElementAtIndex(4), FreeActionOption.Investment, "투자", "자산 ≥30,000원부터 가능, ±50~100% 변동.", "자산 변동 / 피로 +0", false);
            SetDefinition(optionList.GetArrayElementAtIndex(5), FreeActionOption.OpenShop, "아이템 구매", "상점으로 이동하여 소모품/선물을 구매합니다.", "상점 진입", true);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            popupRoot.SetActive(false);

            return panelRoot;
        }

        private static void SetDefinition(SerializedProperty property, FreeActionOption option, string title, string summary, string expected, bool launchesShop)
        {
            property.FindPropertyRelative("option").enumValueIndex = (int)option;
            property.FindPropertyRelative("title").stringValue = title;
            property.FindPropertyRelative("summary").stringValue = summary;
            property.FindPropertyRelative("expectedResult").stringValue = expected;
            property.FindPropertyRelative("launchesShop").boolValue = launchesShop;
        }

        private static Button CreateNavButton(string name, Transform parent, string label)
        {
            return CreateStyledButton(name, parent, label, ButtonStyle.Primary);
        }

        private static Button CreateActionButton(Transform parent, string label)
        {
            var button = CreateStyledButton(label + "Button", parent, label, ButtonStyle.Secondary);
            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredHeight = 70f;
                layout.minHeight = 70f;
            }

            return button;
        }

        private static void ConfigurePopupButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.flexibleWidth = 1f;
                layout.minWidth = 0f;
            }
        }
    }
}
#endif
