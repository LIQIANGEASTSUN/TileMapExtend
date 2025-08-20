using System.Collections.Generic;
using UnityEngine;


namespace Merge.TileMap
{
    public partial class TilemapComponent
    {
        [Header("Serialized Cells")] [SerializeField]
        private List<TileData> serializedCells = new List<TileData>();

        private void Deserialize()
        {
            foreach (var data in GetSerializedCells())
            {
                grid.SetTile(data);
            }
        }
        
        public List<TileData> GetSerializedCells()
        {
            return serializedCells;
        }

    }
}