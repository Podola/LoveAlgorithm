#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LoveAlgo.Editor.PrefabFactories
{
    /// <summary>
    /// Shared constants and helper methods for prefab factories. Centralizing layout/color/TMP helpers keeps the modules consistent.
    /// </summary>
    public static class PrefabBuilderUtils
    {
        public const float CanvasWidth = 1920f;
        public const float CanvasHeight = 1080f;

        private const string PretendardFontPath = "Assets/Fonts/Pretendard-Regular.otf";
        private const string PretendardTmpFontPath = "Assets/Fonts/Pretendard-Regular SDF.asset";

        private static Font s_FallbackFont;
        private static TMP_FontAsset s_DefaultTmpFont;

        #region Style Definitions

        public enum TextStyle
        {
            Display,
            SectionTitle,
            Body,
            Caption,
            Button,
            Currency,
            TooltipBody
        }

        public enum ButtonStyle
        {
            Primary,
            Secondary,
            Ghost
        }

        public enum PanelStyle
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

        public static readonly Color PaletteTextPrimary = new(0.15f, 0.08f, 0.2f, 1f);
        public static readonly Color PaletteTextMuted = new(0.38f, 0.28f, 0.45f, 1f);
        public static readonly Color PaletteCurrency = new(1f, 0.73f, 0.3f, 1f);
        public static readonly Color PaletteScreen = new(1f, 0.86f, 0.93f, 0.95f);
        public static readonly Color PaletteSurface = new(1f, 0.93f, 0.96f, 0.92f);
        public static readonly Color PaletteHighlight = new(1f, 0.97f, 1f, 0.85f);
        public static readonly Color PaletteTooltip = new(1f, 1f, 1f, 0.95f);
        public static readonly Color PaletteCard = new(1f, 1f, 1f, 0.08f);
        public static readonly Color PaletteScroll = new(1f, 0.99f, 1f, 0.35f);
        public static readonly Color PaletteButtonPrimary = new(0.2f, 0.28f, 0.38f, 0.95f);
        public static readonly Color PaletteButtonPrimaryHighlight = new(0.25f, 0.34f, 0.45f, 0.95f);
        public static readonly Color PaletteButtonPrimaryPressed = new(0.16f, 0.23f, 0.32f, 0.95f);
        public static readonly Color PaletteButtonSecondary = new(0.33f, 0.28f, 0.42f, 0.95f);
        public static readonly Color PaletteButtonSecondaryHighlight = new(0.4f, 0.34f, 0.52f, 0.95f);
        public static readonly Color PaletteButtonSecondaryPressed = new(0.27f, 0.22f, 0.36f, 0.95f);
        public static readonly Color PaletteButtonGhost = new(0.2f, 0.16f, 0.32f, 0.8f);

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

        #endregion

        #region Font Helpers

        public static void EnsureDefaultFonts()
        {
            if (s_DefaultTmpFont == null)
            {
                s_DefaultTmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PretendardTmpFontPath);
                if (s_DefaultTmpFont == null)
                {
                    s_DefaultTmpFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                    Debug.LogWarning("Pretendard-Regular SDF.asset 폰트를 찾을 수 없습니다. LiberationSans SDF를 사용합니다.");
                }
            }

            if (s_FallbackFont == null)
            {
                s_FallbackFont = AssetDatabase.LoadAssetAtPath<Font>(PretendardFontPath);
                if (s_FallbackFont == null)
                {
                    s_FallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (s_FallbackFont == null)
                    {
                        s_FallbackFont = Font.CreateDynamicFontFromOSFont("Arial", 20);
                    }
                }
            }
        }

        public static TMP_FontAsset DefaultTmpFont
        {
            get
            {
                EnsureDefaultFonts();
                return s_DefaultTmpFont;
            }
        }

        public static Font FallbackFont
        {
            get
            {
                EnsureDefaultFonts();
                return s_FallbackFont;
            }
        }

        #endregion

        #region UI Creation Helpers

        public static GameObject CreateUIRoot(string name)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Stretch(canvasGo.GetComponent<RectTransform>());
            return canvasGo;
        }

        public static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        public static GameObject CreateStyledPanel(string name, Transform parent, PanelStyle style)
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

        public static TextMeshProUGUI CreateStyledText(string name, Transform parent, TextStyle style, bool addLayoutElement = true)
        {
            if (!TextStyleDefinitions.TryGetValue(style, out var definition))
            {
                definition = new TextStyleDefinition(20, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, 40f, Color.white);
            }

            var text = CreateTMPText(name, parent, definition.FontSize, definition.FontStyle, definition.Alignment, addLayoutElement, definition.MinHeight);
            text.color = definition.Color;
            return text;
        }

        public static Button CreateStyledButton(string name, Transform parent, string label, ButtonStyle style, TextStyle labelStyle = TextStyle.Button, bool addLayoutElement = true)
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

            if (addLayoutElement)
            {
                var layout = buttonGo.AddComponent<LayoutElement>();
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

        public static ScrollRect CreateScrollView(Transform parent, out RectTransform content, string name = "ScrollView", PanelStyle style = PanelStyle.ScrollSurface)
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

        public static TextMeshProUGUI CreateTMPText(string name, Transform parent, int fontSize, FontStyles style, TextAlignmentOptions alignment, bool addLayoutElement = true, float minHeight = 40f)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);
            EnsureDefaultFonts();
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.font = s_DefaultTmpFont;
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

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void SetPixelRect(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left, -top);
        }

        public static void SetStretchRect(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path, out _);
            EditorUtility.SetDirty(prefab);
            return prefab;
        }

        public static void EnsureFolder(string folderPath)
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

        #endregion
    }
}
#endif
