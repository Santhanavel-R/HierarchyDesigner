using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Intercepts Hierarchy Window GUI drawing events and custom renders GameObjects
    /// containing the <see cref="HierarchyHeader"/> component.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyDrawer
    {
        #region Fields

        private static HierarchyDatabase cachedDatabase;
        private static readonly Dictionary<string, HierarchyHeaderData> HeaderCache = new Dictionary<string, HierarchyHeaderData>();

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
        /// Event handler for hierarchy item drawing.
        /// </summary>
        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            // Only draw on Repaint to avoid drawing during input and layout events
            if (Event.current.type != EventType.Repaint) return;

            // Retrieve GameObject using instance ID
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            // Check if it is a hierarchy header
            if (go.TryGetComponent(out HierarchyHeader header))
            {
                if (string.IsNullOrEmpty(header.Guid)) return;

                // Attempt to find layout configurations
                if (HeaderCache.TryGetValue(header.Guid, out HierarchyHeaderData data))
                {
                    DrawHeader(selectionRect, go, data);
                }
            }
        }

        /// <summary>
        /// Performs custom drawing over the default hierarchy item row.
        /// </summary>
        private static void DrawHeader(Rect rect, GameObject go, HierarchyHeaderData data)
        {
            // Calculate a background rect spanning the full row width, fitting exactly within the row height to prevent overlapping
            Rect bgRect = new Rect(
                rect.xMin - HierarchyStyles.LeftOffset,
                rect.y,
                rect.width + HierarchyStyles.LeftOffset + 16f,
                rect.height -.5f
            );

            // Erase the default text by drawing a solid base color matching the editor theme selection state
            bool isSelected = Selection.activeGameObject == go;
            Color baseBgColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f);
            if (isSelected)
            {
                baseBgColor = EditorGUIUtility.isProSkin ? new Color(0.17f, 0.36f, 0.53f, 1f) : new Color(0.24f, 0.48f, 0.9f, 1f);
            }
            EditorGUI.DrawRect(bgRect, baseBgColor);

            // Draw custom background using the configured color from the database (potentially with transparency)
            EditorGUI.DrawRect(bgRect, data.Color);

            // Draw selection highlight frame if the item is selected
            if (isSelected)
            {
                // Soft overlay indicating selection
                EditorGUI.DrawRect(bgRect, new Color(0.24f, 0.48f, 0.9f, 0.25f));
            }

            // Draw left-aligned name label inside the background banner
            GUIStyle labelStyle = HierarchyStyles.HeaderLabelStyle;
            Rect labelRect = new Rect(
                bgRect.xMin + 5f,
                bgRect.y,
                bgRect.width - 10f,
                bgRect.height
            );

            // Draw shadow for depth
            Rect shadowRect = new Rect(labelRect.x + 1f, labelRect.y + 1f, labelRect.width, labelRect.height);
            GUIStyle shadowStyle = new GUIStyle(labelStyle);
            shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
            GUI.Label(shadowRect, data.HeaderName, shadowStyle);

            GUI.Label(labelRect, data.HeaderName, labelStyle);

            if (cachedDatabase == null)
            {
                cachedDatabase = HierarchyUtility.GetOrCreateDatabase();
            }

            Color lineColor = cachedDatabase != null ? cachedDatabase.GlobalLineColor : Color.white;
            HierarchyLineStyle lineStyle = cachedDatabase != null ? cachedDatabase.GlobalLineStyle : HierarchyLineStyle.Solid;

            // Draw top framing line
            float topHeight = lineStyle == HierarchyLineStyle.Double ? 3f : HierarchyStyles.SeparatorHeight;
            Rect topSeparatorRect = new Rect(
                bgRect.xMin,
                bgRect.yMin,
                bgRect.width,
                topHeight
            );
            DrawSeparatorLine(topSeparatorRect, lineColor, lineStyle);

            // Draw bottom framing line
            float bottomHeight = lineStyle == HierarchyLineStyle.Double ? 3f : HierarchyStyles.SeparatorHeight;
            Rect bottomSeparatorRect = new Rect(
                bgRect.xMin,
                bgRect.yMax - bottomHeight,
                bgRect.width,
                bottomHeight
            );
            DrawSeparatorLine(bottomSeparatorRect, lineColor, lineStyle);
        }

        /// <summary>
        /// Draws a separator line using the specified style and color.
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
                float dotLength = rect.height; // Square dot matching height
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
                // Draw a top thin line and a bottom thin line with a gap in the middle
                float lineThickness = Mathf.Max(0.5f, rect.height * 0.35f);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, lineThickness), color);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - lineThickness, rect.width, lineThickness), color);
            }
        }

        #endregion
    }
}
