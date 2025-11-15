using System.Collections.Generic;
using UnityEngine;
using LoveAlgo.Core;

namespace LoveAlgo.UI
{
    public sealed class UiPortal : MonoBehaviour
    {
        [SerializeField] private Canvas storyCanvas;
        [SerializeField] private Canvas freeActionCanvas;
        [SerializeField] private Canvas eventCanvas;
        [SerializeField] private Canvas miniGameCanvas;
        [SerializeField] private Canvas messengerCanvas;

        private readonly Dictionary<GameMode, Canvas> canvasLookup = new();
        private GameModeController modeController;

        private void Awake()
        {
            if (!LoveAlgoContext.Exists)
            {
                Debug.LogWarning("LoveAlgoContext is not ready", this);
                return;
            }

            modeController = LoveAlgoContext.Instance.Get<GameModeController>();
            canvasLookup[GameMode.Story] = storyCanvas;
            canvasLookup[GameMode.FreeAction] = freeActionCanvas;
            canvasLookup[GameMode.Event] = eventCanvas;
            canvasLookup[GameMode.MiniGame] = miniGameCanvas;
            canvasLookup[GameMode.Messenger] = messengerCanvas;
        }

        private void OnEnable()
        {
            if (modeController != null)
            {
                modeController.ModeChanged += HandleModeChanged;
                HandleModeChanged(modeController.CurrentMode);
            }
        }

        private void OnDisable()
        {
            if (modeController != null)
            {
                modeController.ModeChanged -= HandleModeChanged;
            }
        }

        private void HandleModeChanged(GameMode mode)
        {
            foreach (var pair in canvasLookup)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                pair.Value.enabled = pair.Key == mode;
            }
        }
    }
}
