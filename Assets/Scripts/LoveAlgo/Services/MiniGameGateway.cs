using System.Collections.Generic;

namespace LoveAlgo.Services
{
    public sealed class MiniGameGateway
    {
        private readonly Dictionary<string, int> rewardTable = new()
        {
            { "C", 0 },
            { "B", 1 },
            { "A", 2 },
            { "S", 3 }
        };

        public int ResolveBonus(string rank)
        {
            if (string.IsNullOrEmpty(rank))
            {
                return 0;
            }

            return rewardTable.TryGetValue(rank, out var bonus) ? bonus : 0;
        }
    }
}
