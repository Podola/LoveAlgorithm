using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoveAlgo.UI.Modules
{
    /// <summary>
    /// Simple view component representing a single entry inside the cart list.
    /// </summary>
    public sealed class ShopCartEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private TMP_Text quantityLabel;
        [SerializeField] private Button incrementButton;
        [SerializeField] private Button decrementButton;
        [SerializeField] private Button removeButton;

        private UnityAction incrementHandler;
        private UnityAction decrementHandler;
        private UnityAction removeHandler;

        private void Awake()
        {
            if (incrementButton != null)
            {
                incrementButton.onClick.AddListener(() => incrementHandler?.Invoke());
            }

            if (decrementButton != null)
            {
                decrementButton.onClick.AddListener(() => decrementHandler?.Invoke());
            }

            if (removeButton != null)
            {
                removeButton.onClick.AddListener(() => removeHandler?.Invoke());
            }
        }

        public void Initialize(UnityAction onIncrement, UnityAction onDecrement, UnityAction onRemove)
        {
            incrementHandler = onIncrement;
            decrementHandler = onDecrement;
            removeHandler = onRemove;
        }

        public void Refresh(ShopPanelController.ShopItemDefinition definition, int quantity)
        {
            if (nameLabel != null)
            {
                nameLabel.text = definition?.displayName ?? "-";
            }

            if (priceLabel != null)
            {
                priceLabel.text = definition != null ? definition.price.ToString("#,0") + "원" : "-";
            }

            if (quantityLabel != null)
            {
                quantityLabel.text = quantity.ToString();
            }
        }
    }
}
