using System;
using System.Collections.Generic;
using LoveAlgo.Data;

namespace LoveAlgo.Services
{
    public readonly struct EpisodeTimelineEntry
    {
        private readonly ScheduleEntry scheduleEntry;
        private readonly EpisodeDefinition episode;

        public EpisodeTimelineEntry(ScheduleEntry scheduleEntry, EpisodeDefinition episode)
        {
            this.scheduleEntry = scheduleEntry;
            this.episode = episode;
        }

        public int Day => scheduleEntry.day;
        public ScheduleMode Mode => scheduleEntry.mode;
        public string Payload => scheduleEntry.payload;
        public EpisodeDefinition Episode => episode;
        public ScheduleEntry Schedule => scheduleEntry;
        public bool BlocksFreeActions => episode?.LocksFreeActions ?? Mode != ScheduleMode.Free;
    }

    public sealed class ContentService
    {
        private readonly Dictionary<string, EpisodeDefinition> lookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<EpisodeTimelineEntry> timeline = new();

        public ContentService(EpisodeCatalog catalog, ScheduleAsset schedule)
        {
            if (catalog != null)
            {
                foreach (var episode in catalog.Episodes)
                {
                    if (episode == null)
                    {
                        continue;
                    }

                    var id = episode.EpisodeId;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    lookup[id] = episode;
                }
            }

            if (schedule != null)
            {
                foreach (var entry in schedule.Entries)
                {
                    timeline.Add(new EpisodeTimelineEntry(entry, ResolveEpisode(entry.payload)));
                }

                timeline.Sort((a, b) => a.Day.CompareTo(b.Day));
            }
        }

        public IReadOnlyList<EpisodeTimelineEntry> Timeline => timeline;

        public EpisodeDefinition GetEpisode(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId))
            {
                return null;
            }

            return lookup.TryGetValue(episodeId, out var episode) ? episode : null;
        }

        public EpisodeDefinition GetEpisode(EpisodeStage stage)
        {
            foreach (var entry in lookup)
            {
                if (entry.Value.Stage == stage)
                {
                    return entry.Value;
                }
            }

            return null;
        }

        public EpisodeDefinition ResolveSchedule(ScheduleEntry entry)
        {
            return ResolveEpisode(entry.payload);
        }

        public EpisodeTimelineEntry? GetEntryForDay(int day)
        {
            for (var i = 0; i < timeline.Count; i++)
            {
                if (timeline[i].Day == day)
                {
                    return timeline[i];
                }
            }

            return null;
        }

        private EpisodeDefinition ResolveEpisode(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId))
            {
                return null;
            }

            return lookup.TryGetValue(episodeId, out var episode) ? episode : null;
        }
    }
}
