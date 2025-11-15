using System;
using System.Collections;
using System.Collections.Generic;
using LoveAlgo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace LoveAlgo.UI.Dialogue
{
    public enum StandingSlot
    {
        Left,
        Center,
        Right
    }

    [Serializable]
    public sealed class StandingSlotBinding
    {
        public StandingSlot slot = StandingSlot.Left;
        public Image image;
    }

    public sealed class LoveAlgoStandingPresenter : MonoBehaviour
    {
        [SerializeField] private StandingPoseCatalog catalog;
        [SerializeField] private List<StandingSlotBinding> slots = new();
        [SerializeField] [Range(0f, 1f)] private float unfocusedAlpha = 0.45f;
        [SerializeField] [Min(0f)] private float defaultShakeDuration = 0.25f;
        [SerializeField] [Min(0f)] private float defaultShakeMagnitude = 12f;

        private readonly Dictionary<StandingSlot, StandingSlotBinding> slotLookup = new();
        private readonly Dictionary<StandingSlot, StandingAssignment> activeAssignments = new();
        private readonly Dictionary<StandingSlot, Coroutine> shakeRoutines = new();
        private StandingSlot? currentFocusSlot;

        private void Awake()
        {
            slotLookup.Clear();
            activeAssignments.Clear();
            shakeRoutines.Clear();
            currentFocusSlot = null;
            foreach (var binding in slots)
            {
                if (binding == null)
                {
                    continue;
                }

                slotLookup[binding.slot] = binding;
                if (binding.image != null)
                {
                    binding.image.enabled = false;
                }
            }
        }

        private void OnDisable()
        {
            StopAllShakeRoutines();
        }

        public void ApplyLayout(IEnumerable<StandingInstruction> instructions)
        {
            if (instructions == null)
            {
                return;
            }

            foreach (var instruction in instructions)
            {
                Show(instruction.slot, instruction.heroineId, instruction.poseId);
            }
        }

        public void Show(StandingSlot slot, string heroineId, string poseId)
        {
            if (catalog == null || string.IsNullOrEmpty(heroineId))
            {
                return;
            }

            if (!slotLookup.TryGetValue(slot, out var binding) || binding.image == null)
            {
                return;
            }

            if (!catalog.TryGetSprite(heroineId, poseId, out var sprite))
            {
                Debug.LogWarning($"LoveAlgoStandingPresenter: sprite missing for {heroineId}:{poseId}");
                return;
            }

            binding.image.sprite = sprite;
            binding.image.enabled = true;
            binding.image.color = Color.white;
            activeAssignments[slot] = new StandingAssignment(heroineId, poseId);
            ApplyFocusColors();
        }

        public void Hide(StandingSlot slot)
        {
            if (!slotLookup.TryGetValue(slot, out var binding) || binding.image == null)
            {
                return;
            }

            binding.image.enabled = false;
            binding.image.sprite = null;
            activeAssignments.Remove(slot);
            StopShakeRoutine(slot);
            ApplyFocusColors();
        }

        public void HideAll()
        {
            StopAllShakeRoutines();
            foreach (var binding in slotLookup.Values)
            {
                if (binding?.image == null)
                {
                    continue;
                }

                binding.image.enabled = false;
                binding.image.sprite = null;
            }
            activeAssignments.Clear();
            currentFocusSlot = null;
            ApplyFocusColors();
        }

        public void Focus(StandingSlot slot)
        {
            currentFocusSlot = slot;
            ApplyFocusColors();
        }

        public void FocusByHeroine(string heroineId)
        {
            if (string.IsNullOrWhiteSpace(heroineId))
            {
                ClearFocus();
                return;
            }

            if (TryGetSlotForHeroine(heroineId, out var slot))
            {
                Focus(slot);
            }
            else
            {
                ClearFocus();
            }
        }

        public void ClearFocus()
        {
            currentFocusSlot = null;
            ApplyFocusColors();
        }

        public void Shake(StandingSlot slot, float duration = -1f, float magnitude = -1f)
        {
            if (!slotLookup.TryGetValue(slot, out var binding) || binding.image == null)
            {
                return;
            }

            if (!binding.image.enabled)
            {
                return;
            }

            var rect = binding.image.rectTransform;
            if (rect == null)
            {
                return;
            }

            duration = duration <= 0f ? defaultShakeDuration : duration;
            magnitude = magnitude <= 0f ? defaultShakeMagnitude : magnitude;
            if (duration <= 0f || magnitude <= 0f)
            {
                return;
            }

            StopShakeRoutine(slot);
            shakeRoutines[slot] = StartCoroutine(ShakeRoutine(slot, rect, duration, magnitude));
        }

        private void ApplyFocusColors()
        {
            foreach (var pair in slotLookup)
            {
                var binding = pair.Value;
                if (binding?.image == null)
                {
                    continue;
                }

                if (!binding.image.enabled)
                {
                    continue;
                }

                var targetColor = Color.white;
                if (currentFocusSlot.HasValue && pair.Key != currentFocusSlot.Value)
                {
                    targetColor.a = Mathf.Clamp01(unfocusedAlpha);
                }
                else
                {
                    targetColor.a = 1f;
                }

                binding.image.color = targetColor;
            }
        }

        private bool TryGetSlotForHeroine(string heroineId, out StandingSlot slot)
        {
            foreach (var pair in activeAssignments)
            {
                if (string.Equals(pair.Value.heroineId, heroineId, StringComparison.OrdinalIgnoreCase))
                {
                    slot = pair.Key;
                    return true;
                }
            }

            slot = default;
            return false;
        }

        private void StopShakeRoutine(StandingSlot slot)
        {
            if (shakeRoutines.TryGetValue(slot, out var routine) && routine != null)
            {
                StopCoroutine(routine);
            }

            shakeRoutines.Remove(slot);
        }

        private void StopAllShakeRoutines()
        {
            foreach (var routine in shakeRoutines.Values)
            {
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
            }

            shakeRoutines.Clear();
        }

        private IEnumerator ShakeRoutine(StandingSlot slot, RectTransform rect, float duration, float magnitude)
        {
            var elapsed = 0f;
            var original = rect.anchoredPosition;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var offset = UnityEngine.Random.insideUnitCircle * magnitude;
                rect.anchoredPosition = original + offset;
                yield return null;
            }

            rect.anchoredPosition = original;
            shakeRoutines.Remove(slot);
        }

        private readonly struct StandingAssignment
        {
            public readonly string heroineId;
            public readonly string poseId;

            public StandingAssignment(string heroineId, string poseId)
            {
                this.heroineId = heroineId;
                this.poseId = poseId;
            }
        }
    }

    public readonly struct StandingInstruction
    {
        public readonly StandingSlot slot;
        public readonly string heroineId;
        public readonly string poseId;

        public StandingInstruction(StandingSlot slot, string heroineId, string poseId)
        {
            this.slot = slot;
            this.heroineId = heroineId;
            this.poseId = poseId;
        }
    }
}
