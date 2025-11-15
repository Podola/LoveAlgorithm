using System.Text;
using LoveAlgo.Core;
using LoveAlgo.Data;
using LoveAlgo.Services;
using UnityEngine;

namespace LoveAlgo.Testing
{
    /// <summary>
    /// Lightweight harness that subscribes to core services and logs checkpoints, making manual smoke tests easier.
    /// </summary>
    public sealed class LoveAlgoRuntimeSmokeTester : MonoBehaviour
    {
        [SerializeField] private GameLoopSample loopDriver;

        private StatsService statsService;
        private GameClockService clockService;
        private DialogueBridgeService dialogueBridge;
        private FreeActionService freeActionService;
        private ContentService contentService;
        private bool initialized;

        private void Awake()
        {
            Debug.Log("[RuntimeSmokeTester] Awake: waiting for LoveAlgoContext...");
        }

        private void Start()
        {
            if (!LoveAlgoContext.Exists)
            {
                Debug.LogError("[RuntimeSmokeTester] ❌ LoveAlgoContext is missing.", this);
                enabled = false;
                return;
            }

            statsService = LoveAlgoContext.Instance.Get<StatsService>();
            clockService = LoveAlgoContext.Instance.Get<GameClockService>();
            dialogueBridge = LoveAlgoContext.Instance.Get<DialogueBridgeService>();
            freeActionService = LoveAlgoContext.Instance.Get<FreeActionService>();
            contentService = LoveAlgoContext.Instance.Get<ContentService>();

            Subscribe();
            initialized = true;
            Debug.Log("[RuntimeSmokeTester] ✅ Initialized and subscribed to services.", this);
        }

        private void OnDestroy()
        {
            if (!initialized)
            {
                return;
            }

            Unsubscribe();
        }

        public void AutoExecuteFirstFreeAction()
        {
            if (!EnsureReady(nameof(AutoExecuteFirstFreeAction)))
            {
                return;
            }

            var actionId = GetFirstExecutableActionId();
            if (string.IsNullOrEmpty(actionId))
            {
                Debug.LogWarning("[RuntimeSmokeTester] ⚠️ No executable free action found.", this);
                return;
            }

            Debug.Log($"[RuntimeSmokeTester] ▶ Attempting free action '{actionId}'.", this);
            if (freeActionService.TryExecute(actionId))
            {
                Debug.Log($"[RuntimeSmokeTester] ✅ Free action '{actionId}' completed.", this);
            }
            else
            {
                Debug.LogWarning($"[RuntimeSmokeTester] ❌ Free action '{actionId}' blocked.", this);
            }
        }

        public void RequestStatsSync()
        {
            if (!EnsureReady(nameof(RequestStatsSync)))
            {
                return;
            }

            Debug.Log("[RuntimeSmokeTester] ▶ Forcing DialogueBridge.PushStats().", this);
            dialogueBridge.PushStats();
        }

        public void CompleteEvent()
        {
            if (loopDriver == null)
            {
                Debug.LogWarning("[RuntimeSmokeTester] Loop driver not assigned.", this);
                return;
            }

            Debug.Log("[RuntimeSmokeTester] ▶ Completing event via GameLoopSample.", this);
            loopDriver.CompleteEvent();
        }

        public void DumpEpisodeTimeline()
        {
            if (!EnsureReady(nameof(DumpEpisodeTimeline)))
            {
                return;
            }

            if (contentService == null || contentService.Timeline.Count == 0)
            {
                Debug.LogWarning("[RuntimeSmokeTester] Episode timeline is empty.", this);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("[RuntimeSmokeTester] 📚 Episode timeline snapshot:");
            foreach (var entry in contentService.Timeline)
            {
                var episode = entry.Episode;
                if (episode != null)
                {
                    builder.AppendLine($"  • Day {entry.Day}: {episode.DisplayName} [{episode.Stage}] base={episode.Points.EventPoints} payload='{entry.Payload}'");
                }
                else
                {
                    builder.AppendLine($"  • Day {entry.Day}: (mode {entry.Mode}) payload='{entry.Payload}'");
                }
            }

            Debug.Log(builder.ToString(), this);
        }

        private void Subscribe()
        {
            statsService.StatsChanged += HandleStatsChanged;
            dialogueBridge.StatsSyncRequested += HandleStatsSyncRequested;
            clockService.DayChanged += HandleDayChanged;
            clockService.TimeOfDayChanged += HandleTimeOfDayChanged;
            clockService.FreeActionStateChanged += HandleFreeActionStateChanged;
            clockService.ScheduleTriggered += HandleScheduleTriggered;
            freeActionService.ActionExecuted += HandleFreeActionExecuted;
        }

        private void Unsubscribe()
        {
            statsService.StatsChanged -= HandleStatsChanged;
            dialogueBridge.StatsSyncRequested -= HandleStatsSyncRequested;
            clockService.DayChanged -= HandleDayChanged;
            clockService.TimeOfDayChanged -= HandleTimeOfDayChanged;
            clockService.FreeActionStateChanged -= HandleFreeActionStateChanged;
            clockService.ScheduleTriggered -= HandleScheduleTriggered;
            freeActionService.ActionExecuted -= HandleFreeActionExecuted;
        }

        private void HandleStatsChanged(PlayerStatsSnapshot snapshot)
        {
            var builder = new StringBuilder();
            builder.Append("[RuntimeSmokeTester] 📊 Stats -> ");
            builder.Append($"HP:{snapshot.Health} ");
            builder.Append($"INT:{snapshot.Intelligence} ");
            builder.Append($"SOC:{snapshot.Social} ");
            builder.Append($"PER:{snapshot.Persistence} ");
            builder.Append($"FAT:{snapshot.Fatigue} ");
            builder.Append($"₩:{snapshot.Money}");
            Debug.Log(builder.ToString(), this);
        }

        private void HandleStatsSyncRequested(PlayerStatsSnapshot snapshot)
        {
            Debug.Log("[RuntimeSmokeTester] 🔄 DialogueBridge requested stats sync.", this);
        }

        private void HandleDayChanged(int day)
        {
            Debug.Log($"[RuntimeSmokeTester] 📅 Day advanced -> {day}.", this);
        }

        private void HandleTimeOfDayChanged(TimeOfDay time)
        {
            Debug.Log($"[RuntimeSmokeTester] 🌓 Time switched -> {time}.", this);
        }

        private void HandleFreeActionStateChanged(int used, TimeOfDay time)
        {
            Debug.Log($"[RuntimeSmokeTester] 🎯 Free actions used {used}, time {time}, remaining={(freeActionService.HasRemainingActions ? "Yes" : "No")}.", this);
        }

        private void HandleFreeActionExecuted(FreeActionDefinition action)
        {
            Debug.Log($"[RuntimeSmokeTester] ✅ ActionExecuted -> {action.DisplayName} ({action.ActionId}).", this);
        }

        private void HandleScheduleTriggered(ScheduleEntry entry)
        {
            var episode = contentService?.ResolveSchedule(entry);
            if (episode != null)
            {
                Debug.Log($"[RuntimeSmokeTester] 📣 Schedule triggered: Day {entry.day} -> {episode.DisplayName} [{episode.Stage}] base={episode.Points.EventPoints}.", this);
            }
            else
            {
                Debug.Log($"[RuntimeSmokeTester] 📣 Schedule triggered: Day {entry.day} payload='{entry.payload}'.", this);
            }
        }

        private bool EnsureReady(string caller)
        {
            if (!initialized)
            {
                Debug.LogWarning($"[RuntimeSmokeTester] {caller} called before initialization.", this);
                return false;
            }

            return true;
        }

        private string GetFirstExecutableActionId()
        {
            foreach (var action in freeActionService.AvailableActions)
            {
                if (freeActionService.CanExecute(action.ActionId))
                {
                    return action.ActionId;
                }
            }

            return null;
        }
    }
}
