using System.Collections.Generic;
using UnityEngine;


namespace Merge.TileMap
{
    public class MeshBuilder : IMeshBuilder
    {
        private Dictionary<Vector3Int, int> _cellIndexMap;
        private Vector3[] _vertexs;
        private Vector2[] _uvs;
        private int[] _triangles;
        private ParallelogramGrid _grid;

        public Mesh BuildMesh(ParallelogramGrid grid, TileBase[] tileAssets)
        {
            _grid = grid;

            int count = grid.CellCount;
            _cellIndexMap = new Dictionary<Vector3Int, int>(count);
            _vertexs = new Vector3[count * 4];
            _uvs = new Vector2[count * 4];
            _triangles = new int[count * 6];

            var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };

            int i = 0;
            foreach (var cell in grid.GetAllTiles())
            {
                _cellIndexMap[cell.Position] = i;
                int vStart = i * 4;
                int tStart = i * 6;

                var asset = tileAssets[cell.SpriteIndex];
                var sprite = asset.Sprite;
                if (sprite != null)
                {
                    Vector3 center = grid.CellToWorld(cell.Position);
                    RecalculateCell(vStart, asset, cell.Position);
                    
                    _triangles[tStart + 0] = vStart + 0;
                    _triangles[tStart + 1] = vStart + 1;
                    _triangles[tStart + 2] = vStart + 2;
                    _triangles[tStart + 3] = vStart + 2;
                    _triangles[tStart + 4] = vStart + 3;
                    _triangles[tStart + 5] = vStart + 0;
                }

                i++;
            }

            mesh.vertices = _vertexs;
            mesh.uv = _uvs;
            mesh.triangles = _triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        public void RefreshMeshForCell(Mesh mesh, TileBase tileAsset, Vector3Int cellPos)
        {
            if (_cellIndexMap == null || !_cellIndexMap.TryGetValue(cellPos, out int index))
                return;

            int vStart = index * 4;
            var sprite = tileAsset.Sprite;
            if (sprite == null) 
                return;

            RecalculateCell(vStart, tileAsset, cellPos);
            mesh.vertices = _vertexs;
            mesh.uv = _uvs;
            mesh.RecalculateBounds();
        }

        private void RecalculateCell(int vStart, TileBase tileAsset, Vector3Int cellPos)
        {
            var sprite = tileAsset.Sprite;
            Vector3 center = _grid.CellToWorld(cellPos);
            float w = sprite.bounds.size.x;
            float h = sprite.bounds.size.y;
            _vertexs[vStart + 0] = center + new Vector3(-w / 2, -h / 2);
            _vertexs[vStart + 1] = center + new Vector3(-w / 2, h / 2);
            _vertexs[vStart + 2] = center + new Vector3(w / 2, h / 2);
            _vertexs[vStart + 3] = center + new Vector3(w / 2, -h / 2);

            var rect = sprite.textureRect;
            var tex = sprite.texture;
            Vector2 uv00 = new Vector2(rect.xMin / tex.width, rect.yMin / tex.height);
            Vector2 uv11 = new Vector2(rect.xMax / tex.width, rect.yMax / tex.height);
            _uvs[vStart + 0] = new Vector2(uv00.x, uv00.y);
            _uvs[vStart + 1] = new Vector2(uv00.x, uv11.y);
            _uvs[vStart + 2] = new Vector2(uv11.x, uv11.y);
            _uvs[vStart + 3] = new Vector2(uv11.x, uv00.y);
        }
        
    }
}