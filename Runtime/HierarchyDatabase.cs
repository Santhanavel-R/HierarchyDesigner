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

        #endregion
    }
}
