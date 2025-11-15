using UnityEngine;
using PixelCrushers.DialogueSystem.VisualNovelFramework;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Syntax: Background(filename)
    /// Changes the background image. The image should be in a Resources folder.
    /// </summary>
    public class SequencerCommandBackground : SequencerCommand
    {

        private void Awake()
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Sequencer: Background(" + GetParameter(0) + ")");
            BackgroundManager.SetBackgroundImage(GetParameter(0));
        }
    }
}
