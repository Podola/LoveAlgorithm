using UnityEngine;

namespace LoveAlgo.Data
{
    [System.Serializable]
    public sealed class StandingPoseDefinition
    {
        [SerializeField] private string heroineId = "HaYeEun";
        [SerializeField] private string poseId = "default";
        [SerializeField] private Sprite sprite;

        public string HeroineId => heroineId;
        public string PoseId => poseId;
        public Sprite Sprite => sprite;
    }
}
