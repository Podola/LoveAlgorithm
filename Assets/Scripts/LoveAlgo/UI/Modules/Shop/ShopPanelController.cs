using System;
using System.Collections.Generic;
using LoveAlgo.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoveAlgo.UI.Modules
{
    /// <summary>
    /// Skeleton controller for the in-game shop UI. Handles list/card wiring and exposes events for future shopping logic.
    /// </summary>
    public sealed class ShopPanelController : LoveAlgoUIModule
    {
        [Header("Displays")]
        [SerializeField] private TMP_Text moneyLabel;
        [SerializeField] private TMP_Text totalLabel;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button gachaButton;

        [Header("Item Listing")]
        [SerializeField] private RectTransform itemGrid;
        [SerializeField] private ShopItemCard itemCardPrototype;

        [Header("Cart Listing")]
        [SerializeField] private RectTransform cartList;
        [SerializeField] private ShopCartEntryView cartEntryPrototype;

        [Header("Tooltip")]
        [SerializeField] private ShopTooltipPanel tooltipPanel;

        [Header("Mock Inventory")]
        [SerializeField] private List<ShopItemDefinition> mockInventory = new();

        [Header("Events")]
        [SerializeField] private UnityEvent onPurchaseRequested;
        [SerializeField] private UnityEvent onExitRequested;
        [SerializeField] private UnityEvent onGachaRequested;

        private readonly List<ShopItemCard> spawnedCards = new();
        private readonly List<CartEntry> cartEntries = new();

        protected override void OnInitialized()
        {
            EnsurePrototypesHidden();
            BuildItemGrid();
            HookButtons();
            UpdateTotals();
        }

        public override void OnShow()
        {
            base.OnShow();
            tooltipPanel?.Hide();
        }

        private void HookButtons()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(() => onPurchaseRequested?.Invoke());
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() => onExitRequested?.Invoke());
            }

            if (gachaButton != null)
            {
                gachaButton.onClick.RemoveAllListeners();
                gachaButton.onClick.AddListener(() => onGachaRequested?.Invoke());
            }
        }

        private void EnsurePrototypesHidden()
        {
            if (itemCardPrototype != null)
            {
                itemCardPrototype.gameObject.SetActive(false);
            }

            if (cartEntryPrototype != null)
            {
                cartEntryPrototype.gameObject.SetActive(false);
            }
        }

        private void BuildItemGrid()
        {
            if (itemGrid == null || itemCardPrototype == null)
            {
                return;
            }

            ClearSpawnedItems();

            foreach (var definition in mockInventory)
            {
                var card = Instantiate(itemCardPrototype, itemGrid);
                card.gameObject.SetActive(true);
                card.Initialize(definition, OnItemClicked, OnItemHovered, OnItemHoverEnded);
                spawnedCards.Add(card);
            }
        }

        private void ClearSpawnedItems()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null)
                {
                    DestroyObject(card.gameObject);
                }
            }

            spawnedCards.Clear();
        }

        private void OnItemClicked(ShopItemDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            var entry = cartEntries.Find(e => e.Definition == definition);
            if (entry == null)
            {
                entry = new CartEntry(definition);
                cartEntries.Add(entry);
                CreateCartView(entry);
            }
            else
            {
                entry.Quantity = Mathf.Clamp(entry.Quantity + 1, 1, 99);
                entry.View?.Refresh(entry.Definition, entry.Quantity);
            }

            UpdateTotals();
        }

        private void OnItemHovered(ShopItemDefinition definition)
        {
            tooltipPanel?.Show(definition);
        }

        private void OnItemHoverEnded(ShopItemDefinition definition)
        {
            tooltipPanel?.Hide();
        }

        private void CreateCartView(CartEntry entry)
        {
            if (cartList == null || cartEntryPrototype == null)
            {
                return;
            }

            var view = Instantiate(cartEntryPrototype, cartList);
            view.gameObject.SetActive(true);
            view.Initialize(
                () => OnIncrementRequested(entry),
                () => OnDecrementRequested(entry),
                () => OnRemoveRequested(entry));
            view.Refresh(entry.Definition, entry.Quantity);
            entry.View = view;
        }

        private void OnIncrementRequested(CartEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            entry.Quantity = Mathf.Clamp(entry.Quantity + 1, 1, 99);
            entry.View?.Refresh(entry.Definition, entry.Quantity);
            UpdateTotals();
        }

        private void OnDecrementRequested(CartEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            entry.Quantity = Mathf.Clamp(entry.Quantity - 1, 0, 99);
            if (entry.Quantity <= 0)
            {
                RemoveEntry(entry);
            }
            else
            {
                entry.View?.Refresh(entry.Definition, entry.Quantity);
            }

            UpdateTotals();
        }

        private void OnRemoveRequested(CartEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            RemoveEntry(entry);
            UpdateTotals();
        }

        private void RemoveEntry(CartEntry entry)
        {
            if (cartEntries.Remove(entry) && entry.View != null)
            {
                DestroyObject(entry.View.gameObject);
            }
        }

        private void UpdateTotals()
        {
            var total = 0;
            foreach (var entry in cartEntries)
            {
                total += entry.Definition != null ? entry.Definition.price * entry.Quantity : 0;
            }

            if (totalLabel != null)
            {
                totalLabel.text = total.ToString("#,0") + "원";
            }
        }

        private static void DestroyObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        [Serializable]
        public sealed class ShopItemDefinition
        {
            public string id;
            public string displayName;
            public int price;
            [TextArea] public string description;
        }

        [Serializable]
        public sealed class ShopItemDefinitionEvent : UnityEvent<ShopItemDefinition> { }

        [Serializable]
        private sealed class CartEntry
        {
            public CartEntry(ShopItemDefinition definition)
            {
                Definition = definition;
                Quantity = 1;
            }

            public ShopItemDefinition Definition { get; }
            public int Quantity { get; set; }
            public ShopCartEntryView View { get; set; }
        }
    }
}
