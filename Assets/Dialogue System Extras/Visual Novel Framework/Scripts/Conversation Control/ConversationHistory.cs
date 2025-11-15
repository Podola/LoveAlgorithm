using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This script maintains a conversation history. It can also show the history
    /// in a UIPanel by calling ShowHistory().
    /// </summary>
    public class ConversationHistory : Saver
    {

        public bool includeInSavedGames = false;

        public bool clearWhenStartingNewConversation = true;

        [Tooltip("Add actor names to front of lines in history.")]
        public bool prependActorNames = true;

        [Tooltip("You may want to set this false if another option elsewhere already sets the actor's text colors.")]
        public bool colorActorLines = true;

        [Tooltip("Log player lines in this color.")]
        public Color playerColor = Color.blue;

        [Tooltip("Log NPC lines in this color.")]
        public Color npcColor = Color.red;

        public UIPanel historyPanel;
        public UITextField historyText;

        [TextArea] public string history;

        public void ShowHistory()
        {
            historyPanel.Open();
            historyText.text = history;
        }

        void OnConversationStart(Transform actor)
        {
            if (clearWhenStartingNewConversation) history = string.Empty;
        }

        void OnConversationLine(Subtitle subtitle)
        {
            if (subtitle == null | subtitle.formattedText == null | string.IsNullOrEmpty(subtitle.formattedText.text)) return;
            string speakerName = (prependActorNames && subtitle.speakerInfo != null && subtitle.speakerInfo.transform != null)
                ? (subtitle.speakerInfo.transform.name + ": ") : string.Empty;
            history += string.Format("<color={0}>{1}{2}</color>\n", new object[] { GetActorColor(subtitle), speakerName, subtitle.formattedText.text });
        }

        private string GetActorColor(Subtitle subtitle)
        {
            if (subtitle == null | subtitle.speakerInfo == null) return "white";
            return Tools.ToWebColor(subtitle.speakerInfo.isPlayer ? playerColor : npcColor);
        }

        public override string RecordData()
        {
            return includeInSavedGames ? history : string.Empty;
        }

        public override void ApplyData(string s)
        {
            if (includeInSavedGames) history = s;
        }

    }
}
