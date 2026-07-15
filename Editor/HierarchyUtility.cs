using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Utility functions for managing the Hierarchy Designer database, files, and scene objects.
    /// </summary>
    public static class HierarchyUtility
    {
        #region Constants

        private const string DefaultDatabaseFolder = "Assets/HierarchyDesigner";
        private const string DefaultDatabasePath = "Assets/HierarchyDesigner/HierarchyLayout.asset";

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds the existing HierarchyDatabase asset, or creates a new one at the default path if not found.
        /// </summary>
        /// <returns>The active HierarchyDatabase instance.</returns>
        public static HierarchyDatabase GetOrCreateDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:HierarchyDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                HierarchyDatabase database = AssetDatabase.LoadAssetAtPath<HierarchyDatabase>(path);
                if (database != null)
                {
                    // If the database is empty, force populate the defaults immediately
                    if (database.Headers == null || database.Headers.Count == 0)
                    {
                        database.Headers = new List<HierarchyHeaderData>
                        {
                            new HierarchyHeaderData("🎮 XR", new Color(0.25f, 0.48f, 0.85f, 0.85f)),
                            new HierarchyHeaderData("🖥 UI", new Color(0.42f, 0.44f, 0.9f, 0.85f)),
                            new HierarchyHeaderData("🔊 AUDIO", new Color(0.05f, 0.6f, 0.7f, 0.85f)),
                            new HierarchyHeaderData("🌍 ENVIRONMENT", new Color(0.1f, 0.58f, 0.4f, 0.85f)),
                            new HierarchyHeaderData("🎯 INTERACTABLES", new Color(0.85f, 0.58f, 0.1f, 0.85f)),
                            new HierarchyHeaderData("✨ EFFECTS", new Color(0.8f, 0.65f, 0.1f, 0.85f)),
                            new HierarchyHeaderData("⚙ Managers", new Color(0.35f, 0.38f, 0.42f, 0.85f)),
                            new HierarchyHeaderData("🐞 DEBUG", new Color(0.82f, 0.28f, 0.28f, 0.85f))
                        };
                        SaveDatabase(database);
                        HierarchyDrawer.RefreshCache();
                    }
                    return database;
                }
            }

            // Create new database
            if (!Directory.Exists(DefaultDatabaseFolder))
            {
                Directory.CreateDirectory(DefaultDatabaseFolder);
            }

            HierarchyDatabase newDatabase = ScriptableObject.CreateInstance<HierarchyDatabase>();
            newDatabase.Headers = new List<HierarchyHeaderData>
            {
                new HierarchyHeaderData("🎮 XR", new Color(0.25f, 0.48f, 0.85f, 0.85f)),
                new HierarchyHeaderData("🖥 UI", new Color(0.42f, 0.44f, 0.9f, 0.85f)),
                new HierarchyHeaderData("🔊 AUDIO", new Color(0.05f, 0.6f, 0.7f, 0.85f)),
                new HierarchyHeaderData("🌍 ENVIRONMENT", new Color(0.1f, 0.58f, 0.4f, 0.85f)),
                new HierarchyHeaderData("🎯 INTERACTABLES", new Color(0.85f, 0.58f, 0.1f, 0.85f)),
                new HierarchyHeaderData("✨ EFFECTS", new Color(0.8f, 0.65f, 0.1f, 0.85f)),
                new HierarchyHeaderData("⚙ Managers", new Color(0.35f, 0.38f, 0.42f, 0.85f)),
                new HierarchyHeaderData("🐞 DEBUG", new Color(0.82f, 0.28f, 0.28f, 0.85f))
            };

            AssetDatabase.CreateAsset(newDatabase, DefaultDatabasePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            HierarchyDrawer.RefreshCache();

            return newDatabase;
        }

        /// <summary>
        /// Saves changes made to the database ScriptableObject.
        /// </summary>
        /// <param name="database">The database instance.</param>
        public static void SaveDatabase(HierarchyDatabase database)
        {
            if (database == null) return;

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Returns all GameObjects in the current active scene that contain the HierarchyHeader component.
        /// </summary>
        /// <returns>A list of active HierarchyHeader components in the scene.</returns>
        public static List<HierarchyHeader> FindHeadersInScene()
        {
            List<HierarchyHeader> headers = new List<HierarchyHeader>();
            HierarchyHeader[] found = Object.FindObjectsByType<HierarchyHeader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (found != null)
            {
                headers.AddRange(found);
            }
            return headers;
        }

        #endregion
    }
}
