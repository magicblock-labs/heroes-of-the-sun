using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif

namespace Tools.DataAssets.xNode_1._8._0.Scripts.Editor {
    /// <summary> Override graph inspector to show an 'Open Graph' button at the top </summary>
    [CustomEditor(typeof(NodeGraph), true)]
#if ODIN_INSPECTOR
    public class GlobalGraphEditor : OdinEditor {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
                NodeEditorWindow.Open(serializedObject.targetObject as XNode.NodeGraph);
            }
            base.OnInspectorGUI();
        }
    }
#else
    // [CanEditMultipleObjects]
    public class GlobalGraphEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            // serializedObject.Update();
            //
            // if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
            //     NodeEditorWindow.Open(serializedObject.targetObject as NodeGraph);
            // }
            //
            // GUILayout.Space(EditorGUIUtility.singleLineHeight);
            // GUILayout.Label("Raw data", "BoldLabel");
            //
            DrawDefaultInspector();

            if (GUILayout.Button("Save", GUILayout.Height(30)))
            {
                (serializedObject.targetObject as NodeGraph).Save();
            }

            // serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    [CustomEditor(typeof(Node), true)]
#if ODIN_INSPECTOR
    public class GlobalNodeEditor : OdinEditor {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
                SerializedProperty graphProp = serializedObject.FindProperty("graph");
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as XNode.NodeGraph);
                w.Home(); // Focus selected node
            }
            base.OnInspectorGUI();
        }
    }
#else
    public class GlobalNodeEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
                SerializedProperty graphProp = serializedObject.FindProperty("graph");
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as NodeGraph);
                w.Home(); // Focus selected node
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            // Now draw the node itself.
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}