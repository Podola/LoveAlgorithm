using UnityEngine;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{

    public class HandleBackgroundFields : MonoBehaviour
    {

        private BackgroundManager m_backgroundManager = null;
        private BackgroundManager backgroundManager
        {
            get
            {
                if (m_backgroundManager == null) m_backgroundManager = VNFrameworkFindUtility.FindFirstInstance<BackgroundManager>();
                return m_backgroundManager;
            }
        }

        private void OnConversationLine(Subtitle subtitle)
        {
            if (subtitle == null || backgroundManager == null) return;
            var background = Field.LookupValue(subtitle.dialogueEntry.fields, "Background");
            if (!string.IsNullOrEmpty(background))
            {
                BackgroundManager.SetBackgroundImage(background);
            }
            else 
            {
                background = DialogueLua.GetActorField(subtitle.speakerInfo.nameInDatabase, "Background").asString;
                if (!string.IsNullOrEmpty(background))
                {
                    BackgroundManager.SetBackgroundImage(background);
                }
            }
        }
    }
}