using System;
using System.Collections.Generic;
using LoveAlgo.Data;

namespace LoveAlgo.Services
{
    public readonly struct AffectionSnapshot
    {
        public readonly string HeroineId;
        public readonly int Points;
        public readonly int Threshold;

        public AffectionSnapshot(string heroineId, int points, int threshold)
        {
            HeroineId = heroineId;
            Points = points;
            Threshold = threshold;
        }
    }

    public sealed class AffinityService
    {
        private readonly Dictionary<string, int> points = new();
        private readonly Dictionary<string, HeroineDefinition> lookup = new();

        public AffinityService(HeroineRoster roster)
        {
            foreach (var heroine in roster.Heroines)
            {
                if (heroine == null)
                {
                    continue;
                }

                lookup[heroine.HeroineId] = heroine;
                points.TryAdd(heroine.HeroineId, 0);
            }
        }

        public event Action<AffectionSnapshot> AffectionChanged;

        public void AddPoints(string heroineId, int delta)
        {
            if (!lookup.ContainsKey(heroineId))
            {
                return;
            }

            var newValue = Math.Max(0, GetPoints(heroineId) + delta);
            points[heroineId] = newValue;
            var definition = lookup[heroineId];
            AffectionChanged?.Invoke(new AffectionSnapshot(heroineId, newValue, definition.AffectionThreshold));
        }

        public int GetPoints(string heroineId) => points.TryGetValue(heroineId, out var value) ? value : 0;

        public bool MeetsThreshold(string heroineId)
        {
            if (!lookup.TryGetValue(heroineId, out var definition))
            {
                return false;
            }

            return GetPoints(heroineId) >= definition.AffectionThreshold;
        }

        public IEnumerable<AffectionSnapshot> Enumerate()
        {
            foreach (var pair in lookup)
            {
                var heroineId = pair.Key;
                var definition = pair.Value;
                yield return new AffectionSnapshot(heroineId, GetPoints(heroineId), definition.AffectionThreshold);
            }
        }
    }
}
