using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Intercepts Hierarchy Window GUI drawing events and custom renders visual elements,
    /// dynamically fetching properties from the active selected theme preset.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyDrawer
    {
        #region Fields

        private static HierarchyDatabase cachedDatabase;
        private static readonly Dictionary<string, HierarchyHeaderData> HeaderCache = new Dictionary<string, HierarchyHeaderData>();
        
        // Caches for textures to ensure fast rendering
        private static readonly Dictionary<string, Texture2D> GradientTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<Color, Texture2D> PillTextureCache = new Dictionary<Color, Texture2D>();

        // Rainbow color sequences for nesting lines
        private static readonly Color[] RainbowColorsDefault = new Color[]
        {
            new Color(0.9f, 0.3f, 0.3f),  // Muted Red
            new Color(0.9f, 0.6f, 0.2f),  // Muted Orange
            new Color(0.9f, 0.8f, 0.2f),  // Muted Yellow
            new Color(0.3f, 0.8f, 0.4f),  // Muted Green
            new Color(0.2f, 0.6f, 0.9f),  // Muted Blue
            new Color(0.6f, 0.3f, 0.9f)   // Muted Violet
        };

        private static readonly Color[] RainbowColorsPastel = new Color[]
        {
            new Color(1.0f, 0.65f, 0.65f),
            new Color(1.0f, 0.8f, 0.65f),
            new Color(1.0f, 0.95f, 0.7f),
            new Color(0.75f, 0.92f, 0.75f),
            new Color(0.7f, 0.85f, 1.0f),
            new Color(0.88f, 0.78f, 1.0f)
        };

        private static readonly Color[] RainbowColorsNeon = new Color[]
        {
            new Color(1.0f, 0.1f, 0.5f),
            new Color(1.0f, 0.5f, 0.0f),
            new Color(0.85f, 1.0f, 0.0f),
            new Color(0.0f, 1.0f, 0.4f),
            new Color(0.0f, 0.85f, 1.0f),
            new Color(0.7f, 0.0f, 1.0f)
        };

        private static readonly Color[] RainbowColorsWarm = new Color[]
        {
            new Color(0.85f, 0.15f, 0.25f),
            new Color(0.95f, 0.45f, 0.15f),
            new Color(0.95f, 0.75f, 0.15f),
            new Color(0.95f, 0.5f, 0.5f),
            new Color(1.0f, 0.8f, 0.2f),
            new Color(0.85f, 0.75f, 0.65f)
        };

        private static readonly Color[] RainbowColorsCool = new Color[]
        {
            new Color(0.0f, 0.55f, 0.55f),
            new Color(0.5f, 0.85f, 0.7f),
            new Color(0.35f, 0.7f, 0.95f),
            new Color(0.25f, 0.35f, 0.75f),
            new Color(0.55f, 0.25f, 0.6f),
            new Color(0.15f, 0.65f, 0.35f)
        };

        private static readonly Color[] RainbowColorsMonochrome = new Color[]
        {
            new Color(0.35f, 0.35f, 0.35f),
            new Color(0.5f, 0.5f, 0.5f),
            new Color(0.65f, 0.65f, 0.65f),
            new Color(0.45f, 0.5f, 0.55f),
            new Color(0.8f, 0.8f, 0.8f),
            new Color(0.25f, 0.25f, 0.25f)
        };

        private static Color[] GetRainbowColors(HierarchyRainbowPalette palette)
        {
            switch (palette)
            {
                case HierarchyRainbowPalette.Pastel:
                    return RainbowColorsPastel;
                case HierarchyRainbowPalette.Neon:
                    return RainbowColorsNeon;
                case HierarchyRainbowPalette.Warm:
                    return RainbowColorsWarm;
                case HierarchyRainbowPalette.Cool:
                    return RainbowColorsCool;
                case HierarchyRainbowPalette.Monochrome:
                    return RainbowColorsMonochrome;
                case HierarchyRainbowPalette.Default:
                default:
                    return RainbowColorsDefault;
            }
        }

        #endregion

        #region Constructors

        static HierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
            RefreshCache();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rebuilds the fast lookup cache from the database. Call this after modifications.
        /// </summary>
        public static void RefreshCache()
        {
            HeaderCache.Clear();
            cachedDatabase = HierarchyUtility.GetOrCreateDatabase();

            if (cachedDatabase != null && cachedDatabase.Headers != null)
            {
                foreach (var data in cachedDatabase.Headers)
                {
                    if (data != null && !string.IsNullOrEmpty(data.Guid))
                    {
                        HeaderCache[data.Guid] = data;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Event handler for hierarchy item drawing and inputs.
        /// </summary>
        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            // Load database if not already loaded
            if (cachedDatabase == null)
            {
                cachedDatabase = HierarchyUtility.GetOrCreateDatabase();
            }

            // Check if it is a hierarchy header
            bool isHeader = go.TryGetComponent(out HierarchyHeader header);
            HierarchyHeaderData headerData = null;
            if (isHeader && !string.IsNullOrEmpty(header.Guid))
            {
                HeaderCache.TryGetValue(header.Guid, out headerData);
            }

            // Repaint trigger to keep hover transitions responsive
            if (Event.current.type == EventType.MouseMove)
            {
                EditorApplication.RepaintHierarchyWindow();
            }

            // Only draw on Repaint to avoid drawing during input and layout events
            if (Event.current.type != EventType.Repaint) return;

            HierarchyThemeData theme = (cachedDatabase != null) ? cachedDatabase.GetActiveTheme() : null;

            // Render Custom Headers
            if (isHeader && headerData != null)
            {
                DrawHeader(selectionRect, go, headerData, theme);
            }
            else
            {
                // Render Nesting Lines
                if (cachedDatabase != null && cachedDatabase.ShowNestingLines)
                {
                    DrawNestingLines(selectionRect, go, cachedDatabase.NestingLinesColor);
                }

                // Render Right-Aligned Features (Component Icons and Child Count Badges)
                DrawRightSideFeatures(selectionRect, go, theme);

                // Render selected or hovered GameObject row borders
                if (cachedDatabase != null && cachedDatabase.ShowGameObjectBorder)
                {
                    bool isSelected = Selection.activeGameObject == go;
                    bool isHoveredRow = selectionRect.Contains(Event.current.mousePosition);

                    if (isSelected || isHoveredRow)
                    {
                        Color customBorderColor = cachedDatabase.GameObjectBorderColor;
                        float borderOpacity = cachedDatabase.GameObjectBorderOpacity;

                        Color borderCol = isSelected
                            ? new Color(customBorderColor.r, customBorderColor.g, customBorderColor.b, customBorderColor.a * borderOpacity)
                            : new Color(customBorderColor.r, customBorderColor.g, customBorderColor.b, customBorderColor.a * borderOpacity * 0.35f);

                        // Left border line
                        EditorGUI.DrawRect(new Rect(selectionRect.xMin - HierarchyStyles.LeftOffset, selectionRect.y, 1f, selectionRect.height), borderCol);
                        // Right border line
                        EditorGUI.DrawRect(new Rect(selectionRect.xMax + 16f, selectionRect.y, 1f, selectionRect.height), borderCol);
                        // Top border line
                        EditorGUI.DrawRect(new Rect(selectionRect.xMin - HierarchyStyles.LeftOffset, selectionRect.y, selectionRect.width + HierarchyStyles.LeftOffset + 16f, 1f), borderCol);
                        // Bottom border line
                        EditorGUI.DrawRect(new Rect(selectionRect.xMin - HierarchyStyles.LeftOffset, selectionRect.yMax - 1f, selectionRect.width + HierarchyStyles.LeftOffset + 16f, 1f), borderCol);
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom drawing over the default hierarchy item row for headers.
        /// </summary>
        private static void DrawHeader(Rect rect, GameObject go, HierarchyHeaderData data, HierarchyThemeData theme)
        {
            float headerH = rect.height - .5f;

            // Calculate a background rect spanning the full row width
            Rect bgRect = new Rect(
                rect.xMin - HierarchyStyles.LeftOffset,
                rect.y,
                rect.width + HierarchyStyles.LeftOffset + 16f,
                headerH
            );

            // Draw editor background color first to clean up default text and handle rounded corners
            bool isSelected = Selection.activeGameObject == go;
            Color baseBgColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f);
            if (isSelected)
            {
                baseBgColor = EditorGUIUtility.isProSkin ? new Color(0.17f, 0.36f, 0.53f, 1f) : new Color(0.24f, 0.48f, 0.9f, 1f);
            }
            EditorGUI.DrawRect(bgRect, baseBgColor);

            // Use data.Color directly so user customized color changes reflect instantly
            Color headerBaseColor = data.Color;
            Color headerGradientColor = (theme != null) ? theme.headerGradientColor : headerBaseColor * 0.65f;

            // Draw glossy gradient background
            Texture2D gradientTex = GetOrCreateGradientTexture(headerBaseColor, headerGradientColor, theme);
            if (gradientTex != null)
            {
                GUI.DrawTexture(bgRect, gradientTex, ScaleMode.StretchToFill);
            }

            // Draw selection highlight frame overlay if selected
            if (isSelected)
            {
                Color selectionCol = new Color(0.24f, 0.48f, 0.9f, 0.25f);
                EditorGUI.DrawRect(bgRect, selectionCol);
            }

            // Styling labels
            GUIStyle titleStyle = new GUIStyle(HierarchyStyles.HeaderLabelStyle);
            Rect labelRect = new Rect(bgRect.xMin + 8f, bgRect.y, bgRect.width - 40f, bgRect.height);

            // Draw title shadow
            titleStyle.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
            GUI.Label(new Rect(labelRect.x + 1f, labelRect.y + 1f, labelRect.width, labelRect.height), data.HeaderName, titleStyle);
            titleStyle.normal.textColor = Color.white;
            GUI.Label(labelRect, data.HeaderName, titleStyle);

            // Draw child count badge on headers too if they have children
            if (cachedDatabase != null && cachedDatabase.ShowChildCountBadges && go.transform.childCount > 0)
            {
                float badgeX = bgRect.xMax - 30f;
                DrawChildCountBadge(new Rect(badgeX, bgRect.y, 30f, bgRect.height), go.transform.childCount, go);
            }

            // Draw line dividers
            Color lineColor = cachedDatabase != null ? cachedDatabase.GlobalLineColor : Color.white;
            HierarchyLineStyle lineStyle = cachedDatabase != null ? cachedDatabase.GlobalLineStyle : HierarchyLineStyle.Solid;

            float topHeight = lineStyle == HierarchyLineStyle.Double ? 3f : HierarchyStyles.SeparatorHeight;
            Rect topSeparatorRect = new Rect(bgRect.xMin, bgRect.yMin, bgRect.width, topHeight);
            DrawSeparatorLine(topSeparatorRect, lineColor, lineStyle);

            float bottomHeight = lineStyle == HierarchyLineStyle.Double ? 3f : HierarchyStyles.SeparatorHeight;
            Rect bottomSeparatorRect = new Rect(bgRect.xMin, bgRect.yMax - bottomHeight, bgRect.width, bottomHeight);
            DrawSeparatorLine(bottomSeparatorRect, lineColor, lineStyle);
        }

        /// <summary>
        /// Draws the nesting connection lines (breadcrumbs).
        /// </summary>
        private static void DrawNestingLines(Rect rect, GameObject go, Color lineColor)
        {
            Transform t = go.transform;
            if (t.parent == null) return;

            // Collect ancestors up to root
            List<Transform> ancestors = new List<Transform>();
            Transform curr = t.parent;
            while (curr != null)
            {
                ancestors.Insert(0, curr);
                curr = curr.parent;
            }

            int depth = ancestors.Count;
            float indent = 14f;
            
            // Re-calculate the root visual X position
            float rootX = rect.x - indent * depth;
            float lineOffset = -10f; // Alignment offset for foldouts

            bool useRainbow = cachedDatabase != null && cachedDatabase.UseRainbowNesting;
            HierarchyRainbowPalette palette = cachedDatabase != null ? cachedDatabase.RainbowPalette : HierarchyRainbowPalette.Default;
            float opacity = cachedDatabase != null ? cachedDatabase.NestingLinesOpacity : 0.75f;
            Color[] activeRainbowColors = GetRainbowColors(palette);

            for (int i = 0; i < depth; i++)
            {
                float lineX = rootX + i * indent + lineOffset;
                Color baseColor = useRainbow ? activeRainbowColors[i % activeRainbowColors.Length] : lineColor;
                Color segmentColor = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * opacity);

                if (i < depth - 1)
                {
                    // Vertical lines running through ancestor slots
                    bool isLast = false;
                    if (ancestors[i].parent != null)
                    {
                        int siblingCount = ancestors[i].parent.childCount;
                        isLast = ancestors[i].parent.GetChild(siblingCount - 1) == ancestors[i];
                    }
                    else
                    {
                        var rootObjects = go.scene.GetRootGameObjects();
                        if (rootObjects.Length > 0)
                        {
                            isLast = rootObjects[rootObjects.Length - 1] == ancestors[i].gameObject;
                        }
                    }

                    if (!isLast)
                    {
                        Rect lineRect = new Rect(lineX, rect.y, 1f, rect.height);
                        EditorGUI.DrawRect(lineRect, segmentColor);
                    }
                }
                else
                {
                    // Connector line for the immediate parent
                    bool isLastChild = t.parent.GetChild(t.parent.childCount - 1) == t;
                    float yMax = isLastChild ? rect.y + rect.height * 0.5f : rect.y + rect.height;

                    Rect vLineRect = new Rect(lineX, rect.y, 1f, yMax - rect.y);
                    EditorGUI.DrawRect(vLineRect, segmentColor);

                    float branchLength = 8f;
                    Rect hLineRect = new Rect(lineX, rect.y + rect.height * 0.5f, branchLength, 1f);
                    EditorGUI.DrawRect(hLineRect, segmentColor);
                }
            }
        }

        /// <summary>
        /// Handles visual rendering of the filtered component icons and badges.
        /// </summary>
        private static void DrawRightSideFeatures(Rect rect, GameObject go, HierarchyThemeData theme)
        {
            float currentX = rect.xMax - 6f;

            // Calculate label width to prevent icon overlap on short hierarchy windows
            float labelWidth = EditorStyles.label.CalcSize(new GUIContent(go.name)).x;
            float limitX = rect.x + labelWidth + 16f; // Leftmost bound for icon drawing

            // 1. Draw Child Count Badge
            if (cachedDatabase != null && cachedDatabase.ShowChildCountBadges && go.transform.childCount > 0)
            {
                int count = go.transform.childCount;
                HierarchyChildCountBorderStyle style = cachedDatabase.ChildCountBorderStyle;
                
                string text = $"{count}";
                switch (style)
                {
                    case HierarchyChildCountBorderStyle.Classic:
                        text = $"[{count}]";
                        break;
                    case HierarchyChildCountBorderStyle.Bubble:
                        text = $"({count})";
                        break;
                    case HierarchyChildCountBorderStyle.Diamond:
                        text = $"◆ {count}";
                        break;
                    case HierarchyChildCountBorderStyle.Hexagon:
                        text = $"⬢ {count}";
                        break;
                    case HierarchyChildCountBorderStyle.Notification:
                        text = $"● {count}";
                        break;
                    case HierarchyChildCountBorderStyle.Dot:
                        text = $"◉ {count}";
                        break;
                }

                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                Vector2 size = labelStyle.CalcSize(new GUIContent(text));
                float badgeW = size.x + 4f;
                if (style == HierarchyChildCountBorderStyle.Tag)
                {
                    badgeW = size.x + 10f;
                }
                float badgeH = 13f;

                if (currentX - badgeW >= limitX)
                {
                    Rect badgeRect = new Rect(currentX - badgeW, rect.y + (rect.height - badgeH) * 0.5f, badgeW, badgeH);
                    DrawChildCountBadge(badgeRect, count, go);
                    currentX -= badgeW + 6f;
                }
            }

            // 2. Component Icons and Custom Scripts Tooltip
            if (cachedDatabase != null && cachedDatabase.ShowComponentIcons)
            {
                Component[] comps = go.GetComponents<Component>();
                int drawnCount = 0;
                List<string> customScripts = new List<string>();

                for (int i = 0; i < comps.Length; i++)
                {
                    Component comp = comps[i];
                    if (comp == null || comp is Transform || comp is HierarchyHeader) continue;

                    bool isUnityComponent = comp.GetType().Namespace != null && 
                                           (comp.GetType().Namespace.StartsWith("UnityEngine") || 
                                            comp.GetType().Namespace.StartsWith("UnityEditor") ||
                                            comp.GetType().Namespace.StartsWith("Unity"));
                    bool isCustomScript = comp is MonoBehaviour && !isUnityComponent;

                    if (isCustomScript)
                    {
                        customScripts.Add(comp.GetType().Name);
                    }
                    else if (isUnityComponent)
                    {
                        // Draw Unity built-in component icons (limit to 5)
                        if (drawnCount < 5 && currentX - 14f >= limitX)
                        {
                            Texture2D compIcon = EditorGUIUtility.ObjectContent(null, comp.GetType()).image as Texture2D;
                            if (compIcon != null)
                            {
                                Rect iconRect = new Rect(currentX - 14f, rect.y + (rect.height - 14f) * 0.5f, 14f, 14f);
                                GUI.DrawTexture(iconRect, compIcon);
                                currentX -= 16f;
                                drawnCount++;
                            }
                        }
                    }
                }

                if (customScripts.Count > 0)
                {
                    string tooltipText = "Custom Scripts:\n• " + string.Join("\n• ", customScripts);
                    // Draw a transparent label over the selection rect with the tooltip content
                    GUI.Label(rect, new GUIContent("", tooltipText));
                }
            }
        }

        private static void DrawChildCountBadge(Rect rect, int count, GameObject go)
        {
            if (cachedDatabase == null) return;

            // 1. Determine text (inherited count) color based on nesting guide lines depth
            Transform t = go.transform;
            int depth = 0;
            Transform curr = t.parent;
            while (curr != null)
            {
                depth++;
                curr = curr.parent;
            }

            Color baseNestingColor = cachedDatabase.NestingLinesColor;
            float opacity = cachedDatabase.NestingLinesOpacity;
            if (cachedDatabase.UseRainbowNesting)
            {
                Color[] activeRainbowColors = GetRainbowColors(cachedDatabase.RainbowPalette);
                int activeDepth = Mathf.Max(0, depth - 1);
                baseNestingColor = activeRainbowColors[activeDepth % activeRainbowColors.Length];
            }
            Color textColor = new Color(baseNestingColor.r, baseNestingColor.g, baseNestingColor.b, baseNestingColor.a * opacity);

            // 2. Determine decoration color based on the selected style
            HierarchyChildCountBorderStyle style = cachedDatabase.ChildCountBorderStyle;
            Color decColor = Color.white;
            switch (style)
            {
                case HierarchyChildCountBorderStyle.Classic:
                case HierarchyChildCountBorderStyle.Bubble:
                    decColor = new Color(0.65f, 0.65f, 0.65f, 0.85f); // Gray
                    break;
                case HierarchyChildCountBorderStyle.Diamond:
                    decColor = new Color(0.12f, 0.73f, 0.82f, 1.0f); // Cyan
                    break;
                case HierarchyChildCountBorderStyle.Hexagon:
                    decColor = new Color(0.95f, 0.62f, 0.12f, 1.0f); // Orange/Amber
                    break;
                case HierarchyChildCountBorderStyle.Notification:
                    decColor = new Color(0.18f, 0.52f, 0.92f, 1.0f); // Notification Blue
                    break;
                case HierarchyChildCountBorderStyle.Dot:
                    decColor = new Color(0.85f, 0.32f, 0.32f, 1.0f); // Red
                    break;
                case HierarchyChildCountBorderStyle.Tag:
                    decColor = new Color(0.38f, 0.68f, 0.48f, 1.0f); // Soft Tag Green
                    break;
            }

            // 3. Build rich text string based on style
            string hexText = ColorUtility.ToHtmlStringRGBA(textColor);
            string hexDec = ColorUtility.ToHtmlStringRGBA(decColor);

            string richText = "";
            Rect drawRect = rect;

            switch (style)
            {
                case HierarchyChildCountBorderStyle.None:
                    richText = $"<color=#{hexText}>{count}</color>";
                    break;
                case HierarchyChildCountBorderStyle.Classic:
                    richText = $"<color=#{hexDec}>[</color><color=#{hexText}>{count}</color><color=#{hexDec}>]</color>";
                    break;
                case HierarchyChildCountBorderStyle.Bubble:
                    richText = $"<color=#{hexDec}>(</color><color=#{hexText}>{count}</color><color=#{hexDec}>)</color>";
                    break;
                case HierarchyChildCountBorderStyle.Diamond:
                    richText = $"<color=#{hexDec}>◆ </color><color=#{hexText}>{count}</color>";
                    break;
                case HierarchyChildCountBorderStyle.Hexagon:
                    richText = $"<color=#{hexDec}>⬢ </color><color=#{hexText}>{count}</color>";
                    break;
                case HierarchyChildCountBorderStyle.Notification:
                    richText = $"<color=#{hexDec}>● </color><color=#{hexText}>{count}</color>";
                    break;
                case HierarchyChildCountBorderStyle.Dot:
                    richText = $"<color=#{hexDec}>◉ </color><color=#{hexText}>{count}</color>";
                    break;
                case HierarchyChildCountBorderStyle.Tag:
                    richText = $"<color=#{hexText}>{count}</color>";
                    drawRect.xMin += 1f;
                    drawRect.xMax -= 3f;
                    DrawTagBorder(rect, decColor);
                    break;
            }

            // 3. Draw text label
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                richText = true
            };
            
            GUI.Label(drawRect, richText, labelStyle);
        }

        private static void DrawTagBorder(Rect rect, Color color)
        {
            // Left vertical line
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 1f, 1f, rect.height - 2f), color);
            // Top horizontal line
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width - 4f, 1f), color);
            // Bottom horizontal line
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width - 4f, 1f), color);
            // Right arrow/tag tip lines
            EditorGUI.DrawRect(new Rect(rect.xMax - 4f, rect.y + 1f, 1f, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 3f, rect.y + 2f, 1f, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 2f, rect.y + 3f, 1f, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y + 4f, 1f, rect.height - 8f), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 2f, rect.yMax - 4f, 1f, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 3f, rect.yMax - 3f, 1f, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 4f, rect.yMax - 2f, 1f, 1f), color);
        }

        /// <summary>
        /// Generates or loads a cached gradient background texture.
        /// </summary>
        private static Texture2D GetOrCreateGradientTexture(Color baseColor, Color gradientColor, HierarchyThemeData theme)
        {
            string key = $"{baseColor.r}_{baseColor.g}_{baseColor.b}_{gradientColor.r}_{gradientColor.g}_{gradientColor.b}";
            if (GradientTextureCache.TryGetValue(key, out Texture2D tex) && tex != null)
            {
                return tex;
            }

            int width = 256;
            int height = 32;
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int radius = 4;

            for (int y = 0; y < height; y++)
            {
                float tY = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float tX = (float)x / width;

                    // Left to Right gradient
                    Color col = Color.Lerp(baseColor, gradientColor, tX);
                    col.a = baseColor.a;

                    // Thin 1px border overlay
                    bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    if (isBorder)
                    {
                        Color borderColor = (theme != null) ? theme.borderColor : Color.white * 0.2f;
                        col = Color.Lerp(col, borderColor, 0.5f);
                    }

                    // Soft gloss highlight overlay
                    if (tY > 0.4f)
                    {
                        float gloss = (tY - 0.4f) / 0.6f;
                        col = Color.Lerp(col, Color.white, gloss * 0.1f);
                    }

                    // Bottom ambient drop shadow edge
                    if (tY < 0.1f)
                    {
                        col = Color.Lerp(col, Color.black, (0.1f - tY) * 5f * 0.2f);
                    }

                    // Anti-aliased corner masking
                    bool mask = false;
                    if (x < radius && y < radius)
                    {
                        if ((x - radius) * (x - radius) + (y - radius) * (y - radius) > radius * radius) mask = true;
                    }
                    else if (x < radius && y >= height - radius)
                    {
                        if ((x - radius) * (x - radius) + (y - (height - radius)) * (y - (height - radius)) > radius * radius) mask = true;
                    }
                    else if (x >= width - radius && y < radius)
                    {
                        if ((x - (width - radius)) * (x - (width - radius)) + (y - radius) * (y - radius) > radius * radius) mask = true;
                    }
                    else if (x >= width - radius && y >= height - radius)
                    {
                        if ((x - (width - radius)) * (x - (width - radius)) + (y - (height - radius)) * (y - (height - radius)) > radius * radius) mask = true;
                    }

                    tex.SetPixel(x, y, mask ? Color.clear : col);
                }
            }
            tex.Apply();
            GradientTextureCache[key] = tex;
            return tex;
        }

        /// <summary>
        /// Generates or loads a cached pill-shaped texture.
        /// </summary>
        private static Texture2D GetOrCreatePillTexture(Color color)
        {
            if (PillTextureCache.TryGetValue(color, out Texture2D tex) && tex != null)
            {
                return tex;
            }

            int width = 32;
            int height = 16;
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int radius = 6;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool mask = false;
                    if (x < radius && y < radius && (x - radius) * (x - radius) + (y - radius) * (y - radius) > radius * radius) mask = true;
                    else if (x < radius && y >= height - radius && (x - radius) * (x - radius) + (y - (height - radius)) * (y - (height - radius)) > radius * radius) mask = true;
                    else if (x >= width - radius && y < radius && (x - (width - radius)) * (x - (width - radius)) + (y - radius) * (y - radius) > radius * radius) mask = true;
                    else if (x >= width - radius && y >= height - radius && (x - (width - radius)) * (x - (width - radius)) + (y - (height - radius)) * (y - (height - radius)) > radius * radius) mask = true;

                    tex.SetPixel(x, y, mask ? Color.clear : color);
                }
            }
            tex.Apply();
            PillTextureCache[color] = tex;
            return tex;
        }

        /// <summary>
        /// Helper to draw header separators.
        /// </summary>
        private static void DrawSeparatorLine(Rect rect, Color color, HierarchyLineStyle style)
        {
            if (style == HierarchyLineStyle.None) return;

            if (style == HierarchyLineStyle.Solid)
            {
                EditorGUI.DrawRect(rect, color);
            }
            else if (style == HierarchyLineStyle.Dashed)
            {
                float dashLength = 4f;
                float gapLength = 3f;
                float currentX = rect.x;
                float endX = rect.xMax;
                while (currentX < endX)
                {
                    float drawWidth = Mathf.Min(dashLength, endX - currentX);
                    EditorGUI.DrawRect(new Rect(currentX, rect.y, drawWidth, rect.height), color);
                    currentX += dashLength + gapLength;
                }
            }
            else if (style == HierarchyLineStyle.Dotted)
            {
                float dotLength = rect.height;
                float gapLength = 3f;
                float currentX = rect.x;
                float endX = rect.xMax;
                while (currentX < endX)
                {
                    float drawWidth = Mathf.Min(dotLength, endX - currentX);
                    EditorGUI.DrawRect(new Rect(currentX, rect.y, drawWidth, rect.height), color);
                    currentX += dotLength + gapLength;
                }
            }
            else if (style == HierarchyLineStyle.Double)
            {
                float lineThickness = Mathf.Max(0.5f, rect.height * 0.35f);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, lineThickness), color);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - lineThickness, rect.width, lineThickness), color);
            }
        }

        private static Color GetHeaderColorForTheme(string name, HierarchyThemeData theme, Color fallback)
        {
            string upper = name.ToUpperInvariant();
            if (upper.Contains("XR")) return theme.xrColor;
            if (upper.Contains("UI")) return theme.uiColor;
            if (upper.Contains("AUDIO")) return theme.audioColor;
            if (upper.Contains("ENVIRONMENT") || upper.Contains("ENV")) return theme.envColor;
            if (upper.Contains("INTERACT")) return theme.interactColor;
            if (upper.Contains("EFFECT")) return theme.effectsColor;
            if (upper.Contains("MANAGER")) return theme.managersColor;
            if (upper.Contains("DEBUG")) return theme.debugColor;
            return fallback;
        }

        #endregion
    }
}
