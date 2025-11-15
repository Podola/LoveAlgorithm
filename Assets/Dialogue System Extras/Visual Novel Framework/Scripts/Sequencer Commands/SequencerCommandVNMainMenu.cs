using UnityEngine;
using PixelCrushers.DialogueSystem;
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
using UnityEngine.SceneManagement;
#endif

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// This sequencer command returns to the main menu.
    /// </summary>
    public class SequencerCommandVNMainMenu : SequencerCommand
    {

        public void Start()
        {
            var saveHelper = VNFrameworkFindUtility.FindFirstInstance<PixelCrushers.DialogueSystem.VisualNovelFramework.SaveHelper>();
            if (saveHelper == null)
            {
                Debug.LogError("Can't find SaveHelper component.");
            }
            else
            {
                saveHelper.ReturnToMenu();
            }
            Stop();
        }
    }
}
