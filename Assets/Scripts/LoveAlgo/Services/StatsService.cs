using System;
using LoveAlgo.Data;

namespace LoveAlgo.Services
{
    [System.Serializable]
    public readonly struct PlayerStatsSnapshot
    {
        public readonly int Health;
        public readonly int Intelligence;
        public readonly int Social;
        public readonly int Persistence;
        public readonly int Fatigue;
        public readonly int Money;

        public PlayerStatsSnapshot(int health, int intelligence, int social, int persistence, int fatigue, int money)
        {
            Health = health;
            Intelligence = intelligence;
            Social = social;
            Persistence = persistence;
            Fatigue = fatigue;
            Money = money;
        }
    }

    public sealed class StatsService
    {
        private readonly int minFatigue;
        private readonly int maxFatigue;

        private PlayerStatsSnapshot snapshot;

        public StatsService(int minFatigue, int maxFatigue)
        {
            this.minFatigue = minFatigue;
            this.maxFatigue = maxFatigue;
            snapshot = new PlayerStatsSnapshot(10, 10, 10, 10, 0, 10000);
        }

        public event Action<PlayerStatsSnapshot> StatsChanged;

        public PlayerStatsSnapshot Snapshot => snapshot;

        public void ApplyDelta(int health, int intelligence, int social, int persistence, int fatigue, int money)
        {
            var newFatigue = Clamp(snapshot.Fatigue + fatigue, minFatigue, maxFatigue);
            snapshot = new PlayerStatsSnapshot(
                Math.Max(0, snapshot.Health + health),
                Math.Max(0, snapshot.Intelligence + intelligence),
                Math.Max(0, snapshot.Social + social),
                Math.Max(0, snapshot.Persistence + persistence),
                newFatigue,
                Math.Max(0, snapshot.Money + money));

            StatsChanged?.Invoke(snapshot);
        }

        public bool CanAfford(int amount) => snapshot.Money >= amount;

        public bool CanIncreaseFatigue(int delta)
        {
            var target = snapshot.Fatigue + delta;
            return target <= maxFatigue;
        }

        public void SetFatigue(int value)
        {
            snapshot = new PlayerStatsSnapshot(
                snapshot.Health,
                snapshot.Intelligence,
                snapshot.Social,
                snapshot.Persistence,
                Clamp(value, minFatigue, maxFatigue),
                snapshot.Money);

            StatsChanged?.Invoke(snapshot);
        }

        public void Load(PlayerStatsSnapshot saved)
        {
            snapshot = new PlayerStatsSnapshot(
                Math.Max(0, saved.Health),
                Math.Max(0, saved.Intelligence),
                Math.Max(0, saved.Social),
                Math.Max(0, saved.Persistence),
                Clamp(saved.Fatigue, minFatigue, maxFatigue),
                Math.Max(0, saved.Money));

            StatsChanged?.Invoke(snapshot);
        }

        private static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
    }
}
