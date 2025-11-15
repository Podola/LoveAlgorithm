using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.VisualNovelFramework
{
    /// <summary>
    /// This panel shows the achievements that the player has unlocked.
    /// </summary>
    public class AchievementsPanel : UIPanel
    {

        public UnityEngine.UI.GridLayoutGroup gridLayoutGroup;
        public GameObject template;

        private List<GameObject> instantiatedObjects = new List<GameObject>();

        private void Awake()
        {
            template.SetActive(false);
        }

        protected override void OnEnable()
        {
            foreach (var obj in instantiatedObjects)
            {
                Destroy(obj);
            }

            foreach (var achievement in Achievements.instance.achievements)
            {
                var obj = Instantiate(template);
                instantiatedObjects.Add(obj);
                obj.SetActive(true);
                obj.transform.SetParent(gridLayoutGroup.transform, false);
                obj.name = achievement.name;
                if (achievement.IsUnlocked())
                {
                    obj.GetComponentInChildren<UnityEngine.UI.Image>().sprite = achievement.image;
                    var texts = obj.GetComponentsInChildren<UnityEngine.UI.Text>();
                    var localizedName = DialogueManager.GetLocalizedText(achievement.name);
                    var localizedDescription = DialogueManager.GetLocalizedText(achievement.description);
                    if (texts.Length == 1)
                    {
                        texts[0].text = localizedName;
                    }
                    else if (texts.Length > 1)
                    {
                        int size0 = texts[0].fontSize;
                        int size1 = texts[1].fontSize;
                        texts[0].text = (size0 > size1) ? localizedName : localizedDescription;
                        texts[1].text = (size0 > size1) ? localizedDescription : localizedName;
                    }
                }
                else
                {
                    obj.GetComponentInChildren<UnityEngine.UI.Image>().sprite = Achievements.instance.lockedAchievementImage;
                    foreach (var text in obj.GetComponentsInChildren<UnityEngine.UI.Text>())
                    {
                        text.text = string.Empty;
                    }
                }
            }
        }
    }
}
