using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class SMSConversationHandler : MonoBehaviour
{
    public SMSDialogueUI mySMSDialogueUI;

    [ConversationPopup] public string conversation;

    public void StartSMSConversation()
    {
        DialogueManager.UseDialogueUI(mySMSDialogueUI.gameObject);
        DialogueLua.SetVariable("Conversation", conversation);
        if (DialogueLua.DoesVariableExist(mySMSDialogueUI.currentDialogueEntryRecords))
        {
            // Log already exists, so resume the conversation:
            mySMSDialogueUI.OnApplyPersistentData();
        }
        else
        {
            // Log doesn't exist, so start the conversation for the first time:
            DialogueManager.StartConversation(conversation);
        }
    }

    public void StopSMSConversation()
    {
        mySMSDialogueUI.OnRecordPersistentData();
        DialogueManager.StopConversation();
    }
}
