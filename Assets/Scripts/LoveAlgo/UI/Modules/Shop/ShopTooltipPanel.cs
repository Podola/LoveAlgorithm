using TMPro;
using UnityEngine;

namespace LoveAlgo.UI.Modules
{
    /// <summary>
    /// Controls the floating tooltip that describes the currently hovered item.
    /// </summary>
    public sealed class ShopTooltipPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text bodyLabel;

        private void Awake()
        {
            Hide();
        }

        public void Show(ShopPanelController.ShopItemDefinition definition)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (titleLabel != null)
            {
                titleLabel.text = definition?.displayName ?? string.Empty;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = definition?.description ?? string.Empty;
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
