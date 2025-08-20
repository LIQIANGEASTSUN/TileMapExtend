using UnityEngine;


namespace Merge.TileMap
{
    public interface IMeshBuilder
    {
        Mesh BuildMesh(ParallelogramGrid grid, TileBase[] tileAssets);

        void RefreshMeshForCell(Mesh mesh, TileBase tileAsset, Vector3Int cellPos);
    }

}