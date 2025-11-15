#if UNITY_EDITOR
using System.Collections.Generic;
using LoveAlgo.UI;
using LoveAlgo.UI.Modules;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LoveAlgo.Editor
{
    /// <summary>
    /// Generates the modular HUD prefabs (HUD root + FreeAction panel) so designers can iterate without hand-building UI each time.
    /// </summary>
    public static class LoveAlgoHUDScaffolder
    {
        private const string HudFolder = "Assets/Prefabs/Simple/HUD";
        private const string ModulesFolder = "Assets/Prefabs/Simple/Modules";
        private const string HudPrefabPath = HudFolder + "/LoveAlgoHUDRoot.prefab";
        private const string FreeActionPrefabPath = ModulesFolder + "/FreeActionPanel.prefab";
        private const string ShopPrefabPath = ModulesFolder + "/ShopPanel.prefab";
        private const string PretendardFontPath = "Assets/Fonts/Pretendard-Regular.otf";
        private const string PretendardTmpFontPath = "Assets/Fonts/Pretendard-Regular SDF.asset";
        private const float CanvasWidth = 1920f;
        private const float CanvasHeight = 1080f;
        private static Font fallbackFont;
        private static TMP_FontAsset defaultTmpFont;

        private enum TextStyle
        {
            Display,
            SectionTitle,
            Body,
            Caption,
            Button,
            Currency,
            TooltipBody
        }

        private enum ButtonStyle
        {
            Primary,
            Secondary,
            Ghost
        }

        private enum PanelStyle
        {
            ScreenBackground,
            Surface,
            Highlight,
            Tooltip,
            Card,
            ScrollSurface
        }

        private readonly struct TextStyleDefinition
        {
            public TextStyleDefinition(int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, float minHeight, Color color)
            {
                FontSize = fontSize;
                FontStyle = fontStyle;
                Alignment = alignment;
                MinHeight = minHeight;
                Color = color;
            }

            public int FontSize { get; }
            public FontStyles FontStyle { get; }
            public TextAlignmentOptions Alignment { get; }
            public float MinHeight { get; }
            public Color Color { get; }
        }

        private readonly struct ButtonStyleDefinition
        {
            public ButtonStyleDefinition(Color background, Color highlight, Color pressed, Color textColor, float minHeight, float minWidth, bool flexibleWidth)
            {
                BackgroundColor = background;
                HighlightColor = highlight;
                PressedColor = pressed;
                TextColor = textColor;
                MinHeight = minHeight;
                MinWidth = minWidth;
                FlexibleWidth = flexibleWidth;
            }

            public Color BackgroundColor { get; }
            public Color HighlightColor { get; }
            public Color PressedColor { get; }
            public Color TextColor { get; }
            public float MinHeight { get; }
            public float MinWidth { get; }
            public bool FlexibleWidth { get; }
        }

        private readonly struct PanelStyleDefinition
        {
            public PanelStyleDefinition(Color background)
            {
                BackgroundColor = background;
            }

            public Color BackgroundColor { get; }
        }

        private static readonly Color PaletteTextPrimary = new Color(0.15f, 0.08f, 0.2f, 1f);
        private static readonly Color PaletteTextMuted = new Color(0.38f, 0.28f, 0.45f, 1f);
        private static readonly Color PaletteCurrency = new Color(1f, 0.73f, 0.3f, 1f);
        private static readonly Color PaletteScreen = new Color(1f, 0.86f, 0.93f, 0.95f);
        private static readonly Color PaletteSurface = new Color(1f, 0.93f, 0.96f, 0.92f);
        private static readonly Color PaletteHighlight = new Color(1f, 0.97f, 1f, 0.85f);
        private static readonly Color PaletteTooltip = new Color(1f, 1f, 1f, 0.95f);
        private static readonly Color PaletteCard = new Color(1f, 1f, 1f, 0.08f);
        private static readonly Color PaletteScroll = new Color(1f, 0.99f, 1f, 0.35f);
        private static readonly Color PaletteButtonPrimary = new Color(0.2f, 0.28f, 0.38f, 0.95f);
        private static readonly Color PaletteButtonPrimaryHighlight = new Color(0.25f, 0.34f, 0.45f, 0.95f);
        private static readonly Color PaletteButtonPrimaryPressed = new Color(0.16f, 0.23f, 0.32f, 0.95f);
        private static readonly Color PaletteButtonSecondary = new Color(0.33f, 0.28f, 0.42f, 0.95f);
        private static readonly Color PaletteButtonSecondaryHighlight = new Color(0.4f, 0.34f, 0.52f, 0.95f);
        private static readonly Color PaletteButtonSecondaryPressed = new Color(0.27f, 0.22f, 0.36f, 0.95f);
        private static readonly Color PaletteButtonGhost = new Color(0.2f, 0.16f, 0.32f, 0.8f);

        private static readonly Dictionary<TextStyle, TextStyleDefinition> TextStyleDefinitions = new()
        {
            { TextStyle.Display, new TextStyleDefinition(30, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 60f, PaletteTextPrimary) },
            { TextStyle.SectionTitle, new TextStyleDefinition(22, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 48f, PaletteTextPrimary) },
            { TextStyle.Body, new TextStyleDefinition(18, FontStyles.Normal, TextAlignmentOptions.TopLeft, 40f, PaletteTextPrimary) },
            { TextStyle.Caption, new TextStyleDefinition(16, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, 32f, PaletteTextMuted) },
            { TextStyle.Button, new TextStyleDefinition(20, FontStyles.Bold, TextAlignmentOptions.Center, 46f, Color.white) },
            { TextStyle.Currency, new TextStyleDefinition(28, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 52f, PaletteCurrency) },
            { TextStyle.TooltipBody, new TextStyleDefinition(18, FontStyles.Normal, TextAlignmentOptions.TopLeft, 120f, PaletteTextPrimary) }
        };

        private static readonly Dictionary<ButtonStyle, ButtonStyleDefinition> ButtonStyleDefinitions = new()
        {
            { ButtonStyle.Primary, new ButtonStyleDefinition(PaletteButtonPrimary, PaletteButtonPrimaryHighlight, PaletteButtonPrimaryPressed, Color.white, 60f, 0f, true) },
            { ButtonStyle.Secondary, new ButtonStyleDefinition(PaletteButtonSecondary, PaletteButtonSecondaryHighlight, PaletteButtonSecondaryPressed, Color.white, 64f, 0f, true) },
            { ButtonStyle.Ghost, new ButtonStyleDefinition(PaletteButtonGhost, PaletteButtonSecondaryHighlight, PaletteButtonSecondaryPressed, Color.white, 46f, 0f, false) }
        };

        private static readonly Dictionary<PanelStyle, PanelStyleDefinition> PanelStyleDefinitions = new()
        {
            { PanelStyle.ScreenBackground, new PanelStyleDefinition(PaletteScreen) },
            { PanelStyle.Surface, new PanelStyleDefinition(PaletteSurface) },
            { PanelStyle.Highlight, new PanelStyleDefinition(PaletteHighlight) },
            { PanelStyle.Tooltip, new PanelStyleDefinition(PaletteTooltip) },
            { PanelStyle.Card, new PanelStyleDefinition(PaletteCard) },
            { PanelStyle.ScrollSurface, new PanelStyleDefinition(PaletteScroll) }
        };

        [MenuItem("LoveAlgo/Build HUD Prefabs", priority = 40)]
        public static void BuildPrefabs()
        {
            EnsureFolder(HudFolder);
            EnsureFolder(ModulesFolder);

            EnsureDefaultFonts();

            var freeActionGo = CreateFreeActionPanel();
            SavePrefab(freeActionGo, FreeActionPrefabPath);
            Object.DestroyImmediate(freeActionGo);

            var shopGo = CreateShopPanel();
            SavePrefab(shopGo, ShopPrefabPath);
            Object.DestroyImmediate(shopGo);

            var freeActionModulePrefab = AssetDatabase.LoadAssetAtPath<LoveAlgoUIModule>(FreeActionPrefabPath);
            if (freeActionModulePrefab == null)
            {
                Debug.LogError("Failed to load FreeAction module prefab at " + FreeActionPrefabPath);
                return;
            }

            var shopModulePrefab = AssetDatabase.LoadAssetAtPath<LoveAlgoUIModule>(ShopPrefabPath);
            if (shopModulePrefab == null)
            {
                Debug.LogError("Failed to load Shop module prefab at " + ShopPrefabPath);
                return;
            }

            var hudGo = CreateHudRoot(freeActionModulePrefab, shopModulePrefab);
            SavePrefab(hudGo, HudPrefabPath);
            Object.DestroyImmediate(hudGo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("LoveAlgo HUD prefabs regenerated.");
        }

        private static GameObject CreateHudRoot(LoveAlgoUIModule freeActionPrefab, LoveAlgoUIModule shopPrefab)
        {
            var canvasGo = CreateUIRoot("LoveAlgoHUDRoot");
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var hudRoot = canvasGo.AddComponent<LoveAlgoHUDRoot>();

            var navHost = CreateUIObject("ModuleHost", canvasGo.transform);
            var navRect = navHost.GetComponent<RectTransform>();
            Stretch(navRect);
            navRect.offsetMin = new Vector2(0f, 0f);
            navRect.offsetMax = new Vector2(0f, -80f);
            var navigation = navHost.AddComponent<UINavigationController>();

            var topBar = CreateUIObject("TopBar", canvasGo.transform);
            var topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 80f);
            topRect.anchoredPosition = new Vector2(0f, 0f);
            var topImage = topBar.AddComponent<Image>();
            topImage.color = new Color(0.09f, 0.12f, 0.2f, 0.85f);

            var buttonContainer = CreateUIObject("NavButtons", topBar.transform);
            var buttonsRect = buttonContainer.GetComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0f, 0f);
            buttonsRect.anchorMax = new Vector2(1f, 1f);
            buttonsRect.offsetMin = new Vector2(200f, 10f);
            buttonsRect.offsetMax = new Vector2(-200f, -10f);
            var hLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.childForceExpandHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.spacing = 15f;
            hLayout.padding = new RectOffset(10, 10, 10, 10);

            var freeActionButton = CreateNavButton("FreeActionButton", buttonContainer.transform, "자유행동");
            var shopButton = CreateNavButton("ShopButton", buttonContainer.transform, "상점");
            var messengerButton = CreateNavButton("MessengerButton", buttonContainer.transform, "메신저");
            ConfigureNavLayoutElement(freeActionButton);
            ConfigureNavLayoutElement(shopButton);
            ConfigureNavLayoutElement(messengerButton);

            var hudSerialized = new SerializedObject(hudRoot);
            hudSerialized.FindProperty("navigationController").objectReferenceValue = navigation;
            hudSerialized.FindProperty("freeActionModule").objectReferenceValue = freeActionPrefab;
            hudSerialized.FindProperty("shopModule").objectReferenceValue = shopPrefab;
            hudSerialized.FindProperty("freeActionNavButton").objectReferenceValue = freeActionButton;
            hudSerialized.FindProperty("shopNavButton").objectReferenceValue = shopButton;
            hudSerialized.FindProperty("messengerNavButton").objectReferenceValue = messengerButton;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            var navSerialized = new SerializedObject(navigation);
            navSerialized.FindProperty("moduleHost").objectReferenceValue = navRect;
            navSerialized.ApplyModifiedPropertiesWithoutUndo();
            return canvasGo;
        }

        private static GameObject CreateFreeActionPanel()
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

        private static GameObject CreateShopPanel()
        {
            var panelRoot = CreateStyledPanel("ShopPanel", null, PanelStyle.ScreenBackground);
            var rect = panelRoot.GetComponent<RectTransform>();
            Stretch(rect);

            var controller = panelRoot.AddComponent<ShopPanelController>();
            const float outerPadding = 24f;
            const float columnGap = 20f;
            const float leftColumnWidth = 620f;
            const float leftInnerPadding = 24f;
            const float blockSpacing = 18f;
            const float moneyPanelHeight = 126f;
            const float totalRowHeight = 64f;
            const float purchaseButtonHeight = 70f;
            const float quickButtonHeight = 70f;

            var leftColumn = CreateStyledPanel("CartColumn", panelRoot.transform, PanelStyle.Surface);
            var leftRect = leftColumn.GetComponent<RectTransform>();
            var leftColumnHeight = CanvasHeight - outerPadding * 2f;
            SetPixelRect(leftRect, outerPadding, outerPadding, leftColumnWidth, leftColumnHeight);
            var leftContentWidth = leftColumnWidth - leftInnerPadding * 2f;

            var moneyPanel = CreateStyledPanel("MoneyPanel", leftColumn.transform, PanelStyle.Highlight);
            var moneyRect = moneyPanel.GetComponent<RectTransform>();
            SetPixelRect(moneyRect, leftInnerPadding, leftInnerPadding, leftContentWidth, moneyPanelHeight);
            var moneyHeader = CreateStyledText("MoneyHeader", moneyPanel.transform, TextStyle.Caption, false);
            moneyHeader.text = "MONEY";
            SetPixelRect(moneyHeader.rectTransform, 18f, 16f, leftContentWidth - 36f, 34f);
            var moneyValue = CreateStyledText("MoneyValue", moneyPanel.transform, TextStyle.Currency, false);
            moneyValue.text = "₩0";
            SetPixelRect(moneyValue.rectTransform, 18f, 56f, leftContentWidth - 36f, 52f);

            var cartLabel = CreateStyledText("CartLabel", leftColumn.transform, TextStyle.SectionTitle, false);
            cartLabel.text = "CART";
            var cursorY = leftInnerPadding + moneyPanelHeight + blockSpacing;
            SetPixelRect(cartLabel.rectTransform, leftInnerPadding, cursorY, leftContentWidth, 40f);
            cursorY += 40f + blockSpacing;

            var cartScroll = CreateScrollView(leftColumn.transform, out var cartContent, "CartScroll", PanelStyle.ScrollSurface);
            var cartScrollRect = cartScroll.GetComponent<RectTransform>();
            cartScroll.vertical = true;
            cartScroll.horizontal = false;
            var cartContentLayout = cartContent.gameObject.AddComponent<VerticalLayoutGroup>();
            cartContentLayout.spacing = 10f;
            cartContentLayout.childForceExpandHeight = false;
            cartContentLayout.childForceExpandWidth = true;
            cartContentLayout.childControlWidth = true;

            var bottomCursor = leftColumnHeight - leftInnerPadding;

            var gachaButton = CreateStyledButton("GachaButton", leftColumn.transform, "가챠 보러가기", ButtonStyle.Secondary, TextStyle.Button, false);
            var gachaRect = gachaButton.GetComponent<RectTransform>();
            var quickButtonWidth = (leftContentWidth - 12f) * 0.5f;
            bottomCursor -= quickButtonHeight;
            SetPixelRect(gachaRect, leftInnerPadding, bottomCursor, quickButtonWidth, quickButtonHeight);

            var exitButton = CreateStyledButton("ExitButton", leftColumn.transform, "돌아가기", ButtonStyle.Secondary, TextStyle.Button, false);
            var exitRect = exitButton.GetComponent<RectTransform>();
            SetPixelRect(exitRect, leftInnerPadding + quickButtonWidth + 12f, bottomCursor, quickButtonWidth, quickButtonHeight);
            bottomCursor -= blockSpacing;

            var purchaseButton = CreateStyledButton("PurchaseButton", leftColumn.transform, "구매하기", ButtonStyle.Primary, TextStyle.Button, false);
            var purchaseRect = purchaseButton.GetComponent<RectTransform>();
            bottomCursor -= purchaseButtonHeight;
            SetPixelRect(purchaseRect, leftInnerPadding, bottomCursor, leftContentWidth, purchaseButtonHeight);
            bottomCursor -= blockSpacing;

            var totalRow = CreateStyledPanel("TotalRow", leftColumn.transform, PanelStyle.Highlight);
            var totalRect = totalRow.GetComponent<RectTransform>();
            bottomCursor -= totalRowHeight;
            SetPixelRect(totalRect, leftInnerPadding, bottomCursor, leftContentWidth, totalRowHeight);
            bottomCursor -= blockSpacing;
            var titleWidth = leftContentWidth * 0.5f;
            var totalLabelTitle = CreateStyledText("TotalTitle", totalRow.transform, TextStyle.Body, false);
            totalLabelTitle.text = "합계 금액";
            SetPixelRect(totalLabelTitle.rectTransform, 18f, 12f, titleWidth - 12f, totalRowHeight - 24f);
            var totalValueLabel = CreateStyledText("TotalValue", totalRow.transform, TextStyle.Currency, false);
            totalValueLabel.text = "₩0";
            SetPixelRect(totalValueLabel.rectTransform, 18f + titleWidth, 12f, leftContentWidth - titleWidth - 18f, totalRowHeight - 24f);

            var remainingHeight = bottomCursor - cursorY;
            if (remainingHeight < 160f)
            {
                remainingHeight = 160f;
            }
            SetPixelRect(cartScrollRect, leftInnerPadding, cursorY, leftContentWidth, remainingHeight);

            var cartEntryPrototype = CreateCartEntryPrototype(cartContent);
            cartEntryPrototype.gameObject.SetActive(false);

            var rightColumn = CreateStyledPanel("ItemColumn", panelRoot.transform, PanelStyle.Highlight);
            var rightRect = rightColumn.GetComponent<RectTransform>();
            SetStretchRect(rightRect, leftColumnWidth + outerPadding + columnGap, outerPadding, outerPadding, outerPadding);

            var itemsScroll = CreateScrollView(rightColumn.transform, out var itemContent, "ItemsScroll", PanelStyle.ScrollSurface);
            itemsScroll.vertical = true;
            itemsScroll.horizontal = false;
            var itemsRect = itemsScroll.GetComponent<RectTransform>();
            SetStretchRect(itemsRect, leftInnerPadding, leftInnerPadding, leftInnerPadding, leftInnerPadding);
            var grid = itemContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(220f, 260f);
            grid.spacing = new Vector2(20f, 20f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            var itemCardPrototype = CreateShopItemCardPrototype(itemContent);
            itemCardPrototype.gameObject.SetActive(false);

            var tooltipRoot = CreateStyledPanel("TooltipPanel", rightColumn.transform, PanelStyle.Tooltip);
            var tooltipRect = tooltipRoot.GetComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(1f, 1f);
            tooltipRect.anchorMax = new Vector2(1f, 1f);
            tooltipRect.pivot = new Vector2(1f, 1f);
            tooltipRect.sizeDelta = new Vector2(360f, 260f);
            tooltipRect.anchoredPosition = new Vector2(-leftInnerPadding, -(leftInnerPadding + 16f));
            var tooltipLayout = tooltipRoot.AddComponent<VerticalLayoutGroup>();
            tooltipLayout.padding = new RectOffset(20, 20, 20, 20);
            tooltipLayout.spacing = 10f;
            tooltipLayout.childAlignment = TextAnchor.UpperLeft;
            var tooltipTitle = CreateStyledText("TooltipTitle", tooltipRoot.transform, TextStyle.SectionTitle);
            var tooltipBody = CreateStyledText("TooltipBody", tooltipRoot.transform, TextStyle.TooltipBody);
            tooltipBody.textWrappingMode = TextWrappingModes.Normal;
            tooltipBody.overflowMode = TextOverflowModes.Overflow;
            var tooltipComponent = tooltipRoot.AddComponent<ShopTooltipPanel>();
            tooltipRoot.SetActive(false);

            var controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("moneyLabel").objectReferenceValue = moneyValue;
            controllerSerialized.FindProperty("totalLabel").objectReferenceValue = totalValueLabel;
            controllerSerialized.FindProperty("purchaseButton").objectReferenceValue = purchaseButton;
            controllerSerialized.FindProperty("exitButton").objectReferenceValue = exitButton;
            controllerSerialized.FindProperty("gachaButton").objectReferenceValue = gachaButton;
            controllerSerialized.FindProperty("itemGrid").objectReferenceValue = itemContent;
            controllerSerialized.FindProperty("itemCardPrototype").objectReferenceValue = itemCardPrototype;
            controllerSerialized.FindProperty("cartList").objectReferenceValue = cartContent;
            controllerSerialized.FindProperty("cartEntryPrototype").objectReferenceValue = cartEntryPrototype;
            controllerSerialized.FindProperty("tooltipPanel").objectReferenceValue = tooltipComponent;

            var inventoryProperty = controllerSerialized.FindProperty("mockInventory");
            inventoryProperty.arraySize = 4;
            SetShopItemDefinition(inventoryProperty.GetArrayElementAtIndex(0), "popup_ticket", "러블리-톡! 팝업 입장권", 38900, "봄이와 함께 팝업스토어에 갈 수 있는 티켓.");
            SetShopItemDefinition(inventoryProperty.GetArrayElementAtIndex(1), "empire_diamond", "엠파이어 다이아몬드", 168000, "화려한 보석. 선물로 주면 호감도가 크게 오릅니다.");
            SetShopItemDefinition(inventoryProperty.GetArrayElementAtIndex(2), "night_snack", "알밤달밤 밤식빵", 6500, "밤에 간단히 먹기 좋은 간식.");
            SetShopItemDefinition(inventoryProperty.GetArrayElementAtIndex(3), "tropical_album", "트로피컬 글로우 앨범", 25300, "여름 축제 한정 앨범.");

            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            var tooltipSerialized = new SerializedObject(tooltipComponent);
            tooltipSerialized.FindProperty("root").objectReferenceValue = tooltipRoot;
            tooltipSerialized.FindProperty("titleLabel").objectReferenceValue = tooltipTitle;
            tooltipSerialized.FindProperty("bodyLabel").objectReferenceValue = tooltipBody;
            tooltipSerialized.ApplyModifiedPropertiesWithoutUndo();

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

        private static ScrollRect CreateScrollView(Transform parent, out RectTransform content, string name = "ScrollView", PanelStyle style = PanelStyle.ScrollSurface)
        {
            var scrollGo = CreateStyledPanel(name, parent, style);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();

            var viewport = CreateUIObject("Viewport", scrollGo.transform);
            var viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            var viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var contentGo = CreateUIObject("Content", viewport.transform);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.offsetMin = new Vector2(10f, 10f);
            content.offsetMax = new Vector2(-10f, -10f);

            scrollRect.viewport = viewportRect;
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            return scrollRect;
        }

        private static ShopCartEntryView CreateCartEntryPrototype(RectTransform parent)
        {
            var entryGo = CreateStyledPanel("CartEntryPrototype", parent, PanelStyle.Card);
            var rect = entryGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 90f);
            var layout = entryGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            var entryElement = entryGo.AddComponent<LayoutElement>();
            entryElement.preferredHeight = 90f;
            entryElement.minHeight = 80f;

            var info = CreateUIObject("Info", entryGo.transform);
            var infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 4f;
            infoLayout.childAlignment = TextAnchor.UpperLeft;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childControlHeight = true;
            var infoElement = info.AddComponent<LayoutElement>();
            infoElement.flexibleWidth = 1f;
            infoElement.minWidth = 180f;
            var nameLabel = CreateStyledText("NameLabel", info.transform, TextStyle.SectionTitle);
            var priceLabel = CreateStyledText("PriceLabel", info.transform, TextStyle.Body);
            priceLabel.color = PaletteCurrency;

            var decrementButton = CreatePillButton("DecrementButton", entryGo.transform, "-");
            var quantityLabel = CreateStyledText("QuantityLabel", entryGo.transform, TextStyle.SectionTitle, false);
            quantityLabel.alignment = TextAlignmentOptions.Center;
            var quantityLayout = quantityLabel.gameObject.AddComponent<LayoutElement>();
            quantityLayout.preferredWidth = 52f;
            quantityLayout.minWidth = 52f;
            quantityLayout.flexibleWidth = 0f;
            var incrementButton = CreatePillButton("IncrementButton", entryGo.transform, "+");
            var removeButton = CreatePillButton("RemoveButton", entryGo.transform, "삭제");
            var removeLayout = removeButton.GetComponent<LayoutElement>();
            if (removeLayout != null)
            {
                removeLayout.preferredWidth = 100f;
                removeLayout.minWidth = 100f;
            }

            var view = entryGo.AddComponent<ShopCartEntryView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("nameLabel").objectReferenceValue = nameLabel;
            serialized.FindProperty("priceLabel").objectReferenceValue = priceLabel;
            serialized.FindProperty("quantityLabel").objectReferenceValue = quantityLabel;
            serialized.FindProperty("incrementButton").objectReferenceValue = incrementButton;
            serialized.FindProperty("decrementButton").objectReferenceValue = decrementButton;
            serialized.FindProperty("removeButton").objectReferenceValue = removeButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static ShopItemCard CreateShopItemCardPrototype(RectTransform parent)
        {
            var cardGo = CreateStyledPanel("ItemCardPrototype", parent, PanelStyle.Card);
            var rect = cardGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 260f);
            // 수정: Background 자식에서 Image 찾기
            var backgroundTransform = cardGo.transform.Find("Background");
            if (backgroundTransform == null)
            {
                Debug.LogError("Background child not found in styled panel");
                return null;
            }
            var background = backgroundTransform.GetComponent<Image>();
            if (background == null)
            {
                Debug.LogError("Image component not found on Background");
                return null;
            }
            background.raycastTarget = true;
            var button = cardGo.AddComponent<Button>();
            var buttonColors = button.colors;
            buttonColors.normalColor = background.color;
            buttonColors.highlightedColor = new Color(
                Mathf.Clamp01(background.color.r + 0.08f),
                Mathf.Clamp01(background.color.g + 0.08f),
                Mathf.Clamp01(background.color.b + 0.08f),
                Mathf.Clamp01(background.color.a + 0.05f));
            buttonColors.pressedColor = new Color(
                Mathf.Clamp01(background.color.r + 0.02f),
                Mathf.Clamp01(background.color.g + 0.02f),
                Mathf.Clamp01(background.color.b + 0.02f),
                Mathf.Clamp01(background.color.a + 0.1f));
            button.colors = buttonColors;
            var layout = cardGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 25, 25);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var nameLabel = CreateStyledText("NameLabel", cardGo.transform, TextStyle.SectionTitle);
            nameLabel.alignment = TextAlignmentOptions.Center;
            var priceLabel = CreateStyledText("PriceLabel", cardGo.transform, TextStyle.Currency);
            priceLabel.alignment = TextAlignmentOptions.Center;

            var indicator = CreateUIObject("SelectionIndicator", cardGo.transform);
            var indicatorRect = indicator.GetComponent<RectTransform>();
            Stretch(indicatorRect);
            var indicatorImage = indicator.AddComponent<Image>();
            indicatorImage.color = new Color(1f, 1f, 1f, 0.15f);
            indicatorImage.raycastTarget = false;
            indicator.SetActive(false);

            var cardComponent = cardGo.AddComponent<ShopItemCard>();
            var serialized = new SerializedObject(cardComponent);
            serialized.FindProperty("selectButton").objectReferenceValue = button;
            serialized.FindProperty("nameLabel").objectReferenceValue = nameLabel;
            serialized.FindProperty("priceLabel").objectReferenceValue = priceLabel;
            serialized.FindProperty("selectionIndicator").objectReferenceValue = indicator;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return cardComponent;
        }

        private static void SetShopItemDefinition(SerializedProperty property, string id, string title, int price, string description)
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("displayName").stringValue = title;
            property.FindPropertyRelative("price").intValue = price;
            property.FindPropertyRelative("description").stringValue = description;
        }

        private static GameObject CreateStyledPanel(string name, Transform parent, PanelStyle style)
        {
            var panel = CreateUIObject(name, parent);
            var background = CreateUIObject("Background", panel.transform);
            var bgRect = background.GetComponent<RectTransform>();
            Stretch(bgRect);
            var image = background.AddComponent<Image>();
            if (!PanelStyleDefinitions.TryGetValue(style, out var definition))
            {
                definition = new PanelStyleDefinition(new Color(1f, 1f, 1f, 0.5f));
            }

            image.color = definition.BackgroundColor;
            return panel;
        }

        private static TextMeshProUGUI CreateStyledText(string name, Transform parent, TextStyle style, bool addLayoutElement = true)
        {
            if (!TextStyleDefinitions.TryGetValue(style, out var definition))
            {
                definition = new TextStyleDefinition(20, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, 40f, Color.white);
            }

            var text = CreateTMPText(name, parent, definition.FontSize, definition.FontStyle, definition.Alignment, addLayoutElement, definition.MinHeight);
            text.color = definition.Color;
            return text;
        }

        private static Button CreateStyledButton(string name, Transform parent, string label, ButtonStyle style, TextStyle labelStyle = TextStyle.Button, bool addLayoutElement = true)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            if (!ButtonStyleDefinitions.TryGetValue(style, out var definition))
            {
                definition = ButtonStyleDefinitions[ButtonStyle.Primary];
            }

            var image = buttonGo.GetComponent<Image>();
            image.color = definition.BackgroundColor;

            var rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, definition.MinHeight);

            LayoutElement layout = null;
            if (addLayoutElement)
            {
                layout = buttonGo.AddComponent<LayoutElement>();
                layout.preferredHeight = definition.MinHeight;
                layout.minHeight = definition.MinHeight;
                layout.flexibleWidth = definition.FlexibleWidth ? 1f : 0f;
                if (definition.MinWidth > 0f)
                {
                    layout.preferredWidth = definition.MinWidth;
                    layout.minWidth = definition.MinWidth;
                }
            }

            var text = CreateStyledText("Label", buttonGo.transform, labelStyle, false);
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            text.color = definition.TextColor;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var button = buttonGo.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = definition.BackgroundColor;
            colors.highlightedColor = definition.HighlightColor;
            colors.pressedColor = definition.PressedColor;
            colors.selectedColor = definition.HighlightColor;
            colors.disabledColor = new Color(definition.BackgroundColor.r, definition.BackgroundColor.g, definition.BackgroundColor.b, definition.BackgroundColor.a * 0.35f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            return button;
        }

        private static GameObject CreateUIRoot(string name)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var rect = canvasGo.GetComponent<RectTransform>();
            Stretch(rect);
            return canvasGo;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            return go;
        }

        private static Button CreateNavButton(string name, Transform parent, string label)
        {
            return CreateStyledButton(name, parent, label, ButtonStyle.Primary);
        }

        private static Button CreateActionButton(Transform parent, string label)
        {
            var button = CreateStyledButton(label + "Button", parent, label, ButtonStyle.Secondary);
            var layout = button.GetComponent<LayoutElement>();
            layout.preferredHeight = 70f;
            layout.minHeight = 70f;
            return button;
        }

        private static Button CreatePillButton(string name, Transform parent, string label)
        {
            var button = CreateStyledButton(name, parent, label, ButtonStyle.Ghost);
            var layout = button.GetComponent<LayoutElement>();
            layout.flexibleWidth = 0f;
            layout.preferredWidth = 72f;
            layout.minWidth = 52f;
            layout.preferredHeight = 46f;
            layout.minHeight = 46f;
            return button;
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, int fontSize, FontStyles style, TextAlignmentOptions alignment, bool addLayoutElement = true, float minHeight = 40f)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);
            EnsureDefaultFonts();
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.font = defaultTmpFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.richText = true;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            if (addLayoutElement)
            {
                var layout = textGo.AddComponent<LayoutElement>();
                layout.preferredHeight = minHeight;
                layout.minHeight = minHeight;
                layout.flexibleWidth = 1f;
            }
            return text;
        }

        private static void EnsureDefaultFonts()
        {
            if (defaultTmpFont == null)
            {
                defaultTmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PretendardTmpFontPath);
                if (defaultTmpFont == null)
                {
                    defaultTmpFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                    Debug.LogWarning("Pretendard-Regular SDF.asset 폰트를 찾을 수 없습니다. LiberationSans SDF를 사용합니다.");
                }
            }

            if (fallbackFont == null)
            {
                fallbackFont = AssetDatabase.LoadAssetAtPath<Font>(PretendardFontPath);
                if (fallbackFont == null)
                {
                    fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (fallbackFont == null)
                    {
                        fallbackFont = Font.CreateDynamicFontFromOSFont("Arial", 20);
                    }
                }
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetPixelRect(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left, -top);
        }

        private static void SetStretchRect(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path, out _);
            EditorUtility.SetDirty(prefab);
            return prefab;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var segments = folderPath.Split('/');
            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        private static void ConfigureNavLayoutElement(Button button)
        {
            var layout = button != null ? button.GetComponent<LayoutElement>() : null;
            if (layout == null)
            {
                return;
            }

            layout.flexibleWidth = 1f;
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
