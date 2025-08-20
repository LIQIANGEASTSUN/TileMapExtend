using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Merge.TileMap
{
    [CustomEditor(typeof(TilemapComponent))]
    public partial class TilemapComponentEditor : Editor
    {
        private bool isEditing;
        private int brushIndex;
        private string[] brushOptions;
        private SerializedProperty serializedCellsProp;
        private TilemapComponent comp;
        private ParallelogramGrid grid;
        private TilemapRenderer renderer;

        void OnEnable()
        {
            comp = (TilemapComponent)target;
            serializedCellsProp = serializedObject.FindProperty("serializedCells");

            var awakeMethod = typeof(TilemapComponent)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            awakeMethod?.Invoke(comp, null);
            
            grid = (ParallelogramGrid)typeof(TilemapComponent)
                .GetField("grid", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(comp);
            
            var w2gField = typeof(ParallelogramGrid)
                .GetField("_worldToGrid", BindingFlags.Instance | BindingFlags.NonPublic);
            worldToGrid = (Matrix4x4)w2gField.GetValue(grid);

            renderer = comp.GetComponent<TilemapRenderer>();
            var rendAwake = typeof(TilemapRenderer)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            rendAwake?.Invoke(renderer, null);

            var tiles = comp.tileAssets;
            brushOptions = new string[tiles.Length + 1];
            brushOptions[0] = "-1 橡皮擦"; 
            for (int i = 0; i < tiles.Length; i++)
            {
                brushOptions[i + 1] = tiles[i] != null ? tiles[i].name : $"Tile {i}";
            }
            brushIndex = 1;
        }

        public override void OnInspectorGUI()
        {
            OnGizmosInspectorGUI();
            
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "serializedCells");

            if (isEditing)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("▸ 编辑模式", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(serializedCellsProp, new GUIContent("Serialized Cells"), true);
                EditorGUILayout.Space();

                brushIndex = EditorGUILayout.Popup("Brush Tile", brushIndex, brushOptions);
                if (brushIndex < 0)
                    EditorGUILayout.HelpBox("当前为橡皮模式，点击会擦除", MessageType.Info);

                EditorGUILayout.Space();
            }

            if (!isEditing)
            {
                if (GUILayout.Button("编辑 Tilemap"))
                    StartEdit();
            }
            else
            {
                if (GUILayout.Button("保存 Tilemap 数据"))
                    SaveEdit();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            OnGizmosSceneGUI();

            if (!isEditing || Application.isPlaying) return;
            
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                var plane = new Plane(Vector3.forward, new Vector3(0, 0, comp.transform.position.z));
                if (!plane.Raycast(ray, out float enter)) return;

                var worldPos = ray.GetPoint(enter);
                var cell = grid.WorldToCell(worldPos);

                Undo.RecordObject(comp, brushIndex >= 0 ? "Paint Tile" : "Erase Tile");

                if (brushIndex > 0)
                {
                    var data = new TileData() { Position = cell, SpriteIndex = brushIndex - 1 };
                    grid.SetTile(data);
                }
                else
                {
                    grid.RemoveTile(cell);
                }

                renderer.Build(grid, comp.tileAssets);

                var all = grid.GetAllTiles().ToList();
                serializedCellsProp.arraySize = all.Count;
                for (int i = 0; i < all.Count; i++)
                {
                    var elt = serializedCellsProp.GetArrayElementAtIndex(i);
                    elt.FindPropertyRelative("Position").vector3IntValue = all[i].Position;
                    elt.FindPropertyRelative("SpriteIndex").intValue = all[i].SpriteIndex;
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(comp);
                e.Use();
            }
        }

        private void StartEdit()
        {
            grid.Initialize(comp.transform.position, comp.axisRow, comp.axisCol);
            foreach (var c in comp.GetSerializedCells())
                grid.SetTile(c);
            brushIndex = comp.tileAssets.Length > 0 ? 0 : -1;
            renderer.Build(grid, comp.tileAssets);
            isEditing = true;
        }

        private void SaveEdit()
        {
            var all = grid.GetAllTiles().ToList();
            serializedCellsProp.arraySize = all.Count;
            for (int i = 0; i < all.Count; i++)
            {
                var elt = serializedCellsProp.GetArrayElementAtIndex(i);
                elt.FindPropertyRelative("Position").vector3IntValue = all[i].Position;
                elt.FindPropertyRelative("SpriteIndex").intValue = all[i].SpriteIndex;
            }

            serializedObject.ApplyModifiedProperties();
            renderer.Release();
            EditorUtility.SetDirty(comp);
            isEditing = false;
            brushIndex = -1;
        }
    }
}