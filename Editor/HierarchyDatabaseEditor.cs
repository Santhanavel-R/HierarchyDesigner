using UnityEditor;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Custom Inspector editor for <see cref="HierarchyDatabase"/>. Directs users to the setup editor window.
    /// </summary>
    [CustomEditor(typeof(HierarchyDatabase))]
    public class HierarchyDatabaseEditor : UnityEditor.Editor
    {
        #region Engine Callbacks

        /// <summary>
        /// Renders the custom inspector layout.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("Use the Hierarchy Designer Window to configure, order, and manage your visual separators.", MessageType.Info);
            EditorGUILayout.Space(4);

            if (GUILayout.Button("Open Hierarchy Designer Window", GUILayout.Height(32f)))
            {
                HierarchyDesignerWindow.ShowWindow();
            }

            EditorGUILayout.Space(10);
            
            // Draw default properties in a read-only or standard view
            EditorGUI.BeginDisabledGroup(true);
            DrawDefaultInspector();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
