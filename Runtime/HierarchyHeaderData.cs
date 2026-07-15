using System;
using UnityEngine;

namespace HierarchyDesigner.Runtime
{
    /// <summary>
    /// Style of separator lines drawn for headers.
    /// </summary>
    public enum HierarchyLineStyle
    {
        Solid,
        Dashed,
        Dotted,
        Double,
        None
    }

    /// <summary>
    /// Represents the configuration data for a single hierarchy section header.
    /// </summary>
    [Serializable]
    public class HierarchyHeaderData
    {
        #region Fields

        [SerializeField]
        [Tooltip("The text label displayed in the hierarchy header.")]
        private string headerName;

        [SerializeField]
        [Tooltip("The color used for hierarchy background representation.")]
        private Color color;

        [SerializeField]
        [Tooltip("The unique identifier linking this data to a GameObject in the scene.")]
        private string guid;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the text label displayed in the hierarchy header.
        /// </summary>
        public string HeaderName
        {
            get => headerName;
            set => headerName = value;
        }

        /// <summary>
        /// Gets or sets the background color used for hierarchy drawing.
        /// </summary>
        public Color Color
        {
            get => color;
            set => color = value;
        }

        /// <summary>
        /// Gets or sets the unique identifier linking this data to a GameObject in the scene.
        /// </summary>
        public string Guid
        {
            get => guid;
            set => guid = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyHeaderData"/> class.
        /// </summary>
        public HierarchyHeaderData()
        {
            headerName = "New Header";
            color = Color.gray;
            guid = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyHeaderData"/> class with parameters.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="headerColor">The header background color.</param>
        public HierarchyHeaderData(string name, Color headerColor)
        {
            headerName = name;
            color = headerColor;
            guid = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyHeaderData"/> class with obsolete parameters.
        /// </summary>
        public HierarchyHeaderData(string name, string icon, Color headerColor)
        {
            headerName = name;
            color = headerColor;
            guid = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyHeaderData"/> class with background and obsolete parameters.
        /// </summary>
        public HierarchyHeaderData(string name, string icon, Color headerColor, Color headerLineColor)
        {
            headerName = name;
            color = headerColor;
            guid = System.Guid.NewGuid().ToString();
        }

        #endregion
    }
}
