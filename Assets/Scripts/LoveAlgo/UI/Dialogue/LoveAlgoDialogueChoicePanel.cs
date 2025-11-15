using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoveAlgo.UI.Dialogue
{
    /// <summary>
    /// Simple response presenter that spawns lightweight buttons for DSU Response arrays.
    /// Keeps dependencies minimal so a solo developer can preview branching quickly.
    /// </summary>
    public sealed class LoveAlgoDialogueChoicePanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private Button choiceButtonTemplate;

        [Header("Behaviour")]
        [SerializeField] private bool autoSelectSingleResponse;
        [SerializeField] private bool hideWhenEmpty = true;

        private readonly List<Button> spawnedButtons = new();

        private void Awake()
        {
            if (contentRoot == null)
            {
                contentRoot = GetComponent<RectTransform>();
            }

            if (choiceButtonTemplate != null)
            {
                choiceButtonTemplate.gameObject.SetActive(false);
            }

            ApplyVisibility(false);
        }

        public void ShowResponses(Response[] responses)
        {
            ClearButtons();

            if (responses == null || responses.Length == 0)
            {
                ApplyVisibility(false);
                return;
            }

            if (autoSelectSingleResponse && responses.Length == 1 && responses[0].enabled && TrySelectResponse(responses[0]))
            {
                return;
            }

            if (contentRoot == null || choiceButtonTemplate == null)
            {
                Debug.LogWarning("LoveAlgoDialogueChoicePanel: Missing references.", this);
                return;
            }

            ApplyVisibility(true);

            foreach (var response in responses)
            {
                SpawnButton(response);
            }
        }

        public void Hide()
        {
            ClearButtons();
            ApplyVisibility(false);
        }

        private void SpawnButton(Response response)
        {
            if (response == null)
            {
                return;
            }

            var button = Instantiate(choiceButtonTemplate, contentRoot);
            button.gameObject.SetActive(true);
            button.interactable = response.enabled;

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = response.formattedText?.text ?? response.destinationEntry?.Title ?? string.Empty;
            }

            button.onClick.AddListener(() => HandleClick(response));
            spawnedButtons.Add(button);
        }

        private void HandleClick(Response response)
        {
            if (!TrySelectResponse(response))
            {
                return;
            }

            Hide();
        }

        private bool TrySelectResponse(Response response)
        {
            if (response == null)
            {
                return false;
            }

            var controller = DialogueManager.instance != null ? DialogueManager.instance.conversationController : null;
            if (controller == null)
            {
                Debug.LogWarning("LoveAlgoDialogueChoicePanel: Dialogue controller unavailable.", this);
                return false;
            }

            controller.OnSelectedResponse(this, new SelectedResponseEventArgs(response));
            return true;
        }

        private void ClearButtons()
        {
            foreach (var button in spawnedButtons)
            {
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }

            spawnedButtons.Clear();
        }

        private void ApplyVisibility(bool show)
        {
            if (!hideWhenEmpty)
            {
                return;
            }

            if (contentRoot != null)
            {
                contentRoot.gameObject.SetActive(show);
            }
        }
    }
}
