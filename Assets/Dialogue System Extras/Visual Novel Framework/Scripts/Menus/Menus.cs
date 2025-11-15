using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
	
	public class Menus : MonoBehaviour {

        public bool isStartScene = true;

        public GameObject gameplayPanel;
        public GeneralPanel startPanel;
		public LoadGamePanel loadGamePanel;
		public SaveGamePanel saveGamePanel;
        public OptionsPanel optionsPanel;
		public GeneralPanel quitToMenuPanel;
		public QuitProgramPanel quitProgramPanel;
        public AchievementsPanel achievementsPanel;
        public GeneralPanel creditsPanel;
        public string creditsScene = "Credits";

        [Tooltip("When the conversation ends, return to main menu (unless conversation was cancelled by CancelConversation method).")]
        public bool returnToMenuOnConversationEnd = true;

        private bool m_monitorConversation = false;

        private static Menus m_instance = null;

        public static Menus instance { get { return m_instance; } }

        private void Awake() // Singleton.
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void Start()
        {
            if (isStartScene)
            {
                startPanel.Open();
            }
        }

		public void HideAllPanels()
		{
            startPanel.Close();
			loadGamePanel.Close();
			saveGamePanel.Close();
			quitToMenuPanel.Close();
			quitProgramPanel.Close();
		}

		public void ShowLoadGamePanel()
		{
			loadGamePanel.Open();
		}

		public void HideLoadGamePanel()
		{
			loadGamePanel.Close();
		}

		public void ShowSaveGamePanel()
		{
			saveGamePanel.Open();
		}

		public void HideSaveGamePanel()
		{
			saveGamePanel.Close();
		}

        public void ShowOptionsPanel()
        {
            optionsPanel.Open();
        }

        public void HideOptionsPanel()
        {
            optionsPanel.Close();
        }

        public void ShowQuitToMenuPanel()
		{
			quitToMenuPanel.Open();
		}

		public void HideQuitToMenuPanel()
		{
			quitToMenuPanel.Close();
		}

		public void ShowQuitProgramPanel()
		{
			quitProgramPanel.Open();
		}
		
		public void HideQuitProgramPanel()
		{
			quitProgramPanel.Close();
		}

        public void ShowCredits()
        {
            Tools.LoadLevel(creditsScene);
            creditsPanel.Open();
        }

        public void HideCredits()
        {
            Tools.LoadLevel(GetComponent<SaveHelper>().mainMenuScene);
            creditsPanel.Close();
        }

        public void ShowQuestLog()
        {
            var questLogWindow = VNFrameworkFindUtility.FindFirstInstance<QuestLogWindow>();
            if (questLogWindow != null) questLogWindow.Open();
        }

        public void ShowAchievements()
        {
            achievementsPanel.Open();
        }

        public void OnConversationStart(Transform actor)
        {
            MonitorConversation(returnToMenuOnConversationEnd);
        }

        public void MonitorConversation(bool value)
        {
            m_monitorConversation = value;
        }

        public void OnConversationEnd(Transform actor)
        {
            if (!m_monitorConversation) return;
            m_monitorConversation = false;
            if (Debug.isDebugBuild) Debug.Log("Reached the end. Returning to main menu.");
            var saveHelper = GetComponent<SaveHelper>();
            saveHelper.ReturnToMenu();
        }

        public void CancelConversation()
        {
            m_monitorConversation = false;
            if (DialogueManager.IsConversationActive) DialogueManager.StopConversation();
        }

	}

}