using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this script to your Visual Novel Framework Dialogue Manager if you want to keep track
    /// of the current conversation and dialogue entry. When you load a game,
    /// it will resume the conversation at that point.
    /// </summary>
    public class RememberCurrentDialogueEntry : ConversationStateSaver
    {

#if !USE_INK // The Ink integration has its own version of this.

        public override void ApplyData(string s)
        {
            var menus = VNFrameworkFindUtility.FindFirstInstance<PixelCrushers.DialogueSystem.VisualNovelFramework.Menus>();
            if (menus != null) menus.MonitorConversation(false);
            base.ApplyData(s);
        }

        public void OnConversationStart(Transform actor)
        {
            var menus = VNFrameworkFindUtility.FindFirstInstance<PixelCrushers.DialogueSystem.VisualNovelFramework.Menus>();
            if (menus != null) menus.OnConversationStart(actor);
        }

        public void OnConversationEnd(Transform actor)
        {
            var menus = VNFrameworkFindUtility.FindFirstInstance<PixelCrushers.DialogueSystem.VisualNovelFramework.Menus>();
            if (menus != null) menus.OnConversationEnd(actor);
        }

        public override void OnRestartGame()
        {
            base.OnRestartGame();
            if (VisualNovelFramework.Achievements.instance != null)
            {
                VisualNovelFramework.Achievements.instance.OnRestartGame();
            }
        }

#endif

    }
}
