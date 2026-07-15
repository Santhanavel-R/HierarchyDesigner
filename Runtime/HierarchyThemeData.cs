using System;
using UnityEngine;

namespace HierarchyDesigner.Runtime
{
    /// <summary>
    /// Holds visual theme properties to style custom category headers, tree guide lines, 
    /// selection highlights, borders, and count badges.
    /// </summary>
    [Serializable]
    public class HierarchyThemeData
    {
        public string themeName;

        // Custom category header colors
        public Color xrColor;
        public Color uiColor;
        public Color envColor;
        public Color audioColor;
        public Color interactColor;
        public Color effectsColor;
        public Color managersColor;
        public Color debugColor;

        // Styling accents
        public Color headerGradientColor;
        public Color borderColor;
        public Color separatorLineColor;
        public Color treeLineColor;
        public Color badgeBackgroundColor;
        public Color badgeTextColor;
        public Color iconAccentColor;
        public Color hoverOverlayColor;
        public Color selectionOverlayColor;

        public HierarchyThemeData(string name)
        {
            themeName = name;
        }
    }
}
