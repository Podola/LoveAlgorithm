using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This script adds the ability to backtrack conversations. To backtrack, call Backtrack(true).
    /// The bool parameter specifies whether to backtrack to an NPC line, which is what you usually
    /// want to do; otherwise if you're in a response menu you'll keep backtracking to the same 
    /// response menu instead of going back to a previous NPC line.
    /// </summary>
    public class Backtracker : MonoBehaviour
    {

        public bool debug;

        protected Stack<ConversationState> stack = new Stack<ConversationState>();
        protected bool isInMenu = false;

        public virtual void OnConversationStart(Transform actor)
        {
            stack.Clear();
            if (debug) Debug.Log("Backtracker: Starting a new conversation. Clearing stack.");
        }

        public virtual void OnConversationLine(Subtitle subtitle)
        {
            isInMenu = false;
            stack.Push(DialogueManager.CurrentConversationState);
            if (debug) Debug.Log("Backtracker: Recording dialogue entry " + subtitle.dialogueEntry.conversationID + ":" + subtitle.dialogueEntry.id + " on stack: '" + subtitle.formattedText.text + "' (" + subtitle.speakerInfo.characterType + ").");
        }

        public virtual void OnConversationResponseMenu(Response[] responses)
        {
            isInMenu = true;
        }

        // Call this method to go back:
        public virtual void Backtrack(bool toPreviousNPCLine)
        {
            if (stack.Count < 2) return;
            if (!isInMenu)
            {
                stack.Pop(); // Pop current entry.
            }
            var destination = stack.Pop(); // Pop previous entry.
            if (toPreviousNPCLine)
            {
                while (!destination.subtitle.speakerInfo.IsNPC && stack.Count > 0)
                {
                    destination = stack.Pop(); // Keep popping until we get an NPC line.
                }
                if (!destination.subtitle.speakerInfo.IsNPC) return;
            }
            if (debug) Debug.Log("Backtracker: Backtracking to " + destination.subtitle.dialogueEntry.conversationID + ":" + destination.subtitle.dialogueEntry.id + " on stack: '" + destination.subtitle.formattedText.text + "' (" + destination.subtitle.speakerInfo.characterType + ").");
            DialogueManager.ConversationController.GotoState(destination);
        }
    }
}
