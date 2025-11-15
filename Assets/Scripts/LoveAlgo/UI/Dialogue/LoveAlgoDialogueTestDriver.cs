using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace LoveAlgo.UI.Dialogue
{
    /// <summary>
    /// Simple helper that automatically launches a conversation when the dialogue test scene loads.
    /// Buttons in the scene can call RestartConversation to replay the sample data.
    /// </summary>
    public sealed class LoveAlgoDialogueTestDriver : MonoBehaviour
    {
        [SerializeField] private DialogueSystemController dialogueController;
        [SerializeField] private string conversationTitle = "LoveAlgo.TestConversation";
        [SerializeField] private LoveAlgoDialogueView dialogueView;
        [SerializeField] private bool verboseLogging;

        private Coroutine restartRoutine;

        private IEnumerator Start()
        {
            LogVerbose("Starting initialization coroutine");
            yield return EnsureControllerReady();
            StartConversation();
        }

        public void RestartConversation()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (restartRoutine != null)
            {
                StopCoroutine(restartRoutine);
            }

            restartRoutine = StartCoroutine(RestartAfterReady());
        }

        private IEnumerator RestartAfterReady()
        {
            yield return EnsureControllerReady();
            StartConversation();
            restartRoutine = null;
        }

        private IEnumerator EnsureControllerReady()
        {
            while (dialogueController == null)
            {
                LogVerbose("dialogueController reference missing, waiting for DialogueManager.instance");
                dialogueController = DialogueManager.instance;
                if (dialogueController == null)
                {
                    yield return null;
                }
            }

            while (!dialogueController.isInitialized)
            {
                LogVerbose("Waiting for DialogueSystemController initialization");
                yield return null;
            }

            DialogueDebug.level = DialogueDebug.DebugLevel.Info;
            dialogueController.debugLevel = DialogueDebug.DebugLevel.Info;

            LogVerbose("Controller ready, allowing one frame for warmup");
            yield return null; // allow DialogueManager to finish warmup
        }

        private void StartConversation()
        {
            LogVerbose($"StartConversation called with title '{conversationTitle}'");

            if (string.IsNullOrEmpty(conversationTitle))
            {
                Debug.LogError("LoveAlgoDialogueTestDriver: ConversationTitle이 비어 있습니다.", this);
                return;
            }

            if (!DialogueManager.hasInstance)
            {
                Debug.LogError("LoveAlgoDialogueTestDriver: DialogueManager 인스턴스가 없습니다.", this);
                return;
            }

            LogVerbose("Checking dialogue database");
            var database = DialogueManager.masterDatabase;
            if (database == null)
            {
                Debug.LogError("LoveAlgoDialogueTestDriver: DialogueManager.masterDatabase is null", this);
                return;
            }

            LogVerbose($"Master database has {database.conversations.Count} conversations");
            var conversation = database.conversations.Find(c => c.Title == conversationTitle);
            if (conversation == null)
            {
                Debug.LogError($"LoveAlgoDialogueTestDriver: Conversation '{conversationTitle}' not found in database", this);
                Debug.Log("Available conversations:");
                foreach (var conv in database.conversations)
                {
                    Debug.Log($"  - {conv.Title} (ID: {conv.id})");
                }
                return;
            }

            LogVerbose($"Found conversation '{conversationTitle}' with {conversation.dialogueEntries.Count} entries");

            LogVerbose("Completing typewriter and stopping conversations");
            dialogueView?.CompleteTypewriter();
            DialogueManager.StopAllConversations();

            LogVerbose($"Starting conversation '{conversationTitle}'");
            DialogueManager.StartConversation(conversationTitle);
            LogVerbose($"DialogueManager reports isConversationActive={DialogueManager.isConversationActive}");

            var controller = DialogueManager.instance;
            if (controller == null)
            {
                Debug.LogError("LoveAlgoDialogueTestDriver: DialogueManager.instance returned null after StartConversation", this);
                return;
            }

            var conversationController = controller.conversationController;
            LogVerbose(conversationController == null
                ? "conversationController is null"
                : "conversationController exists");

            var activeConversation = controller.activeConversation;
            LogVerbose(activeConversation == null
                ? "activeConversation record is null"
                : $"activeConversation title={activeConversation.conversationTitle}");

            var state = DialogueManager.currentConversationState;
            if (state == null)
            {
                Debug.LogWarning("LoveAlgoDialogueTestDriver: currentConversationState is null immediately after StartConversation", this);
            }
            else
            {
                var entry = state.subtitle?.dialogueEntry;
                LogVerbose($"currentState entryId={entry?.id}, title='{entry?.Title}', isGroup={entry?.isGroup}");
            }
        }

        private void LogVerbose(string message)
        {
            if (!verboseLogging)
            {
                return;
            }

            Debug.Log($"LoveAlgoDialogueTestDriver: {message}", this);
        }
    }
}
