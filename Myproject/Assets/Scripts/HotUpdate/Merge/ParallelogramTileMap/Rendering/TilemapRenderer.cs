using UnityEngine;


namespace Merge.TileMap
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TilemapRenderer : MonoBehaviour
    {
        private IMeshBuilder _builder;
        private MeshFilter _filter;
        private Mesh _mesh;
        void Awake()
        {
            _builder = new MeshBuilder();
            _filter = GetComponent<MeshFilter>();
        }

        public void Build(ParallelogramGrid grid, TileBase[] tileAssets)
        {
            _mesh = _builder.BuildMesh(grid, tileAssets);
            _filter.mesh = _mesh;
        }

        public void RefreshTile(TileBase tile, Vector3Int cell)
        {
            if (_mesh == null) return;
            _builder.RefreshMeshForCell(_mesh, tile, cell);
            _filter.mesh = _mesh;
        }

        
        public void Release()
        {
            _builder = null;
            _filter.mesh = null;
            _filter = null;
        }
    }
}