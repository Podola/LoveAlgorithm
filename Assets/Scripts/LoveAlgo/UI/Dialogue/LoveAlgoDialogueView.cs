using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using ConversationEvents = PixelCrushers.DialogueSystem.DialogueSystemEvents.ConversationEvents;

namespace LoveAlgo.UI.Dialogue
{
    /// <summary>
    /// Connects DSU conversation events to LoveAlgo-specific UI elements (background, standing, labels).
    /// Attach to the same GameObject as the Dialogue UI canvas.
    /// </summary>
    public sealed class LoveAlgoDialogueView : MonoBehaviour
    {
        public static LoveAlgoDialogueView ActiveInstance { get; private set; }

        [Header("Text References")]
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyLabel;

        [Header("Visual Presenters")]
        [SerializeField] private LoveAlgoBackgroundPresenter backgroundPresenter;
        [SerializeField] private LoveAlgoStandingPresenter standingPresenter;
        [SerializeField] private LoveAlgoDialogueChoicePanel choicePanel;

        [Header("Text Animation")]
        [SerializeField] private bool useTypewriterEffect = true;
        [SerializeField] [Min(1f)] private float charactersPerSecond = 45f;

        [Header("Options")]
        [SerializeField] private bool useActorFallbackFields = true;

        [Header("Debugging")]
        [SerializeField] private bool verboseLogging;

        private DialogueSystemEvents dialogueSystemEvents;
        private ConversationEvents cachedEvents;
        private Coroutine typewriterRoutine;
        private Coroutine subscriptionRoutine;
        private DialogueSystemController cachedController;

        public LoveAlgoBackgroundPresenter BackgroundPresenter => backgroundPresenter;
        public LoveAlgoStandingPresenter StandingPresenter => standingPresenter;

        private void Awake()
        {
            LogVerbose("Awake called");

            if (ActiveInstance != null && ActiveInstance != this)
            {
                Debug.LogWarning("Multiple LoveAlgoDialogueView instances detected. Overwriting active instance.", this);
            }

            ActiveInstance = this;
            LogVerbose("Set as ActiveInstance");
        }

        private void OnEnable()
        {
            LogVerbose("OnEnable called");
            BeginSubscriptionRoutine();
        }

        private void OnDisable()
        {
            LogVerbose("OnDisable called");
            CompleteTypewriter();
            StopSubscriptionRoutine();
            UnregisterConversationEvents();
        }

        private void OnDestroy()
        {
            LogVerbose("OnDestroy called");
            StopSubscriptionRoutine();
            UnregisterConversationEvents();

            if (ActiveInstance == this)
            {
                ActiveInstance = null;
            }
        }

        private void BeginSubscriptionRoutine()
        {
            StopSubscriptionRoutine();
            subscriptionRoutine = StartCoroutine(SubscribeWhenReady());
        }

        private IEnumerator SubscribeWhenReady()
        {
            while (isActiveAndEnabled)
            {
                var controller = DialogueManager.instance;
                if (controller == null)
                {
                    yield return null;
                    continue;
                }

                var eventsComponent = controller.GetComponent<DialogueSystemEvents>();
                if (eventsComponent == null)
                {
                    yield return null;
                    continue;
                }

                RegisterConversationEvents(controller, eventsComponent);
                subscriptionRoutine = null;
                yield break;
            }

            subscriptionRoutine = null;
        }

        private void RegisterConversationEvents(DialogueSystemController controller, DialogueSystemEvents eventsComponent)
        {
            LogVerbose("Registering Dialogue System events");

            if (controller != cachedController)
            {
                UnregisterConversationEvents();
            }

            cachedController = controller;
            dialogueSystemEvents = eventsComponent;
            cachedEvents = dialogueSystemEvents.conversationEvents;

            if (cachedEvents == null)
            {
                Debug.LogError("LoveAlgoDialogueView: DialogueSystemEvents.conversationEvents is null.", this);
                return;
            }

            cachedEvents.onConversationLine.AddListener(HandleConversationLine);
            cachedEvents.onConversationResponseMenu.AddListener(HandleResponseMenu);
            cachedEvents.onConversationEnd.AddListener(HandleConversationEnd);

            LogVerbose("Dialogue System events registered");
        }

        private void UnregisterConversationEvents()
        {
            if (cachedEvents == null)
            {
                return;
            }

            LogVerbose("Unregistering Dialogue System events");
            cachedEvents.onConversationLine.RemoveListener(HandleConversationLine);
            cachedEvents.onConversationResponseMenu.RemoveListener(HandleResponseMenu);
            cachedEvents.onConversationEnd.RemoveListener(HandleConversationEnd);

            cachedEvents = null;
            dialogueSystemEvents = null;
            cachedController = null;
        }

        private void StopSubscriptionRoutine()
        {
            if (subscriptionRoutine == null)
            {
                return;
            }

            StopCoroutine(subscriptionRoutine);
            subscriptionRoutine = null;
        }

        private void HandleConversationLine(Subtitle subtitle)
        {
            LogVerbose("HandleConversationLine called");

            choicePanel?.Hide();

            if (subtitle == null)
            {
                Debug.LogWarning("LoveAlgoDialogueView: Subtitle is null", this);
                return;
            }

            var speakerName = subtitle.speakerInfo?.Name ?? string.Empty;
            var dialogueText = subtitle.formattedText?.text ?? string.Empty;

            LogVerbose($"Displaying line - Speaker: '{speakerName}', Text: '{dialogueText}'");

            speakerLabel?.SetText(speakerName);
            ApplyBodyText(dialogueText);

            TryUpdateBackground(subtitle);
            TryUpdateStanding(subtitle);

            LogVerbose("HandleConversationLine completed");
        }

        private void HandleResponseMenu(Response[] responses)
        {
            if (choicePanel != null)
            {
                choicePanel.ShowResponses(responses);
                return;
            }

            if (responses == null || responses.Length == 0)
            {
                return;
            }

            if (responses.Length == 1)
            {
                TrySelectResponse(responses[0]);
            }
        }

        private void HandleConversationEnd(Transform actor)
        {
            LogVerbose($"HandleConversationEnd called with actor: {actor?.name ?? "null"}");

            CompleteTypewriter();
            bodyLabel?.SetText(string.Empty);
            speakerLabel?.SetText(string.Empty);
            standingPresenter?.HideAll();
            standingPresenter?.ClearFocus();
            choicePanel?.Hide();

            LogVerbose("HandleConversationEnd completed");
        }

        private void TryUpdateBackground(Subtitle subtitle)
        {
            if (backgroundPresenter == null)
            {
                return;
            }

            var id = Field.LookupValue(subtitle.dialogueEntry.fields, "Background");
            if (string.IsNullOrEmpty(id) && useActorFallbackFields && subtitle.speakerInfo != null)
            {
                id = DialogueLua.GetActorField(subtitle.speakerInfo.nameInDatabase, "Background").asString;
            }

            if (!string.IsNullOrEmpty(id))
            {
                backgroundPresenter.Show(id);
            }
        }

        private void TryUpdateStanding(Subtitle subtitle)
        {
            if (standingPresenter == null)
            {
                return;
            }

            var layout = Field.LookupValue(subtitle.dialogueEntry.fields, "StandingLayout");
            if (!string.IsNullOrEmpty(layout))
            {
                standingPresenter.ApplyLayout(ParseLayout(layout));
            }

            var hideTokens = Field.LookupValue(subtitle.dialogueEntry.fields, "StandingHide");
            if (!string.IsNullOrEmpty(hideTokens))
            {
                ApplyHideTokens(hideTokens);
            }

            ApplyStandingFocus(subtitle);
        }

        private IEnumerable<StandingInstruction> ParseLayout(string layout)
        {
            var instructions = new List<StandingInstruction>();
            var segments = layout.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                var slotAndData = segment.Split('=');
                if (slotAndData.Length != 2)
                {
                    continue;
                }

                if (!TryParseSlot(slotAndData[0], out var slot))
                {
                    continue;
                }

                var heroAndPose = slotAndData[1].Split('@');
                var heroineId = heroAndPose.Length > 0 ? heroAndPose[0] : string.Empty;
                var poseId = heroAndPose.Length > 1 ? heroAndPose[1] : string.Empty;
                if (string.IsNullOrEmpty(heroineId))
                {
                    continue;
                }

                instructions.Add(new StandingInstruction(slot, heroineId, poseId));
            }

            return instructions;
        }

        private void ApplyStandingFocus(Subtitle subtitle)
        {
            if (standingPresenter == null || subtitle == null)
            {
                return;
            }

            var focusToken = Field.LookupValue(subtitle.dialogueEntry.fields, "StandingFocus");
            if (string.IsNullOrEmpty(focusToken) || focusToken.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                standingPresenter.ClearFocus();
                return;
            }

            if (TryParseSlot(focusToken, out var slot))
            {
                standingPresenter.Focus(slot);
                return;
            }

            standingPresenter.FocusByHeroine(focusToken);
        }

        private void ApplyHideTokens(string tokens)
        {
            if (tokens.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                standingPresenter.HideAll();
                return;
            }

            var parts = tokens.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (TryParseSlot(part, out var slot))
                {
                    standingPresenter.Hide(slot);
                }
            }
        }

        private void ApplyBodyText(string text)
        {
            if (bodyLabel == null)
            {
                return;
            }

            var finalText = text ?? string.Empty;
            if (!useTypewriterEffect || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                CompleteTypewriter();
                bodyLabel.SetText(finalText);
                bodyLabel.maxVisibleCharacters = int.MaxValue;
                return;
            }

            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
            }

            typewriterRoutine = StartCoroutine(TypewriterRoutine(finalText));
        }

        public void CompleteTypewriter()
        {
            LogVerbose("CompleteTypewriter called");

            if (typewriterRoutine != null)
            {
                LogVerbose("Stopping existing typewriter coroutine");
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }

            if (bodyLabel != null)
            {
                LogVerbose("Setting maxVisibleCharacters to int.MaxValue");
                bodyLabel.maxVisibleCharacters = int.MaxValue;
            }
        }

        private IEnumerator TypewriterRoutine(string text)
        {
            LogVerbose($"TypewriterRoutine started with text: '{text}'");

            if (bodyLabel == null)
            {
                Debug.LogWarning("LoveAlgoDialogueView: TypewriterRoutine - bodyLabel is null", this);
                typewriterRoutine = null;
                yield break;
            }

            bodyLabel.SetText(text);
            bodyLabel.maxVisibleCharacters = 0;
            bodyLabel.ForceMeshUpdate();
            var totalCharacters = bodyLabel.textInfo.characterCount;

            LogVerbose($"TypewriterRoutine - totalCharacters: {totalCharacters}");

            if (totalCharacters <= 0)
            {
                Debug.LogWarning("LoveAlgoDialogueView: TypewriterRoutine - no characters to display", this);
                typewriterRoutine = null;
                bodyLabel.maxVisibleCharacters = int.MaxValue;
                yield break;
            }

            var speed = Mathf.Max(1f, charactersPerSecond);
            var visibleCharacters = 0;
            var accumulator = 0f;

            while (visibleCharacters < totalCharacters)
            {
                accumulator += Time.deltaTime * speed;
                var nextVisible = Mathf.Min(totalCharacters, Mathf.FloorToInt(accumulator));
                if (nextVisible != visibleCharacters)
                {
                    visibleCharacters = nextVisible;
                    bodyLabel.maxVisibleCharacters = visibleCharacters;
                    // Debug.Log($"LoveAlgoDialogueView: Typewriter progress - {visibleCharacters}/{totalCharacters}", this);
                }

                yield return null;
            }

            bodyLabel.maxVisibleCharacters = totalCharacters;
            typewriterRoutine = null;

            LogVerbose("TypewriterRoutine completed");
        }

        private static bool TryParseSlot(string value, out StandingSlot slot)
        {
            return Enum.TryParse(value, true, out slot);
        }

        private void LogVerbose(string message)
        {
            if (!verboseLogging)
            {
                return;
            }

            Debug.Log($"LoveAlgoDialogueView: {message}", this);
        }

        private void TrySelectResponse(Response response)
        {
            if (response == null)
            {
                return;
            }

            var controller = DialogueManager.instance != null ? DialogueManager.instance.conversationController : null;
            if (controller == null)
            {
                Debug.LogWarning("LoveAlgoDialogueView: Dialogue controller unavailable, cannot auto-select response.", this);
                return;
            }

            controller.OnSelectedResponse(this, new SelectedResponseEventArgs(response));
        }
    }
}
