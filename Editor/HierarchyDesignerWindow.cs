using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// The main editor window interface for configuring and managing hierarchy headers.
    /// </summary>
    public class HierarchyDesignerWindow : EditorWindow
    {
        #region Fields

        private HierarchyDatabase database;
        private SerializedObject serializedDatabase;
        private SerializedProperty headersProperty;
        private SerializedProperty globalLineColorProperty;
        private SerializedProperty globalLineStyleProperty;
        private SerializedProperty showNestingLinesProperty;
        private SerializedProperty nestingLinesColorProperty;
        private SerializedProperty showComponentIconsProperty;
        private SerializedProperty showChildCountBadgesProperty;
        private ReorderableList headerList;
        private Vector2 scrollPosition;

        #endregion

        #region MenuItem

        /// <summary>
        /// Opens the Hierarchy Designer window from the Unity Tools menu.
        /// </summary>
        [MenuItem("Tools/Hierarchy Designer", false, 50)]
        public static void ShowWindow()
        {
            HierarchyDesignerWindow window = GetWindow<HierarchyDesignerWindow>("Hierarchy Designer");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        #endregion

        #region Engine Callbacks

        private void OnEnable()
        {
            InitializeWindow();
        }

        private void OnGUI()
        {
            if (database == null || serializedDatabase == null)
            {
                InitializeWindow();
                if (database == null)
                {
                    EditorGUILayout.HelpBox("Could not load or create the Hierarchy Layout Database.", MessageType.Error);
                    return;
                }
            }

            serializedDatabase.Update();

            // Window Header
            DrawHeaderTitle();

            // Main Configuration List (Scrollable)
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.Space(5);
            
            // Draw the reorderable list
            headerList.DoLayoutList();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndScrollView();

            // Global Line and Color styling section highlighted in red above "+ Add Section" button
            DrawGlobalSettings();

            // Controls & Modify Actions Panel
            DrawReorderingControls();
            
            EditorGUILayout.Space(2);
            DrawBottomButtons();

            // Handle changes and save database
            if (serializedDatabase.ApplyModifiedProperties())
            {
                OnDatabaseModified();
            }
        }

        #endregion

        #region Helper Setup Methods

        private void InitializeWindow()
        {
            database = HierarchyUtility.GetOrCreateDatabase();
            if (database != null)
            {
                serializedDatabase = new SerializedObject(database);
                headersProperty = serializedDatabase.FindProperty("headers");
                globalLineColorProperty = serializedDatabase.FindProperty("globalLineColor");
                globalLineStyleProperty = serializedDatabase.FindProperty("globalLineStyle");
                showNestingLinesProperty = serializedDatabase.FindProperty("showNestingLines");
                nestingLinesColorProperty = serializedDatabase.FindProperty("nestingLinesColor");
                showComponentIconsProperty = serializedDatabase.FindProperty("showComponentIcons");
                showChildCountBadgesProperty = serializedDatabase.FindProperty("showChildCountBadges");
                SetupReorderableList();
            }
        }

        private void SetupReorderableList()
        {
            headerList = new ReorderableList(serializedDatabase, headersProperty, true, true, false, false)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Hierarchy Sections (Name | BG Color)", EditorStyles.boldLabel);
                },
                onReorderCallback = (ReorderableList list) =>
                {
                    serializedDatabase.ApplyModifiedProperties();
                    OnDatabaseModified();
                },
                elementHeight = 26f,
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = headersProperty.GetArrayElementAtIndex(index);
                    SerializedProperty nameProp = element.FindPropertyRelative("headerName");
                    SerializedProperty colorProp = element.FindPropertyRelative("color");

                    rect.y += 3f;

                    // Divide rect reserving space for inline delete button
                    float deleteBtnWidth = 20f;
                    float width = rect.width - deleteBtnWidth - 12f;
                    float nameWidth = width * 0.65f;
                    float colorWidth = width * 0.35f;

                    Rect nameRect = new Rect(rect.x, rect.y, nameWidth, 20f);
                    Rect bgColorRect = new Rect(rect.x + nameWidth + 4f, rect.y, colorWidth, 20f);
                    Rect deleteRect = new Rect(rect.x + nameWidth + colorWidth + 8f, rect.y, deleteBtnWidth, 19f);

                    // 1. Header Name Field
                    EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);

                    // 2. BG Color Picker
                    EditorGUI.PropertyField(bgColorRect, colorProp, GUIContent.none);

                    // 3. Inline Delete Button
                    GUIStyle deleteStyle = new GUIStyle(GUI.skin.button)
                    {
                        normal = { textColor = new Color(0.85f, 0.25f, 0.25f) },
                        hover = { textColor = Color.white },
                        fontSize = 11,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(0, 0, 0, 0)
                    };

                    if (GUI.Button(deleteRect, "✕", deleteStyle))
                    {
                        int indexToRemove = index;
                        EditorApplication.delayCall += () =>
                        {
                            if (serializedDatabase != null && indexToRemove >= 0)
                            {
                                serializedDatabase.Update();
                                if (indexToRemove < headersProperty.arraySize)
                                {
                                    headersProperty.DeleteArrayElementAtIndex(indexToRemove);
                                    serializedDatabase.ApplyModifiedProperties();
                                    OnDatabaseModified();
                                    Repaint();
                                }
                            }
                        };
                    }
                }
            };
        }

        private void DrawHeaderTitle()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(6);
            GUILayout.Label("HIERARCHY DESIGNER", EditorStyles.boldLabel);
            GUILayout.Label("Organize Unity hierarchy visually with custom headers.", EditorStyles.miniLabel);
            GUILayout.Space(6);
            GUILayout.EndVertical();
        }

        private void DrawGlobalSettings()
        {
            if (serializedDatabase == null || globalLineColorProperty == null || globalLineStyleProperty == null) return;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Global Separation & Styling Settings", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(globalLineColorProperty, new GUIContent("Line Color"));
            EditorGUILayout.PropertyField(globalLineStyleProperty, new GUIContent("Line Style"));
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("Feature Toggles & Configurations", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(showNestingLinesProperty, new GUIContent("Nesting Lines"));
            EditorGUILayout.PropertyField(nestingLinesColorProperty, new GUIContent("Nesting Color"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(showComponentIconsProperty, new GUIContent("Component Icons"));
            EditorGUILayout.PropertyField(showChildCountBadgesProperty, new GUIContent("Child Counts"));
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void DrawReorderingControls()
        {
            GUILayout.BeginHorizontal();
            
            // Add Section button as a prominent full-width action
            if (GUILayout.Button("+ Add Section", GUILayout.Height(26f)))
            {
                AddNewHeader();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private void DrawBottomButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Headers", GUILayout.Height(30f)))
            {
                HierarchyCreator.CreateHeaders(database);
            }

            if (GUILayout.Button("Update Headers", GUILayout.Height(30f)))
            {
                HierarchyCreator.UpdateHeaders(database);
            }

            if (GUILayout.Button("Delete Headers", GUILayout.Height(30f)))
            {
                if (EditorUtility.DisplayDialog("Delete Headers", "Are you sure you want to delete all hierarchy headers in the active scene? This does not delete any custom user components.", "Yes", "No"))
                {
                    HierarchyCreator.DeleteHeaders(database);
                }
            }

            if (GUILayout.Button("Refresh", GUILayout.Height(30f)))
            {
                HierarchyDrawer.RefreshCache();
                EditorApplication.RepaintHierarchyWindow();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8);
        }

        #endregion

        #region Operations Logic

        private void AddNewHeader()
        {
            int index = headersProperty.arraySize;
            headersProperty.InsertArrayElementAtIndex(index);
            
            SerializedProperty element = headersProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("headerName").stringValue = "New Header";
            element.FindPropertyRelative("color").colorValue = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            element.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();

            headerList.index = index;
            serializedDatabase.ApplyModifiedProperties();
            OnDatabaseModified();
        }

        private void RemoveSelectedHeader()
        {
            int index = headerList.index;
            if (index >= 0 && index < headersProperty.arraySize)
            {
                headersProperty.DeleteArrayElementAtIndex(index);
                headerList.index = Mathf.Clamp(index - 1, 0, headersProperty.arraySize - 1);
                serializedDatabase.ApplyModifiedProperties();
                OnDatabaseModified();
            }
        }

        private void MoveSelected(bool up)
        {
            int index = headerList.index;
            if (index < 0 || index >= headersProperty.arraySize) return;

            int targetIndex = up ? index - 1 : index + 1;
            if (targetIndex >= 0 && targetIndex < headersProperty.arraySize)
            {
                headersProperty.MoveArrayElement(index, targetIndex);
                headerList.index = targetIndex;
                serializedDatabase.ApplyModifiedProperties();
                OnDatabaseModified();
            }
        }

        private void OnDatabaseModified()
        {
            HierarchyUtility.SaveDatabase(database);
            HierarchyDrawer.RefreshCache();
            EditorApplication.RepaintHierarchyWindow();
        }

        #endregion
    }
}
