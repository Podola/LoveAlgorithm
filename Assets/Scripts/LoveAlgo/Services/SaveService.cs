using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Services
{
    [System.Serializable]
    public sealed class GameSnapshot
    {
        public int day;
        public PlayerStatsSnapshot stats;
        public List<AffectionRecord> affection = new();
    }

    [System.Serializable]
    public struct AffectionRecord
    {
        public string heroineId;
        public int points;
    }

    public sealed class SaveService
    {
        private readonly StatsService statsService;
        private readonly AffinityService affinityService;
        private readonly GameClockService clockService;

        public SaveService(StatsService statsService, AffinityService affinityService, GameClockService clockService)
        {
            this.statsService = statsService;
            this.affinityService = affinityService;
            this.clockService = clockService;
        }

        public GameSnapshot Capture()
        {
            var snapshot = new GameSnapshot
            {
                day = clockService.CurrentDay,
                stats = statsService.Snapshot
            };

            // 간단한 전체 목록 스냅샷
            foreach (var record in GetAllAffection())
            {
                snapshot.affection.Add(record);
            }

            return snapshot;
        }

        public string ToJson(GameSnapshot snapshot)
        {
            return JsonUtility.ToJson(snapshot);
        }

        public void Restore(GameSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            clockService.SetDay(snapshot.day);
            statsService.Load(snapshot.stats);

            if (snapshot.affection == null)
            {
                return;
            }

            foreach (var record in snapshot.affection)
            {
                var current = affinityService.GetPoints(record.heroineId);
                var delta = record.points - current;
                if (delta != 0)
                {
                    affinityService.AddPoints(record.heroineId, delta);
                }
            }
        }

        private IEnumerable<AffectionRecord> GetAllAffection()
        {
            foreach (var snapshot in affinityService.Enumerate())
            {
                yield return new AffectionRecord
                {
                    heroineId = snapshot.HeroineId,
                    points = snapshot.Points
                };
            }
        }
    }
}
