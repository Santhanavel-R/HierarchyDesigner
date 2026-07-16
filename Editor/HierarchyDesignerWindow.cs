using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Configuration tools window managing hierarchy separator sections, styling, toggles,
    /// and built-in theme presets with a clean dropdown selector and live mockup.
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
        private SerializedProperty activeThemeIndexProperty;
        private SerializedProperty useRainbowNestingProperty;
        private SerializedProperty rainbowPaletteProperty;
        private SerializedProperty nestingLinesOpacityProperty;
        private SerializedProperty showGameObjectBorderProperty;
        private SerializedProperty gameObjectBorderColorProperty;
        private SerializedProperty gameObjectBorderOpacityProperty;
        private SerializedProperty childCountColorModeProperty;
        private SerializedProperty childCountBorderStyleProperty;
        private SerializedProperty childCountTextColorProperty;
        private SerializedProperty childCountBorderColorProperty;

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
            window.minSize = new Vector2(480, 500);
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

            // Window Header Banner
            DrawHeaderTitle();

            // Main scrollable area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.Space(6);
            
            // 1. Theme Presets Dropdown Panel
            DrawThemePresetCards();

            EditorGUILayout.Space(8);

            // 2. Hierarchy Sections list
            headerList.DoLayoutList();
            
            EditorGUILayout.Space(8);

            // 3. Styling Configuration
            DrawGlobalSettings();

            // 4. Custom Section creation triggers
            DrawReorderingControls();
            
            EditorGUILayout.Space(6);
            EditorGUILayout.EndScrollView();

            // Footer controls
            DrawBottomButtons();

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
                activeThemeIndexProperty = serializedDatabase.FindProperty("activeThemeIndex");
                useRainbowNestingProperty = serializedDatabase.FindProperty("useRainbowNesting");
                rainbowPaletteProperty = serializedDatabase.FindProperty("rainbowPalette");
                nestingLinesOpacityProperty = serializedDatabase.FindProperty("nestingLinesOpacity");
                showGameObjectBorderProperty = serializedDatabase.FindProperty("showGameObjectBorder");
                gameObjectBorderColorProperty = serializedDatabase.FindProperty("gameObjectBorderColor");
                gameObjectBorderOpacityProperty = serializedDatabase.FindProperty("gameObjectBorderOpacity");
                childCountColorModeProperty = serializedDatabase.FindProperty("childCountColorMode");
                childCountBorderStyleProperty = serializedDatabase.FindProperty("childCountBorderStyle");
                childCountTextColorProperty = serializedDatabase.FindProperty("childCountTextColor");
                childCountBorderColorProperty = serializedDatabase.FindProperty("childCountBorderColor");

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

                    float deleteBtnWidth = 20f;
                    float width = rect.width - deleteBtnWidth - 12f;
                    float nameWidth = width * 0.65f;
                    float colorWidth = width * 0.35f;

                    Rect nameRect = new Rect(rect.x, rect.y, nameWidth, 20f);
                    Rect bgColorRect = new Rect(rect.x + nameWidth + 4f, rect.y, colorWidth, 20f);
                    Rect deleteRect = new Rect(rect.x + nameWidth + colorWidth + 8f, rect.y, deleteBtnWidth, 19f);

                    EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
                    EditorGUI.PropertyField(bgColorRect, colorProp, GUIContent.none);

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

        #endregion

        #region Card Drawer Methods

        private void DrawHeaderTitle()
        {
            Rect bannerRect = GUILayoutUtility.GetRect(0f, 48f);
            
            // Draw background
            Color bg = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f, 1.0f) : new Color(0.85f, 0.85f, 0.85f, 1.0f);
            EditorGUI.DrawRect(bannerRect, bg);
            
            // Left blue accent stripe
            Rect accentStripe = new Rect(bannerRect.x, bannerRect.y, 4f, bannerRect.height);
            EditorGUI.DrawRect(accentStripe, new Color(0.24f, 0.48f, 0.9f, 1f));
            
            // Top and bottom borders
            Rect topLine = new Rect(bannerRect.x, bannerRect.y, bannerRect.width, 1f);
            EditorGUI.DrawRect(topLine, EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.75f, 0.75f, 0.75f));
            Rect bottomLine = new Rect(bannerRect.x, bannerRect.yMax - 1f, bannerRect.width, 1f);
            EditorGUI.DrawRect(bottomLine, EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.6f, 0.6f, 0.6f));

            // Title label
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            if (!EditorGUIUtility.isProSkin) titleStyle.normal.textColor = Color.black;
            
            Rect titleRect = new Rect(bannerRect.x + 14f, bannerRect.y + 8f, bannerRect.width - 20f, 18f);
            GUI.Label(titleRect, "✨ HIERARCHY DESIGNER", titleStyle);
            
            // Subtitle label
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f) }
            };
            Rect subtitleRect = new Rect(bannerRect.x + 14f, bannerRect.y + 24f, bannerRect.width - 20f, 16f);
            GUI.Label(subtitleRect, "Organize Unity hierarchy visually with custom separator presets.", subtitleStyle);
            
            GUILayout.Space(6);
        }

        private void DrawGroupHeader(string title, string emoji)
        {
            GUILayout.Space(2f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(2f);
            GUIStyle groupTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.15f, 0.15f, 0.15f) }
            };
            GUILayout.Label($"{emoji}  {title.ToUpperInvariant()}", groupTitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        private void DrawThemePresetCards()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGroupHeader("Visual Color Theme Presets", "🎨");
            GUILayout.Space(2f);

            // Fetch theme names
            string[] themeNames = new string[database.Themes.Count];
            for (int idx = 0; idx < database.Themes.Count; idx++)
            {
                themeNames[idx] = database.Themes[idx].themeName;
            }

            // Dropdown Menu
            int currentTheme = activeThemeIndexProperty.intValue;
            int newTheme = EditorGUILayout.Popup("Select Visual Theme", currentTheme, themeNames);
            if (newTheme != currentTheme)
            {
                activeThemeIndexProperty.intValue = newTheme;
                serializedDatabase.ApplyModifiedProperties();

                HierarchyThemeData selectedTheme = database.Themes[newTheme];
                ApplyThemeColorsToHeaders(selectedTheme);

                // Instantly update default nesting line color to match
                database.NestingLinesColor = selectedTheme.treeLineColor;

                OnDatabaseModified();
            }

            GUILayout.Space(6);

            // Display palette and mockup for ONLY the selected theme
            HierarchyThemeData activeTheme = database.Themes[newTheme];

            GUILayout.BeginHorizontal();
            GUILayout.Label("Palette: ", EditorStyles.miniLabel, GUILayout.Width(50f));
            DrawColorMiniBox(activeTheme.xrColor);
            DrawColorMiniBox(activeTheme.uiColor);
            DrawColorMiniBox(activeTheme.audioColor);
            DrawColorMiniBox(activeTheme.envColor);
            DrawColorMiniBox(activeTheme.interactColor);
            DrawColorMiniBox(activeTheme.effectsColor);
            DrawColorMiniBox(activeTheme.managersColor);
            DrawColorMiniBox(activeTheme.debugColor);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Mockup rendering
            DrawMockup(activeTheme);

            GUILayout.Space(4);
            GUILayout.EndVertical();
        }

        private void DrawColorMiniBox(Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(12f, 12f);
            rect.width = 12f;
            rect.height = 12f;
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, color);
            }
            GUILayout.Space(4);
        }

        private void DrawMockup(HierarchyThemeData theme)
        {
            Rect mockupRect = GUILayoutUtility.GetRect(120f, 36f);
            if (Event.current.type != EventType.Repaint) return;

            Color bg = EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUI.DrawRect(mockupRect, bg);

            // 1. Mock Header
            Rect headerRect = new Rect(mockupRect.x, mockupRect.y, mockupRect.width, 18f);
            EditorGUI.DrawRect(headerRect, theme.xrColor);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, headerRect.width, 1f), theme.borderColor);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1f, headerRect.width, 1f), theme.borderColor);

            GUIStyle textStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = Color.white } };
            GUI.Label(new Rect(headerRect.x + 8f, headerRect.y + 2f, 100f, 14f), "🎮 XR SETUP", textStyle);

            // 2. Mock Nested Gameobject
            Rect childRect = new Rect(mockupRect.x, mockupRect.y + 18f, mockupRect.width, 18f);
            EditorGUI.DrawRect(new Rect(childRect.x + 12f, childRect.y, 1f, childRect.height), theme.treeLineColor);
            EditorGUI.DrawRect(new Rect(childRect.x + 12f, childRect.y + 9f, 6f, 1f), theme.treeLineColor);

            GUIStyle childTextStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black } };
            GUI.Label(new Rect(childRect.x + 22f, childRect.y + 2f, 120f, 14f), "Main Camera", childTextStyle);

            GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = theme.badgeTextColor }
            };
            Rect badgeRect = new Rect(childRect.xMax - 32f, childRect.y + 3f, 24f, 12f);
            EditorGUI.DrawRect(badgeRect, theme.badgeBackgroundColor);
            GUI.Label(badgeRect, "[ 2 ]", badgeStyle);
        }

        private void DrawGlobalSettings()
        {
            if (serializedDatabase == null || globalLineColorProperty == null || globalLineStyleProperty == null) return;

            // Box 1: Global Separation & Styling Settings
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGroupHeader("Global Separation & Styling Settings", "📏");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(globalLineColorProperty, new GUIContent("Line Color"));
            EditorGUILayout.PropertyField(globalLineStyleProperty, new GUIContent("Line Style"));
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // Box 2: Feature Toggles & Configurations
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGroupHeader("Feature Toggles & Configurations", "⚙️");
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(showComponentIconsProperty, new GUIContent("Component Icons"));
            EditorGUILayout.PropertyField(showChildCountBadgesProperty, new GUIContent("Child Counts"));
            GUILayout.EndHorizontal();

            if (showChildCountBadgesProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(childCountColorModeProperty, new GUIContent("Color Mode"));
                EditorGUILayout.PropertyField(childCountTextColorProperty, new GUIContent("Text Color"));
                
                EditorGUILayout.PropertyField(childCountBorderStyleProperty, new GUIContent("Border Style"));
                if (childCountColorModeProperty.intValue == (int)HierarchyChildCountColorMode.Custom &&
                    childCountBorderStyleProperty.intValue != (int)HierarchyChildCountBorderStyle.None)
                {
                    EditorGUILayout.PropertyField(childCountBorderColorProperty, new GUIContent("Border Color"));
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(showGameObjectBorderProperty, new GUIContent("GameObject Border"));
            if (showGameObjectBorderProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(gameObjectBorderColorProperty, new GUIContent("Border Color"));
                EditorGUILayout.PropertyField(gameObjectBorderOpacityProperty, new GUIContent("Border Opacity"));
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(4);
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // Box 3: Nesting Lines Settings
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGroupHeader("Nesting Lines Settings", "🌿");
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(showNestingLinesProperty, new GUIContent("Nesting Lines"), GUILayout.Width(150f));
            if (showNestingLinesProperty.boolValue && !useRainbowNestingProperty.boolValue)
            {
                EditorGUILayout.PropertyField(nestingLinesColorProperty, GUIContent.none);
            }
            GUILayout.EndHorizontal();

            if (showNestingLinesProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(useRainbowNestingProperty, new GUIContent("Rainbow Nesting"));

                if (useRainbowNestingProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(rainbowPaletteProperty, new GUIContent("Rainbow Palette Theme"));
                    EditorGUILayout.PropertyField(nestingLinesOpacityProperty, new GUIContent("Rainbow Opacity"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(4);
            GUILayout.EndVertical();
        }

        private void DrawReorderingControls()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGroupHeader("Modify Hierarchy Sections", "✏️");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Section", GUILayout.Height(24f)))
            {
                AddNewHeader();
            }
            if (GUILayout.Button("🎲 Randomize Colors", GUILayout.Height(24f)))
            {
                RandomizeHeaderColors();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawBottomButtons()
        {
            GUILayout.Space(6);
            Rect separator = GUILayoutUtility.GetRect(0f, 1f);
            EditorGUI.DrawRect(separator, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.75f, 0.75f, 0.75f));
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            GUIStyle primaryBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };

            if (GUILayout.Button("Create Headers", primaryBtnStyle, GUILayout.Height(30f)))
            {
                HierarchyCreator.CreateHeaders(database);
            }

            if (GUILayout.Button("Update Headers", primaryBtnStyle, GUILayout.Height(30f)))
            {
                HierarchyCreator.UpdateHeaders(database);
            }

            GUIStyle deleteBtnStyle = new GUIStyle(primaryBtnStyle)
            {
                normal = { textColor = new Color(0.9f, 0.35f, 0.35f) }
            };
            if (GUILayout.Button("Delete Headers", deleteBtnStyle, GUILayout.Height(30f)))
            {
                if (EditorUtility.DisplayDialog("Delete Headers", "Are you sure you want to delete all hierarchy headers in the active scene?", "Yes", "No"))
                {
                    if (database != null)
                    {
                        HierarchyCreator.DeleteHeaders(database);
                    }
                }
            }

            if (GUILayout.Button("Refresh", primaryBtnStyle, GUILayout.Height(30f)))
            {
                HierarchyDrawer.RefreshCache();
                EditorApplication.RepaintHierarchyWindow();
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Operations Logic

        private void AddNewHeader()
        {
            int index = headersProperty.arraySize;
            headersProperty.InsertArrayElementAtIndex(index);
            
            SerializedProperty element = headersProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("headerName").stringValue = "New Header";
            
            float hue = UnityEngine.Random.value;
            Color randomCol = Color.HSVToRGB(hue, 0.7f, 0.75f);
            randomCol.a = 0.85f;
            element.FindPropertyRelative("color").colorValue = randomCol;
            element.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();

            serializedDatabase.ApplyModifiedProperties();
            OnDatabaseModified();
        }

        private void RandomizeHeaderColors()
        {
            if (database == null || database.Headers == null) return;
            
            Undo.RecordObject(database, "Randomize Header Colors");
            for (int i = 0; i < database.Headers.Count; i++)
            {
                float hue = UnityEngine.Random.value;
                Color randomCol = Color.HSVToRGB(hue, 0.7f, 0.75f);
                randomCol.a = 0.85f;
                database.Headers[i].Color = randomCol;
            }
            
            serializedDatabase.Update();
            OnDatabaseModified();
        }

        private void OnDatabaseModified()
        {
            HierarchyUtility.SaveDatabase(database);
            HierarchyDrawer.RefreshCache();
            EditorApplication.RepaintHierarchyWindow();
        }

        private void ApplyThemeColorsToHeaders(HierarchyThemeData theme)
        {
            if (database == null || database.Headers == null) return;

            Color[] themeColors = new Color[]
            {
                theme.xrColor,
                theme.uiColor,
                theme.audioColor,
                theme.envColor,
                theme.interactColor,
                theme.effectsColor,
                theme.managersColor,
                theme.debugColor
            };

            for (int i = 0; i < database.Headers.Count; i++)
            {
                var h = database.Headers[i];
                string upper = h.HeaderName.ToUpperInvariant();
                if (upper.Contains("XR")) h.Color = theme.xrColor;
                else if (upper.Contains("UI")) h.Color = theme.uiColor;
                else if (upper.Contains("AUDIO")) h.Color = theme.audioColor;
                else if (upper.Contains("ENV") || upper.Contains("ENVIRONMENT")) h.Color = theme.envColor;
                else if (upper.Contains("INTERACT")) h.Color = theme.interactColor;
                else if (upper.Contains("EFFECT")) h.Color = theme.effectsColor;
                else if (upper.Contains("MANAGER")) h.Color = theme.managersColor;
                else if (upper.Contains("DEBUG")) h.Color = theme.debugColor;
                else
                {
                    h.Color = themeColors[i % themeColors.Length];
                }
            }
        }

        #endregion
    }
}
