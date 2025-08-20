using UnityEngine;

namespace Merge.TileMap
{
    public abstract class TileBase : ScriptableObject
    {
        public Sprite Sprite;

        public virtual void OnInitialize(TileData data)
        {
        }
    }
}