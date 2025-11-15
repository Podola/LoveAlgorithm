using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "LoveAlgoConfiguration", menuName = "LoveAlgo/Data/Configuration")]
    public sealed class LoveAlgoConfiguration : ScriptableObject
    {
        [Header("Data References")]
        [SerializeField] private HeroineRoster heroineRoster;
        [SerializeField] private FreeActionCatalog freeActionCatalog;
        [SerializeField] private GiftTierCatalog giftTierCatalog;
        [SerializeField] private EpisodeCatalog episodeCatalog;
        [SerializeField] private ScheduleAsset schedule;

        [Header("Gameplay Settings")]
        [SerializeField] private int maxFreeActionsPerDay = 2;
        [SerializeField] private int maxFatigue = 100;
        [SerializeField] private int minFatigue = 0;

        public HeroineRoster HeroineRoster => heroineRoster;
        public FreeActionCatalog FreeActionCatalog => freeActionCatalog;
        public GiftTierCatalog GiftTierCatalog => giftTierCatalog;
        public EpisodeCatalog EpisodeCatalog => episodeCatalog;
        public ScheduleAsset Schedule => schedule;
        public int MaxFreeActionsPerDay => maxFreeActionsPerDay;
        public int MaxFatigue => maxFatigue;
        public int MinFatigue => minFatigue;
    }
}
