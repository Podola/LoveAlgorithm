using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoveAlgo.UI.Modules
{
    public enum FreeActionOption
    {
        Exercise,
        Study,
        ConvenienceStore,
        WarehouseJob,
        Investment,
        OpenShop
    }

    [Serializable]
    public sealed class FreeActionOptionDefinition
    {
        public FreeActionOption option;
        public string title;
        [TextArea] public string summary;
        [TextArea] public string expectedResult;
        public bool launchesShop;
    }

    [Serializable]
    public sealed class FreeActionOptionEvent : UnityEvent<FreeActionOption> { }

    /// <summary>
    /// Skeleton controller for the free action UI. Handles button wiring and exposes UnityEvents for future game logic.
    /// </summary>
    public sealed class FreeActionPanelController : LoveAlgo.UI.LoveAlgoUIModule
    {
        [Header("Action Buttons")]
        [SerializeField] private Button exerciseButton;
        [SerializeField] private Button studyButton;
        [SerializeField] private Button convenienceButton;
        [SerializeField] private Button warehouseButton;
        [SerializeField] private Button investmentButton;
        [SerializeField] private Button openShopButton;

        [Header("Details Panel")]
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text summaryLabel;
        [SerializeField] private TMP_Text expectedResultLabel;

        [Header("Popup")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text popupTitleLabel;
        [SerializeField] private TMP_Text popupBodyLabel;
        [SerializeField] private Button popupConfirmButton;
        [SerializeField] private Button popupCancelButton;

        [Header("Definitions")]
        [SerializeField] private List<FreeActionOptionDefinition> optionDefinitions = new();

        [Header("Events")]
        [SerializeField] private FreeActionOptionEvent onActionConfirmed;
        [SerializeField] private UnityEvent onShopRequested;

        private readonly Dictionary<FreeActionOption, FreeActionOptionDefinition> lookup = new();
        private FreeActionOption currentSelection;

        protected override void OnInitialized()
        {
            WireButton(exerciseButton, FreeActionOption.Exercise);
            WireButton(studyButton, FreeActionOption.Study);
            WireButton(convenienceButton, FreeActionOption.ConvenienceStore);
            WireButton(warehouseButton, FreeActionOption.WarehouseJob);
            WireButton(investmentButton, FreeActionOption.Investment);
            WireButton(openShopButton, FreeActionOption.OpenShop);

            foreach (var definition in optionDefinitions)
            {
                lookup[definition.option] = definition;
            }

            if (popupConfirmButton != null)
            {
                popupConfirmButton.onClick.AddListener(ConfirmSelection);
            }

            if (popupCancelButton != null)
            {
                popupCancelButton.onClick.AddListener(HidePopup);
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            SelectOption(FreeActionOption.Exercise);
        }

        private void WireButton(Button button, FreeActionOption option)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(() => SelectOption(option));
        }

        private void SelectOption(FreeActionOption option)
        {
            currentSelection = option;
            if (!lookup.TryGetValue(option, out var definition))
            {
                UpdateDetailLabels(option.ToString(), "", "");
                return;
            }

            UpdateDetailLabels(definition.title, definition.summary, definition.expectedResult);
            ShowPopup(definition);
        }

        private void UpdateDetailLabels(string title, string summary, string expected)
        {
            if (titleLabel != null)
            {
                titleLabel.text = title;
            }

            if (summaryLabel != null)
            {
                summaryLabel.text = summary;
            }

            if (expectedResultLabel != null)
            {
                expectedResultLabel.text = expected;
            }
        }

        private void ShowPopup(FreeActionOptionDefinition definition)
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }

            if (popupTitleLabel != null)
            {
                popupTitleLabel.text = definition.title;
            }

            if (popupBodyLabel != null)
            {
                popupBodyLabel.text = definition.summary;
            }
        }

        private void HidePopup()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        private void ConfirmSelection()
        {
            if (lookup.TryGetValue(currentSelection, out var definition) && definition.launchesShop)
            {
                onShopRequested?.Invoke();
                Owner?.ShowShop();
            }
            else
            {
                onActionConfirmed?.Invoke(currentSelection);
            }

            HidePopup();
        }
    }
}
