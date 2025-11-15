using System;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace LoveAlgo.UI.Dialogue
{
    /// <summary>
    /// DSU sequencer command: LoveAlgoStandingShake(slot, duration?, magnitude?)
    /// Example: LoveAlgoStandingShake(Left, 0.3, 18)
    /// </summary>
    public sealed class SequencerCommandLoveAlgoStandingShake : SequencerCommand
    {
        public void Start()
        {
            var slotToken = GetParameter(0);
            var duration = GetParameterAsFloat(1, -1f);
            var magnitude = GetParameterAsFloat(2, -1f);

            if (string.IsNullOrEmpty(slotToken))
            {
                Debug.LogWarning("LoveAlgoStandingShake: slot parameter required.");
                Stop();
                return;
            }

            if (!Enum.TryParse(slotToken, true, out StandingSlot slot))
            {
                Debug.LogWarning($"LoveAlgoStandingShake: invalid slot '{slotToken}'.");
                Stop();
                return;
            }

            var view = LoveAlgoDialogueView.ActiveInstance;
            if (view?.StandingPresenter == null)
            {
                Debug.LogWarning("LoveAlgoStandingShake: LoveAlgoDialogueView or StandingPresenter not found.");
                Stop();
                return;
            }

            view.StandingPresenter.Shake(slot, duration, magnitude);
            Stop();
        }
    }
}
