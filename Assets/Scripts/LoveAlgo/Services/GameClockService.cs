using System;
using System.Collections.Generic;
using LoveAlgo.Data;

namespace LoveAlgo.Services
{
    public enum TimeOfDay
    {
        Day,
        Night
    }

    public sealed class GameClockService
    {
        private readonly IReadOnlyList<ScheduleEntry> schedule;
        private readonly int maxFreeActionsPerDay;

        private int currentDay = 1;
        private int freeActionsUsed;
        private TimeOfDay timeOfDay = TimeOfDay.Day;

        public GameClockService(ScheduleAsset scheduleAsset, int maxFreeActions)
        {
            schedule = scheduleAsset?.Entries ?? Array.Empty<ScheduleEntry>();
            maxFreeActionsPerDay = Math.Max(1, maxFreeActions);
        }

        public int CurrentDay => currentDay;
        public int FreeActionsUsed => freeActionsUsed;
        public TimeOfDay CurrentTimeOfDay => timeOfDay;
        public bool CanUseFreeAction => freeActionsUsed < maxFreeActionsPerDay;

        public event Action<int> DayChanged;
        public event Action<TimeOfDay> TimeOfDayChanged;
        public event Action<int, TimeOfDay> FreeActionStateChanged;
        public event Action<ScheduleEntry> ScheduleTriggered;

        public void ConsumeFreeAction()
        {
            if (!CanUseFreeAction)
            {
                return;
            }

            freeActionsUsed++;
            ToggleTimeOfDay();
            FreeActionStateChanged?.Invoke(freeActionsUsed, timeOfDay);
        }

        public void AdvanceDay()
        {
            currentDay++;
            freeActionsUsed = 0;
            timeOfDay = TimeOfDay.Day;
            DayChanged?.Invoke(currentDay);
            FreeActionStateChanged?.Invoke(freeActionsUsed, timeOfDay);
            TryTriggerSchedule(currentDay);
        }

        public void SetDay(int day)
        {
            currentDay = Math.Max(1, day);
            freeActionsUsed = 0;
            timeOfDay = TimeOfDay.Day;
            DayChanged?.Invoke(currentDay);
            FreeActionStateChanged?.Invoke(freeActionsUsed, timeOfDay);
        }

        private void ToggleTimeOfDay()
        {
            timeOfDay = timeOfDay == TimeOfDay.Day ? TimeOfDay.Night : TimeOfDay.Day;
            TimeOfDayChanged?.Invoke(timeOfDay);
        }

        private void TryTriggerSchedule(int day)
        {
            foreach (var entry in schedule)
            {
                if (entry.day == day)
                {
                    ScheduleTriggered?.Invoke(entry);
                    return;
                }
            }
        }
    }
}
