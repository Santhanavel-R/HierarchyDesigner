using UnityEngine;

namespace HierarchyDesigner.Runtime
{
    /// <summary>
    /// Component placed on hierarchy section headers. Contains a unique ID matching
    /// the configuration in HierarchyDatabase. Contains no runtime logic.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Hierarchy Designer/Hierarchy Header")]
    public class HierarchyHeader : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [HideInInspector]
        private string guid;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier linking this header instance to its database definition.
        /// </summary>
        public string Guid
        {
            get => guid;
            set => guid = value;
        }

        #endregion
    }
}
