using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This is an optional achievements system for the Visual Novel Framework.
/// It saves achievements to PlayerPrefs so they're not tied to individual
/// saved games.
/// 
/// Define your achievements in the Achievements component's Achievements list.
/// 
/// Unlock achievements using the UnlockAchievement("name") Lua function.
/// </summary>
namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{

    [Serializable]
    public class Achievement
    {
        public string name;
        public string description;
        public Sprite image;

        public void Unlock()
        {
            PlayerPrefs.SetInt("Achievement_" + name, 1);
        }

        public bool IsUnlocked()
        {
            return PlayerPrefs.GetInt("Achievement_" + name) == 1;
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey("Achievement_" + name);
        }
    }

    public class Achievements : MonoBehaviour
    {

        [Tooltip("When an achievement is locked, show this image, and don't show its name or description.")]
        public Sprite lockedAchievementImage;

        [Tooltip("When unlocking an achievement, show this alert message. Use {0} where you want to show the name and {1} where you want to show the description.")]
        public string unlockedText = "Unlocked: {0}";

        public List<Achievement> achievements;

        [Tooltip("When starting a new game, reset achievements.")]
        public bool resetInNewGames = false;

        public static Achievements instance;

        private void Awake()
        {
            instance = this;
            Lua.RegisterFunction("UnlockAchievement", this, SymbolExtensions.GetMethodInfo(() => UnlockAchievement(string.Empty)));
        }

        public void UnlockAchievement(string achievementName)
        {
            var achievement = achievements.Find(x => string.Equals(x.name, achievementName));
            if (achievement == null)
            {
                Debug.LogWarning("Dialogue System: UnlockAchievement(\"" + achievementName + "\"): Can't find achievement in list.", this);
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: UnlockAchievement(\"" + achievementName + "\")", this);
                if (!achievement.IsUnlocked())
                {
                    achievement.Unlock();
                    DialogueManager.displaySettings.alertSettings.allowAlertsDuringConversations = true;
                    DialogueManager.ShowAlert(string.Format(DialogueManager.GetLocalizedText(unlockedText), DialogueManager.GetLocalizedText(achievement.name), DialogueManager.GetLocalizedText(achievement.description)));
                }
            }
        }

        public void OnRestartGame()
        {
            if (resetInNewGames) ClearAll();
        }

        public void ClearAll()
        {
            foreach (var achievement in achievements)
            {
                achievement.Clear();
            }
            Debug.Log("Achievements cleared from PlayerPrefs.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Achievements), true)]
    public class AchievementsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button(new GUIContent("Clear PlayerPrefs", "Clears all achievements from PlayerPrefs.")))
            {
                (target as Achievements).ClearAll();
            }
        }
    }
#endif
}
