using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
    /// <summary>
    /// Enabled the Start, Continue, and/or Restart buttons as appropriate
    /// for the player's current saved games.
    /// </summary>
	public class StartOrContinueButton : MonoBehaviour {

		public UnityEngine.UI.Button startButton;
		public UnityEngine.UI.Button continueButton;
		public UnityEngine.UI.Button restartButton;

		private bool m_started = false;

		public void Start()
		{
            m_started = true;
			Check();
		}

		public void OnEnable()
		{
			if (m_started) Check();
		}

		public void Check()
		{
			var saveHelper = VNFrameworkFindUtility.FindFirstInstance<SaveHelper>();
			if (saveHelper)
			{
				var hasSavedGame = saveHelper.HasLastSavedGame();
				startButton.gameObject.SetActive(!hasSavedGame);
				continueButton.gameObject.SetActive(hasSavedGame);
				restartButton.gameObject.SetActive(hasSavedGame);
			}
		}

	}

}