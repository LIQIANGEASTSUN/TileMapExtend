#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine;

namespace Cookgame
{
    public class MoveUINodeByArrow
    {
        public static bool isMoveUIByArrow = true;
        [InitializeOnLoadMethod]
        private static void Init()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }
        private static void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && isMoveUIByArrow)
            {
                foreach (var item in Selection.transforms)
                {
                    Transform trans = item;
                    if (trans != null)
                    {
                        bool isHandled = false;
                        if (e.keyCode == KeyCode.UpArrow)
                        {
                            Undo.RecordObject(trans, "移动UI节点");
                            trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y + 1, trans.localPosition.z);
                            isHandled = true;
                        }
                        if (e.keyCode == KeyCode.DownArrow)
                        {
                            Undo.RecordObject(trans, "移动UI节点");
                            trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y - 1, trans.localPosition.z);
                            isHandled = true;
                        }
                        if (e.keyCode == KeyCode.RightArrow)
                        {
                            Undo.RecordObject(trans, "移动UI节点");
                            trans.localPosition = new Vector3(trans.localPosition.x + 1, trans.localPosition.y, trans.localPosition.z);
                            isHandled = true;
                        }
                        if (e.keyCode == KeyCode.LeftArrow)
                        {
                            Undo.RecordObject(trans, "移动UI节点");
                            trans.localPosition = new Vector3(trans.localPosition.x - 1, trans.localPosition.y, trans.localPosition.z);
                            isHandled = true;
                        }
                        if (isHandled)
                        {
                            Event.current.Use();
                        }
                    }
                }
            }
        }
    }
}
#endif