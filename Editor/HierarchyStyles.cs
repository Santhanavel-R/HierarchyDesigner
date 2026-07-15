using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Static class defining the styling and visual parameters for drawing hierarchy headers.
    /// </summary>
    public static class HierarchyStyles
    {
        #region Constants

        /// <summary>
        /// Total height of the visual hierarchy header row.
        /// </summary>
        public const float HeaderHeight = 50f;

        /// <summary>
        /// Height of the separator line drawn below the text or at the bottom.
        /// </summary>
        public const float SeparatorHeight = 1.5f;

        /// <summary>
        /// Left padding offset to account for default hierarchy folding arrows and depth indent.
        /// </summary>
        public const float LeftOffset = 18f;

        /// <summary>
        /// Left offset specifically for drawing the icon.
        /// </summary>
        public const float IconLeftOffset = 32f;

        /// <summary>
        /// Width and height of the icon.
        /// </summary>
        public const float IconSize = 16f;

        /// <summary>
        /// Inner padding for text drawing.
        /// </summary>
        public const float TextLeftOffset = 52f;

        #endregion

        #region Fields

        private static GUIStyle headerLabelStyle;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the GUIStyle used to draw the header text.
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
                        fontSize = 10,
                        fontStyle = FontStyle.Bold
                    };
                    headerLabelStyle.normal.textColor = Color.white;
                }
                return headerLabelStyle;
            }
        }

        /// <summary>
        /// Gets the color used for drawing the separator lines, adapting dynamically to the dark or light skin theme.
        /// </summary>
        public static Color SeparatorColor => Color.black; // Subtle dark-grey line in Light skin;

        #endregion
    }
}
