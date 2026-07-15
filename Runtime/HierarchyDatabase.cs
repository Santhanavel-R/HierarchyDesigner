using System.Collections.Generic;
using UnityEngine;

namespace HierarchyDesigner.Runtime
{
    /// <summary>
    /// ScriptableObject database that stores all hierarchy section header configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "HierarchyLayout", menuName = "Hierarchy Designer/Hierarchy Layout Database", order = 120)]
    public class HierarchyDatabase : ScriptableObject
    {
        #region Fields

        [SerializeField]
        [Tooltip("The list of registered hierarchy section headers.")]
        private List<HierarchyHeaderData> headers = new List<HierarchyHeaderData>();

        [SerializeField]
        [Tooltip("The global line color for header separators.")]
        private Color globalLineColor = Color.white;

        [SerializeField]
        [Tooltip("The global line style for header separators.")]
        private HierarchyLineStyle globalLineStyle = HierarchyLineStyle.Solid;

        [SerializeField]
        [Tooltip("Toggle drawing of nesting connection tree lines (breadcrumbs).")]
        private bool showNestingLines = true;

        [SerializeField]
        [Tooltip("The color for hierarchy tree connection lines.")]
        private Color nestingLinesColor = new Color(1f, 1f, 1f, 0.18f);

        [SerializeField]
        [Tooltip("Toggle drawing of component quick-icons on the right.")]
        private bool showComponentIcons = true;

        [SerializeField]
        [Tooltip("Toggle drawing of child count badges on parent GameObjects.")]
        private bool showChildCountBadges = true;

        #endregion

        #region Reset Method

        /// <summary>
        /// Populates default layout configurations when the ScriptableObject is created or reset.
        /// </summary>
        private void Reset()
        {
            headers = new List<HierarchyHeaderData>
            {
                new HierarchyHeaderData("🎮 XR", new Color(0.12f, 0.45f, 0.6f, 0.85f)),
                new HierarchyHeaderData("🖥 UI", new Color(0.55f, 0.25f, 0.7f, 0.85f)),
                new HierarchyHeaderData("🔊 AUDIO", new Color(0.1f, 0.55f, 0.55f, 0.85f)),
                new HierarchyHeaderData("🌍 ENVIRONMENT", new Color(0.15f, 0.5f, 0.25f, 0.85f)),
                new HierarchyHeaderData("🎯 INTERACTABLES", new Color(0.85f, 0.35f, 0.1f, 0.85f)),
                new HierarchyHeaderData("✨ EFFECTS", new Color(0.8f, 0.6f, 0.1f, 0.85f)),
                new HierarchyHeaderData("⚙ Managers", new Color(0.35f, 0.35f, 0.35f, 0.85f)),
                new HierarchyHeaderData("🐞 DEBUG", new Color(0.75f, 0.2f, 0.2f, 0.85f))
            };
            globalLineColor = Color.white;
            globalLineStyle = HierarchyLineStyle.Solid;
            showNestingLines = true;
            nestingLinesColor = new Color(1f, 1f, 1f, 0.18f);
            showComponentIcons = true;
            showChildCountBadges = true;
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

        #endregion
    }
}
