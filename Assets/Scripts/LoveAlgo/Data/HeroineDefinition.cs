using UnityEngine;

namespace LoveAlgo.Data
{
    public enum HeroineStatType
    {
        Health,
        Intelligence,
        Social,
        Persistence,
        Fatigue
    }

    [CreateAssetMenu(fileName = "HeroineDefinition", menuName = "LoveAlgo/Data/Heroine Definition")]
    public sealed class HeroineDefinition : ScriptableObject
    {
        [SerializeField] private string heroineId = "HaYeEun";
        [SerializeField] private string displayName = "하예은";
        [SerializeField] private HeroineStatType preferredStat = HeroineStatType.Health;
        [SerializeField] private int affectionThreshold = 32;
        [SerializeField] private Color themeColor = Color.white;

        public string HeroineId => heroineId;
        public string DisplayName => displayName;
        public HeroineStatType PreferredStat => preferredStat;
        public int AffectionThreshold => affectionThreshold;
        public Color ThemeColor => themeColor;
    }
}
