using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Data
{
    [CreateAssetMenu(fileName = "BackgroundCatalog", menuName = "LoveAlgo/Data/Background Catalog")]
    public sealed class BackgroundCatalog : ScriptableObject
    {
        [SerializeField] private List<BackgroundDefinition> items = new();

        public bool TryGetSprite(string id, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            foreach (var item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.Id))
                {
                    continue;
                }

                if (item.Id == id)
                {
                    sprite = item.Sprite;
                    return sprite != null;
                }
            }

            return false;
        }
    }
}
