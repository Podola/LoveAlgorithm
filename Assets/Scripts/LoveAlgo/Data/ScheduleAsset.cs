using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    public enum ScheduleMode
    {
        Free,
        StoryEvent,
        Festival,
        Retreat,
        Confession,
        Ending
    }

    [Serializable]
    public struct ScheduleEntry
    {
        public int day;
        public ScheduleMode mode;
        public string payload;
    }

    [CreateAssetMenu(fileName = "ScheduleAsset", menuName = "LoveAlgo/Data/Schedule")]
    public sealed class ScheduleAsset : ScriptableObject
    {
        [SerializeField] private List<ScheduleEntry> entries = new();

        public IReadOnlyList<ScheduleEntry> Entries => entries;
    }
}
