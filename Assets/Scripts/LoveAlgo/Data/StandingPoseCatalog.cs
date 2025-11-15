using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "StandingPoseCatalog", menuName = "LoveAlgo/Data/Standing Pose Catalog")]
    public sealed class StandingPoseCatalog : ScriptableObject
    {
        [SerializeField] private List<StandingPoseDefinition> poses = new();

        public bool TryGetSprite(string heroineId, string poseId, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(heroineId))
            {
                return false;
            }

            foreach (var pose in poses)
            {
                if (pose == null || string.IsNullOrEmpty(pose.HeroineId))
                {
                    continue;
                }

                var matchesHeroine = pose.HeroineId == heroineId;
                var matchesPose = string.IsNullOrEmpty(poseId) || pose.PoseId == poseId;
                if (matchesHeroine && matchesPose)
                {
                    sprite = pose.Sprite;
                    return sprite != null;
                }
            }

            return false;
        }
    }
}
