using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Handles the instantiation, synchronization, and deletion of header GameObjects in the active scene.
    /// </summary>
    public static class HierarchyCreator
    {
        #region Public Methods

        /// <summary>
        /// Creates headers in the active scene for database entries that do not yet exist.
        /// </summary>
        /// <param name="database">The hierarchy layout database.</param>
        public static void CreateHeaders(HierarchyDatabase database)
        {
            if (database == null || database.Headers == null) return;

            // Get existing scene headers by GUID
            List<HierarchyHeader> sceneHeaders = HierarchyUtility.FindHeadersInScene();
            Dictionary<string, HierarchyHeader> headerMap = new Dictionary<string, HierarchyHeader>();
            foreach (var sh in sceneHeaders)
            {
                if (!string.IsNullOrEmpty(sh.Guid))
                {
                    headerMap[sh.Guid] = sh;
                }
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Hierarchy Headers");

            List<GameObject> newlyCreated = new List<GameObject>();

            foreach (var data in database.Headers)
            {
                if (headerMap.ContainsKey(data.Guid))
                {
                    // Skip existing headers during creation
                    continue;
                }

                // Create a new GameObject
                GameObject go = new GameObject(data.HeaderName);
                HierarchyHeader component = go.AddComponent<HierarchyHeader>();
                component.Guid = data.Guid;

                // Register creation for Undo
                Undo.RegisterCreatedObjectUndo(go, "Create Header: " + data.HeaderName);
                newlyCreated.Add(go);
            }

            if (newlyCreated.Count > 0)
            {
                // Trigger sorting to match database order
                SortHeadersInScene(database);
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// Updates the names, properties, and ordering of existing header GameObjects in the active scene.
        /// </summary>
        /// <param name="database">The hierarchy layout database.</param>
        public static void UpdateHeaders(HierarchyDatabase database)
        {
            if (database == null || database.Headers == null) return;

            List<HierarchyHeader> sceneHeaders = HierarchyUtility.FindHeadersInScene();
            Dictionary<string, HierarchyHeader> headerMap = new Dictionary<string, HierarchyHeader>();
            foreach (var sh in sceneHeaders)
            {
                if (!string.IsNullOrEmpty(sh.Guid))
                {
                    headerMap[sh.Guid] = sh;
                }
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Update Hierarchy Headers");

            foreach (var data in database.Headers)
            {
                if (headerMap.TryGetValue(data.Guid, out HierarchyHeader component))
                {
                    if (component.gameObject.name != data.HeaderName)
                    {
                        Undo.RecordObject(component.gameObject, "Rename Header");
                        component.gameObject.name = data.HeaderName;
                    }
                }
            }

            SortHeadersInScene(database);

            Undo.CollapseUndoOperations(undoGroup);
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// Deletes all header GameObjects in the active scene that correspond to database entries.
        /// </summary>
        /// <param name="database">The hierarchy layout database.</param>
        public static void DeleteHeaders(HierarchyDatabase database)
        {
            if (database == null) return;

            List<HierarchyHeader> sceneHeaders = HierarchyUtility.FindHeadersInScene();
            if (sceneHeaders.Count == 0) return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Delete Hierarchy Headers");

            foreach (var sh in sceneHeaders)
            {
                // We delete headers that are marked with a guid
                if (sh != null && sh.gameObject != null)
                {
                    Undo.DestroyObjectImmediate(sh.gameObject);
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorApplication.RepaintHierarchyWindow();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Reorders header GameObjects in the scene hierarchy to match the database sequence.
        /// </summary>
        private static void SortHeadersInScene(HierarchyDatabase database)
        {
            List<HierarchyHeader> sceneHeaders = HierarchyUtility.FindHeadersInScene();
            if (sceneHeaders.Count <= 1) return;

            // Map GUIDs to their desired target index in the database
            Dictionary<string, int> targetOrder = new Dictionary<string, int>();
            for (int i = 0; i < database.Headers.Count; i++)
            {
                targetOrder[database.Headers[i].Guid] = i;
            }

            // Sort scene header components based on database order
            sceneHeaders.Sort((a, b) =>
            {
                int orderA = targetOrder.TryGetValue(a.Guid, out int valA) ? valA : int.MaxValue;
                int orderB = targetOrder.TryGetValue(b.Guid, out int valB) ? valB : int.MaxValue;
                return orderA.CompareTo(orderB);
            });

            // Adjust sibling indices of active root transforms
            for (int i = 0; i < sceneHeaders.Count; i++)
            {
                Transform t = sceneHeaders[i].transform;
                // If it is a root object, we can place it sequentially
                if (t.parent == null)
                {
                    Undo.RecordObject(t, "Reorder Sibling Index");
                    t.SetSiblingIndex(i);
                }
            }
        }

        #endregion
    }
}
