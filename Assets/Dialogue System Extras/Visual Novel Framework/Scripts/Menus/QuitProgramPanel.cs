using UnityEngine;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
	public class QuitProgramPanel : GeneralPanel
    {

        public void QuitProgram()
        {
            #if UNITY_STANDALONE
            Application.Quit();
            #endif

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

    }

}