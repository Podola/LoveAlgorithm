using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "HeroineRoster", menuName = "LoveAlgo/Data/Heroine Roster")]
    public sealed class HeroineRoster : ScriptableObject
    {
        [SerializeField] private List<HeroineDefinition> heroines = new();

        public IReadOnlyList<HeroineDefinition> Heroines => heroines;
    }
}
