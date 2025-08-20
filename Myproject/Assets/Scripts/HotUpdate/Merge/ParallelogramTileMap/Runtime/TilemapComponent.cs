using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace Merge.TileMap
{
    [RequireComponent(typeof(TilemapRenderer))]
    public partial class TilemapComponent : MonoBehaviour
    {
        [Header("Grid Settings")] 
        public Vector3 origin = Vector3.zero;
        public Vector3 axisRow = new Vector3(0.965f, 0.282f, 0);
        public Vector3 axisCol = new Vector3(-0.484f, 0.593f, 0);
        
        [Header("Tiles")] public TileBase[] tileAssets;

        private ParallelogramGrid grid;
        private TilemapRenderer renderer;

        [FormerlySerializedAs("transform")] public Transform transform11;
        void Awake()
        {
            grid = new ParallelogramGrid();
            grid.Initialize(origin, axisRow, axisCol);
            renderer = GetComponent<TilemapRenderer>();
        }

        void Start()
        {
            Deserialize();
            renderer.Build(grid, tileAssets);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (transform11 != null)
                {
                    Vector3Int cellPos = grid.WorldToCell(transform11.position);

                    if (grid.TryGetTile(cellPos,out var cell))
                    {
                        Debug.Log($"Clicked cell with tile: {cellPos}");
                    }
                    else
                    {
                        Debug.Log($"Clicked empty cell: {cellPos}");
                    }
                }

                // var cam = Camera.main;
                // var ray = cam.ScreenPointToRay(Input.mousePosition);
                // if (Physics.Raycast(ray, out RaycastHit hit))
                // {
                //     var cellPos = grid.WorldToCell(hit.point);
                //
                //     if (grid.TryGetCell(cellPos,out var cell))
                //     {
                //         Debug.Log($"Clicked cell with tile: {cellPos}");
                //     }
                //     else
                //     {
                //         Debug.Log($"Clicked empty cell: {cellPos}");
                //     }
                // }
            }
        }

        
        public void SetTile(TileData cell)
        {
            grid.SetTile(cell);
            renderer.RefreshTile(tileAssets[cell.SpriteIndex], cell.Position);
        }
        public bool HasTile(Vector3Int cellPos)
        {
            return grid.HasTile(cellPos);
        }

        public bool TryGetTile(Vector3Int cellPos, out TileData cell)
        {
            return grid.TryGetTile(cellPos, out cell);
        }
        
        public void RemoveTile(Vector3Int pos)
        {
            grid.RemoveTile(pos);
        }

        public int CellCount()
        {
            return grid.CellCount;
        }

        public IEnumerable<TileData> GetAllTiles()
        {
            return grid.GetAllTiles();
        }
        
        public Vector3 CellToWorld(Vector3Int cellPos)
        {
            return grid.CellToWorld(cellPos);
        }
        
        public Vector3Int WorldToCell(Vector3 worldPos)
        {
           return grid.WorldToCell(worldPos);
        }
      
        public BoundsInt GetTileBounds()
        {
            return grid.GetBounds();
        }
  
        public Vector3 cellSize
        {
            get
            {
                float width  = axisRow.magnitude;
                float height = axisCol.magnitude;
                return new Vector3(width, height, 1f);
            }
        }
        public TileData GetTile(Vector3Int cellPos)
        {
            grid.TryGetTile(cellPos, out var cell);
            return cell;
        }
        
    }
}