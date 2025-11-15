using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "GiftTierCatalog", menuName = "LoveAlgo/Data/Gift Tier Catalog")]
    public sealed class GiftTierCatalog : ScriptableObject
    {
        [SerializeField] private List<GiftTierDefinition> tiers = new();

        public IReadOnlyList<GiftTierDefinition> Tiers => tiers;
    }
}
