using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "EpisodeCatalog", menuName = "LoveAlgo/Data/Episode Catalog")]
    public sealed class EpisodeCatalog : ScriptableObject
    {
        [SerializeField] private List<EpisodeDefinition> episodes = new();

        public IReadOnlyList<EpisodeDefinition> Episodes => episodes;
    }
}
