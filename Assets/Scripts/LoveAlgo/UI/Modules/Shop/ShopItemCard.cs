using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LoveAlgo.UI.Modules
{
    /// <summary>
    /// Represents a single item tile in the shop grid. Handles selection and hover callbacks.
    /// </summary>
    public sealed class ShopItemCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private GameObject selectionIndicator;

        private ShopPanelController.ShopItemDefinition definition;
        private UnityAction<ShopPanelController.ShopItemDefinition> clickHandler;
        private UnityAction<ShopPanelController.ShopItemDefinition> hoverHandler;
        private UnityAction<ShopPanelController.ShopItemDefinition> hoverEndHandler;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(HandleClick);
            }
        }

        public void Initialize(
            ShopPanelController.ShopItemDefinition data,
            UnityAction<ShopPanelController.ShopItemDefinition> onClicked,
            UnityAction<ShopPanelController.ShopItemDefinition> onHover,
            UnityAction<ShopPanelController.ShopItemDefinition> onHoverEnd)
        {
            definition = data;
            clickHandler = onClicked;
            hoverHandler = onHover;
            hoverEndHandler = onHoverEnd;

            if (nameLabel != null)
            {
                nameLabel.text = data?.displayName ?? "-";
            }

            if (priceLabel != null)
            {
                priceLabel.text = data != null ? data.price.ToString("#,0") + "원" : "-";
            }

            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(isSelected);
            }
        }

        private void HandleClick()
        {
            clickHandler?.Invoke(definition);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverHandler?.Invoke(definition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverEndHandler?.Invoke(definition);
        }
    }
}
