using LoveAlgo.Core;
using LoveAlgo.Data;
using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace LoveAlgo.Services
{
    public sealed class DialogueBridgeService
    {
        private readonly StatsService statsService;
        private readonly AffinityService affinityService;
        private readonly GameModeController modeController;
        private readonly FreeActionService freeActionService;

        public DialogueBridgeService(StatsService statsService, AffinityService affinityService, GameModeController modeController, FreeActionService freeActionService)
        {
            this.statsService = statsService;
            this.affinityService = affinityService;
            this.modeController = modeController;
            this.freeActionService = freeActionService;

            RegisterLuaFunctions();
        }

        private void RegisterLuaFunctions()
        {
            // DSU Lua 환경에 함수들을 등록
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("PushStats", this, SymbolExtensions.GetMethodInfo(() => PushStats()));
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("ApplyAffinity", this, SymbolExtensions.GetMethodInfo(() => ApplyAffinity("", 0)));
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("EnterMode", this, SymbolExtensions.GetMethodInfo(() => EnterMode(GameMode.Story)));
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("GetHeroineName", this, SymbolExtensions.GetMethodInfo(() => GetHeroineName("")));
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("GetHeroineStats", this, SymbolExtensions.GetMethodInfo(() => GetHeroineStats("")));
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("ExecuteFreeAction", this, SymbolExtensions.GetMethodInfo(() => ExecuteFreeAction("")));
        }

        public delegate void StatsSyncHandler(PlayerStatsSnapshot snapshot);
        public event StatsSyncHandler StatsSyncRequested;

        public void PushStats()
        {
            StatsSyncRequested?.Invoke(statsService.Snapshot);
        }

        public void ApplyAffinity(string heroineId, int delta)
        {
            affinityService.AddPoints(heroineId, delta);
        }

        public void EnterMode(GameMode mode)
        {
            modeController.SetMode(mode);
        }

        public string GetHeroineName(string heroineId)
        {
            if (LoveAlgoContext.Exists)
            {
                var config = LoveAlgoContext.Instance.Get<LoveAlgoConfiguration>();
                var heroines = config.HeroineRoster.Heroines;

                foreach (var heroine in heroines)
                {
                    if (heroine.HeroineId == heroineId)
                    {
                        return heroine.DisplayName;
                    }
                }
            }
            return heroineId; // 찾지 못하면 ID 그대로 반환
        }

        public string GetHeroineStats(string heroineId)
        {
            if (LoveAlgoContext.Exists)
            {
                var affinityService = LoveAlgoContext.Instance.Get<AffinityService>();
                var points = affinityService.GetPoints(heroineId);
                var threshold = GetHeroineAffectionThreshold(heroineId);

                return $"{points}/{threshold}";
            }
            return "0/0";
        }

        private int GetHeroineAffectionThreshold(string heroineId)
        {
            if (LoveAlgoContext.Exists)
            {
                var config = LoveAlgoContext.Instance.Get<LoveAlgoConfiguration>();
                var heroines = config.HeroineRoster.Heroines;

                foreach (var heroine in heroines)
                {
                    if (heroine.HeroineId == heroineId)
                    {
                        return heroine.AffectionThreshold;
                    }
                }
            }
            return 32; // 기본값
        }

        public bool ExecuteFreeAction(string actionId)
        {
            if (freeActionService == null)
            {
                Debug.LogWarning("[DialogueBridgeService] FreeActionService is not available.");
                return false;
            }

            if (!freeActionService.TryExecute(actionId))
            {
                Debug.LogWarning($"[DialogueBridgeService] Failed to execute free action '{actionId}'.");
                return false;
            }

            if (!freeActionService.HasRemainingActions)
            {
                EnterMode(GameMode.Story);
            }

            PushStats();
            return true;
        }
    }
}
