using UnityEngine;

namespace LoveAlgo.Data
{
    [System.Serializable]
    public sealed class BackgroundDefinition
    {
        [SerializeField] private string id = "campus_day";
        [SerializeField] private Sprite sprite;

        public string Id => id;
        public Sprite Sprite => sprite;
    }
}
