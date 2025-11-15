using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// This is a variation of the LoadLevel() sequencer command that disables 
    /// RememberCurrentDialogueEntry.
    /// </summary>
    public class SequencerCommandVNLoadLevel : SequencerCommand
    {

        private RememberCurrentDialogueEntry m_rememberCurrentDialogueEntry;
        private bool m_wasEnabled = false;

        public void Start()
        {
            string levelName = GetParameter(0);
            string spawnpoint = GetParameter(1);
            if (string.IsNullOrEmpty(levelName))
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: VNLoadLevel() level name is an empty string", DialogueDebug.Prefix));
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: VNLoadLevel({1})", DialogueDebug.Prefix, levelName));
                DialogueLua.SetActorField("Player", "Spawnpoint", spawnpoint);
                var saveSystem = VNFrameworkFindUtility.FindFirstInstance<SaveSystem>();
                if (saveSystem != null)
                {
                    PersistentDataManager.LevelWillBeUnloaded();
                    SaveSystem.LoadScene(string.IsNullOrEmpty(spawnpoint) ? levelName : levelName + "@" + spawnpoint);
                }
                else
                {
                    var levelManager = VNFrameworkFindUtility.FindFirstInstance<LevelManager>();
                    if (levelManager != null)
                    {
                        levelManager.LoadLevel(levelName);
                    }
                    else
                    {
                        PersistentDataManager.Record();
                        PersistentDataManager.LevelWillBeUnloaded();
                        SceneManager.LoadScene(levelName);
                        PersistentDataManager.Apply();
                    }
                }
            }
            m_rememberCurrentDialogueEntry = VNFrameworkFindUtility.FindFirstInstance<RememberCurrentDialogueEntry>();
            if (m_rememberCurrentDialogueEntry != null)
            {
                m_wasEnabled = m_rememberCurrentDialogueEntry.enabled;
                m_rememberCurrentDialogueEntry.enabled = false;
            }
            Invoke("Stop", 1);
        }

        public void OnDestroy()
        {
            if (m_rememberCurrentDialogueEntry != null && m_wasEnabled) m_rememberCurrentDialogueEntry.enabled = true;
        }
    }
}
