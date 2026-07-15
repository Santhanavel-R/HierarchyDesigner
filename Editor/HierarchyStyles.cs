using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Constants and GUIStyles used in custom drawing and settings panels.
    /// </summary>
    public static class HierarchyStyles
    {
        #region Constants

        public const float HeaderHeight = 22f;
        public const float SeparatorHeight = 1f;
        public const float LeftOffset = 18f;
        public const float IconLeftOffset = 32f;
        public const float IconSize = 16f;
        public const float TextLeftOffset = 52f;

        #endregion

        #region Custom Styles

        private static GUIStyle headerLabelStyle;

        #endregion

        #region Properties

        /// <summary>
        /// Style used to draw custom header text.
        /// </summary>
        public static GUIStyle HeaderLabelStyle
        {
            get
            {
                if (headerLabelStyle == null)
                {
                    headerLabelStyle = new GUIStyle(EditorStyles.whiteBoldLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 11,
                        fontStyle = FontStyle.Bold
                    };
                    headerLabelStyle.normal.textColor = Color.white;
                }
                return headerLabelStyle;
            }
        }

        public static Color SeparatorColor => EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);

        #endregion
    }
}
