using System;
using System.Collections.Generic;
using LoveAlgo.Data;

namespace LoveAlgo.Services
{
    public sealed class ShopService
    {
        private readonly IReadOnlyList<GiftTierDefinition> tiers;

        public ShopService(GiftTierCatalog catalog)
        {
            tiers = catalog?.Tiers ?? Array.Empty<GiftTierDefinition>();
        }

        public GiftTierDefinition ResolveTier(int price)
        {
            GiftTierDefinition best = null;
            foreach (var tier in tiers)
            {
                if (tier == null)
                {
                    continue;
                }

                if (price >= tier.MinPrice && price <= tier.MaxPrice)
                {
                    best = tier;
                }
            }

            return best;
        }

        public int GetGiftPoints(GiftTierDefinition tier, bool isThirdEvent)
        {
            if (tier == null)
            {
                return 0;
            }

            return isThirdEvent ? tier.ThirdEventPoints : tier.SecondEventPoints;
        }
    }
}
