using System.Collections.Generic;

namespace LoveAlgo.Services
{
    public sealed class InventoryService
    {
        private readonly Dictionary<string, int> items = new();

        public IReadOnlyDictionary<string, int> Items => items;

        public void AddItem(string itemId, int amount = 1)
        {
            if (!items.TryAdd(itemId, amount))
            {
                items[itemId] += amount;
            }
        }

        public bool ConsumeItem(string itemId, int amount = 1)
        {
            if (!items.TryGetValue(itemId, out var current) || current < amount)
            {
                return false;
            }

            var newValue = current - amount;
            if (newValue <= 0)
            {
                items.Remove(itemId);
            }
            else
            {
                items[itemId] = newValue;
            }

            return true;
        }
    }
}
