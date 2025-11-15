using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "GiftTier", menuName = "LoveAlgo/Data/Gift Tier")]
    public sealed class GiftTierDefinition : ScriptableObject
    {
        [SerializeField] private string tierId = "low";
        [SerializeField] private int minPrice = 0;
        [SerializeField] private int maxPrice = 10000;
        [SerializeField] private int secondEventPoints = 1;
        [SerializeField] private int thirdEventPoints = 2;

        public string TierId => tierId;
        public int MinPrice => minPrice;
        public int MaxPrice => maxPrice;
        public int SecondEventPoints => secondEventPoints;
        public int ThirdEventPoints => thirdEventPoints;
    }
}
