using System.Collections.Generic;
using UnityEngine;

namespace Merge.TileMap
{
    public class ParallelogramGrid
    {
        private Dictionary<Vector3Int, TileData> _cells = new Dictionary<Vector3Int, TileData>();
        private Vector3 _origin;
        private Vector3 _axisRow;
        private Vector3 _axisCol;
        private Matrix4x4 _worldToGrid;

        public void Initialize(Vector3 origin, Vector3 axisRow, Vector3 axisCol)
        {
            _origin = origin;
            _axisRow = axisRow;
            _axisCol = axisCol;

            var m =  new Matrix4x4();
            m.SetColumn(0, new Vector4(axisCol.x, axisCol.y, axisCol.z, 0));
            m.SetColumn(1, new Vector4(axisRow.x, axisRow.y, axisRow.z, 0));
            m.SetColumn(2, new Vector4(0, 0, 1, 0));
            m.SetColumn(3, new Vector4(origin.x, origin.y, origin.z, 1));
            _worldToGrid = m.inverse;
            _cells.Clear();
        }

        public void SetTile(TileData cell)
        {
            _cells[cell.Position] = cell;
        }

        public bool HasTile(Vector3Int cellPos)
        {
            return _cells.ContainsKey(cellPos);
        }
        
        public bool TryGetTile(Vector3Int pos, out TileData cell)
        {
            return _cells.TryGetValue(pos, out cell);
        }

        public void RemoveTile(Vector3Int pos)
        {
            _cells.Remove(pos);
        }

        public int CellCount => _cells.Count;
        
        public IEnumerable<TileData> GetAllTiles()
        {
            return _cells.Values;
        }

        public Vector3 CellToWorld(Vector3Int cell)
        {
            return _origin + _axisRow * cell.x + _axisCol * cell.y;
        }

        public Vector3Int WorldToCell(Vector3 worldPos)
        {
            Vector3 localPos = _worldToGrid.MultiplyPoint3x4(worldPos);
            int col = Mathf.FloorToInt(localPos.x + 0.5f);
            int row = Mathf.FloorToInt(localPos.y + 0.5f);
            return new Vector3Int(row, col);
        }

        public BoundsInt GetBounds()
        {
            if (_cells.Count == 0) return default;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var cell in _cells.Keys)
            {
                minX = Mathf.Min(minX, cell.x);
                minY = Mathf.Min(minY, cell.y);
                maxX = Mathf.Max(maxX, cell.x);
                maxY = Mathf.Max(maxY, cell.y);
            }

            return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
        }
    }
}