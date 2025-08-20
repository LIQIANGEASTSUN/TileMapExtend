using System;
using UnityEngine;

namespace Merge.TileMap
{
    [Serializable]
    public struct TileData
    {
        public Vector3Int Position;
        public int SpriteIndex;
    }
}