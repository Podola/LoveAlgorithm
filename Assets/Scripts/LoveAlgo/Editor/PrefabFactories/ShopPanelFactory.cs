#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LoveAlgo.UI.Modules;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static LoveAlgo.Editor.PrefabFactories.PrefabBuilderUtils;

namespace LoveAlgo.Editor.PrefabFactories
{
    public sealed class ShopPanelFactory : IPrefabFactory
    {
        private const string PrefabPath = "Assets/Prefabs/Simple/Modules/ShopPanel.prefab";
        public static string DefaultOutputPath => PrefabPath;

        public string DisplayName => "Shop Panel";
        public string OutputPath => PrefabPath;
        public IEnumerable<FactoryDependency> Dependencies => Array.Empty<FactoryDependency>();

        public GameObject BuildPrefab()
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
            if (cartEntryPrototype != null)
            {
                cartEntryPrototype.gameObject.SetActive(false);
            }

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
            if (itemCardPrototype != null)
            {
                itemCardPrototype.gameObject.SetActive(false);
            }

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

        private static Button CreatePillButton(string name, Transform parent, string label)
        {
            var button = CreateStyledButton(name, parent, label, ButtonStyle.Ghost);
            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.flexibleWidth = 0f;
                layout.preferredWidth = 72f;
                layout.minWidth = 52f;
                layout.preferredHeight = 46f;
                layout.minHeight = 46f;
            }

            return button;
        }

        private static void SetShopItemDefinition(SerializedProperty property, string id, string title, int price, string description)
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("displayName").stringValue = title;
            property.FindPropertyRelative("price").intValue = price;
            property.FindPropertyRelative("description").stringValue = description;
        }
    }
}
#endif
