/// <summary>
/// Provides AutoPlay and SkipAll functionality. To add "Auto Play" and/or 
/// "Skip All" buttons that advances the current conversation:
/// 
/// - Add this script to the dialogue UI.
/// - Add Auto Play and/or Skip All buttons to your subtitle panel(s). Configure 
/// their OnClick() events to call the dialogue UI's ConversationControl.ToggleAutoPlay 
/// and/or ConversationControl.SkipAll methods.
/// </summary>
public class ConversationControl : PixelCrushers.DialogueSystem.ConversationControl 
{
    // Add to dialogue UI. Connect to Skip All and Auto Play buttons.
}
