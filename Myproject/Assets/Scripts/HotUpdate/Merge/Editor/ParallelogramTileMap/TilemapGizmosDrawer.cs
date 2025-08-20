using UnityEngine;
using UnityEditor;

namespace Merge.TileMap
{
    public partial class TilemapComponentEditor
    {
        private bool showGrid;
        private Matrix4x4 worldToGrid;
        private Color minorLineColor = new Color(1, 1, 1, 0.1f);
        private Color majorLineColor = new Color(1, 1, 1, 0.3f);

        private int majorLineInterval = 5; // 每隔几格画一次主线

        private Vector3[] corners = new Vector3[4];
        private void OnGizmosInspectorGUI()
        {
            showGrid = EditorPrefs.GetBool("Tilemap_ShowGrid", true);
            EditorGUI.BeginChangeCheck();
            showGrid = EditorGUILayout.Toggle("Show Grid Gizmos", showGrid);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("Tilemap_ShowGrid", showGrid);
                SceneView.RepaintAll();
            }
        }

        private void OnGizmosSceneGUI()
        {
            if (comp == null || !showGrid) return;
            
            Camera cam = Camera.current;
            if (cam == null) return;
            Plane gridPlane = new Plane(Vector3.forward, new Vector3(0, 0, comp.origin.z));
            Vector3[] worldCorners = new Vector3[4];
            Vector2[] vp = { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            for (int i = 0; i < 4; i++)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(vp[i].x, vp[i].y, 0f));
                if (gridPlane.Raycast(ray, out float enter))
                    worldCorners[i] = ray.GetPoint(enter);
            }
            
            Vector2[] cellSpace = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 lp = worldToGrid.MultiplyPoint3x4(worldCorners[i]);
                cellSpace[i] = new Vector2(lp.x, lp.y);
            }
            
            float minC = Mathf.Min(cellSpace[0].x, cellSpace[1].x, cellSpace[2].x, cellSpace[3].x) - 1;
            float maxC = Mathf.Max(cellSpace[0].x, cellSpace[1].x, cellSpace[2].x, cellSpace[3].x) + 1;
            float minR = Mathf.Min(cellSpace[0].y, cellSpace[1].y, cellSpace[2].y, cellSpace[3].y) - 1;
            float maxR = Mathf.Max(cellSpace[0].y, cellSpace[1].y, cellSpace[2].y, cellSpace[3].y) + 1;

            int colMin = Mathf.FloorToInt(minC);
            int colMax = Mathf.CeilToInt (maxC);
            int rowMin = Mathf.FloorToInt(minR);
            int rowMax = Mathf.CeilToInt (maxR);
            
            
            Vector3 row = comp.axisRow * 0.5f;
            Vector3 col = comp.axisCol * 0.5f;

            corners[0] = -row - col;
            corners[1] = row - col;
            corners[2] = row + col;
            corners[3] = -row + col;
            
            Vector3 originCorner = comp.origin + corners[0];
            for (int c = colMin; c <= colMax; c++)
            {
                Handles.color = (c % majorLineInterval == 0) ? majorLineColor : minorLineColor;
                Vector3 A = originCorner + comp.axisCol * c + comp.axisRow * rowMin;
                Vector3 B = originCorner + comp.axisCol * c + comp.axisRow * rowMax;
                Handles.DrawLine(A, B);
            }

            for (int r = rowMin; r <= rowMax; r++)
            {
                Handles.color = (r % majorLineInterval == 0) ? majorLineColor : minorLineColor;
                Vector3 A = originCorner + comp.axisCol * colMin + comp.axisRow * r;
                Vector3 B = originCorner + comp.axisCol * colMax + comp.axisRow * r;
                Handles.DrawLine(A, B);
            }
        }
    }
}