using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "FreeActionDefinition", menuName = "LoveAlgo/Data/Free Action")]
    public sealed class FreeActionDefinition : ScriptableObject
    {
        [SerializeField] private string actionId = "exercise";
        [SerializeField] private string displayName = "운동";
        [SerializeField] private int healthDelta;
        [SerializeField] private int intelligenceDelta;
        [SerializeField] private int socialDelta;
        [SerializeField] private int persistenceDelta;
        [SerializeField] private int fatigueDelta;
        [SerializeField] private int moneyDelta;
        [SerializeField] private bool oncePerDay;

        public string ActionId => actionId;
        public string DisplayName => displayName;
        public int HealthDelta => healthDelta;
        public int IntelligenceDelta => intelligenceDelta;
        public int SocialDelta => socialDelta;
        public int PersistenceDelta => persistenceDelta;
        public int FatigueDelta => fatigueDelta;
        public int MoneyDelta => moneyDelta;
        public bool OncePerDay => oncePerDay;
    }
}
