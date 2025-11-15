using System;
using System.Collections.Generic;
using LoveAlgo.Data;

namespace LoveAlgo.Services
{
    public sealed class FreeActionService
    {
        private readonly Dictionary<string, FreeActionDefinition> actionLookup;
        private readonly HashSet<string> usedToday = new();
        private readonly StatsService statsService;
        private readonly GameClockService clockService;

        public FreeActionService(FreeActionCatalog catalog, StatsService statsService, GameClockService clockService)
        {
            this.statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));
            this.clockService = clockService ?? throw new ArgumentNullException(nameof(clockService));

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var actions = catalog.Actions ?? Array.Empty<FreeActionDefinition>();
            actionLookup = new Dictionary<string, FreeActionDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var action in actions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.ActionId))
                {
                    continue;
                }

                actionLookup[action.ActionId] = action;
            }

            AvailableActions = actions;
            clockService.DayChanged += HandleDayChanged;
        }

        public IReadOnlyList<FreeActionDefinition> AvailableActions { get; }
        public bool HasRemainingActions => clockService.CanUseFreeAction;

        public event Action<FreeActionDefinition> ActionExecuted;

        public bool CanExecute(string actionId)
        {
            if (!clockService.CanUseFreeAction)
            {
                return false;
            }

            if (!TryGetAction(actionId, out var action))
            {
                return false;
            }

            if (action.OncePerDay && usedToday.Contains(action.ActionId))
            {
                return false;
            }

            if (action.FatigueDelta > 0 && !statsService.CanIncreaseFatigue(action.FatigueDelta))
            {
                return false;
            }

            if (action.MoneyDelta < 0 && !statsService.CanAfford(Math.Abs(action.MoneyDelta)))
            {
                return false;
            }

            return true;
        }

        public bool TryExecute(string actionId)
        {
            if (!CanExecute(actionId))
            {
                return false;
            }

            var action = actionLookup[actionId];
            statsService.ApplyDelta(
                action.HealthDelta,
                action.IntelligenceDelta,
                action.SocialDelta,
                action.PersistenceDelta,
                action.FatigueDelta,
                action.MoneyDelta);

            clockService.ConsumeFreeAction();

            if (action.OncePerDay)
            {
                usedToday.Add(action.ActionId);
            }

            ActionExecuted?.Invoke(action);
            return true;
        }

        private bool TryGetAction(string actionId, out FreeActionDefinition action)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                action = null;
                return false;
            }

            return actionLookup.TryGetValue(actionId, out action);
        }

        private void HandleDayChanged(int _)
        {
            usedToday.Clear();
        }
    }
}
