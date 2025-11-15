using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "FreeActionCatalog", menuName = "LoveAlgo/Data/Free Action Catalog")]
    public sealed class FreeActionCatalog : ScriptableObject
    {
        [SerializeField] private List<FreeActionDefinition> actions = new();

        public IReadOnlyList<FreeActionDefinition> Actions => actions;
    }
}
