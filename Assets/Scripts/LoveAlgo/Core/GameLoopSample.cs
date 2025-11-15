using UnityEngine;
using LoveAlgo.Services;
using LoveAlgo.Data;

namespace LoveAlgo.Core
{
    /// <summary>
    /// 최소한의 게임 루프 데모. UI 버튼 혹은 DSU 커맨드에서 공개 메서드를 호출해 흐름을 진입한다.
    /// </summary>
    public sealed class GameLoopSample : MonoBehaviour
    {
        private GameModeController modeController;
        private GameClockService clockService;
        private DialogueBridgeService dialogueBridge;
        private FreeActionService freeActionService;
        private ContentService contentService;

        private void Start()
        {
            if (!LoveAlgoContext.Exists)
            {
                Debug.LogError("LoveAlgoContext is missing", this);
                enabled = false;
                return;
            }

            modeController = LoveAlgoContext.Instance.Get<GameModeController>();
            clockService = LoveAlgoContext.Instance.Get<GameClockService>();
            dialogueBridge = LoveAlgoContext.Instance.Get<DialogueBridgeService>();
            freeActionService = LoveAlgoContext.Instance.Get<FreeActionService>();
            contentService = LoveAlgoContext.Instance.Get<ContentService>();

            clockService.ScheduleTriggered += HandleSchedule;
            EnterStory();
        }

        public void EnterFreeAction()
        {
            if (!clockService.CanUseFreeAction)
            {
                Debug.Log("[GameLoopSample] ⚠️ EnterFreeAction blocked (no remaining actions).", this);
                return;
            }

            Debug.Log("[GameLoopSample] ▶ Switching to FreeAction mode.", this);
            modeController.SetMode(GameMode.FreeAction);
        }

        public void CompleteFreeAction()
        {
            var fallback = GetFallbackActionId();
            if (!string.IsNullOrEmpty(fallback))
            {
                PerformFreeAction(fallback);
                return;
            }

            // 데이터가 비어있는 경우 기존 동작 유지
            Debug.Log("[GameLoopSample] ▶ Consuming free action via legacy fallback.", this);
            clockService.ConsumeFreeAction();
            AfterFreeAction();
        }

        public void PerformFreeAction(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                Debug.LogWarning("[GameLoopSample] actionId is missing.", this);
                return;
            }

            Debug.Log($"[GameLoopSample] ▶ Performing free action '{actionId}'.", this);
            if (!freeActionService.TryExecute(actionId))
            {
                Debug.LogWarning($"[GameLoopSample] Unable to execute free action '{actionId}'.", this);
                return;
            }

            AfterFreeAction();
        }

        public void EnterStory()
        {
            Debug.Log("[GameLoopSample] ▶ Entering Story mode.", this);
            modeController.SetMode(GameMode.Story);
            dialogueBridge.PushStats();
        }

        public void CompleteEvent()
        {
            Debug.Log("[GameLoopSample] ▶ Completing event and advancing day.", this);
            clockService.AdvanceDay();
            modeController.SetMode(GameMode.Story);
        }

        private void HandleSchedule(ScheduleEntry entry)
        {
            var episode = contentService?.ResolveSchedule(entry);
            if (episode != null)
            {
                Debug.Log($"[GameLoopSample] 📣 Schedule triggered -> {episode.DisplayName} [{episode.Stage}] base={episode.Points.EventPoints} (Day {entry.day}).", this);
            }
            else
            {
                Debug.Log($"[GameLoopSample] 📣 Schedule triggered -> payload '{entry.payload}' (Day {entry.day}).", this);
            }
            modeController.SetMode(GameMode.Event);
            // DSU 커맨드로 이벤트 Conversation을 호출하는 연결부는 DialogueBridge 핸들러에서 구현.
        }

        private void OnDestroy()
        {
            if (clockService != null)
            {
                clockService.ScheduleTriggered -= HandleSchedule;
            }
        }

        private string GetFallbackActionId()
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

        private void AfterFreeAction()
        {
            if (!clockService.CanUseFreeAction)
            {
                EnterStory();
            }
        }
    }
}
