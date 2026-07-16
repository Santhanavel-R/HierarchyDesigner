using System;
using System.Collections.Generic;
using UnityEngine;

namespace HierarchyDesigner.Runtime
{
    /// <summary>
    /// Rainbow color palette types for nesting guide lines.
    /// </summary>
    public enum HierarchyRainbowPalette
    {
        Default,
        Pastel,
        Neon,
        Warm,
        Cool,
        Monochrome
    }

    /// <summary>
    /// Color modes for the Child Count Badges.
    /// </summary>
    public enum HierarchyChildCountColorMode
    {
        Custom,
        InheritNestingColor
    }

    /// <summary>
    /// Border styles for the Child Count Badges.
    /// </summary>
    public enum HierarchyChildCountBorderStyle
    {
        None,
        Outline,
        Solid,
        SolidWithOutline
    }

    /// <summary>
    /// Configuration database storing custom hierarchy headers, line styles,
    /// visual feature toggles, and centralized visual themes.
    /// </summary>
    [CreateAssetMenu(fileName = "HierarchyDatabase", menuName = "Hierarchy Designer/Layout Database", order = 1)]
    public class HierarchyDatabase : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField]
        [Tooltip("The list of hierarchy header configurations.")]
        private List<HierarchyHeaderData> headers = new List<HierarchyHeaderData>();

        [SerializeField]
        [Tooltip("The global line color for header separators.")]
        private Color globalLineColor = Color.white;

        [SerializeField]
        [Tooltip("The global line style for header separators.")]
        private HierarchyLineStyle globalLineStyle = HierarchyLineStyle.Solid;

        [SerializeField]
        [Tooltip("Toggle drawing of nesting guide lines in the hierarchy view.")]
        private bool showNestingLines = true;

        [SerializeField]
        [Tooltip("Color of the nesting guide lines.")]
        private Color nestingLinesColor = new Color(0.7f, 0.7f, 0.7f, 0.55f);

        [SerializeField]
        [Tooltip("Toggle drawing of quick component icons.")]
        private bool showComponentIcons = true;

        [SerializeField]
        [Tooltip("Toggle drawing of child count badges on parent GameObjects.")]
        private bool showChildCountBadges = true;

        [SerializeField]
        [Tooltip("The color resolution mode for child count badges.")]
        private HierarchyChildCountColorMode childCountColorMode = HierarchyChildCountColorMode.InheritNestingColor;

        [SerializeField]
        [Tooltip("The border outline/filled style for child count badges.")]
        private HierarchyChildCountBorderStyle childCountBorderStyle = HierarchyChildCountBorderStyle.None;

        [SerializeField]
        [Tooltip("The text label color inside the child count badge.")]
        private Color childCountTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        [SerializeField]
        [Tooltip("The solid background color of the child count badge (if Custom).")]
        private Color childCountBgColor = new Color(0.18f, 0.18f, 0.18f, 0.85f);

        [SerializeField]
        [Tooltip("The outline border color of the child count badge (if Custom).")]
        private Color childCountBorderColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

        [SerializeField]
        [Tooltip("The index of the selected color theme.")]
        private int activeThemeIndex = 0; // Default to Studio Pro

        [SerializeField]
        [Tooltip("Toggle rainbow color nesting lines.")]
        private bool useRainbowNesting = true;

        [SerializeField]
        [Tooltip("The selected rainbow color theme palette.")]
        private HierarchyRainbowPalette rainbowPalette = HierarchyRainbowPalette.Default;

        [SerializeField]
        [Tooltip("The opacity of the nesting lines (mostly rainbow).")]
        [Range(0f, 1f)]
        private float nestingLinesOpacity = 0.75f;

        [SerializeField]
        [Tooltip("Toggle rendering a thin visual border around selected/hovered GameObjects.")]
        private bool showGameObjectBorder = true;

        [SerializeField]
        [Tooltip("The custom color used to draw GameObject borders.")]
        private Color gameObjectBorderColor = new Color(0.24f, 0.48f, 0.9f, 1f);

        [SerializeField]
        [Tooltip("The opacity of the GameObject borders.")]
        [Range(0f, 1f)]
        private float gameObjectBorderOpacity = 0.6f;

        [SerializeField]
        [Tooltip("The list of built-in and custom visual themes.")]
        private List<HierarchyThemeData> themes = new List<HierarchyThemeData>();

        #endregion

        #region Reset Method

        /// <summary>
        /// Populates default layout configurations when the ScriptableObject is created or reset.
        /// </summary>
        private void Reset()
        {
            headers = new List<HierarchyHeaderData>
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
            globalLineColor = Color.white;
            globalLineStyle = HierarchyLineStyle.Solid;
            showNestingLines = true;
            nestingLinesColor = new Color(0.7f, 0.7f, 0.7f, 0.55f);
            showComponentIcons = true;
            showChildCountBadges = true;
            childCountColorMode = HierarchyChildCountColorMode.InheritNestingColor;
            childCountBorderStyle = HierarchyChildCountBorderStyle.None;
            childCountTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            childCountBgColor = new Color(0.18f, 0.18f, 0.18f, 0.85f);
            childCountBorderColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            useRainbowNesting = true;
            rainbowPalette = HierarchyRainbowPalette.Default;
            nestingLinesOpacity = 0.75f;
            showGameObjectBorder = true;
            gameObjectBorderColor = new Color(0.24f, 0.48f, 0.9f, 1f);
            gameObjectBorderOpacity = 0.6f;
            activeThemeIndex = 0;
            InitializeDefaultThemes();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of hierarchy header configurations.
        /// </summary>
        public List<HierarchyHeaderData> Headers
        {
            get => headers;
            set => headers = value;
        }

        /// <summary>
        /// Gets or sets the global line color for header separators.
        /// </summary>
        public Color GlobalLineColor
        {
            get => globalLineColor;
            set => globalLineColor = value;
        }

        /// <summary>
        /// Gets or sets the global line style for header separators.
        /// </summary>
        public HierarchyLineStyle GlobalLineStyle
        {
            get => globalLineStyle;
            set => globalLineStyle = value;
        }

        /// <summary>
        /// Gets or sets whether to show nesting lines.
        /// </summary>
        public bool ShowNestingLines
        {
            get => showNestingLines;
            set => showNestingLines = value;
        }

        /// <summary>
        /// Gets or sets the nesting lines color.
        /// </summary>
        public Color NestingLinesColor
        {
            get => nestingLinesColor;
            set => nestingLinesColor = value;
        }

        /// <summary>
        /// Gets or sets whether to show component quick-icons.
        /// </summary>
        public bool ShowComponentIcons
        {
            get => showComponentIcons;
            set => showComponentIcons = value;
        }

        /// <summary>
        /// Gets or sets whether to show child count badges.
        /// </summary>
        public bool ShowChildCountBadges
        {
            get => showChildCountBadges;
            set => showChildCountBadges = value;
        }

        /// <summary>
        /// Gets or sets the active color theme index.
        /// </summary>
        public int ActiveThemeIndex
        {
            get => activeThemeIndex;
            set => activeThemeIndex = value;
        }

        /// <summary>
        /// Gets or sets whether to use rainbow colors for nesting guide lines.
        /// </summary>
        public bool UseRainbowNesting
        {
            get => useRainbowNesting;
            set => useRainbowNesting = value;
        }

        /// <summary>
        /// Gets or sets the selected rainbow color theme palette.
        /// </summary>
        public HierarchyRainbowPalette RainbowPalette
        {
            get => rainbowPalette;
            set => rainbowPalette = value;
        }

        /// <summary>
        /// Gets or sets the opacity of the nesting lines.
        /// </summary>
        public float NestingLinesOpacity
        {
            get => nestingLinesOpacity;
            set => nestingLinesOpacity = value;
        }

        /// <summary>
        /// Gets or sets whether to show borders around selected/hovered GameObjects.
        /// </summary>
        public bool ShowGameObjectBorder
        {
            get => showGameObjectBorder;
            set => showGameObjectBorder = value;
        }

        /// <summary>
        /// Gets or sets the child count badge color mode.
        /// </summary>
        public HierarchyChildCountColorMode ChildCountColorMode
        {
            get => childCountColorMode;
            set => childCountColorMode = value;
        }

        /// <summary>
        /// Gets or sets the child count badge border style.
        /// </summary>
        public HierarchyChildCountBorderStyle ChildCountBorderStyle
        {
            get => childCountBorderStyle;
            set => childCountBorderStyle = value;
        }

        /// <summary>
        /// Gets or sets the child count badge text color.
        /// </summary>
        public Color ChildCountTextColor
        {
            get => childCountTextColor;
            set => childCountTextColor = value;
        }

        /// <summary>
        /// Gets or sets the child count badge background color.
        /// </summary>
        public Color ChildCountBgColor
        {
            get => childCountBgColor;
            set => childCountBgColor = value;
        }

        /// <summary>
        /// Gets or sets the child count badge border color.
        /// </summary>
        public Color ChildCountBorderColor
        {
            get => childCountBorderColor;
            set => childCountBorderColor = value;
        }

        /// <summary>
        /// Gets or sets the custom color used to draw GameObject borders.
        /// </summary>
        public Color GameObjectBorderColor
        {
            get => gameObjectBorderColor;
            set => gameObjectBorderColor = value;
        }

        /// <summary>
        /// Gets or sets the opacity of the GameObject borders.
        /// </summary>
        public float GameObjectBorderOpacity
        {
            get => gameObjectBorderOpacity;
            set => gameObjectBorderOpacity = value;
        }

        /// <summary>
        /// Gets or sets the list of visual themes.
        /// </summary>
        public List<HierarchyThemeData> Themes
        {
            get => themes;
            set => themes = value;
        }

        #endregion

        #region Theme Methods

        public HierarchyThemeData GetActiveTheme()
        {
            if (themes == null || themes.Count == 0)
            {
                InitializeDefaultThemes();
            }
            int index = Mathf.Clamp(activeThemeIndex, 0, themes.Count - 1);
            return themes[index];
        }

        public void InitializeDefaultThemes()
        {
            themes = new List<HierarchyThemeData>();

            // 1. Studio Pro (Default)
            HierarchyThemeData t1 = new HierarchyThemeData("Studio Pro");
            t1.xrColor = new Color(0.25f, 0.48f, 0.85f);
            t1.uiColor = new Color(0.42f, 0.44f, 0.9f);
            t1.audioColor = new Color(0.05f, 0.6f, 0.7f);
            t1.envColor = new Color(0.1f, 0.58f, 0.4f);
            t1.interactColor = new Color(0.85f, 0.58f, 0.1f);
            t1.effectsColor = new Color(0.8f, 0.65f, 0.1f);
            t1.managersColor = new Color(0.35f, 0.38f, 0.42f);
            t1.debugColor = new Color(0.82f, 0.28f, 0.28f);
            t1.headerGradientColor = new Color(0.12f, 0.12f, 0.12f, 0.7f);
            t1.borderColor = new Color(0.4f, 0.4f, 0.4f, 0.45f);
            t1.separatorLineColor = new Color(0.6f, 0.6f, 0.6f, 0.4f);
            t1.treeLineColor = new Color(0.7f, 0.7f, 0.7f, 0.55f);
            t1.badgeBackgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.85f);
            t1.badgeTextColor = Color.white;
            t1.iconAccentColor = new Color(0.8f, 0.8f, 0.8f);
            t1.hoverOverlayColor = new Color(1f, 1f, 1f, 0.08f);
            t1.selectionOverlayColor = new Color(0.25f, 0.48f, 0.85f, 0.25f);
            themes.Add(t1);

            // 2. Aqua Flow
            HierarchyThemeData t2 = new HierarchyThemeData("Aqua Flow");
            t2.xrColor = new Color(0.18f, 0.35f, 0.75f);
            t2.uiColor = new Color(0.12f, 0.58f, 0.82f);
            t2.audioColor = new Color(0.16f, 0.65f, 0.48f);
            t2.envColor = new Color(0.1f, 0.62f, 0.58f);
            t2.interactColor = new Color(0.85f, 0.48f, 0.22f);
            t2.effectsColor = new Color(0.88f, 0.72f, 0.15f);
            t2.managersColor = new Color(0.32f, 0.4f, 0.48f);
            t2.debugColor = new Color(0.78f, 0.22f, 0.22f);
            t2.headerGradientColor = new Color(0.03f, 0.2f, 0.25f, 0.7f);
            t2.borderColor = new Color(0.12f, 0.58f, 0.82f, 0.45f);
            t2.separatorLineColor = new Color(0.1f, 0.62f, 0.58f, 0.4f);
            t2.treeLineColor = new Color(0.1f, 0.62f, 0.58f, 0.55f);
            t2.badgeBackgroundColor = new Color(0.05f, 0.15f, 0.2f, 0.85f);
            t2.badgeTextColor = Color.white;
            t2.iconAccentColor = new Color(0.4f, 0.85f, 0.95f);
            t2.hoverOverlayColor = new Color(0.12f, 0.58f, 0.82f, 0.08f);
            t2.selectionOverlayColor = new Color(0.1f, 0.62f, 0.58f, 0.25f);
            themes.Add(t2);

            // 3. Solar Fusion
            HierarchyThemeData t3 = new HierarchyThemeData("Solar Fusion");
            t3.xrColor = new Color(0.35f, 0.38f, 0.78f);
            t3.uiColor = new Color(0.58f, 0.28f, 0.82f);
            t3.audioColor = new Color(0.08f, 0.48f, 0.58f);
            t3.envColor = new Color(0.18f, 0.52f, 0.28f);
            t3.interactColor = new Color(0.82f, 0.32f, 0.08f);
            t3.effectsColor = new Color(0.88f, 0.65f, 0.12f);
            t3.managersColor = new Color(0.42f, 0.4f, 0.38f);
            t3.debugColor = new Color(0.78f, 0.18f, 0.18f);
            t3.headerGradientColor = new Color(0.25f, 0.1f, 0.03f, 0.7f);
            t3.borderColor = new Color(0.82f, 0.32f, 0.08f, 0.45f);
            t3.separatorLineColor = new Color(0.82f, 0.32f, 0.08f, 0.4f);
            t3.treeLineColor = new Color(0.82f, 0.52f, 0.32f, 0.55f);
            t3.badgeBackgroundColor = new Color(0.22f, 0.18f, 0.15f, 0.85f);
            t3.badgeTextColor = Color.white;
            t3.iconAccentColor = new Color(0.95f, 0.6f, 0.4f);
            t3.hoverOverlayColor = new Color(0.88f, 0.65f, 0.12f, 0.08f);
            t3.selectionOverlayColor = new Color(0.82f, 0.32f, 0.08f, 0.25f);
            themes.Add(t3);

            // 4. Cloud Light
            HierarchyThemeData t4 = new HierarchyThemeData("Cloud Light");
            t4.xrColor = new Color(0.48f, 0.68f, 0.92f);
            t4.uiColor = new Color(0.68f, 0.58f, 0.92f);
            t4.audioColor = new Color(0.48f, 0.82f, 0.88f);
            t4.envColor = new Color(0.52f, 0.85f, 0.72f);
            t4.interactColor = new Color(0.92f, 0.72f, 0.52f);
            t4.effectsColor = new Color(0.92f, 0.85f, 0.58f);
            t4.managersColor = new Color(0.58f, 0.62f, 0.65f);
            t4.debugColor = new Color(0.92f, 0.52f, 0.52f);
            t4.headerGradientColor = new Color(0.4f, 0.4f, 0.45f, 0.7f);
            t4.borderColor = new Color(0.68f, 0.58f, 0.92f, 0.45f);
            t4.separatorLineColor = new Color(0.8f, 0.8f, 0.8f, 0.35f);
            t4.treeLineColor = new Color(0.68f, 0.58f, 0.92f, 0.45f);
            t4.badgeBackgroundColor = new Color(0.32f, 0.3f, 0.35f, 0.85f);
            t4.badgeTextColor = new Color(0.95f, 0.95f, 0.95f);
            t4.iconAccentColor = new Color(0.75f, 0.7f, 0.95f);
            t4.hoverOverlayColor = new Color(1f, 1f, 1f, 0.12f);
            t4.selectionOverlayColor = new Color(0.68f, 0.58f, 0.92f, 0.2f);
            themes.Add(t4);

            // 5. Midnight Pro
            HierarchyThemeData t5 = new HierarchyThemeData("Midnight Pro");
            t5.xrColor = new Color(0.12f, 0.32f, 0.78f);
            t5.uiColor = new Color(0.42f, 0.18f, 0.78f);
            t5.audioColor = new Color(0.05f, 0.38f, 0.38f);
            t5.envColor = new Color(0.08f, 0.42f, 0.22f);
            t5.interactColor = new Color(0.68f, 0.22f, 0.05f);
            t5.effectsColor = new Color(0.68f, 0.48f, 0.02f);
            t5.managersColor = new Color(0.24f, 0.28f, 0.34f);
            t5.debugColor = new Color(0.62f, 0.08f, 0.08f);
            t5.headerGradientColor = new Color(0.08f, 0.08f, 0.12f, 0.8f);
            t5.borderColor = new Color(0.2f, 0.2f, 0.2f, 0.55f);
            t5.separatorLineColor = new Color(0.3f, 0.3f, 0.3f, 0.45f);
            t5.treeLineColor = new Color(0.45f, 0.48f, 0.55f, 0.55f);
            t5.badgeBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            t5.badgeTextColor = Color.white;
            t5.iconAccentColor = new Color(0.55f, 0.6f, 0.7f);
            t5.hoverOverlayColor = new Color(1f, 1f, 1f, 0.05f);
            t5.selectionOverlayColor = new Color(0.12f, 0.32f, 0.78f, 0.3f);
            themes.Add(t5);
        }

        #endregion
    }
}
